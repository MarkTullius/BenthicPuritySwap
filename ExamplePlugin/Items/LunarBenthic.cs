using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace PhilsBenthicPuritySwap.Items
{
    internal class LunarBenthic
    {
        public static ItemDef itemDef;

        static string itemName = "CloverLunar";
        static string upperName = itemName.ToUpper();

        static Xoroshiro128Plus cloverVoidRng;

        public static Sprite LoadSprite()
        {
            return Assets.MainAssets.LoadAsset<Sprite>("assets/import/benthicpurityswap_icons/cloverLunar.png");
        }
        public static GameObject LoadPrefab()
        {
            return Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/CloverVoid/PickupCloverVoid.prefab").WaitForCompletion();
        }
        public static ItemDef CreateItem()
        {
            ItemDef item = ScriptableObject.CreateInstance<ItemDef>();

            item.name = itemName;
            item.nameToken = $"PHILSBENTHICPURITYSWAP_{upperName}_NAME";
            item.pickupToken = $"PHILSBENTHICPURITYSWAP_{upperName}_PICKUP";
            item.descriptionToken = $"PHILSBENTHICPURITYSWAP_{upperName}_DESC";
            item.loreToken = $"PHILSBENTHICPURITYSWAP_{upperName}_LORE";

            item.tags = new ItemTag[] { ItemTag.Utility, ItemTag.OnStageBeginEffect };
            item.deprecatedTier = ItemTier.Lunar;
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
            On.RoR2.CharacterMaster.OnServerStageBegin += OnNextStage;

            On.RoR2.CharacterMaster.OnInventoryChanged += OnInventoryChanged;
        }

        private static void OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);

            int itemCount;
            if (self.inventory && (itemCount = self.inventory.GetItemCount(itemDef)) > 0)
            {
                self.luck -= itemCount;
            }
        }

        private static void OnNextStage(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            if (self.inventory && self.inventory.GetItemCount(itemDef) > 0)
            {
                TryCloverUpgrades(self);
            }
        }

        private static void TryCloverUpgrades(CharacterMaster characterMaster)
        {
            Inventory inventory = characterMaster.inventory;

            if (!NetworkServer.active)
            {
                return;
            }
            if (cloverVoidRng == null)
            {
                cloverVoidRng = new Xoroshiro128Plus(Run.instance.seed);
            }
            int itemCount = inventory.GetItemCount(itemDef);
            List<PickupIndex> availableTier2Drops = new List<PickupIndex>(Run.instance.availableTier2DropList);
            List<PickupIndex> availableTier3Drops = new List<PickupIndex>(Run.instance.availableTier3DropList);
            List<ItemIndex> upgradeItems = new List<ItemIndex>(inventory.itemAcquisitionOrder);
            Util.ShuffleList(upgradeItems, cloverVoidRng);
            int numberOfItems = itemCount * 3;
            int currentBloomIndex = 0;
            int currentItemIndex = 0;
            while (currentBloomIndex < numberOfItems && currentItemIndex < upgradeItems.Count)
            {
                ItemDef startingItemDef = ItemCatalog.GetItemDef(upgradeItems[currentItemIndex]);
                ItemDef itemDef = null;
                List<PickupIndex> upgradedItems = null;
                switch (startingItemDef.tier)
                {
                    case ItemTier.Tier1:
                        upgradedItems = availableTier2Drops;
                        break;
                    case ItemTier.Tier2:
                        upgradedItems = availableTier3Drops;
                        break;
                }
                if (upgradedItems != null && upgradedItems.Count > 0)
                {
                    Util.ShuffleList(upgradedItems, cloverVoidRng);
                    upgradedItems.Sort(CompareTags);
                    itemDef = ItemCatalog.GetItemDef(upgradedItems[0].itemIndex);
                }
                if (itemDef != null)
                {
                    if (inventory.GetItemCount(itemDef.itemIndex) == 0)
                    {
                        upgradeItems.Add(itemDef.itemIndex);
                    }
                    currentBloomIndex++;
                    int itemCount2 = inventory.GetItemCount(startingItemDef.itemIndex);
                    inventory.RemoveItem(startingItemDef.itemIndex, itemCount2);
                    inventory.GiveItem(itemDef.itemIndex, itemCount2);
                    CharacterMasterNotificationQueue.SendTransformNotification(characterMaster, startingItemDef.itemIndex, itemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.CloverVoid);
                }
                currentItemIndex++;
                int CompareTags(PickupIndex lhs, PickupIndex rhs)
                {
                    int num4 = 0;
                    int num5 = 0;
                    ItemDef itemDef2 = ItemCatalog.GetItemDef(lhs.itemIndex);
                    ItemDef itemDef3 = ItemCatalog.GetItemDef(rhs.itemIndex);
                    if (startingItemDef.ContainsTag(ItemTag.Damage))
                    {
                        if (itemDef2.ContainsTag(ItemTag.Damage))
                        {
                            num4 = 1;
                        }
                        if (itemDef3.ContainsTag(ItemTag.Damage))
                        {
                            num5 = 1;
                        }
                    }
                    if (startingItemDef.ContainsTag(ItemTag.Healing))
                    {
                        if (itemDef2.ContainsTag(ItemTag.Healing))
                        {
                            num4 = 1;
                        }
                        if (itemDef3.ContainsTag(ItemTag.Healing))
                        {
                            num5 = 1;
                        }
                    }
                    if (startingItemDef.ContainsTag(ItemTag.Utility))
                    {
                        if (itemDef2.ContainsTag(ItemTag.Utility))
                        {
                            num4 = 1;
                        }
                        if (itemDef3.ContainsTag(ItemTag.Utility))
                        {
                            num5 = 1;
                        }
                    }
                    return num5 - num4;
                }
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
