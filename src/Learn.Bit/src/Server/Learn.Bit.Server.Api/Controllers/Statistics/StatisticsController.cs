using Learn.Bit.Server.Api.Services;
using Learn.Bit.Shared.Controllers.Statistics;
using Learn.Bit.Shared.Dtos.Statistics;

namespace Learn.Bit.Server.Api.Controllers.Statistics;
[ApiController, Route("api/[controller]/[action]")]
public partial class StatisticsController : AppControllerBase, IStatisticsController
{
    [AutoInject] private NugetStatisticsHttpClient nugetHttpClient = default!;

    [AllowAnonymous]
    [HttpGet("{packageId}")]
    public async Task<NugetStatsDto> GetNugetStats(string packageId, CancellationToken cancellationToken)
    {
        return await nugetHttpClient.GetPackageStats(packageId, cancellationToken);
    }
}
