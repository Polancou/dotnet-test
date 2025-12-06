using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace IntegrationTest;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly CustomWebApplicationFactory<Program> _factory;
    protected readonly ApplicationDbContext _dbContext;
    private static Respawner? _respawner;
    private readonly string _connectionString;

    protected BaseIntegrationTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var config = _scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" },
                WithReseed = true,
                DbAdapter = DbAdapter.SqlServer
            });
        }

        await _respawner.ResetAsync(connection);
    }

    public Task DisposeAsync()
    {
        _scope?.Dispose();
        return Task.CompletedTask;
    }
}
