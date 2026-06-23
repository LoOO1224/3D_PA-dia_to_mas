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

        private static readonly string[] BuildRootNames =
        {
            "GameManager",
            "Scene_Environment",
            "Player_Mage",
            "Merchant",
            "Main Camera",
            "ThirdPersonCamera",
            "ShopCanvas",
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
            ShopPresenter shopPresenter = CreateShopCanvas(itemRowPrefab, inventoryRowPrefab);
            CreateGameManager();
            CreateEnvironment(groundMaterial, fallbackMaterial, woodMaterial);

            Camera camera = CreateCamera();
            GameObject player = CreatePlayer(camera.transform);
            ThirdPersonCameraFollow cameraFollow = camera.GetComponent<ThirdPersonCameraFollow>();
            cameraFollow.SetTarget(player.transform);

            CreateMerchant(shopPresenter);
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

        private static void CreateGameManager()
        {
            GameObject gameManagerObject = new("GameManager");
            GameDataManager dataManager = gameManagerObject.AddComponent<GameDataManager>();
            GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

            SetObject(dataManager, "_currencyDataJson", Load<TextAsset>("Assets/Resources/GameData/currency_data.json"));
            SetObject(dataManager, "_shopItemDataJson", Load<TextAsset>("Assets/Resources/GameData/shop_item_data.json"));
            SetObject(dataManager, "_startingInventoryDataJson", Load<TextAsset>("Assets/Resources/GameData/starting_inventory_data.json"));
            SetObject(dataManager, "_playerMovementDataJson", Load<TextAsset>("Assets/Resources/GameData/player_movement_data.json"));
            SetObject(gameManager, "_gameDataManager", dataManager);
        }

        private static void CreateEnvironment(Material groundMaterial, Material fallbackMaterial, Material woodMaterial)
        {
            GameObject root = new("Scene_Environment");

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            GameObject shopRoot = new("Shop_Interior");
            shopRoot.transform.SetParent(root.transform);
            shopRoot.transform.position = Vector3.zero;

            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/House/SM_Bld_House_Floor_Wood_01.prefab", "Shop_Floor", new Vector3(0f, 0.02f, 0f), Quaternion.identity, Vector3.one * 1.25f, shopRoot.transform, woodMaterial);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/House/SM_Bld_House_Wall_Door_01.prefab", "Shop_Door_Wall", new Vector3(0f, 0f, 3.2f), Quaternion.Euler(0f, 180f, 0f), Vector3.one, shopRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/House/SM_Bld_House_Wall_01.prefab", "Shop_Back_Wall", new Vector3(0f, 0f, -3.2f), Quaternion.identity, Vector3.one, shopRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/House/SM_Bld_House_Wall_02.prefab", "Shop_Left_Wall", new Vector3(-3.2f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one, shopRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/House/SM_Bld_House_Wall_02.prefab", "Shop_Right_Wall", new Vector3(3.2f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), Vector3.one, shopRoot.transform, fallbackMaterial);

            PlacePrefab("Assets/ImportedAssets/VillageInteriorsKit/3DForge/Fantasy_Interiors/Villages_&_Towns/Prefabs/Props/Counters/fi_vil_counter01_01.prefab", "Merchant_Counter", new Vector3(0f, 0f, 1.1f), Quaternion.identity, Vector3.one, shopRoot.transform, woodMaterial);
            PlacePrefab("Assets/ImportedAssets/VillageInteriorsKit/3DForge/Fantasy_Interiors/Villages_&_Towns/Prefabs/Props/Library/Bookshelves/fi_vil_library_bookshelf_02a.prefab", "Shop_Bookshelf", new Vector3(-2.3f, 0f, -1.6f), Quaternion.Euler(0f, 90f, 0f), Vector3.one, shopRoot.transform, woodMaterial);
            PlacePrefab("Assets/ImportedAssets/VillageInteriorsKit/3DForge/Fantasy_Interiors/Villages_&_Towns/Prefabs/Props/Containers/Barrels/fi_vil_container_barrel_big_fruit.prefab", "Fruit_Barrel", new Vector3(2.1f, 0f, -1.4f), Quaternion.identity, Vector3.one, shopRoot.transform, woodMaterial);
            PlacePrefab("Assets/ImportedAssets/VillageInteriorsKit/3DForge/Fantasy_Interiors/Villages_&_Towns/Prefabs/Props/Containers/Sacks/fi_vil_container_sack03_grain.prefab", "Grain_Sack", new Vector3(2.4f, 0f, 0.2f), Quaternion.Euler(0f, -20f, 0f), Vector3.one, shopRoot.transform, woodMaterial);
            PlacePrefab("Assets/ImportedAssets/VillageInteriorsKit/3DForge/Fantasy_Interiors/Villages_&_Towns/Prefabs/Props/Food/fi_vil_food_bread03.prefab", "Counter_Bread", new Vector3(-0.7f, 1f, 1.2f), Quaternion.identity, Vector3.one, shopRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Items/SM_Item_Gem_04.prefab", "Counter_Crystal", new Vector3(0.9f, 1f, 1.2f), Quaternion.identity, Vector3.one * 0.8f, shopRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/ImportedAssets/VillageInteriorsKit/3DForge/Fantasy_Interiors/Villages_&_Towns/Prefabs/Props/Lighting/Hanging/fi_vil_light_lamp04_02.prefab", "Shop_Lamp", new Vector3(0f, 2.5f, -0.8f), Quaternion.identity, Vector3.one, shopRoot.transform, fallbackMaterial);

            GameObject villageRoot = new("Outside_Village");
            villageRoot.transform.SetParent(root.transform);
            PlacePrefab("Assets/ImportedAssets/PBMedievalVillages1/3DForge/Blueprints/PremiumBlueprints/PB_VEK/MedievalVillages1/BLUEPRINTS/MedVil_Shops/PB_MedVil_Shop01.prefab", "Village_Shop_01", new Vector3(-8f, 0f, 11f), Quaternion.Euler(0f, 35f, 0f), Vector3.one, villageRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/ImportedAssets/PBMedievalVillages1/3DForge/Blueprints/PremiumBlueprints/PB_VEK/MedievalVillages1/BLUEPRINTS/MedVil_Markets/PB_MedVil_Market01.prefab", "Village_Market_01", new Vector3(7f, 0f, 11f), Quaternion.Euler(0f, -25f, 0f), Vector3.one, villageRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Buildings/Preset_Houses/SM_Bld_Preset_Tavern_01_Optimized.prefab", "Village_Tavern", new Vector3(0f, 0f, 17f), Quaternion.Euler(0f, 180f, 0f), Vector3.one, villageRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Market_Stall_01.prefab", "Village_Stall_01", new Vector3(-2.5f, 0f, 7.3f), Quaternion.Euler(0f, 160f, 0f), Vector3.one, villageRoot.transform, fallbackMaterial);
            PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Props/SM_Prop_Market_Stall_04.prefab", "Village_Stall_02", new Vector3(2.5f, 0f, 7.1f), Quaternion.Euler(0f, 200f, 0f), Vector3.one, villageRoot.transform, fallbackMaterial);
        }

        private static GameObject CreatePlayer(Transform cameraTransform)
        {
            GameObject player = PlacePrefab("Assets/ImportedAssets/FantasyAnimatedCharacters/Fantasy animated characters pack/Mage animated character/Prefab/Mage.prefab", "Player_Mage", new Vector3(0f, 0.2f, -1.8f), Quaternion.identity, Vector3.one, null, null);
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
            SetObject(clickInteractor, "_camera", cameraTransform.GetComponent<Camera>());
            return player;
        }

        private static void CreateMerchant(ShopPresenter shopPresenter)
        {
            GameObject merchant = PlacePrefab("Assets/PolygonFantasyKingdom/Prefabs/Characters/SM_Chr_Merchant_01.prefab", "Merchant", new Vector3(0f, 0f, 0.25f), Quaternion.Euler(0f, 180f, 0f), Vector3.one, null, null);
            CapsuleCollider trigger = merchant.GetComponent<CapsuleCollider>();
            if (trigger == null)
            {
                trigger = merchant.AddComponent<CapsuleCollider>();
            }

            trigger.height = 2.2f;
            trigger.radius = 2.2f;
            trigger.center = Vector3.up;
            trigger.isTrigger = true;

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
            camera.fieldOfView = 58f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 200f;
            camera.transform.position = new Vector3(0f, 4.5f, -7f);
            camera.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
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
            shopLightObject.transform.position = new Vector3(0f, 3.2f, -0.5f);
            Light shopLight = shopLightObject.AddComponent<Light>();
            shopLight.type = LightType.Point;
            shopLight.color = new Color(1f, 0.75f, 0.45f);
            shopLight.intensity = 2.4f;
            shopLight.range = 8f;
        }

        private static ShopPresenter CreateShopCanvas(ShopItemButtonView itemRowPrefab, InventoryItemRowView inventoryRowPrefab)
        {
            GameObject canvasObject = new("ShopCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject promptObject = CreateText("PromptText", canvasObject.transform, "Press E or Left Click to trade", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(680f, 48f)).gameObject;
            promptObject.SetActive(false);

            GameObject panel = CreatePanel("ShopPanel", canvasObject.transform, new Color(0.08f, 0.065f, 0.045f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(980f, 620f));
            Text titleText = CreateText("TitleText", panel.transform, "Cinderkeep Merchant", 31, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -38f), new Vector2(-70f, 54f));
            titleText.color = new Color(1f, 0.86f, 0.56f);

            Text walletText = CreateText("WalletText", panel.transform, string.Empty, 19, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -88f), new Vector2(-70f, 34f));

            CreateText("ShopHeaderText", panel.transform, "Shop Stock", 22, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(258f, -128f), new Vector2(420f, 32f));
            Transform itemRoot = CreatePanel("ItemRoot", panel.transform, new Color(0.12f, 0.1f, 0.075f, 0.82f), new Vector2(0f, 1f), new Vector2(520f, 350f)).transform;
            RectTransform itemRootRect = itemRoot.GetComponent<RectTransform>();
            itemRootRect.anchorMin = new Vector2(0f, 1f);
            itemRootRect.anchorMax = new Vector2(0f, 1f);
            itemRootRect.anchoredPosition = new Vector2(40f, -168f);
            AddVerticalLayout(itemRoot.gameObject, new RectOffset(10, 10, 10, 10), 8f);

            Text inventoryText = CreateText("InventoryText", panel.transform, "Inventory: Empty", 22, TextAnchor.MiddleLeft, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-248f, -128f), new Vector2(390f, 32f));
            Transform inventoryRoot = CreatePanel("InventoryRoot", panel.transform, new Color(0.105f, 0.09f, 0.07f, 0.82f), new Vector2(1f, 1f), new Vector2(360f, 350f)).transform;
            RectTransform inventoryRootRect = inventoryRoot.GetComponent<RectTransform>();
            inventoryRootRect.anchorMin = new Vector2(1f, 1f);
            inventoryRootRect.anchorMax = new Vector2(1f, 1f);
            inventoryRootRect.anchoredPosition = new Vector2(-40f, -168f);
            AddVerticalLayout(inventoryRoot.gameObject, new RectOffset(10, 10, 10, 10), 8f);

            Text feedbackText = CreateText("FeedbackText", panel.transform, string.Empty, 20, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 54f), new Vector2(-70f, 36f));
            feedbackText.color = new Color(1f, 0.78f, 0.42f);

            Button sellButton = CreateButton("SellLootButton", panel.transform, "Sell Loot", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(126f, 24f), new Vector2(160f, 42f));
            Button closeButton = CreateButton("CloseButton", panel.transform, "Close", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-84f, -36f), new Vector2(120f, 42f));

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
            SetObject(presenter, "_sellLootButton", sellButton);

            panel.SetActive(false);
            CreateEventSystem();
            return presenter;
        }

        private static ShopItemButtonView CreateShopItemRowPrefab()
        {
            GameObject row = CreatePanel("ShopItemRow", null, new Color(0.18f, 0.145f, 0.1f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(500f, 78f));
            Text nameText = CreateText("NameText", row.transform, "Item", 18, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(112f, -18f), new Vector2(210f, 24f));
            Text descriptionText = CreateText("DescriptionText", row.transform, "Description", 13, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(204f, 22f), new Vector2(340f, 24f));
            Text priceText = CreateText("PriceText", row.transform, "0 Gold", 15, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-152f, -20f), new Vector2(120f, 26f));
            Text stockText = CreateText("StockText", row.transform, "Stock 0", 13, TextAnchor.MiddleRight, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-152f, 22f), new Vector2(120f, 22f));
            Button buyButton = CreateButton("BuyButton", row.transform, "Buy", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-48f, 0f), new Vector2(74f, 34f));

            ShopItemButtonView buttonView = row.AddComponent<ShopItemButtonView>();
            SetObject(buttonView, "_nameText", nameText);
            SetObject(buttonView, "_descriptionText", descriptionText);
            SetObject(buttonView, "_priceText", priceText);
            SetObject(buttonView, "_stockText", stockText);
            SetObject(buttonView, "_buyButton", buyButton);

            PrefabUtility.SaveAsPrefabAsset(row, RowPrefabPath);
            Object.DestroyImmediate(row);
            return Load<GameObject>(RowPrefabPath).GetComponent<ShopItemButtonView>();
        }

        private static InventoryItemRowView CreateInventoryItemRowPrefab()
        {
            GameObject row = CreatePanel("InventoryItemRow", null, new Color(0.17f, 0.135f, 0.095f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(340f, 58f));
            Text nameText = CreateText("NameText", row.transform, "Item", 17, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(86f, 10f), new Vector2(160f, 24f));
            Text amountText = CreateText("AmountText", row.transform, "x0", 15, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(86f, -12f), new Vector2(70f, 22f));
            Text sellPriceText = CreateText("SellPriceText", row.transform, "0 Gold", 14, TextAnchor.MiddleRight, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-108f, 0f), new Vector2(88f, 24f));
            Button sellButton = CreateButton("SellButton", row.transform, "Sell", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-38f, 0f), new Vector2(64f, 32f));

            InventoryItemRowView rowView = row.AddComponent<InventoryItemRowView>();
            SetObject(rowView, "_nameText", nameText);
            SetObject(rowView, "_amountText", amountText);
            SetObject(rowView, "_sellPriceText", sellPriceText);
            SetObject(rowView, "_sellButton", sellButton);

            PrefabUtility.SaveAsPrefabAsset(row, InventoryRowPrefabPath);
            Object.DestroyImmediate(row);
            return Load<GameObject>(InventoryRowPrefabPath).GetComponent<InventoryItemRowView>();
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

        private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 pivot, Vector2 size)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
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
            Text labelText = CreateText("Label", buttonObject.transform, label, 17, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static void AddVerticalLayout(GameObject target, RectOffset padding, float spacing)
        {
            VerticalLayoutGroup layoutGroup = target.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = spacing;
            layoutGroup.padding = padding;
        }

        private static T Load<T>(string assetPath) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
