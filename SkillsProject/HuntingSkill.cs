using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace SkillsProject;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid)]
//[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
internal class HuntingSkill : BaseUnityPlugin
{
    public const string PluginGUID = "AstralVoid.HuntingSkill";
    public const string PluginName = "Hunting Skill";
    public const string PluginVersion = "0.0.1";

    private const string skillNameToken = "$skill_hunting";
    private const string skillDescriptionToken = "$skill_hunting_desc";
    private const string iconPath = "HuntingSkill/Assets/HuntingIcon.png";

    private KeyCode increaseSkillLevelKey = KeyCode.Keypad2;
    private KeyCode resetSkillLevelKey = KeyCode.Keypad0;

    public static string[] Animals = new[]
    {
        "Boar",
        "Deer",
        "Wolf",
        "Lox",
        "Neck",
        "Hare",
        "Bat"
    };

    public static string[] Birds = new[]
    {
        "Seagal",
        "Crow"
    };

    public static float XpGain = 4f;
    private static float finalMult = 3f;
    public static float multPerLevel = (finalMult - 1f) / 100f;

    public static Skills.SkillType SkillType;

    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private readonly Harmony harmony = new Harmony(PluginGUID);

    public void PatchHuntingSkill()
    {

        Jotunn.Logger.LogInfo("HuntingSkill has landed");

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
            Identifier = "AstralVoid.HuntingSkill.HuntingSkill",
            Name = skillNameToken,
            Description = skillDescriptionToken,
            IconPath = iconPath,
        });
    }

    private void AddLocalisations()
    {
        Localization.AddTranslation("English", new Dictionary<string, string>
        {
            {skillNameToken, "Hunting" },
            {skillDescriptionToken, "Loot dropped from animals" }
        });

        Localization.AddTranslation("Russian", new Dictionary<string, string>
        {
            {skillNameToken, "Охота" },
            {skillDescriptionToken, "Количество предметов с животных" }
        });
    }

    public static bool IsAnimal(string name)
    {
        if (Animals != null)
        {
            foreach (var i in Animals)
            {
                if (i.Replace("(Clone)", "") == name.Replace("(Clone)", ""))
                {
                    return true;
                }
            }

        }
        return false;
    }

    public static bool IsBird(string name)
    {
        if (Animals != null)
        {
            foreach (var i in Birds)
            {
                if (i.Replace("(Clone)", "") == name.Replace("(Clone)", ""))
                {
                    return true;
                }
            }

        }
        return false;
    }

    private static int GetDropMult()
    {
        float skillLevel = Player.m_localPlayer.GetSkillLevel(SkillType);
        float mult = skillLevel * multPerLevel + 1;

        int guaranteedMult = Mathf.FloorToInt(mult);
        int randomMult = GetRandomMult(mult - guaranteedMult);
        return (guaranteedMult + randomMult);
    }

    private static int GetRandomMult(float chance)
    {
        return UnityEngine.Random.value < chance ? 1 : 0;
    }



    [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
    public class HuntingSkillIncreaseAnimals
    {
        [HarmonyPrefix]
        public static void Prefix(HitData ___m_lastHit, Character __instance)
        {
            bool killedByPlayer = ___m_lastHit != null && ___m_lastHit.GetAttacker() == Player.m_localPlayer;
            bool isAnimal = IsAnimal(__instance.gameObject.name);

            if (killedByPlayer)
                Jotunn.Logger.LogInfo("[HuntingSkill] Killed Character: " + __instance.gameObject.name + " of Faction: " + __instance.m_faction + "IsAnimal: " + isAnimal);

            if (killedByPlayer && isAnimal)
            {
                Player.m_localPlayer.RaiseSkill(SkillType, XpGain);

                var charDrop = __instance.GetComponent<CharacterDrop>();
                var drops = charDrop.m_drops;

                for (int i = 0; i < drops.Count; i++)
                {
                    if (drops[i].m_prefab.name.Contains("Trophy"))
                        continue;

                    int dropMult = GetDropMult();
                    drops[i].m_amountMin *= dropMult;
                    drops[i].m_amountMax *= dropMult;

                    if (dropMult > 0)
                        Jotunn.Logger.LogInfo("[HuntingSkill] Drops of item: " + drops[i].m_prefab.name + " increased times: " + dropMult);
                }
            }
        }

        [HarmonyPatch(typeof(DropOnDestroyed), nameof(DropOnDestroyed.OnDestroyed))]
        public class HuntingSkillIncreaseBirds
        {
            [HarmonyPrefix]
            public static void Prefix(DropTable ___m_dropWhenDestroyed, DropOnDestroyed __instance)
            {
                if (IsBird(__instance.gameObject.name))
                    Player.m_localPlayer.RaiseSkill(SkillType, XpGain);
            }
        }



        //[HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.DropItems))]
        //public static class DropMultiplyAnimals
        //{
        //    [HarmonyPrefix]
        //    public static void Prefix(List<KeyValuePair<GameObject, int>> drops, Vector3 centerPos, float dropArea, CharacterDrop __instance)
        //    {
        //        for (int i = 0; i < drops.Count; i++)
        //        {
        //            if (drops[i].Key.name.Contains("Trophy"))
        //                continue;

        //            int initialAmount = drops[i].Value;

        //            float skillLevel = Player.m_localPlayer.GetSkillLevel(SkillType);
        //            float mult = skillLevel * multPerLevel + 1;

        //            int guaranteedMult = Mathf.FloorToInt(mult);
        //            int randomMult = 0;
        //            float chance = mult - guaranteedMult;
        //            for (int j = 0; j < initialAmount; j++)
        //            {
        //                randomMult += GetRandomMult(chance);
        //            }

        //            drops[i] = new KeyValuePair<GameObject, int>(drops[i].Key, initialAmount * guaranteedMult + randomMult);

        //            int dropIncrease = drops[i].Value - initialAmount;
        //            if (dropIncrease > 0)
        //                Jotunn.Logger.LogInfo("[HuntingSkill] Drops of item: " + drops[i].Key.name + " increased by: " + dropIncrease);
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(DropOnDestroyed), nameof(DropOnDestroyed.OnDestroyed))]
        public class DropMultiplyBirds
        {
            [HarmonyPrefix]
            public static void Prefix(DropTable ___m_dropWhenDestroyed, DropOnDestroyed __instance)
            {
                ___m_dropWhenDestroyed.m_dropMin *= GetDropMult();
                ___m_dropWhenDestroyed.m_dropMax *= GetDropMult();
            }
        }
    }
}