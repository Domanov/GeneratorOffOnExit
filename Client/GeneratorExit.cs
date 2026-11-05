using System.Linq;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.UI;
using SPT.Reflection.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GeneratorOffOnExit.Client;

internal static class GeneratorExit
{
    private const string ButtonText = "Turn off & exit (Enter)";
    private const int ToggleTimeoutMs = 5000;

    private static DefaultUIButton _button;
    private static MessageWindow _window;
    private static float _originalWidth = -1f;

    internal static bool TurnOffRequested;
    internal static bool TurningOff;

    public static bool IsExitDialog(string caption) =>
        caption == "UI/leave_game_confirmation_caption".Localized((string)null);

    public static bool PrepareDescription(ref string description)
    {
        string warning = "UI/Generator is working".Localized((string)null);
        if (description != null && description.Contains(warning))
        {
            return true;
        }

        if (Singleton<HideoutRepresentation>.Instantiated || !IsGeneratorOnInProfile())
        {
            return false;
        }

        description += "\n<color=\"red\">" + warning + "</color>";
        return true;
    }

    public static void UpdateButton(MessageWindow window, bool visible)
    {
        TurnOffRequested = false;

        if (_button == null)
        {
            if (!visible || window == null || window._acceptButton == null)
            {
                return;
            }

            _button = CreateButton(window);
        }

        _button.gameObject.SetActive(visible);
        if (visible)
        {
            _button.SetHeaderText(ButtonText, window._acceptButton.HeaderSize);
        }

        ResizeWindow(window, visible);
    }

    public static bool TryConfirm(BaseMessageWindow window)
    {
        if (!IsButtonActiveOn(window))
        {
            return false;
        }

        Confirm();
        return true;
    }

    public static void OnAccept(object window)
    {
        if (IsButtonActiveOn(window) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            TurnOffRequested = true;
        }
    }

    public static async Task TurnOffThenExit(MainMenuShowOperation operation)
    {
        TurningOff = true;
        try
        {
            await TurnOff();
        }
        finally
        {
            TurningOff = false;
        }

        await operation.FinishNarrate();
    }

    private static bool IsButtonActiveOn(object window) =>
        _button != null && _window != null && ReferenceEquals(window, _window) && _button.gameObject.activeInHierarchy;

    private static DefaultUIButton CreateButton(MessageWindow window)
    {
        DefaultUIButton accept = window._acceptButton;
        GameObject clone = Object.Instantiate(accept.gameObject, accept.transform.parent, false);
        clone.name = "Dmnv-GeneratorOffOnExitButton";
        clone.transform.SetSiblingIndex(accept.transform.GetSiblingIndex());

        DefaultUIButton button = clone.GetComponent<DefaultUIButton>();
        button.OnClick.RemoveAllListeners();
        for (int i = 0; i < button.OnClick.GetPersistentEventCount(); i++)
        {
            button.OnClick.SetPersistentListenerState(i, UnityEventCallState.Off);
        }

        button.OnClick.AddListener(Confirm);
        _window = window;

        return button;
    }

    private static void Confirm()
    {
        TurnOffRequested = true;
        _window.Accept();
    }

    private static void ResizeWindow(MessageWindow window, bool visible)
    {
        RectTransform windowRect = window?.WindowTransform;
        if (windowRect == null || _button == null)
        {
            return;
        }

        if (_originalWidth < 0f)
        {
            _originalWidth = windowRect.sizeDelta.x;
        }

        float width = _originalWidth;
        if (visible)
        {
            var buttonRect = (RectTransform)_button.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRect);
            float extra = Mathf.Max(LayoutUtility.GetPreferredWidth(buttonRect), buttonRect.rect.width);
            var group = buttonRect.parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (group != null)
            {
                extra += group.spacing;
            }

            width += extra;
        }

        windowRect.sizeDelta = new Vector2(width, windowRect.sizeDelta.y);
        LayoutRebuilder.ForceRebuildLayoutImmediate(windowRect);
        windowRect.SetInCenter();
    }

    private static bool IsGeneratorOnInProfile()
    {
        AreaInfo[] areas = ClientAppUtils.GetMainApp()?.GetClientBackEndSession()?.Profile?.Hideout?.Areas;
        return areas != null && areas.Any(a => (EAreaType)a.Type == EAreaType.Generator && a.Active);
    }

    private static async Task TurnOff()
    {
        IEftSession session = ClientAppUtils.GetMainApp()?.GetClientBackEndSession();
        if (session == null)
        {
            Plugin.Log.LogWarning("No backend session, exiting without turning the generator off");
            return;
        }

        var done = new TaskCompletionSource<bool>();
        Callback previous = EftClientBackendSession.CG_Class1577.callback_3;
        EftClientBackendSession.CG_Class1577.callback_3 = result => done.TrySetResult(result.Succeed);
        try
        {
            session.ToggleArea(EAreaType.Generator, false);
        }
        finally
        {
            EftClientBackendSession.CG_Class1577.callback_3 = previous;
        }

        Task finished = await Task.WhenAny(done.Task, Task.Delay(ToggleTimeoutMs));
        if (finished != done.Task)
        {
            Plugin.Log.LogWarning("Generator toggle timed out, exiting anyway");
            return;
        }

        if (!done.Task.Result)
        {
            Plugin.Log.LogWarning("Server rejected generator toggle, exiting anyway");
            return;
        }

        AreaInfo area = session.Profile?.Hideout?.Areas?.FirstOrDefault(a => (EAreaType)a.Type == EAreaType.Generator);
        if (area != null)
        {
            area.Active = false;
        }

        Plugin.Log.LogInfo("Generator turned off");
    }
}
