using BepInEx;
using HarmonyLib;
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
        public static int xp = 0;

        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.DoAttack))]
        public static class Humanoid_UseItem_Prefix_Patch
        {
            public static void Postfix(Character ___m_character, Character target)
            {
                if (___m_character.IsTamed())
                {
                    Debug.Log(___m_character.GetHoverName() + " attacked " + target.GetHoverName());

                    xp++;

                    Debug.Log(xp);

                    int CurrentLevel = ___m_character.GetLevel();
                    if (xp >= 5 && CurrentLevel == 1)
                    {
                        ___m_character.SetLevel(CurrentLevel + 1);
                    }
                    else if (xp >= 10 && CurrentLevel == 2)
                    {
                        ___m_character.SetLevel(CurrentLevel + 1);
                    }
                }
            }
        }
    }
}
