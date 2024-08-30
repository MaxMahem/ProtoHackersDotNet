using System.ComponentModel.DataAnnotations;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class AbsoluteUriAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
        => value is Uri { IsAbsoluteUri: true } ? ValidationResult.Success : new ValidationResult($"{context.DisplayName} must be an absolute Url.");
}
