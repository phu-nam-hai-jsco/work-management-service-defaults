using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shouldly;

namespace WorkManagement.ServiceDefaults.Tests;

[TestFixture]
public class ExtensionsTests
{
    [Test]
    public async Task ShouldRegisterHealthySelfCheck_WhenAddDefaultHealthChecksCalled()
    {
        var builder = WebApplication.CreateBuilder();

        builder.AddDefaultHealthChecks();

        using var provider = builder.Services.BuildServiceProvider();
        var healthCheckService = provider.GetRequiredService<HealthCheckService>();

        var report = await healthCheckService.CheckHealthAsync();

        report.Status.ShouldBe(HealthStatus.Healthy);
        report.Entries.Keys.ShouldContain("self");
        report.Entries["self"].Tags.ShouldContain("live");
    }

    [Test]
    public void ShouldReturnSameBuilderInstance_WhenAddServiceDefaultsCalled()
    {
        var builder = WebApplication.CreateBuilder();

        var result = builder.AddServiceDefaults();

        result.ShouldBeSameAs(builder);
    }

    [Test]
    public void ShouldRegisterOpenTelemetryProviders_WhenConfigureOpenTelemetryCalled()
    {
        var builder = WebApplication.CreateBuilder();

        builder.ConfigureOpenTelemetry();

        using var provider = builder.Services.BuildServiceProvider();

        provider.GetRequiredService<TracerProvider>().ShouldNotBeNull();
        provider.GetRequiredService<MeterProvider>().ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldMapHealthAndAliveEndpoints_WhenMapDefaultEndpointsCalled()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddDefaultHealthChecks();
        await using var app = builder.Build();

        app.MapDefaultEndpoints();

        var routePatterns = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(e => e.RoutePattern.RawText)
            .ToList();

        routePatterns.ShouldContain("/health");
        routePatterns.ShouldContain("/alive");
    }
}
