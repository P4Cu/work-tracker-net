using System.Diagnostics;

public class Storage : IDisposable
{
    public string FilePath { get; private set; }
    private string BackupDirectory => Path.GetDirectoryName(FilePath)!;

    private GLib.Cancellable WatcherCancellable = new();
    private GLib.FileMonitor? FileMonitor;

    public event Action? FileChanged;

    public Storage()
    {
        FilePath = Path.Combine(GetAppDataFolder("working_time_net"), "data.txt");

        if (!File.Exists(FilePath))
        {
            Trace.TraceInformation($"Creating {FilePath}");
            File.CreateText(FilePath);
        }

        StartFileWatcher();
    }

    public string ReadFile() => File.ReadAllText(FilePath);

    // Write to the file and ensure daily backup
    public void WriteToFile(string content)
    {
        CreateDailyBackup();
        File.WriteAllText(FilePath, content);
    }

    private void CreateDailyBackup()
    {
        string backupFileName = FilePath + $".bak_{DateTime.Now.AddDays(-1):MM_dd}";

        if (!File.Exists(backupFileName))
        {
            File.Copy(FilePath, backupFileName, true);
        }

        ManageBackups();
    }

    // Keep only the last 5 backups
    private void ManageBackups()
    {
        string backupPattern = Path.GetFileName(FilePath) + ".bak_*";
        var backupFiles = Directory
            .GetFiles(BackupDirectory!, backupPattern)
            .OrderByDescending(File.GetCreationTime)
            .ToList();

        Trace.TraceInformation($"Backup files: {backupFiles}");

        for (int i = 5; i < backupFiles.Count; i++)
        {
            Trace.TraceWarning($"Deleting: {backupFiles}[i]");
            File.Delete(backupFiles[i]);
        }
    }

    private void StartFileWatcher()
    {
        Trace.TraceInformation($"Watching {FilePath} {Path.GetFileName(FilePath)}");

        var file = GLib.FileFactory.NewForPath(FilePath);

        FileMonitor = file.Monitor(GLib.FileMonitorFlags.None, WatcherCancellable);
        //
        // Connect to the changed signal.
        FileMonitor.Changed += (obj, e) =>
        {
            if (e.Args.Contains(GLib.FileMonitorEvent.ChangesDoneHint))
            {
                Console.WriteLine($"File changed");
                FileChanged?.Invoke();
            }
        };
    }

    public void Dispose()
    {
        WatcherCancellable.Cancel();
        FileMonitor?.Dispose();
        WatcherCancellable?.Dispose();
        GC.SuppressFinalize(this);
    }

    private string GetAppDataFolder(string appName)
    {
        string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appDataFolder = Path.Combine(baseFolder, appName);

        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }

        return appDataFolder;
    }
}
