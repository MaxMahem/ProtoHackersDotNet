using System.ComponentModel.DataAnnotations;

namespace ProtoHackersDotNet.Servers.UdpDatabase;

public class UdpDatabaseServerOptions
{
    [RegularExpression(@"^=.*", ErrorMessage = "Must start with '='.")]
    [MaxByteSize(UdpDatabaseServer.MAX_MESSAGE_BYTES), Required]
    public required string Version { get; init; }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class MaxByteSizeAttribute(int MaxSize) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string stringValue) return ValidationResult.Success;
        
        var bytes = Encoding.Default.GetBytes(stringValue).Length;
        if (bytes <= MaxSize) return ValidationResult.Success;

        var errorMessage = string.Format(ErrorMessage ?? "String '{0}' byte size ({1}) exceeds the maximum allowed size ({2})",
                    validationContext.DisplayName, ByteSize.FromBytes(bytes), ByteSize.FromBytes(MaxSize));
        return new ValidationResult(errorMessage);
    }
}