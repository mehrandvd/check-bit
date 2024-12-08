﻿using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Learn.Bit.Client.Core.Services;
public abstract partial class ClientExceptionHandlerBase : SharedExceptionHandler, IExceptionHandler
{
    [AutoInject] protected Bit.Butil.Console Console = default!;
    [AutoInject] protected ITelemetryContext TelemetryContext = default!;
    [AutoInject] protected readonly SnackBarService SnackBarService = default!;
    [AutoInject] protected ILogger<ClientExceptionHandlerBase> Logger = default!;
    [AutoInject] protected readonly MessageBoxService MessageBoxService = default!;

    public void Handle(Exception exception,
        bool nonInterrupting = false,
        Dictionary<string, object?>? parameters = null,
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        parameters = TelemetryContext.ToDictionary(parameters);

        parameters[nameof(filePath)] = filePath;
        parameters[nameof(memberName)] = memberName;
        parameters[nameof(lineNumber)] = lineNumber;
        parameters["exceptionId"] = Guid.NewGuid(); // This will remain consistent across different registered loggers, such as Sentry, Application Insights, etc.

        Handle(exception, nonInterrupting, parameters.ToDictionary(i => i.Key, i => i.Value ?? string.Empty));
    }

    protected virtual void Handle(Exception exception,
        bool nonInterrupting,
        Dictionary<string, object> parameters)
    {
        var isDevEnv = AppEnvironment.IsDev();

        using (var scope = Logger.BeginScope(parameters.ToDictionary(i => i.Key, i => i.Value ?? string.Empty)))
        {
            var exceptionMessageToLog = GetExceptionMessageToLog(exception);

            if (exception is KnownException)
            {
                Logger.LogError(exception, exceptionMessageToLog);
            }
            else
            {
                Logger.LogCritical(exception, exceptionMessageToLog);
            }
        }

        string exceptionMessageToShow = GetExceptionMessageToShow(exception);

        if (nonInterrupting)
        {
            SnackBarService.Error("Learn.Bit", exceptionMessageToShow);
        }
        else
        {
            MessageBoxService.Show(exceptionMessageToShow, Localizer[nameof(AppStrings.Error)]);
        }

        if (isDevEnv)
        {
            Debugger.Break();
        }
    }
}
