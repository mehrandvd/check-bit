﻿using Learn.Bit.Shared.Controllers.Statistics;
using Learn.Bit.Shared.Dtos.Statistics;

namespace Learn.Bit.Client.Core.Components.Pages;
public partial class HomePage
{
    protected override string? Title => Localizer[nameof(AppStrings.Home)];
    protected override string? Subtitle => string.Empty;

    [CascadingParameter] private BitDir? currentDir { get; set; }

    [AutoInject] private IStatisticsController statisticsController = default!;

    private bool isLoadingGitHub = true;
    private bool isLoadingNuget = true;
    private GitHubStats? gitHubStats;
    private NugetStatsDto? nugetStats;

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        // If required, you should typically manage the authorization header for external APIs in **AuthDelegatingHandler.cs**
        // and handle error extraction from failed responses in **ExceptionDelegatingHandler.cs**.  

        // These external API calls are provided as sample references for anonymous API usage in pre-rendering anonymous pages,
        // and comprehensive exception handling is not intended for these examples.  

        // However, the logic in other HTTP message handlers, such as **LoggingDelegatingHandler** and **RetryDelegatingHandler**,
        // effectively addresses most scenarios.

        await Task.WhenAll(LoadGitHub(), LoadNuget());
    }

    private async Task LoadGitHub()
    {
        try
        {
            gitHubStats = await statisticsController.GetGitHubStats(CurrentCancellationToken);
        }
        finally
        {
            isLoadingGitHub = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadNuget()
    {
        try
        {
            nugetStats = await statisticsController.GetNugetStats(packageId: "Bit.BlazorUI", CurrentCancellationToken);
        }
        finally
        {
            isLoadingNuget = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
