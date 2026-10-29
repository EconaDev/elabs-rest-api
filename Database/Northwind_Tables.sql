USE [Northwind]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Form_Submissions (was Load_Status) ---------- */
CREATE TABLE [dbo].[Form_Submissions](
    [ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
    [FileName] [varchar](255) NULL,
    [FormName] [varchar](255) NULL,
    [ProcessStatus] [int] NULL,
    [StartTime] [datetime] NULL,
    [EndTime] [datetime] NULL,
    [DBProcStatus] [smallint] NULL,
 CONSTRAINT [PK_Form_Submissions] PRIMARY KEY CLUSTERED ([ID] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/* ---------- Submission_Data ---------- */
CREATE TABLE [dbo].[Submission_Data](
    [fileid] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
    [project] [varchar](255) NULL,
    [filename] [varchar](1000) NULL,
    [field] [varchar](1000) NULL,
    [value] [varchar](max) NULL,
    [created] [datetime] NULL,
    [loadid] [int] NULL,
 CONSTRAINT [PK_Submission_Data] PRIMARY KEY CLUSTERED ([fileid] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 /* loadid stays nullable; no cascade, so deleting a submission that still has data is blocked */
 CONSTRAINT [FK_Submission_Data_Form_Submissions] FOREIGN KEY ([loadid]) REFERENCES [dbo].[Form_Submissions] ([ID]),
 /* seek path for "all fields of one submission"; field/value deliberately not included */
 INDEX [IX_Submission_Data_loadid] NONCLUSTERED ([loadid] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/* ---------- ELabs_Web_Services_Logs ---------- */
CREATE TABLE [dbo].[ELabs_Web_Services_Logs](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [LogCorrelationId] [nvarchar](50) NULL,
    [LogTimestamp] [datetime] NOT NULL,
    [LogLevel] [nvarchar](50) NULL,
    [LogServiceName] [nvarchar](200) NULL,
    [LogMethodName] [nvarchar](200) NULL,
    [LogStatus] [nvarchar](50) NULL,
    [LogDurationMs] [int] NULL,
    [LogUserIdentity] [nvarchar](200) NULL,
    [LogFormPanel] [nvarchar](200) NULL,
    [LogFormName] [nvarchar](200) NULL,
    [LogClientIP] [nvarchar](100) NULL,
    [LogUserAgent] [nvarchar](500) NULL,
    [LogMessage] [nvarchar](max) NULL,
    [LogException] [nvarchar](max) NULL,
 CONSTRAINT [PK_ELabs_Web_Services_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/* ---------- ELabs_REST_API_Logs (Serilog sink) ---------- */
CREATE TABLE [dbo].[ELabs_REST_API_Logs](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Message] [nvarchar](max) NULL,
    [MessageTemplate] [nvarchar](max) NULL,
    [Level] [nvarchar](128) NULL,
    [TimeStamp] [datetimeoffset](7) NOT NULL,
    [Exception] [nvarchar](max) NULL,
    [Properties] [nvarchar](max) NULL,
    [LogEvent] [nvarchar](max) NULL,
    [CorrelationId] [nvarchar](100) NULL,
    [UserEmail] [nvarchar](200) NULL,
    [UserName] [nvarchar](200) NULL,
    [FormName] [nvarchar](200) NULL,
    [FormSection] [nvarchar](200) NULL,
    [ClientIP] [nvarchar](100) NULL,
    [UserAgent] [nvarchar](500) NULL,
    [RequestName] [nvarchar](200) NULL,
    [ElapsedUs] [bigint] NULL,
    [AfterJson] [nvarchar](max) NULL,
    [ValidationErrorsJson] [nvarchar](max) NULL,
 CONSTRAINT [PK_ELabs_REST_API_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/* ---------- ELabs_REST_API_Audit ---------- */
CREATE TABLE [dbo].[ELabs_REST_API_Audit](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [CorrelationId] [nvarchar](100) NULL,
    [UserEmail] [nvarchar](200) NULL,
    [UserName] [nvarchar](200) NULL,
    [Action] [nvarchar](200) NOT NULL,
    [RequestName] [nvarchar](200) NULL,
    [RequestParams] [nvarchar](800) NULL,
    [FormName] [nvarchar](200) NULL,
    [FormSection] [nvarchar](200) NULL,
    [IpAddress] [nvarchar](100) NULL,
    [CreatedAt] [datetimeoffset](7) NOT NULL CONSTRAINT [DF_ELabs_REST_API_Audit_CreatedAt] DEFAULT (sysdatetimeoffset()),
    [FromCache] [bit] NOT NULL CONSTRAINT [DF_ELabs_REST_API_Audit_FromCache] DEFAULT ((0)),
    [Status] [nvarchar](50) NOT NULL CONSTRAINT [DF_ELabs_REST_API_Audit_Status] DEFAULT ('SUCCESS'),
 CONSTRAINT [PK_ELabs_REST_API_Audit] PRIMARY KEY CLUSTERED ([Id] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/* ---------- PowerAppAttributes (definition table) ---------- */
CREATE TABLE [dbo].[PowerAppAttributes](
    [AttributeID] [int] IDENTITY(1,1) NOT NULL,
    [AttributeName] [nvarchar](100) NOT NULL,
    [FormName] [nvarchar](100) NOT NULL,
    [DataType] [nvarchar](50) NOT NULL,
    [AssociatedColumn] [nvarchar](50) NOT NULL,
    [IsRequired] [bit] NOT NULL,
    [IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_PowerAppAttributes] PRIMARY KEY CLUSTERED ([AttributeID] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_PAA_FormScope] UNIQUE NONCLUSTERED ([FormName] ASC, [AttributeName] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PowerAppAttributes] ADD CONSTRAINT [DF_PAA_IsRequired] DEFAULT ((0)) FOR [IsRequired]
GO
ALTER TABLE [dbo].[PowerAppAttributes] ADD CONSTRAINT [DF_PAA_IsActive] DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[PowerAppAttributes] WITH CHECK ADD CONSTRAINT [CHK_PAA_AssociatedColumn] CHECK (([AssociatedColumn]='ValueBoolean' OR [AssociatedColumn]='ValueDate' OR [AssociatedColumn]='ValueNumber' OR [AssociatedColumn]='ValueText'))
GO
ALTER TABLE [dbo].[PowerAppAttributes] CHECK CONSTRAINT [CHK_PAA_AssociatedColumn]
GO

/* ---------- PowerAppSubmissions ---------- */
CREATE TABLE [dbo].[PowerAppSubmissions](
    [PowerAppID] [int] IDENTITY(1,1) NOT NULL,
    [PowerAppName] [nvarchar](100) NOT NULL,
    [FormName] [nvarchar](100) NOT NULL,
    [FormStatus] [nvarchar](50) NOT NULL,
    [CreatedDate] [datetime2](7) NOT NULL,
    [SubmittedBy] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_PowerAppSubmissions] PRIMARY KEY CLUSTERED ([PowerAppID] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PowerAppSubmissions] ADD CONSTRAINT [DF_PASub_FormName] DEFAULT ('Unspecified') FOR [FormName]
GO
ALTER TABLE [dbo].[PowerAppSubmissions] ADD CONSTRAINT [DF_PASub_Status] DEFAULT ('Draft') FOR [FormStatus]
GO
ALTER TABLE [dbo].[PowerAppSubmissions] ADD CONSTRAINT [DF_PASub_CreatedDate] DEFAULT (sysdatetime()) FOR [CreatedDate]
GO

/* ---------- PowerAppAttributeValues (values table; must follow the two above) ---------- */
CREATE TABLE [dbo].[PowerAppAttributeValues](
    [PowerAppValueID] [int] IDENTITY(1,1) NOT NULL,
    [PowerAppID] [int] NOT NULL,
    [AttributeID] [int] NOT NULL,
    [InstanceNumber] [int] NOT NULL,
    [ValueText] [nvarchar](max) NULL,
    [ValueNumber] [numeric](18, 5) NULL,
    [ValueDate] [date] NULL,
    [ValueBoolean] [bit] NULL,
 CONSTRAINT [PK_PAAV] PRIMARY KEY CLUSTERED ([PowerAppValueID] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_PAAV] UNIQUE NONCLUSTERED ([PowerAppID] ASC, [AttributeID] ASC, [InstanceNumber] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[PowerAppAttributeValues] ADD CONSTRAINT [DF_PAAV_InstanceNumber] DEFAULT ((1)) FOR [InstanceNumber]
GO
ALTER TABLE [dbo].[PowerAppAttributeValues] WITH CHECK ADD CONSTRAINT [FK_PAAV_PowerAppAttributes] FOREIGN KEY([AttributeID]) REFERENCES [dbo].[PowerAppAttributes] ([AttributeID])
GO
ALTER TABLE [dbo].[PowerAppAttributeValues] CHECK CONSTRAINT [FK_PAAV_PowerAppAttributes]
GO
ALTER TABLE [dbo].[PowerAppAttributeValues] WITH CHECK ADD CONSTRAINT [FK_PAAV_Submissions] FOREIGN KEY([PowerAppID]) REFERENCES [dbo].[PowerAppSubmissions] ([PowerAppID]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PowerAppAttributeValues] CHECK CONSTRAINT [FK_PAAV_Submissions]
GO
