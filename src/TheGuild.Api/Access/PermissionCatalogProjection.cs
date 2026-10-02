using TheGuild.DataLayer.Access;
using TheGuild.DataLayer.Models.Access;

namespace TheGuild.Api.Access;

public sealed class PermissionCatalogProjection : IHostedService
{
    private readonly IPermissionDescriptorRepository _repository;
    private readonly ILogger<PermissionCatalogProjection> _logger;

    public PermissionCatalogProjection(
        IPermissionDescriptorRepository repository,
        ILogger<PermissionCatalogProjection> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _repository.ProjectAsync(PermissionCatalog.All);
        }
        catch (Exception exception)
        {
            // The projection only feeds the settings screen; checks and grant validation read the
            // code catalogue, so an unreachable database must not keep the API from starting.
            _logger.LogError(exception, "Could not project the permission catalogue into the database");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
