using RF5SteamInput;

namespace RF5Fix.Patches;

[HarmonyPatch]
public class ControllerPatch
{
    // Spoof RF5's steam input controller type
    [HarmonyPatch(typeof(SteamInputManager), nameof(SteamInputManager.GetConnectingControllerType))]
    [HarmonyPostfix]
    public static void Glyphy(SteamInputManager __instance, ref SteamInputManager.ControllerType __result)
    {
        __result = Rf5Fix.sControllerType.Value switch
        {
            "Xbox" => SteamInputManager.ControllerType.Xbox, // Yes
            "PS4" => SteamInputManager.ControllerType.PS4, // Yes
            "PS5" => SteamInputManager.ControllerType.PS5, // Yes
            "Switch" => SteamInputManager.ControllerType.Switch, // Yes
            "Keyboard" => SteamInputManager.ControllerType.Keyboard, // Nope, keyboard glyphs are loaded differently.
            "Max" => SteamInputManager.ControllerType.Max, // Broken?
            "None" => SteamInputManager.ControllerType.None, // Broken?
            "Default" => SteamInputManager.ControllerType.Default, // Xbox (One) Glyphs
            _ => SteamInputManager.ControllerType.Default,
        };
    }
}