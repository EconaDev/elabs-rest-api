using FluentValidation;

namespace ELabs.RestApi.Application.Features.Stores.Queries.GetStoreByBusinessEntityId;

public class GetStoreByBusinessEntityIdValidator: AbstractValidator<GetStoreByBusinessEntityIdQuery>
{
    public GetStoreByBusinessEntityIdValidator()
    {
        RuleFor(x => x.BusinessEntityId)
            .GreaterThan(0)
            .WithMessage("Business entity ID must be greater than 0");
    }
}
