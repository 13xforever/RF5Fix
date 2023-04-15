using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace RF5Fix.Patches;

[HarmonyPatch]
public class MiscellaneousPatch
{
    public static RenderTexture rt;
    private static ManualLogSource Log => Rf5Fix.Log;

    // Load game settings
    [HarmonyPatch(typeof(BootSystem), nameof(BootSystem.ApplyOption))]
    [HarmonyPostfix]
    public static void GameSettingsOverride(BootSystem __instance)
    {
        // Anisotropic Filtering
        if (Rf5Fix.iAnisotropicFiltering.Value > 0)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            Texture.SetGlobalAnisotropicFilteringLimits(Rf5Fix.iAnisotropicFiltering.Value, Rf5Fix.iAnisotropicFiltering.Value);
            Log.LogInfo($"Anisotropic filtering force enabled. Value = {Rf5Fix.iAnisotropicFiltering.Value}");
        }

        // Shadow Cascades
        if (Rf5Fix.iShadowCascades.Value == 4)
        {
            QualitySettings.shadowCascades = 4; // Default = 1
            // Need to set ShadowProjection to CloseFit or we get visual glitches at 4 cascades.
            QualitySettings.shadowProjection = ShadowProjection.CloseFit; // Default = StableFit
            Log.LogInfo($"Shadow Cascades set to {QualitySettings.shadowCascades}. ShadowProjection = CloseFit");
        }
        else if (Rf5Fix.iShadowCascades.Value == 2)
        {
            QualitySettings.shadowCascades = 2; // Default = 1
            Log.LogInfo($"Shadow Cascades set to {QualitySettings.shadowCascades}");
        }

        // Shadow Distance
        if (Rf5Fix.fShadowDistance.Value >= 1f)
        {
            QualitySettings.shadowDistance = Rf5Fix.fShadowDistance.Value; // Default = 120f
            Log.LogInfo($"Shadow Distance set to {QualitySettings.shadowDistance}");
        }

        // LOD Bias
        if (Rf5Fix.fLODBias.Value >= 0.1f)
        {
            QualitySettings.lodBias = Rf5Fix.fLODBias.Value; // Default = 1.5f    
            Log.LogInfo($"LOD Bias set to {Rf5Fix.fLODBias.Value}");
        }

        // Mouse Sensitivity
        if (Rf5Fix.bMouseSensitivity.Value)
        {
            BootSystem.m_Option.MouseSensitivity = Rf5Fix.iMouseSensitivity.Value;
            Log.LogInfo($"Mouse sensitivity override. Value = {BootSystem.m_Option.MouseSensitivity}");
        }

        // NPC Distances
        if (Rf5Fix.fNPCDistance.Value >= 1f)
        {
            NpcSetting.ShowDistance = Rf5Fix.fNPCDistance.Value;
            NpcSetting.HideDistance = Rf5Fix.fNPCDistance.Value;
            Log.LogInfo($"NPC Distance set to {NpcSetting.ShowDistance}");
        }

        // Unity update rate
        // TODO: Replace this with camera movement interpolation?
        if (Rf5Fix.fUpdateRate.Value == 0) // Set update rate to screen refresh rate
        {
            Time.fixedDeltaTime = (float)1 / Screen.currentResolution.refreshRate;
            Log.LogInfo($"fixedDeltaTime set to {(float)1} / {Screen.currentResolution.refreshRate} = {Time.fixedDeltaTime}");
        }
        else if (Rf5Fix.fUpdateRate.Value > 50)
        {
            Time.fixedDeltaTime = 1 / Rf5Fix.fUpdateRate.Value;
            Log.LogInfo($"fixedDeltaTime set to {(float)1} / {Rf5Fix.fUpdateRate.Value} = {Time.fixedDeltaTime}");
        }

    }

    // Sun & Moon | Shadow Resolution
    [HarmonyPatch(typeof(Funly.SkyStudio.OrbitingBody), nameof(Funly.SkyStudio.OrbitingBody.LayoutOribit))]
    [HarmonyPostfix]
    public static void AdjustSunMoonLight(Funly.SkyStudio.OrbitingBody __instance)
    {
        if (Rf5Fix.iShadowResolution.Value >= 64)
        {
            __instance.BodyLight.shadowCustomResolution = Rf5Fix.iShadowResolution.Value; // Default = ShadowQuality (i.e VeryHigh = 4096)
        }
    }

    // RealtimeBakeLight | Shadow Resolution
    [HarmonyPatch(typeof(RealtimeBakeLight), nameof(RealtimeBakeLight.Start))]
    [HarmonyPostfix]
    public static void AdjustLightShadow(RealtimeBakeLight __instance)
    {
        if (Rf5Fix.iShadowResolution.Value >= 64)
        {
            __instance.Light.shadowCustomResolution = Rf5Fix.iShadowResolution.Value; // Default = ShadowQuality (i.e VeryHigh = 4096)
        }
    }

    // Fix low res render textures
    [HarmonyPatch(typeof(CampMenuMain), nameof(CampMenuMain.Start))]
    [HarmonyPatch(typeof(UIMonsterNaming), nameof(UIMonsterNaming.Start))]
    [HarmonyPostfix]
    public static void CampRenderTextureFix(CampMenuMain __instance)
    {
        if (Rf5Fix.bCampRenderTextureFix.Value)
        {
            if (!rt)
            {
                const float defaultAspectRatio = 16f / 9f;

                // Render from UI camera at higher resolution and with anti-aliasing
                var newHorizontalRes = Mathf.Floor(Screen.currentResolution.height * defaultAspectRatio);
                rt = new((int)newHorizontalRes, Screen.currentResolution.height, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = QualitySettings.antiAliasing
                };

                var uiCam = UIMainManager.Instance.GetComponent<Camera>(UIMainManager.AttachId.UICamera);
                uiCam.targetTexture = rt;
                uiCam.Render();

                Log.LogInfo($"Created new render texture for UI Camera.");
            }

            // Find raw images, even inactive ones
            // This is probably quite performance intensive.
            // There's probably a better way to do this.
            var rawImages = new List<RawImage>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (!s.isLoaded)
                    continue;
                
                foreach (var go in s.GetRootGameObjects())
                    rawImages.AddRange(go.GetComponentsInChildren<RawImage>(true));
            }

            // Find RawImages that use UICameraRenderTexture
            foreach (var rawImage in rawImages)
            {
                if (rawImage.m_Texture.name != "UICameraRenderTexture")
                    continue;
                
                rawImage.m_Texture = rt;
                Log.LogInfo($"Set {rawImage.gameObject.GetParent().name} texture to new high-res render texture.");
            }
        }
    }

    // Disable Hatching
    [HarmonyPatch(typeof(MeshFadeController), nameof(MeshFadeController.OnEnable))]
    [HarmonyPostfix]
    public static void DisableHatching(MeshFadeController __instance)
    {
        if (!Rf5Fix.bDisableCrossHatching.Value)
            return;
        
        // This is super hacky
        var meshRenderer = __instance.Renderers[0];
        var sketchTex = meshRenderer.material.GetTexture("_SketchTex");
        sketchTex.wrapMode = TextureWrapMode.Clamp;
    }
}