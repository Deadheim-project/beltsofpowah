using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using ItemManager;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;

namespace BeltsOfPowah
{
    [BepInPlugin(PluginGUID, PluginGUID, Version)]

    public class Plugin : BaseUnityPlugin
    {
        ConfigSync configSync = new ConfigSync(PluginGUID) { DisplayName = PluginGUID, CurrentVersion = Version, MinimumRequiredVersion = Version };

        public const string Version = "1.0.3";
        public const string PluginGUID = "Detalhes.BeltsOfPowah";      

        Harmony _harmony = new Harmony(PluginGUID);

        public static Dictionary<string, ConfigEntry<float>> skillValue = new();
        public static Dictionary<string, ConfigEntry<string>> beltText = new();

        static List<GameObject> belts = new();

        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

        [HarmonyPatch(typeof(ConfigSync), "RPC_ConfigSync")]
        public static class RPC_ConfigSync
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix()
            {
                if (ZNet.instance.IsServer()) return;

                foreach (var item in belts)
                {
                    ItemDrop itemDrop = item.GetComponent<ItemDrop>();
                    SE_Stats effect = (SE_Stats)itemDrop.m_itemData.m_shared.m_equipStatusEffect;

                    effect.m_skillLevelModifier = skillValue[effect.name.ToLower()].Value;
                    itemDrop.m_itemData.m_shared.m_description = beltText[effect.name.ToLower()].Value;
                }
            }
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        public static class OnSpawned
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix()
            {
                foreach (var item in belts)
                {
                    ItemDrop itemDrop = item.GetComponent<ItemDrop>();
                    SE_Stats effect = (SE_Stats)itemDrop.m_itemData.m_shared.m_equipStatusEffect;

                    effect.m_skillLevelModifier = skillValue[effect.name.ToLower()].Value;
                    itemDrop.m_itemData.m_shared.m_description = beltText[effect.name.ToLower()].Value;
                }
            }
        }

        private void Awake()
        {
            Config.SaveOnConfigSet = true;

            _harmony.PatchAll();

            CreatePrefab("BeltLumberjack");
            CreatePrefab("BeltMiner");
            CreatePrefab("BeltOfSpeed");
            CreatePrefab("BeltOfJumper");
            CreatePrefab("BeltOfBoxer");
            CreatePrefab("BeltOfBlock");
            CreatePrefab("BeltOfRider");
            CreatePrefab("BeltOfRogue");
            CreatePrefab("BeltOfSwordsman");
            CreatePrefab("BeltOfAxeman");
            CreatePrefab("BeltOfClubs");
            CreatePrefab("BeltOfPolearm");
            CreatePrefab("BeltOfSpears");
            CreatePrefab("beltOfGreatMagicResistence");
            CreatePrefab("beltOfMagicResistence");
        }

        unsafe private void CreatePrefab(string name)
        {
            Item item = new Item("beltsofpowah", name);
            item.RequiredItems.Add("Iron", 10);
            item.RequiredItems.Add("TrollHide", 50);
            item.RequiredItems.Add("LeatherScraps", 50);
            item.RequiredItems.Add("WolfPelt", 10);
            item.Crafting.Add(CraftingTable.Workbench, 2);
            SE_Stats effect = (SE_Stats) item.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_equipStatusEffect;
            belts.Add(item.Prefab);
            skillValue.Add(effect.name.ToLower(), config("1 - Skill", $"{name}", effect.m_skillLevelModifier, name));
            beltText.Add(effect.name.ToLower(),config("1 - Text", $"{name}", item.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_description.Replace('\'', ' '), name));
        }
    }
}
