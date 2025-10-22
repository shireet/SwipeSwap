using EntryPoint.Dtos;
using FluentValidation;


namespace SwipeSwap.WebApi.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one number.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");
        RuleFor(x => x.DisplayName)
            
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(2).MaximumLength(50)
            .WithMessage("Display name must be between 2 and 30 characters.");
    }
}