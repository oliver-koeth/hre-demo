using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AuthModule.Foundation.Api;

public static class DiagnosticsEndpoints
{
    public static IEndpointRouteBuilder MapFoundationDiagnostics(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/internal/foundation");

        group.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTimeOffset.UtcNow,
        }));

        group.MapGet(
            "/integrity",
            async (IStoreIntegrityService integrityService, HttpContext httpContext) =>
            {
                var context = RequestContext.CreateAnonymous(httpContext.Connection.RemoteIpAddress?.ToString());
                var result = await integrityService.VerifyAllStoresAsync(context);
                return Results.Ok(result);
            });

        return app;
    }
}

