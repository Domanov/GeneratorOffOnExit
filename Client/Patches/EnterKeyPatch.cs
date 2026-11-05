using System.Reflection;
using EFT.InputSystem;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace GeneratorOffOnExit.Client.Patches;

public class EnterKeyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(BaseMessageWindow), nameof(BaseMessageWindow.TranslateCommand));

    [PatchPrefix]
    public static bool PatchPrefix(BaseMessageWindow __instance, ECommand command, ref InputNode.ETranslateResult __result)
    {
        if (command != ECommand.Enter || !GeneratorExit.TryConfirm(__instance))
        {
            return true;
        }

        __result = InputNode.ETranslateResult.Block;
        return false;
    }
}
