using System.ComponentModel.DataAnnotations;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

/// <summary>Validates that a given timespan falls within the range [<paramref name="min"/>, <paramref name="max"/>].</summary>
/// <param name="min">String representation of the minimum range. If null, <see cref="TimeSpan.MinValue"/> is used.</param>
/// <param name="min">String representation of the miximum range. If null, <see cref="TimeSpan.MaxValue"/> is used.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class TimeSpanRangeAttribute(string? min = null, string? max = null) : ValidationAttribute
{
    readonly TimeSpan min = min is not null ? TimeSpan.Parse(min) : TimeSpan.MinValue;
    readonly TimeSpan max = max is not null ? TimeSpan.Parse(max) : TimeSpan.MaxValue;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
        => value is TimeSpan span && span >= this.min && span <= this.max ? ValidationResult.Success 
            : new ValidationResult($"{context.DisplayName} must be within the range [{this.min}, {this.max}].");
}