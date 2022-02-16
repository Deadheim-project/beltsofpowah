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

namespace BeltsOfPowah
{
    [BepInPlugin(PluginGUID, PluginGUID, Version)]

    public class Plugin : BaseUnityPlugin
    {
        ConfigSync configSync = new ConfigSync(PluginGUID) { DisplayName = PluginGUID, CurrentVersion = Version, MinimumRequiredVersion = Version };

        public const string Version = "1.0.0";
        public const string PluginGUID = "Detalhes.BeltsOfPowah";
      
        public static ConfigEntry<string> BlockedRockAndTreeDropAtEndAge;

        public static GameObject dontCraftPrefab;

        Harmony _harmony = new Harmony(PluginGUID);

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
        }

        unsafe private void CreatePrefab(string name)
        {
            Item item = new Item("beltsofpowah", name);
            item.RequiredItems.Add("Iron", 10);
            item.RequiredItems.Add("TrollHide", 50);
            item.RequiredItems.Add("LeatherScraps", 50);
            item.RequiredItems.Add("WolfPelt", 10);
            item.Crafting.Add(CraftingTable.Workbench, 2);
        } 
    }
}
