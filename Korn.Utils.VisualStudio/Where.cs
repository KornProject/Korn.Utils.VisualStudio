namespace Korn.Utils.VisualStudio;
public static class VSWhere
{
    const string VSWHERE_PATH = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";

    public static bool IsServiceInstalled => File.Exists(VSWHERE_PATH);

    const int TIMEOUT_SECONDS = 30;
    public static string ResolveVisualStudioPath()
    {
        if (!IsServiceInstalled)
            throw new KornError(
                "VSWhere->ResolveVisualStudioPath:",
                "The vswhere service is not installed. Apparently, you do not have Visual Studio installed."
            );

        var commandLine = new CommandLine(
            host: VSWHERE_PATH,
            arguments: "-latest -nocolor",
            outputHandler: OnOutputLineReceived,
            exitHandler: OnExit
        );

        string? result = null;
        bool exited = false;

        var startTime = DateTime.Now;
        while (!exited)
        {
            Thread.Sleep(1);
            if ((DateTime.Now - startTime).TotalSeconds > TIMEOUT_SECONDS)
                throw new KornError(
                    "VSWhere->ResolveVisualStudioPath:",
                    "The vswhere service not responding, timeout."
                );
        }

        if (result is null)
            throw new KornError(
                "VSWhere->ResolveVisualStudioPath:",
                "The vswhere service finished with no result.",
                "This indicates that the vswhere service is failing or that you don't have Visual Studio installed."
            );

        return result;

        void OnOutputLineReceived(string line)
        {
            if (line.StartsWith("resolvedInstallationPath: "))
            {
                result = line.Split(": ")[1];
                exited = true; // fast exit
            }
        }

        void OnExit() => exited = true;
    }
}