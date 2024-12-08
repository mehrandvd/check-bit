﻿using System.Reflection;

namespace Learn.Bit.Client.Core.Components.Layout;
public partial class RootLayout : IDisposable
{
    private BitDir? currentDir;
    private string? currentUrl;
    private readonly BitModalParameters modalParameters = new() { Classes = new() { Root = "modal" } };

    /// <summary>
    /// <inheritdoc cref="Parameters.IsOnline"/>
    /// </summary>
    private bool? isOnline = null;
    private bool? isAuthenticated;

    /// <summary>
    /// <inheritdoc cref="Parameters.IsCrossLayoutPage"/>
    /// </summary>
    private bool? isCrossLayoutPage;
    private AppThemeType? currentTheme;
    private RouteData? currentRouteData;
    private List<Action> unsubscribers = [];


    [AutoInject] private Keyboard keyboard = default!;
    [AutoInject] private AuthManager authManager = default!;
    [AutoInject] private ThemeService themeService = default!;
    [AutoInject] private PubSubService pubSubService = default!;
    [AutoInject] private IExceptionHandler exceptionHandler = default!;
    [AutoInject] private ITelemetryContext telemetryContext = default!;
    [AutoInject] private NavigationManager navigationManager = default!;
    [AutoInject] private IPrerenderStateService prerenderStateService = default!;


    [CascadingParameter] public Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;


    protected override async Task OnInitializedAsync()
    {
        try
        {
            navigationManager.LocationChanged += NavigationManagerLocationChanged;
            authManager.AuthenticationStateChanged += AuthenticationStateChanged;
            unsubscribers.Add(pubSubService.Subscribe(ClientPubSubMessages.CULTURE_CHANGED, async _ =>
            {
                SetCurrentDir();
                StateHasChanged();
            }));
            unsubscribers.Add(pubSubService.Subscribe(ClientPubSubMessages.THEME_CHANGED, async payload =>
            {
                if (payload is null) return;
                currentTheme = (AppThemeType)payload;
                StateHasChanged();
            }));
            unsubscribers.Add(pubSubService.Subscribe(ClientPubSubMessages.ROUTE_DATA_UPDATED, async payload =>
            {
                currentRouteData = (RouteData?)payload;
                SetIsCrossLayout();
                StateHasChanged();
            }));

            unsubscribers.Add(pubSubService.Subscribe(ClientPubSubMessages.IS_ONLINE_CHANGED, async payload =>
            {
                telemetryContext.IsOnline = isOnline = (bool?)payload;
                await InvokeAsync(StateHasChanged);
            }));

            isAuthenticated = await prerenderStateService.GetValue(async () => (await AuthenticationStateTask).User.IsAuthenticated());

            SetCurrentDir();
            SetCurrentUrl();
            currentTheme = await themeService.GetCurrentTheme();

            await base.OnInitializedAsync();
        }
        catch (Exception exp)
        {
            exceptionHandler.Handle(exp);
        }
    }

    protected override void OnParametersSet()
    {
        // TODO: we can try to recover from exception after rendering the ErrorBoundary with this line.
        // but for now it's better to persist the error ui until a force refresh.
        // ErrorBoundaryRef.Recover();

        base.OnParametersSet();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await keyboard.Add(ButilKeyCodes.KeyX, OpenDiagnosticModal, ButilModifiers.Ctrl | ButilModifiers.Shift);
        }

        await base.OnAfterRenderAsync(firstRender);
    }


    private async void AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        try
        {
            isAuthenticated = (await task).User.IsAuthenticated();
        }
        catch (Exception ex)
        {
            exceptionHandler.Handle(ex);
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private void NavigationManagerLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        SetCurrentUrl();
        StateHasChanged();
    }


    private void SetCurrentDir()
    {
        currentDir = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? BitDir.Rtl : null;
    }

    private void SetCurrentUrl()
    {
        var path = navigationManager.GetUriPath();

        currentUrl = Urls.All.SingleOrDefault(pageUrl =>
        {
            return pageUrl == Urls.HomePage
                    ? pageUrl == path
                    : path.StartsWith(pageUrl);
        });
    }

    /// <summary>
    /// <inheritdoc cref="Parameters.IsCrossLayoutPage"/>
    /// </summary>
    private void SetIsCrossLayout()
    {
        if (currentRouteData is null)
        {
            isCrossLayoutPage = true;
            return;
        }

        var type = currentRouteData.PageType;

        if (type.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any())
        {
            isCrossLayoutPage = false;
            return;
        }

        if (type.Namespace?.Contains("Client.Core.Components.Pages.Identity") ?? false)
        {
            isCrossLayoutPage = false;
            return;
        }

        isCrossLayoutPage = true;
    }

    private void OpenDiagnosticModal()
    {
        pubSubService.Publish(ClientPubSubMessages.SHOW_DIAGNOSTIC_MODAL);
    }


    private string GetMainCssClass()
    {
        var authClass = isAuthenticated is false ? "unauthenticated"
                      : isAuthenticated is true ? "authenticated"
                      : string.Empty;

        var crossClass = isCrossLayoutPage is true ? " cross-layout" : string.Empty;

        return authClass + crossClass;
    }


    public void Dispose()
    {
        navigationManager.LocationChanged -= NavigationManagerLocationChanged;

        authManager.AuthenticationStateChanged -= AuthenticationStateChanged;

        unsubscribers.ForEach(d => d.Invoke());

        _ = keyboard?.DisposeAsync();
    }
}
