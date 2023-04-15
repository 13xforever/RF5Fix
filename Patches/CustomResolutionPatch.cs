namespace RF5Fix.Patches;

[HarmonyPatch]
public class CustomResolutionPatch
{
    private static ManualLogSource Log => Rf5Fix.Log;
    
    [HarmonyPatch(typeof(ScreenUtil), nameof(ScreenUtil.SetResolution), new Type[] { typeof(int), typeof(int), typeof(BootOption.WindowMode) })]
    [HarmonyPrefix]
    public static bool SetCustomRes(ref int __0, ref int __1, ref BootOption.WindowMode __2)
    {
        var fullscreenMode = Rf5Fix.iWindowMode.Value switch
        {
            1 => BootOption.WindowMode.FullScreen,
            2 => BootOption.WindowMode.Borderless,
            3 => BootOption.WindowMode.Window,
            _ => BootOption.WindowMode.Borderless,
        };

        Log.LogInfo($"Original resolution is {__0}x{__1}. Fullscreen = {__2}.");

        __0 = (int)Rf5Fix.fDesiredResolutionX.Value;
        __1 = (int)Rf5Fix.fDesiredResolutionY.Value;
        __2 = fullscreenMode;

        Log.LogInfo($"Custom resolution set to {__0}x{__1}. Fullscreen = {__2}.");
        return true;
    }
}