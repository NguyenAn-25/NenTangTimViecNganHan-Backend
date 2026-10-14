using DotNetSkeleton.Modules.ExampleModule.DTOs.Requests;
using FluentValidation;

namespace DotNetSkeleton.Modules.ExampleModule.Validators;

public class CreateExampleDtoValidator : AbstractValidator<CreateExampleDto>
{
    public CreateExampleDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name không được để trống")
            .MinimumLength(3).WithMessage("Name phải có ít nhất 3 ký tự")
            .MaximumLength(20).WithMessage("Name không được vượt quá 20 ký tự");
    }
}