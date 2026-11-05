using System;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

namespace GeneratorOffOnExit.Client.Patches;

public class ExitDialogPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.ShowMessageWindow),
            new[] { typeof(string), typeof(Action), typeof(Action), typeof(string), typeof(float), typeof(bool), typeof(TextAlignmentOptions) });

    [PatchPrefix]
    public static void PatchPrefix(ref string description, string caption, out bool __state)
    {
        __state = GeneratorExit.IsExitDialog(caption) && GeneratorExit.PrepareDescription(ref description);
    }

    [PatchPostfix]
    public static void PatchPostfix(ItemUiContext __instance, bool __state)
    {
        GeneratorExit.UpdateButton(__instance._messageWindow, __state);
    }
}
