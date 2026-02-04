using FluentValidation;
using Projects.Abstractions.Commands.Projects;

namespace Projects.API.Validators;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Project type is required")
            .MaximumLength(50).WithMessage("Project type must not exceed 50 characters");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required");
    }
}
