using System;
using System.IO;
using System.Management.Automation;

namespace IISParser.PowerShell;

public abstract partial class AsyncPSCmdlet {
    /// <summary>Returns the effective error action preference.</summary>
    protected ActionPreference GetErrorActionPreference() {
        if (MyInvocation.BoundParameters.ContainsKey("ErrorAction")) {
            string? errorActionString = MyInvocation.BoundParameters["ErrorAction"]?.ToString();
            if (!string.IsNullOrWhiteSpace(errorActionString) &&
                Enum.TryParse(errorActionString, true, out ActionPreference parsed)) {
                return parsed;
            }
        }

        object? preference = GetVariableValue("ErrorActionPreference");
        if (preference is ActionPreference actionPreference) {
            return actionPreference;
        }

        if (preference is string preferenceString &&
            Enum.TryParse(preferenceString, true, out ActionPreference parsedPreference)) {
            return parsedPreference;
        }

        return ActionPreference.Continue;
    }

    /// <summary>Resolves a provider path and verifies that the file exists.</summary>
    protected bool EnsureFileExists(
        string path,
        ActionPreference errorAction,
        out string resolvedPath) {
        try {
            resolvedPath = GetUnresolvedProviderPathFromPSPath(path);
        } catch (ItemNotFoundException) {
            resolvedPath = path;
        }

        if (File.Exists(resolvedPath)) {
            return true;
        }

        string message = $"{MyInvocation.InvocationName} - The specified file does not exist: {resolvedPath}";
        if (errorAction == ActionPreference.Stop) {
            FileNotFoundException exception = new("The specified file does not exist.", resolvedPath);
            ThrowTerminatingError(
                new ErrorRecord(
                    exception,
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    resolvedPath));
        } else {
            LoggingMessages.Logger.WriteWarning(message);
        }

        return false;
    }
}
