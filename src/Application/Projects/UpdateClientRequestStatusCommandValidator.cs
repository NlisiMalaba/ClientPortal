using FluentValidation;

namespace Application.Projects;

public sealed class UpdateClientRequestStatusCommandValidator : AbstractValidator<UpdateClientRequestStatusCommand>
{
    public UpdateClientRequestStatusCommandValidator()
    {
        RuleFor(command => command.RequestId)
            .NotEmpty();
    }
}
