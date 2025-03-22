using System.IO;
using Almanac.UI;
using BepInEx;

namespace Almanac.API;

public static class TrackMinimalUI
{
    public static void SetupWatcher()
    {
        FileSystemWatcher watcher = new(Paths.ConfigPath, "Azumatt.MinimalUI.cfg");
        watcher.Changed += OnFileChange;
        watcher.IncludeSubdirectories = false;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private static void OnFileChange(object sender, FileSystemEventArgs e)
    {
        if (!AlmanacUI.m_instance || !SidePanel.m_instance) return;
        AlmanacUI.m_instance.ReloadAssets();
    }

}