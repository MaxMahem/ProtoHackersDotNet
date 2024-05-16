using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ProtoHackersDotNet.Validators;

public sealed partial class ValidatedIPAddressAttribute() : ValidationAttribute
{
    [GeneratedRegex("^(?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])$")]
    private static partial Regex IPv4Regex();

    [GeneratedRegex("((([0-9a-fA-F]){1,4})\\:){7}([0-9a-fA-F]){1,4}")]
    private static partial Regex IPv6Regex();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        => value is null || value is string str && (IPv4Regex().IsMatch(str) || IPv6Regex().IsMatch(str)) ? ValidationResult.Success : new($"{value} is not a valid IP address.");
}

