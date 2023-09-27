using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;

namespace PetLevelUp
{
    [BepInPlugin("org.bepinex.plugins.PetLevelUp", "PetLevelUp", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class PetLevelUp : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("org.bepinex.plugins.PetLevelUp");

        void Awake()
        {
            harmony.PatchAll();

            LevelOneXPRequirement = ((BaseUnityPlugin)this).Config.Bind<int>("PetLevelUp", "LevelOneXPRequirement", PetLevelUp_Patch.LevelOneXPRequirement, "xp required for pet to level up");
            LevelTwoXPRequirement = ((BaseUnityPlugin)this).Config.Bind<int>("PetLevelUp", "LevelTwoXPRequirement", PetLevelUp_Patch.LevelTwoXPRequirement, "xp required for pet to level up");
            XPFromAttacking = ((BaseUnityPlugin)this).Config.Bind<bool>("PetLevelUp", "XPFromAttacking", PetLevelUp_Patch.XPFromAttacking, "Description of setting 3");
            XPFromDefending = ((BaseUnityPlugin)this).Config.Bind<bool>("PetLevelUp", "XPFromDefending", PetLevelUp_Patch.XPFromDefending, "Description of setting 3");

            LevelOneXPRequirement.SettingChanged += LevelOneXPRequirement_SettingChanged;
            LevelTwoXPRequirement.SettingChanged += LevelTwoXPRequirement_SettingChanged;
            XPFromAttacking.SettingChanged += XPFromAttacking_SettingChanged;
            XPFromDefending.SettingChanged += XPFromAttacking_SettingChanged;

            SettingsChanged();
        }
        private ConfigEntry<int> LevelOneXPRequirement;
        private ConfigEntry<int> LevelTwoXPRequirement;
        private ConfigEntry<bool> XPFromAttacking;
        private ConfigEntry<bool> XPFromDefending;

        private void SettingsChanged()
        {
            LevelOneXPRequirement_SettingChanged(null, null);
            LevelTwoXPRequirement_SettingChanged(null, null);
            XPFromAttacking_SettingChanged(null, null);
            XPFromDefending_SettingChanged(null, null);
        }
        private void LevelOneXPRequirement_SettingChanged(object sender, EventArgs e)
        {
            PetLevelUp_Patch.LevelOneXPRequirement = LevelOneXPRequirement.Value;
        }
        private void LevelTwoXPRequirement_SettingChanged(object sender, EventArgs e)
        {
            PetLevelUp_Patch.LevelTwoXPRequirement = LevelTwoXPRequirement.Value;
        }
        private void XPFromAttacking_SettingChanged(object sender, EventArgs e)
        {
            PetLevelUp_Patch.XPFromAttacking = XPFromAttacking.Value;
        }
        private void XPFromDefending_SettingChanged(object sender, EventArgs e)
        {
            PetLevelUp_Patch.XPFromDefending = XPFromDefending.Value;
        }

        [HarmonyPatch]
        internal class PetLevelUp_Patch
        {
            public static int LevelOneXPRequirement = 500;
            public static int LevelTwoXPRequirement = 1000;
            public static bool XPFromAttacking = true;
            public static bool XPFromDefending = true;

            [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
            public static class Character_Damage_Prefix_Patch
            {
                public static void Prefix(Character __instance, ref HitData hit, ZNetView ___m_nview)
                {
                    Character defender = __instance;
                    if (!___m_nview.IsValid())
                    {
                        Debug.Log("ZNetView not valid!");
                        return;
                    }
                    if (hit == null)
                    {
                        Debug.Log("No HitData found!");
                        return;
                    }
                    if (defender == null)
                    {
                        Debug.Log("No defender found!");
                        return;
                    }
                    Character attacker = hit.GetAttacker();
                    if (attacker == null)
                    {
                        Debug.Log("No attacker found!");
                        return;
                    }
                    Debug.Log("Attacker = " + attacker);
                    Debug.Log("Defender = " + defender);
                    if (attacker.IsTamed())
                    {
                        defender = null;
                    }
                    else if (defender.IsTamed())
                    {
                        attacker = null;
                    }
                    else
                    {
                        return;
                    }
                    if (attacker && XPFromAttacking == true || defender && XPFromDefending == true)
                    {
                        Character pet = attacker ? attacker : defender ? defender : null;
                        Debug.Log("Pet = " + pet);
                        ZNetView znetview = pet.GetComponentInParent<ZNetView>();
                        ZDO zdo = znetview.GetZDO();
                        int currentXP = zdo.GetInt("xp", 0);
                        int newXP = currentXP + 1;

                        zdo.Set("xp", newXP);

                        Debug.Log("zdo = " + zdo.GetInt("xp", 0));

                        int CurrentLevel = pet.GetLevel();
                        if (newXP >= LevelOneXPRequirement && CurrentLevel == 1)
                        {
                            pet.SetLevel(CurrentLevel + 1);
                        }
                        else if (newXP >= LevelTwoXPRequirement && CurrentLevel == 2)
                        {
                            pet.SetLevel(CurrentLevel + 1);
                        }
                    }
                }
            }
        }
    }
}
