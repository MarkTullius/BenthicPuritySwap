using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VoidItemAPI;

namespace PhilsBenthicPuritySwap.Items
{
    internal class VoidPurity
    {
        public static ItemDef itemDef;

        static string itemName = "VoidBadLuck";
        static string upperName = itemName.ToUpper();

        public static Sprite LoadSprite()
        {
            return Assets.MainAssets.LoadAsset<Sprite>("assets/import/benthicpurityswap_icons/voidbadluck.png");
        }
        public static GameObject LoadPrefab()
        {
            return Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarBadLuck/PickupStarSeed.prefab").WaitForCompletion();
        }
        public static ItemDef CreateItem()
        {
            ItemDef item = ScriptableObject.CreateInstance<ItemDef>();

            item.name = itemName;
            item.nameToken = $"PHILSBENTHICPURITYSWAP_{upperName}_NAME";
            item.pickupToken = $"PHILSBENTHICPURITYSWAP_{upperName}_PICKUP";
            item.descriptionToken = $"PHILSBENTHICPURITYSWAP_{upperName}_DESC";
            item.loreToken = $"PHILSBENTHICPURITYSWAP_{upperName}_LORE";

            item.tags = new ItemTag[] { ItemTag.Utility };
            item.deprecatedTier = ItemTier.VoidTier3;
            item.canRemove = true;
            item.hidden = false;

            item.pickupModelPrefab = LoadPrefab();
            item.pickupIconSprite = LoadSprite();

            return item;
        }
        public static ItemDisplayRuleDict CreateDisplayRules()
        {
            return new ItemDisplayRuleDict(null);
        }

        public static void AddHooks()
        {
            VoidTransformation.CreateTransformation(itemDef, "Clover");

            RecalculateStatsAPI.GetStatCoefficients += EditStats;
        }

        private static void EditStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int itemCount;
            if (sender.inventory && (itemCount = sender.inventory.GetItemCount(itemDef)) > 0)
            {
                float cooldownReduction = 2f + 1f * (itemCount - 1);

                args.cooldownReductionAdd = cooldownReduction;
            }
        }

        public static void Init()
        {
            itemDef = CreateItem();
            ItemAPI.Add(new CustomItem(itemDef, CreateDisplayRules()));
            Log.Debug("Created " + itemName);

            AddHooks();
        }
    }
}
