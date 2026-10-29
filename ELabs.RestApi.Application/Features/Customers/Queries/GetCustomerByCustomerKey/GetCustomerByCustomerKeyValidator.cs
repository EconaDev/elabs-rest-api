using FluentValidation;

namespace ELabs.RestApi.Application.Features.Customers.Queries.GetCustomerByCustomerKey;

public class GetCustomerByCustomerKeyValidator : AbstractValidator<GetCustomerByCustomerKeyQuery>
{
    public GetCustomerByCustomerKeyValidator()
    {
        RuleFor(x => x.CustomerKey)
            .GreaterThan(0)
            .WithMessage("Customer key must be greater than 0");
    }
}
