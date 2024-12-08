﻿using Learn.Bit.Client.Core.Components;
using Learn.Bit.Client.Windows.Services;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Web.WebView2.Core;
using Velopack;

namespace Learn.Bit.Client.Windows;
public partial class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.ThreadException += (_, e) => LogException(e.Exception, reportedBy: nameof(Application.ThreadException));
        AppDomain.CurrentDomain.UnhandledException += (_, e) => LogException(e.ExceptionObject, reportedBy: nameof(AppDomain.UnhandledException));
        TaskScheduler.UnobservedTaskException += (_, e) => { LogException(e.Exception, reportedBy: nameof(TaskScheduler.UnobservedTaskException)); e.SetObserved(); };

        ApplicationConfiguration.Initialize();

        AppPlatform.IsBlazorHybrid = true;
        ITelemetryContext.Current = new WindowsTelemetryContext();

        Application.SetColorMode(SystemColorMode.System);

        var configuration = new ConfigurationBuilder().AddClientConfigurations(clientEntryAssemblyName: "Learn.Bit.Client.Windows").Build();
        var services = new ServiceCollection();
        services.AddClientWindowsProjectServices(configuration);
        Services = services.BuildServiceProvider();

        if (CultureInfoManager.MultilingualEnabled)
        {
            Services.GetRequiredService<CultureInfoManager>().SetCurrentCulture(
                Application.UserAppDataRegistry.GetValue("Culture") as string ?? // 1- User settings
                CultureInfo.CurrentUICulture.Name); // 2- OS Settings
        }
        Services.GetRequiredService<PubSubService>().Subscribe(ClientPubSubMessages.CULTURE_CHANGED, async culture =>
        {
            Application.Restart();
        });

        // https://github.com/velopack/velopack
        VelopackApp.Build().Run();
        _ = Task.Run(async () =>
        {
            try
            {
                var windowsUpdateSettings = Services.GetRequiredService<ClientWindowsSettings>().WindowsUpdate;
                if (string.IsNullOrEmpty(windowsUpdateSettings?.FilesUrl))
                {
                    return;
                }
                var updateManager = new UpdateManager(windowsUpdateSettings.FilesUrl);
                var updateInfo = await updateManager.CheckForUpdatesAsync();
                if (updateInfo is not null)
                {
                    await updateManager.DownloadUpdatesAsync(updateInfo);
                    if (windowsUpdateSettings.AutoReload)
                    {
                        updateManager.ApplyUpdatesAndRestart(updateInfo, args);
                    }
                }
            }
            catch (Exception exp)
            {
                Services.GetRequiredService<IExceptionHandler>().Handle(exp);
            }
        });

        var form = new Form()
        {
            Text = "Learn.Bit",
            Height = 768,
            Width = 1024,
            MinimumSize = new Size(375, 667),
            WindowState = FormWindowState.Maximized,
            BackColor = ColorTranslator.FromHtml("#0D2960"),
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        };

        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--unsafely-treat-insecure-origin-as-secure=https://0.0.0.1 --enable-notifications");

        var blazorWebView = new BlazorWebView
        {
            Dock = DockStyle.Fill,
            Services = Services,
            HostPage = @"wwwroot\index.html",
            BackColor = ColorTranslator.FromHtml("#0D2960")
        };

        blazorWebView.WebView.DefaultBackgroundColor = ColorTranslator.FromHtml("#0D2960");


        blazorWebView.RootComponents.Add(new RootComponent("#app-container", typeof(Routes), null));

        blazorWebView.BlazorWebViewInitialized += delegate
        {
            blazorWebView.WebView.CoreWebView2.PermissionRequested += async (sender, args) =>
            {
                args.Handled = true;
                args.State = CoreWebView2PermissionState.Allow;
            };
            var settings = blazorWebView.WebView.CoreWebView2.Settings;
            if (AppEnvironment.IsDev() is false)
            {
                settings.IsZoomControlEnabled = false;
                settings.AreBrowserAcceleratorKeysEnabled = false;
            }
            blazorWebView.WebView.NavigationCompleted += async delegate
            {
                await blazorWebView.WebView.ExecuteScriptAsync("Blazor.start()");
            };
        };

        form.Controls.Add(blazorWebView);

        Application.Run(form);
    }

    private static void LogException(object? error, string reportedBy)
    {
        var errorMessage = error?.ToString() ?? "Unknown error";
        if (Services is not null && error is Exception exp)
        {
            Services.GetRequiredService<IExceptionHandler>().Handle(exp, parameters: new()
            {
                { nameof(reportedBy), reportedBy }
            });
        }
        else
        {
            Clipboard.SetText(errorMessage);
            System.Windows.Forms.MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static IServiceProvider? Services { get; private set; }
}