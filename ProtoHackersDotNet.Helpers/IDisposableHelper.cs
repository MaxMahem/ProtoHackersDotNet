﻿using System.Diagnostics.CodeAnalysis;

namespace ProtoHackersDotNet.Helpers;

public static class IDisposableHelper
{
    /// <summary>Discards an unneeded IDisposable.</summary>
    /// <param name="disposable">The value to be discarded.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Value deliberately unused so it can be discarded")]
    public static void DiscardUnsubscribe<T>(this T disposable) where T : IDisposable { }
}