using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace GeneratorOffOnExit.Client.Patches;

public class DialogAcceptPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(DialogWindow<DialogWindowContext>), nameof(DialogWindow<DialogWindowContext>.Accept));

    [PatchPrefix]
    public static void PatchPrefix(object __instance)
    {
        GeneratorExit.OnAccept(__instance);
    }
}
