using System.Linq;
using System.Reflection;
using Accountant.Gui.Config;
using Accountant.Manager;
using AddonWatcher;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using TimerWindow = Accountant.Gui.Timer.TimerWindow;

namespace Accountant;

public class Accountant : IDalamudPlugin
{
    public string Name
        => "Accountant";

    public static string Version = "";

    public static   AccountantConfiguration Config   = null!;
    public static   IGameData               GameData = null!;
    public static   IAddonWatcher           Watcher  = null!;
    public readonly TimerManager            Timers;
    public readonly TimerWindow             TimerWindow;
    public readonly ConfigWindow            ConfigWindow;

    public Accountant(DalamudPluginInterface pluginInterface)
    {
        Dalamud.Initialize(pluginInterface);
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
        Config  = AccountantConfiguration.Load();

        Watcher  = AddonWatcherFactory.Create(Dalamud.GameGui, Dalamud.SigScanner);
        GameData = GameDataFactory.Create(Dalamud.GameGui, Dalamud.ClientState, Dalamud.Framework, Dalamud.GameData);

        Timers       = new TimerManager();
        TimerWindow  = new TimerWindow(Timers);
        ConfigWindow = new ConfigWindow(Timers, TimerWindow);

        Dalamud.Commands.AddHandler("/accountant", new CommandInfo(OnAccountant)
        {
            HelpMessage = "Open Accountant config. Use '/acct' or '/accountant timers' to toggle the timer window.",
            ShowInHelp  = true,
        });

        Dalamud.Commands.AddHandler("/acct", new CommandInfo(OnAcct)
        {
            HelpMessage = "Toggle the timer window.",
            ShowInHelp  = true,
        });
    }

    private void OnAccountant(string command, string arguments)
    {
        if (arguments.ToLowerInvariant() != "timers")
        {
            ConfigWindow.Toggle();
            return;
        }

        Config.WindowVisible = !Config.WindowVisible;
        Config.Save();
    }

    private static void OnAcct(string command, string _)
    {
        Config.WindowVisible = !Config.WindowVisible;
        Config.Save();
    }

    public void Dispose()
    {
        Timers.Dispose();
        ConfigWindow.Dispose();
        TimerWindow.Dispose();
        Dalamud.Commands.RemoveHandler("/accountant");
        Dalamud.Commands.RemoveHandler("/acct");
        Timers.Dispose();
        GameData.Dispose();
        Watcher.Dispose();
    }
}
