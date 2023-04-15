namespace RF5Fix.Patches;

[HarmonyPatch]
public class FOVPatch
{
    public static float NewAspectRatio = (float)Screen.width / Screen.height;
    public static float DefaultAspectRatio = 16f / 9f;
            
    public static bool farmFOVHasRun = false;
    public static bool trackingFOVHasRun = false;

    private static ManualLogSource Log => Rf5Fix.Log;
    
    // Adjust tracking camera FOV
    // Indoor, outdoor, dungeon
    [HarmonyPatch(typeof(PlayerTrackingCamera), nameof(PlayerTrackingCamera.Start))]
    [HarmonyPostfix]
    public static void TrackingFOV(PlayerTrackingCamera __instance)
    {
        // Only run this once
        if (!trackingFOVHasRun)
        {
            var InDoor = __instance.GetSetting(Define.TrackinCameraType.InDoor);
            var OutDoor = __instance.GetSetting(Define.TrackinCameraType.OutDoor);
            var Dangeon = __instance.GetSetting(Define.TrackinCameraType.Dangeon);

            Log.LogInfo($"Tracking Camera. Current InDoor FOV = {InDoor.minFov}. Current OutDoor FOV = {OutDoor.minFov}. Current Dungeon FOV = {Dangeon.minFov}");

            // Vert+ FOV
            if (NewAspectRatio < DefaultAspectRatio)
            {
                var newInDoorFOV = Mathf.Floor(Mathf.Atan(Mathf.Tan(InDoor.minFov * Mathf.PI / 360) / NewAspectRatio * DefaultAspectRatio) * 360 / Mathf.PI);
                var newOutDoorFOV = Mathf.Floor(Mathf.Atan(Mathf.Tan(OutDoor.minFov * Mathf.PI / 360) / NewAspectRatio * DefaultAspectRatio) * 360 / Mathf.PI);
                var newDungeonFOV = Mathf.Floor(Mathf.Atan(Mathf.Tan(Dangeon.minFov * Mathf.PI / 360) / NewAspectRatio * DefaultAspectRatio) * 360 / Mathf.PI);

                InDoor.minFov = newInDoorFOV;
                OutDoor.minFov = newOutDoorFOV;
                Dangeon.minFov = newDungeonFOV;

            }
            // Add FOV
            if (Rf5Fix.fAdditionalFOV.Value > 0f)
            {
                InDoor.minFov += Rf5Fix.fAdditionalFOV.Value;
                OutDoor.minFov += Rf5Fix.fAdditionalFOV.Value;
                Dangeon.minFov += Rf5Fix.fAdditionalFOV.Value;
            }

            Log.LogInfo($"Tracking Camera: New InDoor FOV = {InDoor.minFov}. New OutDoor FOV = {OutDoor.minFov}. New Dungeon FOV = {Dangeon.minFov}");
            trackingFOVHasRun = true;
        }
    }


    // Farming FOV
    [HarmonyPatch(typeof(PlayerFarmingCamera), nameof(PlayerFarmingCamera.Awake))]
    [HarmonyPrefix]
    public static void FarmingFOV(PlayerFarmingCamera __instance)
    {
        // Only run this once
        if (!farmFOVHasRun)
        {
            var battleInst = BattleConst.Instance;
            Log.LogInfo($"PlayerFarmingCamera: Current FOV = {battleInst.FarmCamera_FOV}.");

            // Vert+ FOV
            if (NewAspectRatio < DefaultAspectRatio)
            {
                var newFOV = Mathf.Floor(Mathf.Atan(Mathf.Tan(battleInst.FarmCamera_FOV * Mathf.PI / 360) / NewAspectRatio * DefaultAspectRatio) * 360 / Mathf.PI);
                battleInst.FarmCamera_FOV = newFOV;

            }
            // Add FOV
            if (Rf5Fix.fAdditionalFOV.Value > 0f)
            {
                battleInst.FarmCamera_FOV += Rf5Fix.fAdditionalFOV.Value;
            }

            Log.LogInfo($"PlayerFarmingCamera: New FOV = {battleInst.FarmCamera_FOV}.");
            farmFOVHasRun = true;
        }
    }

    // FOV adjustment for every camera
    // Does not effect tracking camera and farm camera, maybe more. They are forced to a specific FOV
    // Adjust to Vert+ at narrower than 16:9
    [HarmonyPatch(typeof(Cinemachine.CinemachineVirtualCamera), nameof(Cinemachine.CinemachineVirtualCamera.OnEnable))]
    [HarmonyPostfix]
    public static void GlobalFOV(Cinemachine.CinemachineVirtualCamera __instance)
    {
        var currLens = __instance.m_Lens;
        var currFOV = currLens.FieldOfView;

        Log.LogInfo($"Cinemachine VCam: Current camera name = {__instance.name}.");
        Log.LogInfo($"Cinemachine VCam: Current camera FOV = {currFOV}.");

        // Vert+ FOV
        if (NewAspectRatio < DefaultAspectRatio)
        {
            var newFOV = Mathf.Floor(Mathf.Atan(Mathf.Tan(currFOV * Mathf.PI / 360) / NewAspectRatio * DefaultAspectRatio) * 360 / Mathf.PI);
            currLens.FieldOfView = Mathf.Clamp(newFOV, 1f, 180f);
        }
        // Add FOV for everything but cutscenes
        if (Rf5Fix.fAdditionalFOV.Value > 0f && __instance.name != "CMvcamCutBuffer" && __instance.name != "CMvcamShortPlay")
        {
            currLens.FieldOfView += Rf5Fix.fAdditionalFOV.Value;
            Log.LogInfo($"Cinemachine VCam: Cam name = {__instance.name}. Added gameplay FOV = {Rf5Fix.fAdditionalFOV.Value}.");
        }

        __instance.m_Lens = currLens;
        Log.LogInfo($"Cinemachine VCam: New camera FOV = {__instance.m_Lens.FieldOfView}.");
    }
}