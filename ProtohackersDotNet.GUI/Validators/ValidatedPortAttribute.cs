using System.ComponentModel.DataAnnotations;

namespace ProtoHackersDotNet.GUI.Validators;

public sealed class ValidatedPortAttribute() : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        => value is null || ushort.TryParse(value?.ToString(), out ushort port) ? ValidationResult.Success : new($"{value} is not a valid port number.");
}

