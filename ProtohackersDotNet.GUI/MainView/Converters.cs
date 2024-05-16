using System.Globalization;
using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using Avalonia.Data.Converters;
using CommunityToolkit.Diagnostics;

namespace ProtoHackersDotNet.GUI.MainView;

public static class Converters
{
    /// <summary>Converts an endpoint and a string into a name.</summary>
    public static FuncMultiValueConverter<object?, string> ClientNameConverter { get; } =
        new(objects => objects.ToArray() switch {
            [EndPoint endPoint, string name] => $"{endPoint} - \"{name}\"",
            [EndPoint endPoint, _] => endPoint.ToString() ?? ThrowHelper.ThrowArgumentNullException<string>(),
            _ => string.Empty // ThrowHelper.ThrowArgumentException<string>()
        });
}