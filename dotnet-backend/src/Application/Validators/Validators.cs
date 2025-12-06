using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

/// <summary>
/// Validator for <see cref="LoginRequest"/> ensuring required fields are present.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// Initializes validation rules for login requests.
    /// </summary>
    public LoginRequestValidator()
    {
        // Username must not be empty.
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");

        // Password must not be empty.
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

/// <summary>
/// Validator for <see cref="RegisterRequest"/> ensuring username, email, and password meet requirements.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// Initializes validation rules for user registration requests.
    /// </summary>
    public RegisterRequestValidator()
    {
        // Username must not be empty and at least 3 characters long.
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.");

        // Email must not be empty and must be a valid email address.
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        // Password must not be empty and at least 6 characters long.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
    }
}

/// <summary>
/// Validator for <see cref="DocumentUploadRequest"/> ensuring file information and content are present.
/// </summary>
public class DocumentUploadRequestValidator : AbstractValidator<DocumentUploadRequest>
{
    /// <summary>
    /// Initializes validation rules for document upload requests.
    /// </summary>
    public DocumentUploadRequestValidator()
    {
        // FileName must not be empty.
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");

        // ContentType must not be empty.
        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.");

        // Content stream must not be null.
        RuleFor(x => x.Content)
            .NotNull().WithMessage("File content cannot be null.");
    }
}
