using System.Reflection;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace GeneratorOffOnExit.Client.Patches;

public class FinishNarratePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(MainMenuShowOperation), nameof(MainMenuShowOperation.FinishNarrate));

    [PatchPrefix]
    public static bool PatchPrefix(MainMenuShowOperation __instance, ref Task __result)
    {
        if (GeneratorExit.TurningOff)
        {
            __result = Task.CompletedTask;
            return false;
        }

        if (!GeneratorExit.TurnOffRequested)
        {
            return true;
        }

        GeneratorExit.TurnOffRequested = false;
        __result = GeneratorExit.TurnOffThenExit(__instance);
        return false;
    }
}
