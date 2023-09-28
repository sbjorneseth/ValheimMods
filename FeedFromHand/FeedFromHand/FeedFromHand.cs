using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using static ItemSets;

namespace FeedFromHand
{
    [BepInPlugin("org.bepinex.plugins.feedfromhand", "FeedFromHand", "1.2.0")]
    [BepInProcess("valheim.exe")]
    public class FeedFromHand : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("org.bepinex.plugins.feedfromhand");

        void Awake()
        {
            harmony.PatchAll();

            HealingReceived = ((BaseUnityPlugin)this).Config.Bind<int>("FeedFromHand", "HealingReceived", FeedFromHand_Patch.HealingReceived, "Amount that is healed upon consuming item");
            Cooldown = ((BaseUnityPlugin)this).Config.Bind<float>("FeedFromHand", "Cooldown", FeedFromHand_Patch.Cooldown, "Amount that is healed upon consuming item");
            UseItem = ((BaseUnityPlugin)this).Config.Bind<bool>("FeedFromHand", "UseItem", FeedFromHand_Patch.UseItem, "Whether or not success message is shown upon consuming item");
            SuccessMessage = ((BaseUnityPlugin)this).Config.Bind<bool>("FeedFromHand", "SuccessMessage", FeedFromHand_Patch.SuccessMessage, "Whether or not success message is shown upon consuming item");

            HealingReceived.SettingChanged += HealingReceived_SettingChanged;
            Cooldown.SettingChanged += Cooldown_SettingChanged;
            UseItem.SettingChanged += UseItem_SettingChanged;
            SuccessMessage.SettingChanged += SuccessMessage_SettingChanged;

            SettingsChanged();
        }
        private ConfigEntry<int> HealingReceived;
        private ConfigEntry<float> Cooldown;
        private ConfigEntry<bool> UseItem;
        private ConfigEntry<bool> SuccessMessage;
        private void SettingsChanged()
        {
            HealingReceived_SettingChanged(null, null);
            Cooldown_SettingChanged(null, null);
            UseItem_SettingChanged(null, null);
            SuccessMessage_SettingChanged(null, null);
        }
        private void HealingReceived_SettingChanged(object sender, EventArgs e)
        {
            FeedFromHand_Patch.HealingReceived = HealingReceived.Value;
        }
        private void Cooldown_SettingChanged(object sender, EventArgs e)
        {
            FeedFromHand_Patch.Cooldown = Cooldown.Value;
        }
        private void UseItem_SettingChanged(object sender, EventArgs e)
        {
            FeedFromHand_Patch.UseItem = UseItem.Value;
        }
        private void SuccessMessage_SettingChanged(object sender, EventArgs e)
        {
            FeedFromHand_Patch.SuccessMessage = SuccessMessage.Value;
        }

        [HarmonyPatch]
        internal class FeedFromHand_Patch
        {
            public static int HealingReceived = 50;
            public static float Cooldown = 60f;
            public static bool UseItem = true;
            public static bool SuccessMessage = true;

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
                    ZNetView znetview = hoverObject.GetComponent<ZNetView>();

                    string hoverName = character.GetHoverName();

                    if (character.IsTamed() == false)
                    {
                        return true;
                    }
                    if (monsterAI.CanConsume(item) == false)
                    {
                        if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
                        {
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
                        ZDO zdo = znetview.GetZDO();
                        bool CanConsume = zdo.GetBool("CanConsume");

                        if (CanConsume == false)
                        {
                            return false;
                        }
                        character.Heal(HealingReceived, true);
                        tameable.Interact(humanoid, false, false);
                        __instance.DoInteractAnimation(hoverObject.transform.position);

                        if (UseItem == true) {
                            Inventory inventory = __instance.GetInventory();
                            inventory.RemoveOneItem(item);
                        }
                        if (SuccessMessage == true)
                        {
                            __instance.Message(MessageHud.MessageType.Center, hoverName + " is happy!");
                        }
                        zdo.Set("CanConsume", false);
                        return false;
                    }
                    else
                    {
                        __instance.Message(MessageHud.MessageType.Center, hoverName + " is already max health.");
                        return false;
                    }
                }
            }
            [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
            public static class MonsterAI_UpdateAI_Prefix_Patch
            {
                public static void Prefix(MonsterAI __instance, ref float dt)
                {
                    Character character = __instance.GetComponentInParent<Character>();

                    if (character.IsTamed())
                    {
                        ZNetView znetview = __instance.GetComponentInParent<ZNetView>();
                        ZDO zdo = znetview.GetZDO();
                        float timeSinceConsumption = zdo.GetFloat("TimeSinceConsumption") + dt;
                        zdo.Set("TimeSinceConsumption", timeSinceConsumption);

                        if (timeSinceConsumption >= FeedFromHand_Patch.Cooldown)
                        {
                            zdo.Set("CanConsume", true);
                            zdo.Set("TimeSinceConsumption", 0f);
                        }
                    }
                }
            }
        }
    }
}