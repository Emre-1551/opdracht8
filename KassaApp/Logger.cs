namespace KassaApp;

public static class Logger
{
    private static string GetSolutionRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

    private static readonly string LogFile = Path.Combine(
        GetSolutionRoot(),
        $"systeem-{DateTime.Now:yyyy-MM-dd}.log"
    );

    static Logger()
    {
        // Ensure log directory exists
        var logDir = Path.GetDirectoryName(LogFile);
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir!);
    }

    public static void Log(string category, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[{timestamp}] [{category}] {message}";

        try
        {
            File.AppendAllText(LogFile, logEntry + Environment.NewLine);
        }
        catch
        {
            // Silently fail if logging fails
        }
    }

    public static void LogSystem(string message) => Log("SYSTEM", message);
    public static void LogKassa(string message) => Log("KASSA", message);
}
