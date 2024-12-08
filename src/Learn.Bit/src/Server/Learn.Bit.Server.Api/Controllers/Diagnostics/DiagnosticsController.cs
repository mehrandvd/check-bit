using System.Text;
using Learn.Bit.Server.Api.Models.Identity;
using Learn.Bit.Server.Api.Services;
using Learn.Bit.Shared.Controllers.Diagnostics;

namespace Learn.Bit.Server.Api.Controllers.Diagnostics;
[ApiController, AllowAnonymous]
[Route("api/[controller]/[action]")]
public partial class DiagnosticsController : AppControllerBase, IDiagnosticsController
{

    [HttpPost]
    public async Task<string> PerformDiagnostics(CancellationToken cancellationToken)
    {
        StringBuilder result = new();

        foreach (var header in Request.Headers.Where(h => h.Key.StartsWith("X-", StringComparison.InvariantCulture)))
        {
            result.AppendLine($"{header.Key}: {header.Value}");
        }

        result.AppendLine($"Client IP: {HttpContext.Connection.RemoteIpAddress}");

        result.AppendLine($"Trace => {Request.HttpContext.TraceIdentifier}");

        var isAuthenticated = User.IsAuthenticated();
        Guid? userSessionId = null;
        UserSession? userSession = null;

        if (isAuthenticated)
        {
            userSessionId = User.GetSessionId();
            userSession = await DbContext
                .UserSessions.SingleAsync(us => us.Id == userSessionId, cancellationToken);
        }

        result.AppendLine($"IsAuthenticated: {isAuthenticated.ToString().ToLowerInvariant()}");



        result.AppendLine($"Culture => C: {CultureInfo.CurrentCulture.Name}, UC: {CultureInfo.CurrentUICulture.Name}");

        return result.ToString();
    }
}
