using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace InGame.EditorTools
{
    /// <summary>Tools/BloodLuck/Style Shop View Prefab applies Gui_parts sprites and the intro Timeline to ShopView.prefab.</summary>
    public static class ShopViewPrefabStyler
    {
        private const string PrefabPath = "Assets/Prefabs/ShopView.prefab";
        private const string SpriteFolder = "Assets/GUI_Parts/Gui_parts/";

        private const float PanelFadeDuration = 0.35f;
        private const float PanelStartAlpha = 0.1f;
        private const float ItemFirstDelay = 0.2f;
        private const float ItemStagger = 0.12f;
        private const float ItemDuration = 0.35f;
        private const float SlideDistance = 60f;

        private static readonly string[] IntroItems =
        {
            "Title", "PullCounter", "UpgradeCard1", "UpgradeCard2", "UpgradeCard3", "BloodyDiceButton", "LeaveShopButton"
        };

        [MenuItem("Tools/BloodLuck/Style Shop View Prefab")]
        public static void Apply()
        {
            SetBorder("Frame_mid", 100);
            SetBorder("mid_background", 40);
            SetBorder("Mini_frame1", 60);
            SetBorder("Mini_background", 40);
            SetBorder("button", 60);
            SetBorder("button2", 80);

            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                StyleShopPanel(root.transform.Find("ShopPanel"));
                StyleDiceOverlay(root.transform.Find("DiceOverlay"));
                BuildIntro(root.transform.Find("ShopPanel").gameObject, IntroItems, "ShopIntro");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            Debug.Log("[ShopViewPrefabStyler] Saved " + PrefabPath);
        }

        private static void StyleShopPanel(Transform panel)
        {
            Slice(panel, "mid_background", 1f);
            AddFrame(panel, "ShopFrame", "Frame_mid", 2.5f, true);

            for (int i = 1; i <= 3; i++)
            {
                Transform card = panel.Find("UpgradeCard" + i);
                Slice(card, "Mini_background", 1f);
                Image frame = AddFrame(card, "CardFrame", "Mini_frame1", 1.5f, false);
                AddRarityTint(card);
                SetFrameHover(card.GetComponent<Button>(), frame,new Color(0.7f, 0.7f, 0.7f), new Color(1f, 0.85f, 0.5f), new Color(0.6f, 0.45f, 0.25f));
            }

            Transform dice = panel.Find("BloodyDiceButton");
            Slice(dice, "button2", 1.5f);
            SetTint(dice.GetComponent<Button>());

            StyleButton(panel.Find("LeaveShopButton"));
        }

        private static void StyleDiceOverlay(Transform overlay)
        {
            Slice(overlay.Find("GothicPanel"), "mid_background", 1f);
            AddFrame(overlay.Find("GothicPanel"), "GothicFrame", "Frame_mid", 2.5f, true);

            var board = (RectTransform)overlay.Find("RollBoard");
            var boardFrame = (RectTransform)overlay.Find("RollBoardFrame");
            Slice(board, "mid_background", 1f);
            board.sizeDelta = boardFrame.sizeDelta;
            Slice(boardFrame, "Frame_mid", 2f);
            boardFrame.GetComponent<Image>().raycastTarget = false;
            boardFrame.SetSiblingIndex(board.GetSiblingIndex() + 1);

            StyleButton(overlay.Find("RollButton"));
            StyleButton(overlay.Find("ReturnButton"));
        }

        private static void AddRarityTint(Transform card)
        {
            const float inset = 8f;
            Transform existing = card.Find("RarityTint");
            GameObject go = existing != null
                ? existing.gameObject
                : new GameObject("RarityTint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(card, false);
            go.transform.SetAsFirstSibling();

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
            go.GetComponent<LayoutElement>().ignoreLayout = true;

            var image = go.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = false;

            var view = card.GetComponent<BloodLuck.InfusionCardView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("rarityTint").objectReferenceValue = image;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void StyleButton(Transform button)
        {
            Slice(button, "button", 3f);
            SetTint(button.GetComponent<Button>());
        }

        internal static void SetTint(Button button)
        {
            button.targetGraphic = button.GetComponent<Image>();
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            // Vertex colors clamp at 1, so the resting state must be dimmed for hover to brighten.
            colors.normalColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        internal static void SetFrameHover(Button button, Image frame, Color normal, Color highlighted, Color pressed)
        {
            button.targetGraphic = frame;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = highlighted;
            colors.selectedColor = highlighted;
            colors.pressedColor = pressed;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        internal static void Slice(Transform target, string spriteName, float pixelsPerUnitMultiplier)
        {
            var image = target.GetComponent<Image>();
            image.sprite = LoadSprite(spriteName);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
        }

        internal static Image AddFrame(Transform parent, string name, string spriteName, float pixelsPerUnitMultiplier, bool behindContent)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<LayoutElement>().ignoreLayout = true;
            if (behindContent) rect.SetAsFirstSibling();

            var image = go.GetComponent<Image>();
            image.sprite = LoadSprite(spriteName);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            image.raycastTarget = false;
            return image;
        }

        internal static void BuildIntro(GameObject panel, string[] introItems, string introName)
        {
            string clipPath = "Assets/Prefabs/" + introName + ".anim";
            string timelinePath = "Assets/Prefabs/" + introName + ".playable";
            var group = GetOrAdd<CanvasGroup>(panel);
            group.interactable = false;
            var animator = GetOrAdd<Animator>(panel);
            var director = GetOrAdd<PlayableDirector>(panel);
            director.playOnAwake = true;
            director.extrapolationMode = DirectorWrapMode.Hold;
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

            var clip = new AnimationClip { name = introName, frameRate = 60 };
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(CanvasGroup), "m_Alpha"),
                AnimationCurve.EaseInOut(0, PanelStartAlpha, PanelFadeDuration, 1f));

            float end = PanelFadeDuration;
            for (int i = 0; i < introItems.Length; i++)
            {
                string path = introItems[i];
                Transform item = panel.transform.Find(path);
                GetOrAdd<CanvasGroup>(item.gameObject);
                float restY = ((RectTransform)item).anchoredPosition.y;
                float t0 = ItemFirstDelay + ItemStagger * i;
                float t1 = t0 + ItemDuration;
                end = Mathf.Max(end, t1);

                AnimationUtility.SetEditorCurve(clip,
                    EditorCurveBinding.FloatCurve(path, typeof(CanvasGroup), "m_Alpha"),
                    new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(t0, 0, 0, 0), new Keyframe(t1, 1f, 0, 0)));
                AnimationUtility.SetEditorCurve(clip,
                    EditorCurveBinding.FloatCurve(path, typeof(RectTransform), "m_AnchoredPosition.y"),
                    new AnimationCurve(new Keyframe(0, restY + SlideDistance, 0, 0), new Keyframe(t0, restY + SlideDistance, 0, 0), new Keyframe(t1, restY, 0, 0)));
            }

            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(CanvasGroup), "m_Interactable"),
                new AnimationCurve(
                    new Keyframe(0, 0, float.PositiveInfinity, float.PositiveInfinity),
                    new Keyframe(end, 1f, float.PositiveInfinity, float.PositiveInfinity)));

            AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);

            AssetDatabase.DeleteAsset(timelinePath);
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, timelinePath);
            var track = timeline.CreateTrack<AnimationTrack>(null, "Intro");
            TimelineClip timelineClip = track.CreateClip(clip);
            timelineClip.start = 0;
            // Margin keeps the final Interactable key off the clip boundary.
            timelineClip.duration = end + 0.05f;
            timelineClip.displayName = introName;
            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();

            director.playableAsset = timeline;
            director.SetGenericBinding(track, animator);
        }

        internal static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        internal static Sprite LoadSprite(string name) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");

        internal static void SetBorder(string name, float border)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SpriteFolder + name + ".png");
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
        }
    }
}
