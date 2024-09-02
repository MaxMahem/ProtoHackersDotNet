using Microsoft.Extensions.DependencyInjection;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Options;

namespace ProtoHackersDotNet.GUI;

public static class ServiceCollectionHelper
{
    public static IServiceCollection RegisterOption<T>(this IServiceCollection services) where T : class
        => services.AddOptions<T>().BindConfiguration(typeof(T).Name)
                   .ValidateDataAnnotations().ValidateOnStart()
                   .Services
                   .AddSingleton(provider => provider.GetRequiredService<IOptions<T>>().Value);

    public static IServiceCollection AddResolver<TType, TInterface>(this IServiceCollection services)
        where TType : class, TInterface
        where TInterface : class
        => services.AddSingleton<TInterface>(provider => provider.GetRequiredService<TType>());

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for discard")]
    public static void EndChain(this IServiceCollection services) { }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for discard")]
    public static void EndChain(this IHttpClientBuilder builder) { }
}
