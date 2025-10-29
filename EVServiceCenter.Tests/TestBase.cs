using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Tests;

/// <summary>
/// Base class for unit tests with common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected EVDbContext DbContext { get; private set; }
    protected ServiceProvider ServiceProvider { get; private set; }

    protected TestBase()
    {
        SetupTestEnvironment();
    }

    private void SetupTestEnvironment()
    {
        // Create in-memory database
        var services = new ServiceCollection();
        
        services.AddDbContext<EVDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        services.AddLogging(builder => builder.AddConsole());
        
        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<EVDbContext>();
        
        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Create a mock logger for testing
    /// </summary>
    protected ILogger<T> CreateMockLogger<T>()
    {
        return ServiceProvider.GetRequiredService<ILogger<T>>();
    }

    /// <summary>
    /// Seed test data if needed
    /// </summary>
    protected virtual async Task SeedTestDataAsync()
    {
        // Override in derived classes to add specific test data
        await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        ServiceProvider?.Dispose();
    }
}
