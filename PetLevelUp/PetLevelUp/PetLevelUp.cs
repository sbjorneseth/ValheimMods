using BepInEx;
using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PetLevelUp
{
    [BepInPlugin("org.bepinex.plugins.PetLevelUp", "Pet LevelUp", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class PetLevelUp : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("org.bepinex.plugins.PetLevelUp");

        void Awake()
        {
            harmony.PatchAll();
        }
        //public static int xp = 0;
        
        //[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.DoAttack))]
        //public static class MonsterAI_DoAttack_Postfix_Patch
        //{
        //    public static void Postfix(Character ___m_character, Character target)
        //    {
        //        if (___m_character.IsTamed())
        //        {
        //            Debug.Log(___m_character.GetHoverName() + " attacked " + target.GetHoverName());
        //
        //            xp++;
        //
        //            Debug.Log(xp);
        //
        //            int CurrentLevel = ___m_character.GetLevel();
        //            if (xp >= 5 && CurrentLevel == 1)
        //            {
        //                ___m_character.SetLevel(CurrentLevel + 1);
        //            }
        //            else if (xp >= 10 && CurrentLevel == 2)
        //            {
        //                ___m_character.SetLevel(CurrentLevel + 1);
        //            }
        //        }
        //    }
        //}
        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        public static class Character_Damage_Prefix_Patch
        {
            public static void Prefix(Character __instance, ref HitData hit, ZNetView ___m_nview)
            {
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
                if (__instance == null)
                {
                    Debug.Log("No character found!");
                    return;
                }
                Character attacker = hit.GetAttacker();
                float totalDamage = hit.GetTotalDamage();

                if (!attacker.IsTamed())
                {
                    Debug.Log("attacker is not tamed.");
                    return;
                }
                Debug.Log("Damage dealt: " + totalDamage);

                ZNetView znetview = attacker.GetComponentInParent<ZNetView>();
                ZDO zdo = znetview.GetZDO();

                int currentXP = zdo.GetInt("xpAttack", 0);
                int newXP = currentXP + 1;

                zdo.Set("xpAttack", newXP); // 2

                Debug.Log(zdo.GetInt("xpAttack", 0));

                int CurrentLevel = attacker.GetLevel();
                if (newXP >= 10 && CurrentLevel == 1)
                {
                    attacker.SetLevel(CurrentLevel + 1);
                }
                else if (newXP >= 20 && CurrentLevel == 2)
                {
                    attacker.SetLevel(CurrentLevel + 1);
                }
                //"xpAttack" 0-1000
                //"levelAttack" 0-10
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.OnDamaged))]
        public static class Character_OnDamaged_Prefix_Patch
        {
            public static void Prefix(Character __instance, ref HitData hit)
            {
                //"xpDefence" 0-1000
                //"levelDefence" 0-10
            }
        }
        //"xpAttack" 0-1000
        //"levelAttack" 0-10
        //"xpDefence" 0-1000
        //"levelDefence" 0-10
        //"xpMobility" 0-1000
        //"levelAttack" 0-10
    }
}
