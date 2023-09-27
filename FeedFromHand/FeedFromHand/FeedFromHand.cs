using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using static ItemSets;

namespace FeedFromHand
{
    [BepInPlugin("org.bepinex.plugins.feedfromhand", "FeedFromHand", "1.1.0")]
    [BepInProcess("valheim.exe")]
    public class FeedFromHand : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("org.bepinex.plugins.feedfromhand");

        void Awake()
        {
            harmony.PatchAll();

            HealingReceived = ((BaseUnityPlugin)this).Config.Bind<int>("FeedFromHand", "HealingReceived", Humanoid_UseItem_Prefix_Patch.HealingReceived, "Amount that is healed upon consuming item");
            SuccessMessage = ((BaseUnityPlugin)this).Config.Bind<bool>("FeedFromHand", "SuccessMessage", Humanoid_UseItem_Prefix_Patch.SuccessMessage, "Whether or not success message is shown upon consuming item");
            SuccessMessageText = ((BaseUnityPlugin)this).Config.Bind<string>("FeedFromHand", "SuccessMessageText", Humanoid_UseItem_Prefix_Patch.SuccessMessageText, "Message shown upon consuming item. Default: 'PetName is happy!'");
            HealingReceived.SettingChanged += HealingReceived_SettingChanged;
            SuccessMessage.SettingChanged += SuccessMessage_SettingChanged;
            SuccessMessageText.SettingChanged += SuccessMessageText_SettingChanged;

            SettingsChanged();
        }
        private ConfigEntry<int> HealingReceived;
        private ConfigEntry<bool> SuccessMessage;
        private ConfigEntry<string> SuccessMessageText;
        private void SettingsChanged()
        {
            HealingReceived_SettingChanged(null, null);
            SuccessMessage_SettingChanged(null, null);
            SuccessMessageText_SettingChanged(null, null);
        }
        private void HealingReceived_SettingChanged(object sender, EventArgs e)
        {
            Humanoid_UseItem_Prefix_Patch.HealingReceived = HealingReceived.Value;
        }
        private void SuccessMessage_SettingChanged(object sender, EventArgs e)
        {
            Humanoid_UseItem_Prefix_Patch.SuccessMessage = SuccessMessage.Value;
        }
        private void SuccessMessageText_SettingChanged(object sender, EventArgs e)
        {
            Humanoid_UseItem_Prefix_Patch.SuccessMessageText = SuccessMessageText.Value;
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem))]
        public static class Humanoid_UseItem_Prefix_Patch
        {
            public static int HealingReceived = 50;
            public static bool SuccessMessage = true;
            public static string SuccessMessageText;

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
                string hoverName = character.GetHoverName();

                if (character.IsTamed() == false) 
                {
                    return true;
                }
                if (monsterAI.CanConsume(item) == false)
                {
                    if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable) {
                        __instance.Message(MessageHud.MessageType.Center, hoverName + " can't consume " + item.m_shared.m_name + ".");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                if (character.GetHealth() < character.GetMaxHealth())
                {
                    character.Heal(HealingReceived, true);
                    tameable.Interact(humanoid, false, false);
                    __instance.DoInteractAnimation(hoverObject.transform.position);

                    Inventory inventory = __instance.GetInventory();
                    inventory.RemoveOneItem(item);
                    
                    if (SuccessMessage == true)
                    {
                        string message = (SuccessMessageText != null) ? SuccessMessageText : hoverName + " is happy!";
                        __instance.Message(MessageHud.MessageType.Center, message);
                    }
                    return false;
                }
                else
                {
                    __instance.Message(MessageHud.MessageType.Center, hoverName + " is already max health.");
                    return false;
                }
            }
        }
    }
}