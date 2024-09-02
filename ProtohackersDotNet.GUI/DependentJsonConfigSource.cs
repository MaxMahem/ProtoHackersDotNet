using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace ProtoHackersDotNet.GUI;

/// <summary>Represents a JsonConfig source that that is loaded from a file who's path is defined in the previously loaded configurations.</summary>
/// <param name="jsonPathKey">The key to the json path value.</param>
/// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
/// <param name="optional">Whether the file is optional.</param>
/// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
public class DependentJsonConfigSource(string jsonPathKey, IFileProvider? provider, bool optional, bool reloadOnChange) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        // this is pretty jank, but no other way to get a file from the existing config sources
        var savePath = builder.Sources
            .Where(s => s != this) // Exclude this source to prevent recursion
            .Aggregate((IConfigurationBuilder) new ConfigurationBuilder(), (b, s) => b.Add(s))
            .Build()
            .GetValue<string>(jsonPathKey) ?? ThrowArgumentException<string>($"Key {jsonPathKey} not found.");

        return new JsonConfigurationSource(){ 
            Path = savePath, 
            FileProvider = provider, 
            Optional = optional, 
            ReloadOnChange = reloadOnChange 
        }.Build(builder);
    }
}
