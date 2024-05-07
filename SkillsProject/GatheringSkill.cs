using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace SkillsProject
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class GatheringSkill : BaseUnityPlugin
    {
        public const string PluginGUID = "AstralVoid.GatheringSkill";
        public const string PluginName = "Gathering Skill";
        public const string PluginVersion = "0.0.1";

        private const string skillNameToken = "$skill_gathering";
        private const string skillDescriptionToken = "$skill_gathering_desc";
        private const string iconPath = "GatheringSkill/Assets/GatheringIcon.png";

        private KeyCode increaseSkillLevelKey = KeyCode.Keypad1;
        private KeyCode resetSkillLevelKey = KeyCode.Keypad0;

        public static string[] IgnoredPickables = new[] { "Pickable_SurtlingCoreStand" };
        public static float XpGain = 1.0f;
        private static float finalMult = 3f;
        public static float multPerLevel = (finalMult - 1f) / 100f;

        public static Skills.SkillType SkillType;
        
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        public void PatchGatheringSkill()
        {
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("GatheringSkill has landed");

            AddLocalisations();
            AddSkill();
        }

        private void Update()
        {
            if (Input.GetKeyDown(increaseSkillLevelKey))
            {
                Player.m_localPlayer.RaiseSkill(SkillType, 10000);
            }
            if (Input.GetKeyDown(resetSkillLevelKey))
            {
                Player.m_localPlayer.m_skills.ResetSkill(SkillType);
            }
        }

        private void AddSkill()
        {
            SkillType = SkillManager.Instance.AddSkill(new SkillConfig
            {
                Identifier = "AstralVoid.GatheringSkill.GatheringSkill",
                Name = skillNameToken,
                Description = skillDescriptionToken,
                IconPath = iconPath,
            });
        }

        private void AddLocalisations()
        {
            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {skillNameToken, "Gathering" },
                {skillDescriptionToken, "Amount of things picked" }
            });

            Localization.AddTranslation("Russian", new Dictionary<string, string>
            {
                {skillNameToken, "Собирательство" },
                {skillDescriptionToken, "Количество подбираемых предметов" }
            });
        }

        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
        public class GatheringSkillIncrease
        {
            [HarmonyPrefix]
            public static void Prefix(Humanoid character, bool ___m_picked, Pickable __instance)
            {
                if (IsIgnoredPickable(__instance.name))
                    return;

                if (!___m_picked)
                {
                    character.RaiseSkill(SkillType, XpGain);
                }
            }
        }

        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Drop))]
        public static class DropMultiply
        {
            [HarmonyPrefix]
            public static void Prefix(GameObject prefab, int offset, ref int stack, ZNetView ___m_nview, bool ___m_picked, Pickable __instance)
            {
                Jotunn.Logger.LogInfo("[GatheringSkill] Respawn time: " + __instance.m_respawnTimeMinutes);

                if (__instance.m_respawnTimeMinutes == 0)
                    return;

                if (IsIgnoredPickable(__instance.name))
                    return;

                if (!___m_nview.IsValid())
                {
                    return;
                }

                stack = stack * GetDropMult();
            }

            private static int GetDropMult()
            {
                float skillLevel = Player.m_localPlayer.GetSkillLevel(SkillType);
                float mult = skillLevel * multPerLevel + 1;

                int guaranteedMult = Mathf.FloorToInt(mult);
                int randomMult = UnityEngine.Random.value < mult - guaranteedMult ? 1 : 0;
                return (guaranteedMult + randomMult);
            }
        }

        public static bool IsIgnoredPickable(string name)
        {
            if (IgnoredPickables != null)
            {
                foreach (var i in IgnoredPickables)
                {
                    if (i.Replace("(Clone)", "") == name.Replace("(Clone)", ""))
                    {
                        return true;
                    }
                }

            }
            return false;
        }
    }
}

