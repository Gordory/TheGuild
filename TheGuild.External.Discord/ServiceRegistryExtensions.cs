using LightInject;

namespace TheGuild.External.Discord;

public static class ServiceRegistryExtensions
{
    public static void RegisterDiscordDependencies(this IServiceRegistry serviceRegistry, CancellationToken? cancellationToken = null)
    {
        serviceRegistry.RegisterInstance(cancellationToken ?? CancellationToken.None);

        serviceRegistry.RegisterSingleton<IDiscordClientFactory, DiscordClientFactory>();
        serviceRegistry.RegisterSingleton(sf => sf.GetInstance<IDiscordClientFactory>().CreateAsync().GetAwaiter().GetResult());
    }
}