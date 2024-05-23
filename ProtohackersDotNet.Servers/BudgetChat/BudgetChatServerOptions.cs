using System.ComponentModel.DataAnnotations;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed class BudgetChatServerOptions
{
    const int MIN_MAX_NAME_LENGTH = 16;
    const int MIN_MAX_MESSAGE_LENGTH = 1000;

    [RegularExpression(@"^[ -~]+$", ErrorMessage = "Only Printable Ascii permitted.")]
    public required string WelcomeMessage { get; init; }

    [RegularExpression(@"^[ -~]+$", ErrorMessage = "Only Printable Ascii permitted.")]
    public required string NameDelimiter { get; init; }

    /// <summary>Notice detailing the joined users in the chat. Must begin with '*".</summary>
    [RegularExpression(@"^\*[ -~]+{n}[ -~]*$", ErrorMessage = "Must begin with '*', contain the marker {n} for insertion, and consist of only Printable Ascii.")]
    public required string PresenceNotice { get; init; }

    /// <summary>Notice to display when a user joins the chat. Must begin with '*'.</summary>
    [RegularExpression(@"^\*[ -~]+{n}[ -~]*$", ErrorMessage = "Must begin with '*', contain the marker {n} for insertion, and consist of only Printable Ascii.")]
    public required string JoinNotice { get; init; }

    /// <summary>Notice to display when a user leaves the chat. Must begin with '*'.</summary>
    [RegularExpression(@"^\*[ -~]+{n}[ -~]*$", ErrorMessage = "Must begin with '*', contain the marker {n} for insertion, and consist of only Printable Ascii.")]
    public required string PartNotice { get; init; }

    [Range(MIN_MAX_NAME_LENGTH, int.MaxValue)]
    public required int MaxNameLength { get; init; }

    [Range(MIN_MAX_MESSAGE_LENGTH, int.MaxValue)]
    public required int MaxMessageLength { get; init; }
}
