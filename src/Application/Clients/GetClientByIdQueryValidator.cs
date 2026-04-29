using FluentValidation;

namespace Application.Clients;

public sealed class GetClientByIdQueryValidator : AbstractValidator<GetClientByIdQuery>
{
    public GetClientByIdQueryValidator()
    {
        RuleFor(query => query.ClientId)
            .NotEmpty();
    }
}
