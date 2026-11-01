using DotNetSkeleton.Modules.ExampleModule.DTOs.Requests;
using DotNetSkeleton.Shared.Validation;
using FluentValidation;

namespace DotNetSkeleton.Modules.ExampleModule.Validators;

public class CreateExampleDtoValidator : AbstractValidator<CreateExampleDto>
{
    public CreateExampleDtoValidator()
    {
        RuleFor(x => x.Name)
            .MustBeRequiredText().WithMessage("Name không được để trống")
            .MustHaveLengthBetween(3, 20).WithMessage("Name phải có từ 3 đến 20 ký tự");
    }
}
