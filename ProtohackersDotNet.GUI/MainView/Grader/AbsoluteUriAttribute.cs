using System.ComponentModel.DataAnnotations;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

/// <summary>Specifies that a field or property must be a valid absolute URI.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class AbsoluteUriAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
        => value is Uri { IsAbsoluteUri: true } ? ValidationResult.Success 
            : new ValidationResult($"{context.DisplayName} must be an absolute Url.");
}
