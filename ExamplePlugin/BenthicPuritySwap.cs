using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PhilsBenthicPuritySwap
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(VoidItemAPI.VoidItemAPI.MODGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class BenthicPuritySwap : BaseUnityPlugin
    {
        public const string PluginGUID    = PluginAuthor + "." + PluginName;
        public const string PluginAuthor  = "Boaphil";
        public const string PluginName    = "PhilsBenthicPuritySwap";
        public const string PluginVersion = "1.0.0";

        public static PluginInfo PInfo { get; private set; }
        
        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Log.Init(Logger);

            PInfo = Info;

            Assets.Init();

            On.RoR2.ItemCatalog.Init += OnItemCatalogInit;

            Items.LunarBenthic.Init();
            Items.VoidPurity.Init();
        }

        private void OnItemCatalogInit(On.RoR2.ItemCatalog.orig_Init orig)
        {
            foreach(ItemDef itemDef in RoR2.ContentManagement.ContentManager.itemDefs)
            {
                if (itemDef.name == "LunarBadLuck" ||
                    itemDef.name == "CloverVoid")
                {
                    itemDef._itemTierDef = null;
                    itemDef.deprecatedTier = ItemTier.NoTier;
                }
            }
            orig();
        }
    }
}
