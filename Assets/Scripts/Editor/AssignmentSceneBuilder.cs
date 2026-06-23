using DiaToMas.CameraSystem;
using DiaToMas.Interaction;
using DiaToMas.Managers;
using DiaToMas.Player;
using DiaToMas.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DiaToMas.Editor
{
    public static class AssignmentSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/3D_PA-dia_to_mas.unity";
        private const string RowPrefabPath = "Assets/Prefabs/UI/ShopItemButtonView.prefab";
        private const string InventoryRowPrefabPath = "Assets/Prefabs/UI/InventoryItemRowView.prefab";
        private const string PickupPrefabPath = "Assets/Prefabs/World/WorldItemPickup.prefab";
        private const string PanelFrameSpritePath = "Assets/ImportedAssets/GuiProCasualGame/Frame/BasicFrame_Round20.png";
        private const string ListFrameSpritePath = "Assets/ImportedAssets/GuiProCasualGame/Frame/BasicFrame_Square.png";
        private const string RowFrameSpritePath = "Assets/ImportedAssets/GuiProCasualGame/Frame/ItemFrame02_Single_Yellow.png";
        private const string ButtonSpritePath = "Assets/ImportedAssets/GuiProCasualGame/Button/Button_TaperedDown_Green.png";
        private const string GoldIconSpritePath = "Assets/Resources/UI/GuiProCasualGame/Icon/Pictoicon_Coin_Star.png";
        private const string CrystalIconSpritePath = "Assets/Resources/UI/GuiProCasualGame/Icon/Pictoicon_Crystal.png";
        private const string DefaultItemIconSpritePath = "Assets/Resources/UI/GuiProCasualGame/Icon/Icon_Chest.png";

        private static readonly string[] BuildRootNames =
        {
            "GameManager",
            "Scene_Environment",
            "Player_Mage",
            "Player_Adventurer",
            "Merchant",
            "Main Camera",
            "ThirdPersonCamera",
            "ShopCanvas",
            "WorldPickup_WolfPelt",
            "EventSystem",
            "Directional Light",
            "Sun Light",
            "Shop Warm Light"
        };

        [MenuItem("Tools/DiaToMas/Build Assignment Scene")]
        public static void BuildScene()
        {
            EditorSceneManager.OpenScene(ScenePath);
            RemoveGeneratedObjects();

            Material groundMaterial = CreateMaterial("Ground_Grass", new Color(0.22f, 0.42f, 0.24f));
            Material fallbackMaterial = CreateMaterial("Fallback_Warm_Stone", new Color(0.52f, 0.48f, 0.4f));
            Material woodMaterial = CreateMaterial("Fallback_Wood", new Color(0.42f, 0.25f, 0.12f));

            ShopItemButtonView itemRowPrefab = CreateShopItemRowPrefab();
            InventoryItemRowView inventoryRowPrefab = CreateInventoryItemRowPrefab();
            WorldItemPickup pickupPrefab = CreateWorldItemPickupPrefab();
            ShopPresenter shopPresenter = CreateShopCanvas(itemRowPrefab, inventoryRowPrefab);
            CreateGameManager(pickupPrefab);
            CreateEnvironment(groundMaterial, fallbackMaterial, woodMaterial);

            Camera camera = CreateCamera();
            GameObject player = CreatePlayer(camera.transform);
            ThirdPersonCameraFollow cameraFollow = camera.GetComponent<ThirdPersonCameraFollow>();
            cameraFollow.SetTarget(player.transform);

            CreateMerchant(shopPresenter);
            CreateStartingWorldPickup(pickupPrefab);
            CreateLighting();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        private static void RemoveGeneratedObjects()
        {
            foreach (string rootName in BuildRootNames)
            {
                GameObject target = GameObject.Find(rootName);
                while (target != null)
                {
                    Object.DestroyImmediate(target);
                    target = GameObject.Find(rootName);
                }
            }
        }

        private static void CreateGameManager(WorldItemPickup pickupPrefab)
        {
            GameObject gameManagerObject = new("GameManager");
            GameDataManager dataManager = gameManagerObject.AddComponent<GameDataManager>();
            WorldItemDropper itemDropper = gameManagerObject.AddComponent<WorldItemDropper>();
            GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

            SetObject(dataManager, "_currencyDataJson", Load<TextAsset>("Assets/Resources/GameData/currency_data.json"));
            SetObject(dataManager, "_shopItemDataJson", Load<TextAsset>("Assets/Resources/GameData/shop_item_data.json"));
            SetObject(dataManager, "_startingInventoryDataJson", Load<TextAsset>("Assets/Resources/GameData/starting_inventory_data.json"));
            SetObject(dataManager, "_playerMovementDataJson", Load<TextAsset>("Assets/Resources/GameData/player_movement_data.json"));
            SetObject(itemDropper, "_pickupPrefab", pickupPrefab);
            SetObject(gameManager, "_gameDataManager", dataManager);
            SetObject(gameManager, "_worldItemDropper", itemDropper);
        }

        private static void CreateEnvironment(Material groundMaterial, Material fallbackMaterial, Material woodMaterial)
        {
            GameObject root = new("Scene_Environment");

            Material dirtMaterial = CreateTexturedMaterial("Market_Dirt", "Assets/PolygonFantasyKingdom/Textures/Ground/PFK_Texture_Ground_Mud_02.png", new Color(0.42f, 0.34f, 0.24f), new Vector2(8f, 8f));
            Material roadMaterial = CreateTexturedMaterial("Market_Road_Stone", "Assets/PolygonFantasyKingdom/Textures/Ground/PFK_Texture_Ground_Sand_03.png", new Color(0.42f, 0.39f, 0.34f), new Vector2(4f, 7f));

            CreateGroundPlane("Ground", root.transform, dirtMaterial, Vector3.zero, new Vector3(9f, 1f, 9f), true);
            CreateGroundPlane("Market_Stone_Path", root.transform, roadMaterial, new Vector3(0f, 0.012f, 1.8f), new Vector3(0.55f, 1f, 1.25f), false);

            GameObject marketRoot = new("Market_Square");
            marketRoot.transform.SetParent(root.transform);

            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Market_Stall_01.prefab", "Merchant_Stall", new Vector3(0f, 0f, 0.25f), Quaternion.Euler(0f, 180f, 0f), Vector3.one, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Market_Stall_04.prefab", "Side_Stall_Left", new Vector3(-5.4f, 0f, 1.6f), Quaternion.Euler(0f, 135f, 0f), Vector3.one * 0.95f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Market_Stall_07.prefab", "Side_Stall_Right", new Vector3(5.6f, 0f, 0.8f), Quaternion.Euler(0f, 225f, 0f), Vector3.one * 0.95f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/Furniture/SM_Prop_Table_Wood_03.prefab", "Merchant_Display_Table", new Vector3(0f, 0f, 1.28f), Quaternion.Euler(0f, 180f, 0f), Vector3.one, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Items/SM_Item_Bottle_Clear_Labelled_02.prefab", "Display_Potion_Bottle", new Vector3(-0.55f, 0.86f, 1.22f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 0.58f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Items/SM_Item_Gem_04.prefab", "Display_Crystal", new Vector3(0.16f, 0.86f, 1.2f), Quaternion.Euler(0f, -14f, 0f), Vector3.one * 0.62f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Items/SM_Item_Pouch_01.prefab", "Display_Coin_Pouch", new Vector3(0.68f, 0.82f, 1.2f), Quaternion.Euler(0f, -35f, 0f), Vector3.one * 0.6f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Items/SM_Item_Book_03.prefab", "Display_Ledger", new Vector3(-1.0f, 0.82f, 1.18f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 0.58f, marketRoot.transform, null);

            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Crate_Stack_02.prefab", "Market_Crates_Left", new Vector3(-2.8f, 0f, 1.85f), Quaternion.Euler(0f, 20f, 0f), Vector3.one * 0.85f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Crate_Wood_01.prefab", "Market_Crate_Right", new Vector3(2.8f, 0f, 2.15f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.8f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Barrel_Stack_01.prefab", "Market_Barrels_Left", new Vector3(-3.55f, 0f, -0.45f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 0.9f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Items/SM_Item_Jar_03.prefab", "Market_Jar_Right", new Vector3(3.55f, 0f, 0.15f), Quaternion.Euler(0f, -40f, 0f), Vector3.one * 0.85f, marketRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Well_01.prefab", "Village_Well", new Vector3(-6.8f, 0f, -4.2f), Quaternion.Euler(0f, 24f, 0f), Vector3.one, marketRoot.transform, null);

            GameObject villageRoot = new("Village_Backdrop");
            villageRoot.transform.SetParent(root.transform);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Buildings/Preset_Houses/SM_Bld_Preset_Tavern_01_Optimized.prefab", "Village_Tavern", new Vector3(0f, 0f, -14.5f), Quaternion.Euler(0f, 0f, 0f), Vector3.one * 1.12f, villageRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Buildings/Preset_Houses/SM_Bld_Preset_Blacksmith_01_Optimized.prefab", "Village_Blacksmith", new Vector3(12f, 0f, -6.5f), Quaternion.Euler(0f, -35f, 0f), Vector3.one, villageRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Buildings/Preset_Houses/SM_Bld_Preset_House_05_Optimized.prefab", "Village_House_Left", new Vector3(-12f, 0f, -6.8f), Quaternion.Euler(0f, 35f, 0f), Vector3.one, villageRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Buildings/Preset_Houses/SM_Bld_Preset_House_08_Optimized.prefab", "Village_House_Right", new Vector3(13.5f, 0f, 7.2f), Quaternion.Euler(0f, -115f, 0f), Vector3.one * 0.95f, villageRoot.transform, null);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Buildings/Preset_Houses/SM_Bld_Preset_Stables_01_Optimized.prefab", "Village_Stables", new Vector3(-12.5f, 0f, 8.5f), Quaternion.Euler(0f, 130f, 0f), Vector3.one, villageRoot.transform, null);
        }

        private static GameObject CreatePlayer(Transform cameraTransform)
        {
            GameObject player = PlacePrefab("Assets/ImportedAssets/FantasyAnimatedCharacters/Fantasy animated characters pack/Peasant animated character/Prefab/Peasant.prefab", "Player_Adventurer", new Vector3(0f, 0.2f, 5.4f), Quaternion.Euler(0f, 180f, 0f), Vector3.one, null, null);
            player.tag = "Player";

            Rigidbody rigidbody = player.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = player.AddComponent<Rigidbody>();
            }

            rigidbody.mass = 1f;
            rigidbody.useGravity = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            CapsuleCollider collider = player.GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = player.AddComponent<CapsuleCollider>();
            }

            collider.height = 1.8f;
            collider.radius = 0.35f;
            collider.center = new Vector3(0f, 0.9f, 0f);

            PlayerInputReader inputReader = player.GetComponent<PlayerInputReader>();
            if (inputReader == null)
            {
                inputReader = player.AddComponent<PlayerInputReader>();
            }

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement == null)
            {
                movement = player.AddComponent<PlayerMovement>();
            }

            PlayerAnimatorView animatorView = player.GetComponent<PlayerAnimatorView>();
            if (animatorView == null)
            {
                animatorView = player.AddComponent<PlayerAnimatorView>();
            }

            PlayerClickInteractor clickInteractor = player.GetComponent<PlayerClickInteractor>();
            if (clickInteractor == null)
            {
                clickInteractor = player.AddComponent<PlayerClickInteractor>();
            }

            GameObject groundCheckPoint = new("GroundCheckPoint");
            groundCheckPoint.transform.SetParent(player.transform);
            groundCheckPoint.transform.localPosition = new Vector3(0f, 0.08f, 0f);

            SetObject(movement, "_rigidbody", rigidbody);
            SetObject(movement, "_inputReader", inputReader);
            SetObject(movement, "_cameraTransform", cameraTransform);
            SetObject(movement, "_groundCheckPoint", groundCheckPoint.transform);
            SetObject(animatorView, "_movement", movement);
            SetObject(animatorView, "_animator", player.GetComponentInChildren<Animator>());
            SetString(animatorView, "_idleStateName", "Peasant_Idle");
            SetString(animatorView, "_walkStateName", "Peasant_Walk");
            SetString(animatorView, "_runStateName", "Peasant_Walk");
            SetFloat(animatorView, "_runPlaybackSpeed", 1.5f);
            SetObject(clickInteractor, "_camera", cameraTransform.GetComponent<Camera>());
            return player;
        }

        private static void CreateMerchant(ShopPresenter shopPresenter)
        {
            GameObject merchant = PlacePrefab("Assets/ImportedAssets/FantasyAnimatedCharacters/Fantasy animated characters pack/Mage animated character/Prefab/Mage.prefab", "Merchant", new Vector3(0f, 0f, -0.95f), Quaternion.identity, Vector3.one, null, null);
            CapsuleCollider trigger = merchant.GetComponent<CapsuleCollider>();
            if (trigger == null)
            {
                trigger = merchant.AddComponent<CapsuleCollider>();
            }

            trigger.height = 2.2f;
            trigger.radius = 2.2f;
            trigger.center = Vector3.up;
            trigger.isTrigger = true;

            NpcIdleAnimatorView idleAnimatorView = merchant.GetComponent<NpcIdleAnimatorView>();
            if (idleAnimatorView == null)
            {
                idleAnimatorView = merchant.AddComponent<NpcIdleAnimatorView>();
            }

            SetObject(idleAnimatorView, "_animator", merchant.GetComponentInChildren<Animator>());
            SetString(idleAnimatorView, "_idleStateName", "Mage_Idle");

            ShopInteractable interactable = merchant.GetComponent<ShopInteractable>();
            if (interactable == null)
            {
                interactable = merchant.AddComponent<ShopInteractable>();
            }
            SetObject(interactable, "_shopPresenter", shopPresenter);
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new("ThirdPersonCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 54f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 200f;
            camera.transform.position = new Vector3(0f, 4.3f, 11.2f);
            camera.transform.rotation = Quaternion.Euler(24f, 180f, 0f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<ThirdPersonCameraFollow>();
            return camera;
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new("Sun Light");
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;

            GameObject shopLightObject = new("Shop Warm Light");
            shopLightObject.transform.position = new Vector3(0f, 4.2f, 0.4f);
            Light shopLight = shopLightObject.AddComponent<Light>();
            shopLight.type = LightType.Point;
            shopLight.color = new Color(1f, 0.75f, 0.45f);
            shopLight.intensity = 1.7f;
            shopLight.range = 10f;
        }

        private static ShopPresenter CreateShopCanvas(ShopItemButtonView itemRowPrefab, InventoryItemRowView inventoryRowPrefab)
        {
            GameObject canvasObject = new("ShopCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1600f, 900f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject promptObject = CreateText("PromptText", canvasObject.transform, "상인과 거래: E 또는 좌클릭", 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(620f, 44f)).gameObject;
            promptObject.SetActive(false);

            GameObject panel = CreatePanel("ShopPanel", canvasObject.transform, new Color(0.08f, 0.065f, 0.045f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(900f, 560f));
            ApplyImageSprite(panel, PanelFrameSpritePath, Image.Type.Sliced);
            GameObject titleFrame = CreatePanel("TitleFrame", panel.transform, new Color(0.33f, 0.18f, 0.08f, 0.72f), new Vector2(0.5f, 1f), new Vector2(790f, 50f));
            RectTransform titleFrameRect = titleFrame.GetComponent<RectTransform>();
            titleFrameRect.anchorMin = new Vector2(0.5f, 1f);
            titleFrameRect.anchorMax = new Vector2(0.5f, 1f);
            titleFrameRect.anchoredPosition = new Vector2(-24f, -34f);
            ApplyImageSprite(titleFrame, ListFrameSpritePath, Image.Type.Sliced);
            Text titleText = CreateText("TitleText", panel.transform, "신더킵 시장 상점", 29, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(4f, -34f), new Vector2(-116f, 50f));
            titleText.color = new Color(1f, 0.86f, 0.56f);

            CreateImage("GoldIcon", panel.transform, GoldIconSpritePath, new Color(1f, 0.82f, 0.3f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -78f), new Vector2(24f, 24f));
            CreateImage("CrystalIcon", panel.transform, CrystalIconSpritePath, new Color(0.48f, 0.9f, 1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(72f, -78f), new Vector2(24f, 24f));
            Text walletText = CreateText("WalletText", panel.transform, string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(44f, -78f), new Vector2(-150f, 32f));
            Text quantityText = CreateText("QuantityText", panel.transform, "수량: 1", 17, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-136f, -78f), new Vector2(210f, 32f));

            CreateText("ShopHeaderText", panel.transform, "상점 상품", 21, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(238f, -118f), new Vector2(390f, 30f));
            Transform itemRoot = CreateScrollContent("ItemScrollFrame", panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(236f, -290f), new Vector2(410f, 310f));

            Text inventoryText = CreateText("InventoryText", panel.transform, "내 인벤토리", 21, TextAnchor.MiddleLeft, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-224f, -118f), new Vector2(390f, 30f));
            Transform inventoryRoot = CreateScrollContent("InventoryScrollFrame", panel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-236f, -290f), new Vector2(410f, 310f));

            Text feedbackText = CreateText("FeedbackText", panel.transform, string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 74f), new Vector2(-92f, 34f));
            feedbackText.color = new Color(1f, 0.78f, 0.42f);
            GameObject dismantleDropZone = CreatePanel("DismantleDropZone", panel.transform, new Color(0.24f, 0.13f, 0.1f, 0.92f), new Vector2(0.5f, 0f), new Vector2(280f, 38f));
            ApplyImageSprite(dismantleDropZone, RowFrameSpritePath, Image.Type.Sliced);
            RectTransform dismantleDropZoneRect = dismantleDropZone.GetComponent<RectTransform>();
            dismantleDropZoneRect.anchorMin = new Vector2(0.5f, 0f);
            dismantleDropZoneRect.anchorMax = new Vector2(0.5f, 0f);
            dismantleDropZoneRect.anchoredPosition = new Vector2(0f, 36f);
            CreateText("DropZoneText", dismantleDropZone.transform, "드래그해서 분해", 16, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            dismantleDropZone.AddComponent<DismantleDropZone>();

            Button closeButton = CreateButton("CloseButton", panel.transform, "닫기", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-70f, -34f), new Vector2(96f, 36f));
            ShopQuantitySelector quantitySelector = panel.AddComponent<ShopQuantitySelector>();
            SetObject(quantitySelector, "_quantityText", quantityText);

            ShopPresenter presenter = canvasObject.AddComponent<ShopPresenter>();
            SetObject(presenter, "_rootObject", panel);
            SetObject(presenter, "_itemRoot", itemRoot);
            SetObject(presenter, "_inventoryRoot", inventoryRoot);
            SetObject(presenter, "_itemButtonPrefab", itemRowPrefab);
            SetObject(presenter, "_inventoryItemRowPrefab", inventoryRowPrefab);
            SetObject(presenter, "_walletText", walletText);
            SetObject(presenter, "_inventoryText", inventoryText);
            SetObject(presenter, "_feedbackText", feedbackText);
            SetObject(presenter, "_promptText", promptObject.GetComponent<Text>());
            SetObject(presenter, "_closeButton", closeButton);
            SetObject(presenter, "_quantitySelector", quantitySelector);

            panel.SetActive(false);
            CreateEventSystem();
            return presenter;
        }

        private static ShopItemButtonView CreateShopItemRowPrefab()
        {
            GameObject row = CreatePanel("ShopItemRow", null, new Color(0.18f, 0.145f, 0.1f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(386f, 76f));
            AddLayoutElement(row, 386f, 76f);
            ApplyImageSprite(row, RowFrameSpritePath, Image.Type.Sliced);
            Image iconImage = CreateImage("IconImage", row.transform, DefaultItemIconSpritePath, Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(42f, 0f), new Vector2(42f, 42f));
            Text nameText = CreateText("NameText", row.transform, "아이템", 17, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(92f, -18f), new Vector2(180f, 22f));
            Text descriptionText = CreateText("DescriptionText", row.transform, "설명", 12, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(176f, 21f), new Vector2(240f, 22f));
            Text priceText = CreateText("PriceText", row.transform, "0 골드", 14, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-116f, -20f), new Vector2(90f, 24f));
            Text stockText = CreateText("StockText", row.transform, "재고 0", 12, TextAnchor.MiddleRight, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-116f, 21f), new Vector2(90f, 22f));
            Button buyButton = CreateButton("BuyButton", row.transform, "구매", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(64f, 32f));

            ShopItemButtonView buttonView = row.AddComponent<ShopItemButtonView>();
            SetObject(buttonView, "_nameText", nameText);
            SetObject(buttonView, "_descriptionText", descriptionText);
            SetObject(buttonView, "_priceText", priceText);
            SetObject(buttonView, "_stockText", stockText);
            SetObject(buttonView, "_iconImage", iconImage);
            SetObject(buttonView, "_buyButton", buyButton);

            PrefabUtility.SaveAsPrefabAsset(row, RowPrefabPath);
            Object.DestroyImmediate(row);
            return Load<GameObject>(RowPrefabPath).GetComponent<ShopItemButtonView>();
        }

        private static InventoryItemRowView CreateInventoryItemRowPrefab()
        {
            GameObject row = CreatePanel("InventoryItemRow", null, new Color(0.17f, 0.135f, 0.095f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(386f, 66f));
            AddLayoutElement(row, 386f, 66f);
            ApplyImageSprite(row, RowFrameSpritePath, Image.Type.Sliced);
            Image iconImage = CreateImage("IconImage", row.transform, DefaultItemIconSpritePath, Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(38f, 38f));
            Text nameText = CreateText("NameText", row.transform, "아이템", 16, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(78f, 10f), new Vector2(132f, 22f));
            Text amountText = CreateText("AmountText", row.transform, "x0", 14, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(78f, -12f), new Vector2(62f, 20f));
            Text sellPriceText = CreateText("SellPriceText", row.transform, "0 골드", 13, TextAnchor.MiddleRight, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-158f, 0f), new Vector2(78f, 22f));
            Button sellButton = CreateButton("SellButton", row.transform, "판매", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-88f, 0f), new Vector2(54f, 30f));
            Button dismantleButton = CreateButton("DismantleButton", row.transform, "분해", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(54f, 30f));

            InventoryItemRowView rowView = row.AddComponent<InventoryItemRowView>();
            SetObject(rowView, "_nameText", nameText);
            SetObject(rowView, "_amountText", amountText);
            SetObject(rowView, "_sellPriceText", sellPriceText);
            SetObject(rowView, "_iconImage", iconImage);
            SetObject(rowView, "_sellButton", sellButton);
            SetObject(rowView, "_dismantleButton", dismantleButton);

            PrefabUtility.SaveAsPrefabAsset(row, InventoryRowPrefabPath);
            Object.DestroyImmediate(row);
            return Load<GameObject>(InventoryRowPrefabPath).GetComponent<InventoryItemRowView>();
        }

        private static WorldItemPickup CreateWorldItemPickupPrefab()
        {
            EnsureFolder("Assets/Prefabs/World", "Assets/Prefabs", "World");
            GameObject sourcePrefab = Load<GameObject>("Assets/PolygonFantasyKingdom/Prefabs/Items/SM_Item_Pouch_01.prefab");
            GameObject pickupObject = sourcePrefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            pickupObject.name = "WorldItemPickup";
            SphereCollider trigger = pickupObject.GetComponent<SphereCollider>();
            if (trigger == null)
            {
                trigger = pickupObject.AddComponent<SphereCollider>();
            }

            trigger.radius = 1f;
            trigger.isTrigger = true;

            WorldItemPickup pickup = pickupObject.GetComponent<WorldItemPickup>();
            if (pickup == null)
            {
                pickup = pickupObject.AddComponent<WorldItemPickup>();
            }

            SetString(pickup, "_itemId", "wolf_pelt");
            SetInt(pickup, "_amount", 1);
            PrefabUtility.SaveAsPrefabAsset(pickupObject, PickupPrefabPath);
            Object.DestroyImmediate(pickupObject);
            return Load<GameObject>(PickupPrefabPath).GetComponent<WorldItemPickup>();
        }

        private static void CreateStartingWorldPickup(WorldItemPickup pickupPrefab)
        {
            WorldItemPickup pickup = (WorldItemPickup)PrefabUtility.InstantiatePrefab(pickupPrefab);
            pickup.name = "WorldPickup_WolfPelt";
            pickup.transform.SetPositionAndRotation(new Vector3(1.9f, 0.35f, 4.15f), Quaternion.Euler(0f, 25f, 0f));
            pickup.Setup("wolf_pelt", 1);
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static GameObject PlacePrefab(string assetPath, string objectName, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, Material fallbackMaterial)
        {
            GameObject prefab = Load<GameObject>(assetPath);
            GameObject instance = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            instance.name = objectName;
            instance.transform.SetParent(parent);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.localScale = scale;

            if (fallbackMaterial != null)
            {
                ApplyFallbackMaterial(instance, fallbackMaterial);
            }

            return instance;
        }

        private static GameObject CreateGroundPlane(string name, Transform parent, Material material, Vector3 position, Vector3 scale, bool hasCollider)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = name;
            ground.transform.SetParent(parent);
            ground.transform.position = position;
            ground.transform.localScale = scale;

            Renderer renderer = ground.GetComponent<Renderer>();
            renderer.sharedMaterial = material;

            if (!hasCollider)
            {
                Collider collider = ground.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }

            return ground;
        }

        private static void ApplyFallbackMaterial(GameObject root, Material fallbackMaterial)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    renderer.sharedMaterial = fallbackMaterial;
                    continue;
                }

                bool hasChanged = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null || materials[i].name == "Default-Material")
                    {
                        materials[i] = fallbackMaterial;
                        hasChanged = true;
                    }
                }

                if (hasChanged)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        private static Material CreateMaterial(string materialName, Color color)
        {
            string path = $"Assets/Materials/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            material = new Material(shader)
            {
                color = color
            };

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateTexturedMaterial(string materialName, string texturePath, Color fallbackColor, Vector2 textureScale)
        {
            string path = $"Assets/Materials/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = fallbackColor;
            Texture2D texture = Load<Texture2D>(texturePath);
            if (texture != null)
            {
                material.mainTexture = texture;
                material.mainTextureScale = textureScale;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 pivot, Vector2 size)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = size;
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Image CreateImage(string name, Transform parent, string spritePath, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject imageObject = CreatePanel(name, parent, color, new Vector2(0.5f, 0.5f), sizeDelta);
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;

            Image image = imageObject.GetComponent<Image>();
            image.raycastTarget = false;
            ApplyImageSprite(imageObject, spritePath, Image.Type.Simple);
            return image;
        }

        private static Transform CreateScrollContent(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject frame = CreatePanel(name, parent, new Color(0.105f, 0.09f, 0.07f, 0.86f), new Vector2(0.5f, 0.5f), sizeDelta);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = anchorMin;
            frameRect.anchorMax = anchorMax;
            frameRect.anchoredPosition = anchoredPosition;
            ApplyImageSprite(frame, ListFrameSpritePath, Image.Type.Sliced);

            ScrollRect scrollRect = frame.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = CreatePanel("Viewport", frame.transform, new Color(0f, 0f, 0f, 0f), new Vector2(0.5f, 0.5f), Vector2.zero);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(10f, 10f);
            viewportRect.offsetMax = new Vector2(-10f, -10f);
            viewport.AddComponent<RectMask2D>();

            GameObject content = new("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            AddVerticalLayout(content, new RectOffset(2, 2, 2, 2), 8f);
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            return content.transform;
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;

            Text textComponent = textObject.GetComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            textComponent.fontSize = fontSize;
            textComponent.alignment = alignment;
            textComponent.color = Color.white;
            return textComponent;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject buttonObject = CreatePanel(name, parent, new Color(0.62f, 0.42f, 0.2f, 1f), new Vector2(0.5f, 0.5f), sizeDelta);
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            ApplyImageSprite(buttonObject, ButtonSpritePath, Image.Type.Sliced);
            ApplyButtonColors(button);
            Text labelText = CreateText("Label", buttonObject.transform, label, 17, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static void ApplyImageSprite(GameObject target, string spritePath, Image.Type imageType)
        {
            Image image = target.GetComponent<Image>();
            Sprite sprite = Load<Sprite>(spritePath);
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.type = imageType;
        }

        private static void ApplyButtonColors(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.82f, 0.58f, 0.25f, 1f);
            colors.highlightedColor = new Color(1f, 0.72f, 0.32f, 1f);
            colors.pressedColor = new Color(0.54f, 0.34f, 0.14f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.32f, 0.28f, 0.22f, 0.68f);
            button.colors = colors;
        }

        private static void AddVerticalLayout(GameObject target, RectOffset padding, float spacing)
        {
            VerticalLayoutGroup layoutGroup = target.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = spacing;
            layoutGroup.padding = padding;
        }

        private static void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
        {
            LayoutElement layoutElement = target.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minHeight = preferredHeight;
        }

        private static T Load<T>(string assetPath) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private static void EnsureFolder(string folderPath, string parentFolderPath, string folderName)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentFolderPath, folderName);
            }
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new(target);
            serializedObject.FindProperty(propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new(target);
            serializedObject.FindProperty(propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new(target);
            serializedObject.FindProperty(propertyName).intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
