using System.Collections.Generic;
using BloodLuck;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.EditorTools
{
    /// <summary>Builds the intentionally small playable shop prototype in the currently open scene.</summary>
    public static class ShopPrototypeSetup
    {
        private const string InfusionFolder = "Assets/Data/Infusions";
        private static readonly Color PanelColor = new Color(0.10f, 0.035f, 0.12f, 0.97f);
        private static readonly Color CardColor = new Color(0.22f, 0.06f, 0.18f, 1f);
        private static readonly Color Gold = new Color(0.92f, 0.68f, 0.21f, 1f);

        [MenuItem("Tools/BloodLuck/Build Shop Prototype")]
        public static void Build()
        {
            var canvas = FindOrCreateCanvas();
            DeleteChild(canvas.transform, "ShopPanel");
            var infusions = CreateSampleInfusions();

            var system = GameObject.Find("ShopSystem") ?? new GameObject("ShopSystem");
            var wallet = system.GetComponent<SlotPullWallet>() ?? system.AddComponent<SlotPullWallet>();
            var shop = system.GetComponent<BloodLuckShop>() ?? system.AddComponent<BloodLuckShop>();
            var diceRoll = system.GetComponent<BloodyDiceRoll>() ?? system.AddComponent<BloodyDiceRoll>();

            var panel = CreatePanel(canvas.transform, "ShopPanel", new Vector2(1680, 730));
            var title = Text(panel.transform, "Title", "BLOOD MARKET", 48, Gold);
            Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -48), new Vector2(700, 70));
            var pullLabel = Text(panel.transform, "PullCounter", "PULLS: 6", 28, Color.white);
            Place(pullLabel.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-105, -48), new Vector2(220, 55));
            var walletView = pullLabel.gameObject.AddComponent<ShopWalletView>();
            Set(walletView, "label", pullLabel);

            var cards = new Button[3];
            var cardViews = new InfusionCardView[3];
            for (int i = 0; i < 3; i++)
            {
                var card = CreateCard(panel.transform, i, -600 + i * 400, out cardViews[i]);
                cards[i] = card.GetComponent<Button>();
            }
            var diceButton = CreateDiceCard(panel.transform, 600);
            var leaveButton = CreateButton(panel.transform, "LeaveShopButton", "LEAVE SHOP", new Color(.38f, .13f, .25f, 1));
            Place(leaveButton.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-105, 58), new Vector2(190, 62));

            var diceRoot = GameObject.Find("DiceRollRoot");
            var dice = diceRoot != null && diceRoot.transform.childCount > 0 ? diceRoot.transform.GetChild(0) : null;
            ConfigureDiceRendering(diceRoot);
            // The main gameplay camera owns audio. DiceCamera is render-texture-only.
            var diceCamera = GameObject.Find("DiceCamera");
            if (diceCamera != null)
            {
                var listener = diceCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }
            var overlay = CreateDiceOverlay(canvas.transform, out var diceDisplay, out var rollButton, out var returnButton);
            TMP_Text rewardLabel = null;
            if (diceDisplay != null)
            {
                rewardLabel = diceDisplay.GetComponentInChildren<TMP_Text>(true);
                if (rewardLabel == null)
                {
                    rewardLabel = Text(diceDisplay.transform, "RewardLabel", "", 25, Gold);
                    Place(rewardLabel.rectTransform, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, -35), new Vector2(500, 50));
                }
            }
            Set(shop, "shopPanel", panel);
            Set(shop, "cardButtons", cards);
            Set(shop, "diceButton", diceButton.GetComponent<Button>());
            Set(shop, "leaveButton", leaveButton.GetComponent<Button>());
            Set(shop, "cardViews", cardViews);
            Set(shop, "walletView", walletView);
            Set(shop, "diceRoll", diceRoll);
            Set(shop, "diceOverlay", overlay);
            Set(shop, "diceRollButton", rollButton);
            Set(shop, "diceReturnButton", returnButton);
            Set(shop, "wallet", wallet);
            Set(shop, "infusionPool", infusions);
            Set(diceRoll, "rollDisplay", diceDisplay);
            Set(diceRoll, "dice", dice);
            Set(diceRoll, "rewardLabel", rewardLabel);

            var opener = Object.FindFirstObjectByType<ShopOpen>();
            if (opener == null) opener = system.AddComponent<ShopOpen>();
            Set(opener, "shop", shop);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Selection.activeGameObject = system;
            Debug.Log("[ShopPrototypeSetup] Shop UI and dice references are ready.");
        }

        private static List<InfusionDefinition> CreateSampleInfusions()
        {
            EnsureFolder("Assets/Data"); EnsureFolder(InfusionFolder);
            return new List<InfusionDefinition>
            {
                Infusion("Vitality", "Increase maximum health. (Test infusion)", InfusionRarity.Bloodstained),
                Infusion("Rending Claws", "Modify the three-hit combo. (Test infusion)", InfusionRarity.Bloodbound),
                Infusion("Crimson Nova", "A game-changing burst of blood magic. (Test infusion)", InfusionRarity.Bloodwrought),
                Infusion("Nightfall", "Alter the battlefield with a permanent aura. (Test infusion)", InfusionRarity.Bloodwrought),
                Infusion("Blood Engine", "Turn every attack into a devastating ritual. (Test infusion)", InfusionRarity.Bloodwrought)
            };
        }

        private static InfusionDefinition Infusion(string displayName, string description, InfusionRarity rarity)
        {
            string path = InfusionFolder + "/" + displayName.Replace(" ", "") + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<InfusionDefinition>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<InfusionDefinition>(); AssetDatabase.CreateAsset(asset, path); }
            asset.displayName = displayName; asset.description = description; asset.rarity = rarity;
            EditorUtility.SetDirty(asset); return asset;
        }

        private static Canvas FindOrCreateCanvas()
        {
            var existing = Object.FindFirstObjectByType<Canvas>();
            if (existing != null) return existing;
            var go = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            return go.GetComponent<Canvas>();
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = PanelColor; Place(go.GetComponent<RectTransform>(), new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, size); return go;
        }

        private static GameObject CreateCard(Transform parent, int index, float xPosition, out InfusionCardView view)
        {
            var go = new GameObject("UpgradeCard" + (index + 1), typeof(RectTransform), typeof(Image), typeof(Button), typeof(InfusionCardView)); go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = CardColor; Place(go.GetComponent<RectTransform>(), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(xPosition, 30), new Vector2(370, 390));
            var title = Text(go.transform, "Name", "", 29, Gold); Place(title.rectTransform, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-40), new Vector2(350,45));
            var rarity = Text(go.transform, "Rarity", "", 20, Color.white); Place(rarity.rectTransform, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-85), new Vector2(350,35));
            var description = Text(go.transform, "Description", "", 19, Color.white); description.alignment = TextAlignmentOptions.TopLeft; description.textWrappingMode = TextWrappingModes.Normal; Place(description.rectTransform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,-20), new Vector2(330,150));
            var cost = Text(go.transform, "Cost", "", 24, Gold); Place(cost.rectTransform, new Vector2(.5f,0), new Vector2(.5f,0), new Vector2(0,35), new Vector2(330,45));
            view = go.GetComponent<InfusionCardView>(); Set(view,"title",title); Set(view,"rarity",rarity); Set(view,"description",description); Set(view,"cost",cost); Set(view,"contents",go); return go;
        }

        private static GameObject CreateDiceCard(Transform parent, float xPosition)
        {
            var go = new GameObject("BloodyDiceButton", typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(.46f, .08f, .07f, 1f);
            Place(go.GetComponent<RectTransform>(), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(xPosition,30), new Vector2(370,390));
            var title = Text(go.transform, "Name", "BLOODY DICE", 29, Gold); Place(title.rectTransform, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-40), new Vector2(350,45));
            var subtitle = Text(go.transform, "Special", "GUARANTEED BLOODWROUGHT", 18, new Color(1f,.72f,.55f)); Place(subtitle.rectTransform, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-85), new Vector2(350,35));
            var description = Text(go.transform, "Description", "Roll for a random guaranteed Bloodwrought infusion.", 19, Color.white); description.textWrappingMode = TextWrappingModes.Normal; Place(description.rectTransform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,-20), new Vector2(330,150));
            var cost = Text(go.transform, "Cost", "2 PULLS", 24, Gold); Place(cost.rectTransform, new Vector2(.5f,0), new Vector2(.5f,0), new Vector2(0,35), new Vector2(330,45));
            return go;
        }

        private static GameObject CreateDiceOverlay(Transform canvas, out GameObject diceDisplay, out Button rollButton, out Button returnButton)
        {
            var existingDisplay = FindOrCreateDiceDisplay(canvas);
            if (existingDisplay.transform.parent != canvas) existingDisplay.transform.SetParent(canvas, false);
            DeleteChild(canvas, "DiceOverlay");
            var overlay = new GameObject("DiceOverlay", typeof(RectTransform), typeof(Image)); overlay.transform.SetParent(canvas, false);
            overlay.GetComponent<Image>().color = new Color(.015f, .008f, .02f, 1f);
            Place(overlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // A simple opaque panel and board keep the render-texture dice legible without a new 3D scene.
            var panel = new GameObject("GothicPanel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(overlay.transform, false);
            panel.GetComponent<Image>().color = new Color(.09f, .025f, .075f, 1f);
            Place(panel.GetComponent<RectTransform>(), new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(1200, 820));
            var boardFrame = new GameObject("RollBoardFrame", typeof(RectTransform), typeof(Image)); boardFrame.transform.SetParent(overlay.transform, false);
            boardFrame.GetComponent<Image>().color = new Color(.32f, .18f, .08f, 1f);
            Place(boardFrame.GetComponent<RectTransform>(), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,100), new Vector2(700, 430));
            var board = new GameObject("RollBoard", typeof(RectTransform), typeof(Image)); board.transform.SetParent(overlay.transform, false);
            board.GetComponent<Image>().color = new Color(.105f, .055f, .035f, 1f);
            Place(board.GetComponent<RectTransform>(), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,100), new Vector2(660, 390));
            diceDisplay = existingDisplay;
            diceDisplay.transform.SetParent(overlay.transform, false);
            Place(diceDisplay.GetComponent<RectTransform>(), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,100), new Vector2(460,460));
            diceDisplay.SetActive(false);
            var heading = Text(overlay.transform, "Heading", "BLOODY DICE", 44, Gold); Place(heading.rectTransform, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-80), new Vector2(600,60));
            var instruction = Text(overlay.transform, "Instruction", "Your Bloodwrought reward has been chosen. Roll to reveal it.", 24, Color.white); Place(instruction.rectTransform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,-210), new Vector2(900,50));
            var roll = CreateButton(overlay.transform, "RollButton", "ROLL", Gold); Place(roll.GetComponent<RectTransform>(), new Vector2(.5f,0), new Vector2(.5f,0), new Vector2(-150,100), new Vector2(240,65));
            var back = CreateButton(overlay.transform, "ReturnButton", "RETURN TO SHOP", new Color(.38f,.13f,.25f,1)); Place(back.GetComponent<RectTransform>(), new Vector2(.5f,0), new Vector2(.5f,0), new Vector2(150,100), new Vector2(240,65));
            overlay.SetActive(false); rollButton = roll.GetComponent<Button>(); returnButton = back.GetComponent<Button>(); return overlay;
        }

        private static void ConfigureDiceRendering(GameObject diceRoot)
        {
            const int diceLayer = 6; // DiceUI
            if (diceRoot == null) return;

            // DiceCamera looks at world origin. Keeping the roll root there makes the camera and animation agree.
            diceRoot.transform.localPosition = Vector3.zero;
            SetLayerRecursively(diceRoot, diceLayer);

            var diceCameraObject = GameObject.Find("DiceCamera");
            var diceCamera = diceCameraObject != null ? diceCameraObject.GetComponent<Camera>() : null;
            if (diceCamera != null)
            {
                diceCamera.cullingMask = 1 << diceLayer;
                diceCamera.targetTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/Art/Dice/DiceRenderTExture.renderTexture");
            }

            var mainCamera = Camera.main;
            if (mainCamera != null) mainCamera.cullingMask &= ~(1 << diceLayer);
        }

        private static GameObject FindOrCreateDiceDisplay(Transform canvas)
        {
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (transform.name == "DiceRollDisplay" && transform.gameObject.scene == EditorSceneManager.GetActiveScene())
                    return transform.gameObject;

            var display = new GameObject("DiceRollDisplay", typeof(RectTransform), typeof(RawImage));
            display.transform.SetParent(canvas, false);
            var image = display.GetComponent<RawImage>();
            image.texture = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/Art/Dice/DiceRenderTExture.renderTexture");
            image.raycastTarget = false;
            return display;
        }

        private static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform) SetLayerRecursively(child.gameObject, layer);
        }

        private static GameObject CreateButton(Transform parent, string name, string label, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false); go.GetComponent<Image>().color = color;
            var text = Text(go.transform, "Text", label, 22, Color.black); text.alignment = TextAlignmentOptions.Center; Place(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); return go;
        }
        private static TextMeshProUGUI Text(Transform parent, string name, string content, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false); var text = go.GetComponent<TextMeshProUGUI>(); text.font = TMP_Settings.defaultFontAsset; text.text = content; text.fontSize = size; text.color = color; text.alignment = TextAlignmentOptions.Center; return text;
        }
        private static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size) { rect.anchorMin=min; rect.anchorMax=max; rect.anchoredPosition=position; rect.sizeDelta=size; }
        private static void DeleteChild(Transform parent, string name) { var child=parent.Find(name); if(child!=null) Object.DestroyImmediate(child.gameObject); }
        private static void EnsureFolder(string folder) { if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(global::System.IO.Path.GetDirectoryName(folder).Replace('\\','/'), global::System.IO.Path.GetFileName(folder)); }
        private static void Set(Object target, string field, Object value) { var so=new SerializedObject(target); so.FindProperty(field).objectReferenceValue=value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Set<T>(Object target, string field, T[] values) where T : Object { var so=new SerializedObject(target); var p=so.FindProperty(field); p.arraySize=values.Length; for(int i=0;i<values.Length;i++) p.GetArrayElementAtIndex(i).objectReferenceValue=values[i]; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Set(Object target, string field, List<InfusionDefinition> values) { var so=new SerializedObject(target); var p=so.FindProperty(field); p.arraySize=values.Count; for(int i=0;i<values.Count;i++) p.GetArrayElementAtIndex(i).objectReferenceValue=values[i]; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
