using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace SkillsProject
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SkillsProject : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.SkillsProject";
        public const string PluginName = "SkillsProject";
        public const string PluginVersion = "0.0.1";
        private readonly Harmony harmony = new Harmony(PluginGUID);
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            harmony.PatchAll();
            
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("SkillsProject has landed");

            var gSkill = gameObject.AddComponent<GatheringSkill>();
            var hSkill = gameObject.AddComponent<HuntingSkill>();
            
            gSkill.PatchGatheringSkill();
            hSkill.PatchHuntingSkill();
            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html
        }
    }
}

