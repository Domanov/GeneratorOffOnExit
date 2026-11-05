using BepInEx;
using BepInEx.Logging;
using GeneratorOffOnExit.Client.Patches;

namespace GeneratorOffOnExit.Client;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency("com.SPT.core", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.dmnv.generatoroffonexit";
    public const string Name = "Dmnv-GeneratorOffOnExit";
    public const string Version = "1.0.0";

    internal static ManualLogSource Log;

    private void Awake()
    {
        Log = Logger;
        new ExitDialogPatch().Enable();
        new FinishNarratePatch().Enable();
        new EnterKeyPatch().Enable();
        new DialogAcceptPatch().Enable();
        Log.LogInfo($"{Name} {Version} loaded");
    }
}
