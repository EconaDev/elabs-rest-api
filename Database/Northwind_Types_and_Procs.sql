USE [Northwind]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* Requires: PowerAppSubmissions, PowerAppAttributes, PowerAppAttributeValues, Self_Reference_ListItems (Northwind_Tables.sql) */

/* ---------- Table type ---------- */
/* Guarded so the script can be re-run; a changed type definition must be dropped manually first. */
IF TYPE_ID(N'dbo.PowerAppAttributeSubmissionType') IS NULL
BEGIN
    CREATE TYPE [dbo].[PowerAppAttributeSubmissionType] AS TABLE(
        [AttributeName] [nvarchar](100) NOT NULL,
        [AssociatedColumn] [nvarchar](50) NOT NULL,
        [DataType] [nvarchar](50) NOT NULL,
        [InstanceNumber] [int] NOT NULL,
        [ValueText] [nvarchar](max) NULL,
        [ValueNumber] [numeric](18, 5) NULL,
        [ValueDate] [date] NULL,
        [ValueBoolean] [bit] NULL
    )
END
GO

/* ---------- usp_ELabs_PA_UpsertSubmissionAttributes ---------- */
CREATE OR ALTER PROCEDURE [dbo].[usp_ELabs_PA_UpsertSubmissionAttributes]
    @PowerAppID     INT,
    @FormName       NVARCHAR(100),
    @AttributeJSON  NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    ------------------------------------------------------------------
    -- Validate submission exists
    ------------------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.PowerAppSubmissions
        WHERE PowerAppID = @PowerAppID
    )
    BEGIN
        RAISERROR (
            'Invalid PowerAppID: submission does not exist.',
            16, 1
        );
        RETURN;
    END;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @AttributeList dbo.PowerAppAttributeSubmissionType;

        ------------------------------------------------------------------
        -- Parse JSON into TVP
        ------------------------------------------------------------------
        INSERT INTO @AttributeList
        (
            AttributeName,
            DataType,
            AssociatedColumn,
            InstanceNumber,
            ValueText,
            ValueNumber,
            ValueDate,
            ValueBoolean
        )
        SELECT
            JSON_VALUE(j.value, '$.AttributeName'),
            CASE
                WHEN JSON_VALUE(j.value, '$.DataType')
                     IN ('Text','Number','Date','Boolean')
                THEN JSON_VALUE(j.value, '$.DataType')
                ELSE 'Text'
            END,
            JSON_VALUE(j.value, '$.AssociatedColumn'),
            ISNULL(TRY_CAST(JSON_VALUE(j.value, '$.InstanceNumber') AS INT), 1),
            JSON_VALUE(j.value, '$.ValueText'),
            TRY_CAST(JSON_VALUE(j.value, '$.ValueNumber') AS NUMERIC(18,5)),
            TRY_CAST(JSON_VALUE(j.value, '$.ValueDate') AS DATE),
            TRY_CAST(JSON_VALUE(j.value, '$.ValueBoolean') AS BIT)
        FROM OPENJSON(@AttributeJSON) j;

        ------------------------------------------------------------------
        -- Reject DataType / AssociatedColumn conflicts. The stored definition is authoritative:
        -- an attribute is identified by (FormName, AttributeName), matching UQ_PAA_FormScope.
        ------------------------------------------------------------------
        DECLARE @Conflict NVARCHAR(1000);

        -- inside the payload: same attribute sent with different types or columns
        SELECT TOP (1)
            @Conflict = CONCAT(N'Attribute ', AttributeName, N' is sent with conflicting DataType/AssociatedColumn.')
        FROM @AttributeList
        GROUP BY AttributeName
        HAVING COUNT(DISTINCT DataType) > 1
            OR COUNT(DISTINCT AssociatedColumn) > 1;

        IF @Conflict IS NOT NULL
            THROW 50001, @Conflict, 1;

        -- against the stored definition
        SELECT TOP (1)
            @Conflict = CONCAT(N'Attribute ', T.AttributeName, N' is defined as ', A.DataType,
                               N'; received ', T.DataType, N'.')
        FROM @AttributeList T
        INNER JOIN dbo.PowerAppAttributes A
            ON A.FormName      = @FormName
           AND A.AttributeName = T.AttributeName
        WHERE A.DataType <> T.DataType;

        IF @Conflict IS NOT NULL
            THROW 50001, @Conflict, 1;

        ------------------------------------------------------------------
        -- Insert missing attribute definitions
        ------------------------------------------------------------------
        INSERT INTO dbo.PowerAppAttributes
        (
            AttributeName,
            FormName,
            DataType,
            AssociatedColumn,
            IsRequired,
            IsActive
        )
        SELECT DISTINCT
            T.AttributeName,
            @FormName,
            T.DataType,
            T.AssociatedColumn,
            0,
            1
        FROM @AttributeList T
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.PowerAppAttributes A
            WHERE A.FormName      = @FormName
              AND A.AttributeName = T.AttributeName
        );

        ------------------------------------------------------------------
        -- UPDATE existing attribute values
        ------------------------------------------------------------------
        UPDATE V
        SET
            ValueText    = T.ValueText,
            ValueNumber  = T.ValueNumber,
            ValueDate    = T.ValueDate,
            ValueBoolean = T.ValueBoolean
        FROM dbo.PowerAppAttributeValues V
        INNER JOIN dbo.PowerAppAttributes A
            ON V.AttributeID = A.AttributeID
        INNER JOIN @AttributeList T
            ON A.AttributeName = T.AttributeName
           AND A.FormName      = @FormName
        WHERE
            V.PowerAppID     = @PowerAppID
            AND V.InstanceNumber = T.InstanceNumber;

        ------------------------------------------------------------------
        -- INSERT missing attribute values
        ------------------------------------------------------------------
        INSERT INTO dbo.PowerAppAttributeValues
        (
            PowerAppID,
            AttributeID,
            InstanceNumber,
            ValueText,
            ValueNumber,
            ValueDate,
            ValueBoolean
        )
        SELECT
            @PowerAppID,
            A.AttributeID,
            T.InstanceNumber,
            T.ValueText,
            T.ValueNumber,
            T.ValueDate,
            T.ValueBoolean
        FROM @AttributeList T
        INNER JOIN dbo.PowerAppAttributes A
            ON A.FormName      = @FormName
           AND A.AttributeName = T.AttributeName
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.PowerAppAttributeValues V
            WHERE V.PowerAppID     = @PowerAppID
              AND V.AttributeID    = A.AttributeID
              AND V.InstanceNumber = T.InstanceNumber
        );

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;

        DECLARE @ErrMsg NVARCHAR(4000);
        DECLARE @ErrSeverity INT;
        DECLARE @ErrState INT;

        SELECT
            @ErrMsg = ERROR_MESSAGE(),
            @ErrSeverity = ERROR_SEVERITY(),
            @ErrState = ERROR_STATE();

        RAISERROR (
            'usp_ELabs_PA_UpsertSubmissionAttributes failed: %s',
            @ErrSeverity,
            @ErrState,
            @ErrMsg
        );
    END CATCH

END;
GO

/* ---------- usp_ELabs_PA_GetSubmissionAttributesJSON ---------- */
CREATE OR ALTER PROCEDURE [dbo].[usp_ELabs_PA_GetSubmissionAttributesJSON]
    @FormName         NVARCHAR(100),
    @FilterAttribute  NVARCHAR(100),
    @FilterValue      NVARCHAR(MAX),
    @FormStatus       NVARCHAR(50) = 'Submitted'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PowerAppID INT;

    ------------------------------------------------------------------
    -- Resolve most recent matching submission (FULL datatype support)
    ------------------------------------------------------------------
    SELECT TOP (1)
        @PowerAppID = S.PowerAppID
    FROM dbo.PowerAppSubmissions S
    WHERE
        S.FormName   = @FormName
        AND S.FormStatus = @FormStatus
        AND EXISTS (
            SELECT 1
            FROM dbo.PowerAppAttributeValues V
            INNER JOIN dbo.PowerAppAttributes A
                ON V.AttributeID = A.AttributeID
            WHERE
                V.PowerAppID     = S.PowerAppID
                AND A.FormName   = S.FormName
                AND A.AttributeName = @FilterAttribute
                AND (
                        V.ValueText = @FilterValue
                    OR V.ValueNumber = TRY_CONVERT(NUMERIC(18,5), @FilterValue)
                    OR (
                        V.ValueDate IS NOT NULL
                        AND TRY_CONVERT(DATE, V.ValueDate)
                            = TRY_CONVERT(DATE, @FilterValue)
                    )
                    OR (
                        V.ValueBoolean IS NOT NULL
                        AND TRY_CONVERT(BIT, @FilterValue) = V.ValueBoolean
                    )
                )
        )
    ORDER BY S.CreatedDate DESC;

    ------------------------------------------------------------------
    -- No match found
    ------------------------------------------------------------------
    IF @PowerAppID IS NULL
    BEGIN
        SELECT NULL AS AttributesJSON;
        RETURN;
    END;

    ------------------------------------------------------------------
    -- Emit full attribute set as JSON
    ------------------------------------------------------------------
    SELECT (
        SELECT
            A.AttributeName,
            A.DataType,
            A.AssociatedColumn,
            V.InstanceNumber,
            V.ValueText,
            V.ValueNumber,
            V.ValueDate,
            V.ValueBoolean
        FROM dbo.PowerAppAttributeValues V
        INNER JOIN dbo.PowerAppAttributes A
            ON V.AttributeID = A.AttributeID
        WHERE V.PowerAppID = @PowerAppID
        ORDER BY A.AttributeID, V.InstanceNumber
        FOR JSON PATH
    ) AS AttributesJSON;
END;
GO

--/* ---------- sp_AddHierarchyItem ----------
--   Depends on dbo.Self_Reference_ListItems (created in Northwind_Tables.sql). */
--CREATE OR ALTER PROCEDURE [dbo].[sp_AddHierarchyItem] (
--    @itempParentId INT,
--    @itemValue VARCHAR(250),
--    @itemText VARCHAR(250),
--    @isItem CHAR(1),
--    @newUserId INT OUTPUT
--)
--AS
--BEGIN
--    DECLARE @itemNode HIERARCHYID, @lc HIERARCHYID;

--    SELECT @itemNode = ItemNode
--    FROM dbo.Self_Reference_ListItems
--    WHERE ID = @itempParentId;

--    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

--    BEGIN TRANSACTION;

--    SELECT @lc = MAX(ItemNode)
--    FROM dbo.Self_Reference_ListItems
--    WHERE ItemNode.GetAncestor(1) = @itemNode;

--    INSERT dbo.Self_Reference_ListItems (ItemNode, ItemValue, ItemText, DisplayFlag, ActiveFlag, SortOrder, IsItem, CreatedTimestamp, UpdatedTimestamp)
--    VALUES (@itemNode.GetDescendant(@lc, NULL), @itemValue, @itemText, '1', '1', 1, @isItem, GETDATE(), GETDATE());

--    COMMIT;
--    SET @newUserId = SCOPE_IDENTITY()
--END;
--GO

--/* ---------- sp_GetFormPrefill ----------
--   Depends on dbo.Submission_Data. */
----
---- STORED PROCEDURE
----     sp_GetFormPrefill
----
---- DESCRIPTION
----     Used for the prefill of PDF forms via web service
----
---- PARAMETERS
----     @formName
----         * this is the form name used to retrieve data for just that form
----     @version
----         * this is the version used to retrieve data for just that form
----     @uniqueId
----         * this is a unique id to retrieve data for a specific form
----
---- RETURN VALUE
----         data set return for the specific parameters to be used by the web service
----
----
--CREATE OR ALTER PROCEDURE [dbo].[sp_GetFormPrefill]

--@formName nvarchar(255),
--@version nvarchar(10),
--@uniqueId nvarchar (50) = NULL

--AS
--BEGIN
--SET NOCOUNT ON

--DECLARE @hasVersion varchar(10) = null

--SET @hasVersion = (SELECT TOP 1 [value] FROM [dbo].[Submission_Data]
--                        WHERE project = @formName AND field = 'version'
--                        ORDER BY [value] DESC);

--    IF(@uniqueId IS NULL OR @uniqueId = '')
--        BEGIN
--            IF(@hasVersion <> @version)
--                BEGIN
--                    SELECT * FROM [dbo].[Submission_Data]
--                    WHERE loadid = (
--                        SELECT TOP 1 loadid FROM
--                        (
--                            SELECT loadid,[field],[value] FROM [dbo].[Submission_Data]
--                            WHERE [project] = @formName AND loadid IS NOT NULL
--                        ) AS SourceTable
--                        PIVOT( MAX([value]) FOR [field] IN ([VERSION], [InstitutionId])
--                        ) AS PivotTable WHERE [VERSION] = @hasVersion
--                        ORDER BY loadid DESC
--                    )
--                END
--            ELSE
--                BEGIN
--                    SELECT * FROM [dbo].[Submission_Data]
--                    WHERE loadid = (
--                        SELECT TOP 1 loadid FROM
--                        (
--                            SELECT loadid,[field],[value] FROM [dbo].[Submission_Data]
--                            WHERE [project] = @formName AND loadid IS NOT NULL
--                        ) AS SourceTable
--                        PIVOT( MAX([value]) FOR [field] IN ([VERSION], [InstitutionId])
--                        ) AS PivotTable WHERE [VERSION] = @version
--                        ORDER BY loadid DESC
--                    )
--                END
--        END
--    ELSE
--        BEGIN
--            SELECT * FROM [dbo].[Submission_Data]
--            WHERE loadid = (
--                SELECT TOP 1 loadid FROM
--                (
--                    SELECT loadid,[field],[value] FROM [dbo].[Submission_Data]
--                    WHERE [project] = @formName AND loadid IS NOT NULL
--                ) AS SourceTable
--                PIVOT( MAX([value]) FOR [field] IN ([VERSION], [UniqueId])
--                ) AS PivotTable WHERE [UniqueId] = @uniqueId
--                ORDER BY loadid DESC
--            )
--        END
--END
--GO
