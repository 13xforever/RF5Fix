﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using RF5Fix.Patches;

namespace RF5Fix;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
// ReSharper disable once ClassNeverInstantiated.Global
public class Rf5Fix : BasePlugin
{
    // ReSharper disable MemberCanBePrivate.Global
    internal new static ManualLogSource Log;

    // ReSharper disable InconsistentNaming
    public static ConfigEntry<bool> bUltrawideFixes;
    public static ConfigEntry<bool> bIntroSkip;
    public static ConfigEntry<bool> bLetterboxing;
    public static ConfigEntry<bool> bCampRenderTextureFix;
    public static ConfigEntry<bool> bDisableCrossHatching;
    public static ConfigEntry<bool> bFOVAdjust;
    public static ConfigEntry<float> fAdditionalFOV;
    public static ConfigEntry<float> fUpdateRate;
    public static ConfigEntry<bool> bMouseSensitivity;
    public static ConfigEntry<int> iMouseSensitivity;
    public static ConfigEntry<int> iAnisotropicFiltering;
    public static ConfigEntry<int> iShadowResolution;
    public static ConfigEntry<float> fLODBias;
    public static ConfigEntry<float> fNPCDistance;
    public static ConfigEntry<int> iShadowCascades;
    public static ConfigEntry<float> fShadowDistance;
    public static ConfigEntry<bool> bCustomResolution;
    public static ConfigEntry<float> fDesiredResolutionX;
    public static ConfigEntry<float> fDesiredResolutionY;
    public static ConfigEntry<int> iWindowMode;
    public static ConfigEntry<bool> bControllerType;

    public static ConfigEntry<string> sControllerType;
    // ReSharper restore InconsistentNaming
    // ReSharper restore MemberCanBePrivate.Global

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo("If the plugin stopped working and you see errors from Detour about failed IL2CPP patch backend initialization, delete cache, patchers, unhollowed, and unity-libs folders");

        // Features
        bUltrawideFixes = Config.Bind(
            "Ultrawide UI Fixes",
            "UltrawideFixes",
            true,
            "Set to true to enable ultrawide UI fixes."
        );

        bLetterboxing = Config.Bind(
            "Ultrawide UI Fixes",
            "Letterboxing",
            true,
            "Letterboxes UI (not gameplay). Set to false to disable letterboxing everywhere."
        );

        fUpdateRate = Config.Bind(
            "General",
            "PhysicsUpdateRate",
            0f, // 0 = Auto (Set to refresh rate) || Default = 50
            new ConfigDescription(
                "Set desired update rate. This will improve camera smoothness in particular. \n0 = Auto (Set to refresh rate). Game default = 50",
                new AcceptableValueRange<float>(0f, 5000f)
            )
        );

        bIntroSkip = Config.Bind(
            "General",
            "IntroSkip",
            true,
            "Skip intro logos."
        );

        bCampRenderTextureFix = Config.Bind(
            "General",
            "LowResMenuFix",
            true,
            "Fixes low-resolution 3D models in the equip menu/3D model viewer."
        );

        bDisableCrossHatching = Config.Bind(
            "General",
            "DisableCrossHatching",
            false,
            "Set to true to disable the crosshatch/sketch effect."
        );

        // Game Overrides
        bFOVAdjust = Config.Bind(
            "FOV Adjustment",
            "FOVAdjustment",
            true, // True by default to enable Vert+ for narrow aspect ratios.
            "Set to true to enable adjustment of the FOV. \nIt will also adjust the FOV to be Vert+ if your aspect ratio is narrower than 16:9."
        );

        fAdditionalFOV = Config.Bind(
            "FOV Adjustment",
            "AdditionalFOV.Value",
            0f,
            new ConfigDescription(
                "Set additional FOV in degrees. This does not adjust FOV in cutscenes.",
                new AcceptableValueRange<float>(0f, 180f)
            )
        );

        bMouseSensitivity = Config.Bind(
            "Mouse Sensitivity",
            "MouseSensitivity.Override",
            false, // Disable by default.
            "Set to true to enable mouse sensitivity override."
        );

        iMouseSensitivity = Config.Bind(
            "Mouse Sensitivity",
            "MouseSensitivity.Value",
            100, // Default = 100
            new ConfigDescription(
                "Set desired mouse sensitivity.",
                new AcceptableValueRange<int>(1, 9999)
            )
        );

        bControllerType = Config.Bind(
            "Controller Icon Override",
            "ControllerType.Override",
            false, // Disable by default.
            "Set to true to enable controller icon override."
        );

        sControllerType = Config.Bind(
            "Controller Icon Override",
            "ControllerType",
            "Xbox",
            new ConfigDescription(
                "Set desired controller icon type.",
                new AcceptableValueList<string>("Xbox", "PS4", "PS5", "Switch") // Others are broken/invalid
            )
        );

        // Custom Resolution
        bCustomResolution = Config.Bind(
            "Set Custom Resolution",
            "CustomResolution",
            false, // Disable by default as launcher should suffice.
            "Set to true to enable the custom resolution below."
        );

        fDesiredResolutionX = Config.Bind(
            "Set Custom Resolution",
            "ResolutionWidth",
            (float)Display.main.systemWidth, // Set default to display width so we don't leave an unsupported resolution as default.
            "Set desired resolution width."
        );

        fDesiredResolutionY = Config.Bind(
            "Set Custom Resolution",
            "ResolutionHeight",
            (float)Display.main.systemHeight, // Set default to display height so we don't leave an unsupported resolution as default.
            "Set desired resolution height."
        );

        iWindowMode = Config.Bind(
            "Set Custom Resolution",
            "WindowMode",
            2,
            new ConfigDescription(
                "Set window mode. 1 = Fullscreen, 2 = Borderless, 3 = Windowed.",
                new AcceptableValueRange<int>(1, 3)
            )
        );

        // Graphical Settings
        iAnisotropicFiltering = Config.Bind(
            "Graphical Tweaks",
            "AnisotropicFiltering.Value",
            1,
            new ConfigDescription(
                "Set Anisotropic Filtering level. 16 is recommended for quality.",
                new AcceptableValueRange<int>(1, 16)
            )
        );

        fLODBias = Config.Bind(
            "Graphical Tweaks",
            "LODBias.Value",
            1.5f, // Default = 1.5f
            new ConfigDescription(
                "Set LOD Bias. Controls distance for level of detail switching. 4 is recommended for quality.",
                new AcceptableValueRange<float>(0.1f, 10f)
            )
        );

        fNPCDistance = Config.Bind(
            "Graphical Tweaks",
            "NPCDistance.Value",
            2025f, // Default = 2025f (High)
            new ConfigDescription(
                "Set NPC Draw Distance. Controls distance at which NPCs render. 10000 is recommended for quality.",
                new AcceptableValueRange<float>(1f, 100000f)
            )
        );


        iShadowResolution = Config.Bind(
            "Graphical Tweaks",
            "ShadowResolution.Value",
            4096, // Default = Very High (4096)
            new ConfigDescription(
                "Set Shadow Resolution. 4096 is recommended for quality.",
                new AcceptableValueRange<int>(64, 32768)
            )
        );

        iShadowCascades = Config.Bind(
            "Graphical Tweaks",
            "ShadowCascades.Value",
            1, // Default = 1
            new ConfigDescription(
                "Set number of Shadow Cascades. 4 is recommended for quality but 2 is decent.",
                new AcceptableValueList<int>(1, 2, 4)
            )
        );

        fShadowDistance = Config.Bind(
            "Graphical Tweaks",
            "ShadowDistance.Value",
            120f, // Default = 120
            new ConfigDescription(
                "Set Shadow Distance. Controls distance at which shadows render. 180 is recommended for quality.",
                new AcceptableValueRange<float>(1f, 999f)
            )
        );

        // Run CustomResolutionPatch
        if (bCustomResolution.Value)
            Harmony.CreateAndPatchAll(typeof(CustomResolutionPatch));

        // Run UltrawidePatches
        if (bUltrawideFixes.Value)
            Harmony.CreateAndPatchAll(typeof(UltrawidePatches));

        // Run LetterboxingPatch
        if (bLetterboxing.Value)
            Harmony.CreateAndPatchAll(typeof(LetterboxingPatch));

        // Run IntroSkipPatch
        if (bIntroSkip.Value)
            Harmony.CreateAndPatchAll(typeof(IntroSkipPatch));

        // Run FOVPatch
        if (bFOVAdjust.Value)
            Harmony.CreateAndPatchAll(typeof(FOVPatch));

        // Run ControllerPatch
        if (bControllerType.Value)
            Harmony.CreateAndPatchAll(typeof(ControllerPatch));

        // Run MiscellaneousPatch
        Harmony.CreateAndPatchAll(typeof(MiscellaneousPatch));
    }
}