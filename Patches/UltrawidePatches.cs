namespace RF5Fix.Patches;

[HarmonyPatch]
public class UltrawidePatches
{
    public static float DefaultAspectRatio = 16f / 9f;
    public static float NewAspectRatio = (float)Screen.width / Screen.height;
    public static float AspectMultiplier = NewAspectRatio / DefaultAspectRatio;
    public static float AspectDivider = DefaultAspectRatio / NewAspectRatio;

    private static ManualLogSource Log => Rf5Fix.Log;
    
    // Set screen match mode when object has canvasscaler enabled
    [HarmonyPatch(typeof(CanvasScaler), nameof(CanvasScaler.OnEnable))]
    [HarmonyPostfix]
    public static void SetScreenMatchMode(CanvasScaler __instance)
    {
        if (NewAspectRatio > DefaultAspectRatio || NewAspectRatio < DefaultAspectRatio)
        {
            __instance.m_ScreenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
    }

    // ViewportRect
    [HarmonyPatch(typeof(ViewportRectController), nameof(ViewportRectController.OnEnable))]
    [HarmonyPatch(typeof(ViewportRectController), nameof(ViewportRectController.ResetRect))]
    [HarmonyPostfix]
    public static void ViewportRectDisable(ViewportRectController __instance)
    {
        __instance.m_Camera.rect = new(0f, 0f, 1f, 1f);
        Log.LogInfo($"Camera viewport rect patched.");
    }

    // Letterbox
    [HarmonyPatch(typeof(LetterBoxController), nameof(LetterBoxController.OnEnable))]
    [HarmonyPostfix]
    public static void LetterboxDisable(LetterBoxController __instance)
    {
        if (Rf5Fix.bLetterboxing.Value)
            return; // Do nothing if UI letterboxing is enabled

        // If letterboxing is disabled
        __instance.transform.parent.gameObject.SetActive(false);
        Log.LogInfo("Letterboxing disabled. For good.");
    }

    // Span UI fade to black
    [HarmonyPatch(typeof(UIFadeScreen), nameof(UIFadeScreen.ScreenFade))]
    [HarmonyPostfix]
    public static void UIFadeScreenFix(UIFadeScreen __instance)
    {
        if (NewAspectRatio < DefaultAspectRatio)
        {
            // Increase height to scale correctly
            __instance.BlackOutPanel.transform.localScale = new(1f, 1 * AspectDivider, 1f);
        }
        else if (NewAspectRatio > DefaultAspectRatio)
        {
            // Increase width to scale correctly
            __instance.BlackOutPanel.transform.localScale = new(1 * AspectMultiplier, 1f, 1f);
        }
    }

    // Span UI load fade
    // Can't find a better way to hook this. It shouldn't impact performance much and even if it does it's only during UI loading fades.
    [HarmonyPatch(typeof(UILoaderFade), nameof(UILoaderFade.Update))]
    [HarmonyPostfix]
    public static void UILoaderFadeFix(UILoaderFade __instance)
    {
        if (NewAspectRatio < DefaultAspectRatio)
        {
            // Increase height to scale correctly
            __instance.gameObject.transform.localScale = new(1f, 1 * AspectDivider, 1f);
        }
        else if (NewAspectRatio > DefaultAspectRatio)
        {
            // Increase width to scale correctly
            __instance.gameObject.transform.localScale = new(1 * AspectMultiplier, 1f, 1f);
        }
    }
}