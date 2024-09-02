using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace ProtoHackersDotNet.GUI;

public static class DependentJsonFileSourceHelper
{
    /// <summary>Adds a new json file who's path is defined by a key in an existing config element.</summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="filePathKey">The key in the existing config sources that locates the json file.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns></returns>
    public static IConfigurationBuilder AddDependentJsonFile(this IConfigurationBuilder builder, 
        string filePathKey,
        IFileProvider? fileProvider = null,
        bool optional = true, 
        bool reloadOnChange = false)
    {
        DependentJsonConfigSource nestedConfigSource = new(filePathKey, provider: fileProvider, optional: optional, reloadOnChange: reloadOnChange);
        return builder.Add(nestedConfigSource);
    }
}