using System;
using System.Collections.Generic;
using DiaToMas.CameraSystem;
using DiaToMas.Data;
using DiaToMas.Interaction;
using DiaToMas.Managers;
using DiaToMas.Models;
using DiaToMas.Player;
using DiaToMas.Services;
using DiaToMas.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiaToMas.Editor
{
    public static class AssignmentSmokeTest
    {
        private const string ScenePath = "Assets/Scenes/3D_PA-dia_to_mas.unity";
        private const string CurrencyDataPath = "Assets/Resources/GameData/currency_data.json";
        private const string ShopItemDataPath = "Assets/Resources/GameData/shop_item_data.json";
        private const string StartingInventoryDataPath = "Assets/Resources/GameData/starting_inventory_data.json";

        public static void Run()
        {
            EditorSceneManager.OpenScene(ScenePath);
            ValidateSceneObjects();
            ValidateGameDataReferences();
            ValidateShopPresenterReferences();
            ValidateTransactionFlow();
            Debug.Log("Assignment smoke test passed.");
        }

        private static void ValidateSceneObjects()
        {
            GameObject player = RequireGameObject("Player_Adventurer");
            RequireComponent<Rigidbody>(player);
            RequireComponent<PlayerInputReader>(player);
            RequireComponent<PlayerClickInteractor>(player);
            RequireComponent<PlayerMovement>(player);

            if (player.GetComponentInChildren<Animator>() == null)
            {
                throw new InvalidOperationException("Player_Adventurer needs an Animator in children.");
            }

            GameObject merchant = RequireGameObject("Merchant");
            RequireComponent<ShopInteractable>(merchant);
            Collider merchantCollider = RequireComponent<Collider>(merchant);
            Require(merchantCollider.isTrigger, "Merchant collider must be a trigger.");

            GameObject cameraObject = RequireGameObject("ThirdPersonCamera");
            RequireComponent<Camera>(cameraObject);
            RequireComponent<ThirdPersonCameraFollow>(cameraObject);

            RequireGameObject("ShopCanvas");
            RequireGameObject("EventSystem");
        }

        private static void ValidateGameDataReferences()
        {
            GameDataManager dataManager = UnityEngine.Object.FindFirstObjectByType<GameDataManager>();
            Require(dataManager != null, "Scene needs a GameDataManager.");

            RequireReference(dataManager, "_currencyDataJson");
            RequireReference(dataManager, "_shopItemDataJson");
            RequireReference(dataManager, "_startingInventoryDataJson");
            RequireReference(dataManager, "_playerMovementDataJson");
        }

        private static void ValidateShopPresenterReferences()
        {
            ShopPresenter presenter = UnityEngine.Object.FindFirstObjectByType<ShopPresenter>();
            Require(presenter != null, "Scene needs a ShopPresenter.");

            RequireReference(presenter, "_rootObject");
            RequireReference(presenter, "_itemRoot");
            RequireReference(presenter, "_inventoryRoot");
            RequireReference(presenter, "_itemButtonPrefab");
            RequireReference(presenter, "_inventoryItemRowPrefab");
            RequireReference(presenter, "_walletText");
            RequireReference(presenter, "_inventoryText");
            RequireReference(presenter, "_feedbackText");
            RequireReference(presenter, "_promptText");
            RequireReference(presenter, "_closeButton");
            RequireReference(presenter, "_quantitySelector");

            RequirePrefabComponent<ShopItemButtonView>("Assets/Prefabs/UI/ShopItemButtonView.prefab");
            RequirePrefabComponent<InventoryItemRowView>("Assets/Prefabs/UI/InventoryItemRowView.prefab");
        }

        private static void ValidateTransactionFlow()
        {
            List<CurrencyData> currencyDataList = LoadData<CurrencyData>(CurrencyDataPath);
            List<ShopItemData> shopItemDataList = LoadData<ShopItemData>(ShopItemDataPath);
            List<StartingInventoryData> startingInventoryDataList = LoadData<StartingInventoryData>(StartingInventoryDataPath);
            ShopItemData potionData = shopItemDataList.Find(itemData => itemData.id == "traveler_potion");
            ShopItemData runeShardData = shopItemDataList.Find(itemData => itemData.id == "rune_shard");
            Require(potionData != null, "traveler_potion data is required.");
            Require(runeShardData != null, "rune_shard data is required.");
            Require(potionData.isShopListed, "traveler_potion should be listed in the shop.");
            Require(!runeShardData.isShopListed, "rune_shard should be inventory loot, not a shop-listed item.");

            PlayerModel playerModel = new();
            foreach (CurrencyData currencyData in currencyDataList)
            {
                playerModel.WalletModel.SetAmount(currencyData.id, currencyData.startAmount);
            }

            foreach (StartingInventoryData inventoryData in startingInventoryDataList)
            {
                playerModel.InventoryModel.AddItem(inventoryData.itemId, inventoryData.amount);
            }

            ShopStockModel stockModel = new();
            stockModel.Initialize(shopItemDataList);
            ShopTransactionService transactionService = new();

            int startGold = playerModel.WalletModel.GetAmount("gold");
            int startCrystal = playerModel.WalletModel.GetAmount("crystal");
            int startStock = stockModel.GetStockCount(potionData.id);
            int startRuneShardAmount = playerModel.InventoryModel.GetAmount(runeShardData.id);
            Require(startRuneShardAmount > 0, "Starting inventory should include rune shards.");

            Require(transactionService.TryBuy(playerModel, stockModel, potionData, 2, out _), "Potion purchase should succeed.");
            Require(playerModel.InventoryModel.GetAmount(potionData.id) == 2, "Potion purchase should add inventory items.");
            Require(playerModel.WalletModel.GetAmount("gold") == startGold - potionData.priceAmount * 2, "Potion purchase should spend gold.");
            Require(stockModel.GetStockCount(potionData.id) == startStock - 2, "Potion purchase should reduce stock.");

            Require(transactionService.TrySell(playerModel, stockModel, potionData, 2, out _), "Potion sale should succeed.");
            Require(playerModel.InventoryModel.GetAmount(potionData.id) == 0, "Potion sale should remove inventory item.");
            Require(playerModel.WalletModel.GetAmount("gold") == startGold - potionData.priceAmount * 2 + potionData.sellAmount * 2, "Potion sale should add sell currency.");
            Require(stockModel.GetStockCount(potionData.id) == startStock, "Potion sale should restore stock.");

            Require(transactionService.TryDismantle(playerModel, runeShardData, 1, out _), "Rune shard dismantle should succeed.");
            Require(playerModel.InventoryModel.GetAmount(runeShardData.id) == startRuneShardAmount - 1, "Rune shard dismantle should remove one shard.");
            Require(playerModel.WalletModel.GetAmount("crystal") == startCrystal + runeShardData.dismantleAmount, "Rune shard dismantle should add crystal currency.");
        }

        private static GameObject RequireGameObject(string objectName)
        {
            GameObject gameObject = GameObject.Find(objectName);
            Require(gameObject != null, $"{objectName} is required.");
            return gameObject;
        }

        private static T RequireComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            Require(component != null, $"{gameObject.name} needs {typeof(T).Name}.");
            return component;
        }

        private static void RequireReference(UnityEngine.Object target, string propertyName)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Require(property != null, $"{target.name} is missing serialized property {propertyName}.");
            Require(property.objectReferenceValue != null, $"{target.name}.{propertyName} is not assigned.");
        }

        private static void RequirePrefabComponent<T>(string assetPath) where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Require(prefab != null, $"{assetPath} is required.");
            Require(prefab.GetComponent<T>() != null, $"{assetPath} needs {typeof(T).Name}.");
        }

        private static List<T> LoadData<T>(string assetPath) where T : GameDataBase
        {
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            Require(textAsset != null, $"{assetPath} is required.");

            GameDataList<T> dataList = JsonUtility.FromJson<GameDataList<T>>(textAsset.text);
            Require(dataList != null && dataList.items.Count > 0, $"{assetPath} has no data.");
            return dataList.items;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
