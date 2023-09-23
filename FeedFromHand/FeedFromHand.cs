using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace FeedFromHand
{
    [BepInPlugin("org.bepinex.plugins.feedfromhand", "Feed From Hand", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class FeedFromHand : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("org.bepinex.plugins.feedfromhand");

        void Awake()
        {
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem))]
        public static class Humanoid_UseItem_Prefix_Patch
        {
            public static bool Prefix(Humanoid __instance, ItemDrop.ItemData item)
            {
                GameObject hoverObject = __instance.GetHoverObject();

                if (hoverObject == null)
                {
                    return true;
                }
                if (!hoverObject.GetComponent<MonsterAI>() && !hoverObject.GetComponent<Character>() && !hoverObject.GetComponent<Tameable>() && !hoverObject.GetComponent<Humanoid>())
                {
                    return true;
                }
                MonsterAI monsterAI = hoverObject.GetComponent<MonsterAI>();
                Character character = hoverObject.GetComponent<Character>();
                Tameable tameable = hoverObject.GetComponent<Tameable>();
                Humanoid humanoid = hoverObject.GetComponent<Humanoid>();

                if (character.IsTamed() && monsterAI.CanConsume(item))
                {
                    string hoverName = character.GetHoverName();

                    if (character.GetHealth() < character.GetMaxHealth())
                    {
                        character.Heal(50, true);
                        tameable.ResetFeedingTimer();
                        tameable.Interact(humanoid, false, false);
                        __instance.DoInteractAnimation(hoverObject.transform.position);

                        __instance.Message(MessageHud.MessageType.Center, hoverName + " is happy.");

                        Inventory inventory = __instance.GetInventory();
                        inventory.RemoveOneItem(item);

                        return false;
                    }
                    else
                    {
                        __instance.Message(MessageHud.MessageType.Center, hoverName + " is already max health.");
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}