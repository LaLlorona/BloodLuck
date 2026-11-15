using InGame.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace InGame.EditorTools
{
    /// <summary>Tools/BloodLuck/Build Perk Selection Prefab 메뉴로 Perk 선택창 Prefab을 생성한다.</summary>
    public static class PerkSelectionPrefabBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/PerkSelectionUI.prefab";

        private static readonly Color Gold = new Color(0.85f, 0.68f, 0.2f, 1f);
        private static readonly Color DarkGold = new Color(0.45f, 0.35f, 0.1f, 1f);
        private static readonly Color Purple = new Color(0.27f, 0.12f, 0.45f, 1f);
        private static readonly Color CardColor = new Color(0.08f, 0.05f, 0.12f, 0.95f);

        [MenuItem("Tools/BloodLuck/Build Perk Selection Prefab")]
        public static void Build()
        {
            Sprite box = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            var root = new GameObject("PerkSelectionUI", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(PerkSelectionView));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // 전체 화면 딤
            RectTransform dim = Img("Dim", root.transform, new Color(0, 0, 0, 0.6f), null);
            Stretch(dim);

            // 금색 테두리 + 보라색 패널
            RectTransform frame = Img("Frame", root.transform, Gold, box, true);
            Center(frame, new Vector2(1120, 820));
            RectTransform panel = Img("Panel", frame, Purple, box, true);
            Stretch(panel, 8);

            RectTransform title = Txt("Title", panel, "CHOOSE A PERK", 52, Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            title.anchorMin = new Vector2(0, 1);
            title.anchorMax = new Vector2(1, 1);
            title.pivot = new Vector2(0.5f, 1);
            title.anchoredPosition = new Vector2(0, -30);
            title.sizeDelta = new Vector2(0, 80);

            var cardsRoot = new GameObject("Cards", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            cardsRoot.SetParent(panel, false);
            cardsRoot.anchorMin = Vector2.zero;
            cardsRoot.anchorMax = Vector2.one;
            cardsRoot.offsetMin = new Vector2(60, 50);
            cardsRoot.offsetMax = new Vector2(-60, -130);
            var v = cardsRoot.GetComponent<VerticalLayoutGroup>();
            v.spacing = 20;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = true;

            // 슬롯(레이아웃이 위치를 관리) 안에 카드를 두어, 카드는 Timeline으로 자유롭게 움직일 수 있게 한다.
            var cards = new PerkCardView[3];
            for (int i = 0; i < 3; i++)
            {
                RectTransform slot = new GameObject("Slot" + (i + 1), typeof(RectTransform), typeof(LayoutElement))
                    .GetComponent<RectTransform>();
                slot.SetParent(cardsRoot, false);
                slot.GetComponent<LayoutElement>().preferredHeight = 180;
                cards[i] = BuildCard(slot, i, box);
            }

            // 등장 연출 Timeline
            root.AddComponent<Animator>();
            var director = root.AddComponent<PlayableDirector>();
            director.playOnAwake = true; // 창이 활성화되면 연출 자동 재생
            director.extrapolationMode = DirectorWrapMode.Hold; // 끝난 뒤 마지막 상태 유지
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime; // timeScale 0(일시정지)에서도 재생
            root.GetComponent<CanvasGroup>().interactable = false; // 연출 끝에서 Timeline이 켠다
            BuildIntroTimeline(director, root.GetComponent<Animator>());

            // 참조 연결
            var view = root.GetComponent<PerkSelectionView>();
            var so = new SerializedObject(view);
            so.FindProperty("_introDirector").objectReferenceValue = director;
            var arr = so.FindProperty("_cards");
            arr.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            global::System.IO.Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log("[PerkSelectionPrefabBuilder] Saved " + PrefabPath);
        }

        private const string TimelinePath = "Assets/Prefabs/PerkSelectionIntro.playable";
        private const string ClipPath = "Assets/Prefabs/PerkSelectionIntro.anim";

        // 연출 타이밍(초)
        private const float RootFadeDuration = 0.4f;
        private const float RootStartAlpha = 0.1f;
        private const float CardFirstDelay = 0.3f;
        private const float CardStagger = 0.15f;
        private const float CardDuration = 0.35f;
        private const float CardSlideDistance = 60f;

        private static void BuildIntroTimeline(PlayableDirector director, Animator animator)
        {
            // 1) 연출 AnimationClip: 전체 알파 + 카드별 알파/슬라이드
            var clip = new AnimationClip { name = "PerkSelectionIntro" };
            clip.frameRate = 60;

            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(CanvasGroup), "m_Alpha"),
                AnimationCurve.EaseInOut(0, RootStartAlpha, RootFadeDuration, 1f));

            float end = RootFadeDuration;
            for (int i = 0; i < 3; i++)
            {
                string path = "Frame/Panel/Cards/Slot" + (i + 1) + "/Card" + (i + 1);
                float t0 = CardFirstDelay + CardStagger * i;
                float t1 = t0 + CardDuration;
                end = Mathf.Max(end, t1);

                // t0 이전에는 숨김 유지, t0~t1에 나타남
                // 탄젠트를 0으로 고정해 오버슈트 없이 ease-in-out
                var alpha = new AnimationCurve(
                    new Keyframe(0, 0, 0, 0), new Keyframe(t0, 0, 0, 0), new Keyframe(t1, 1f, 0, 0));
                AnimationUtility.SetEditorCurve(clip,
                    EditorCurveBinding.FloatCurve(path, typeof(CanvasGroup), "m_Alpha"), alpha);

                var slide = new AnimationCurve(
                    new Keyframe(0, CardSlideDistance, 0, 0), new Keyframe(t0, CardSlideDistance, 0, 0), new Keyframe(t1, 0f, 0, 0));
                AnimationUtility.SetEditorCurve(clip,
                    EditorCurveBinding.FloatCurve(path, typeof(RectTransform), "m_AnchoredPosition.y"), slide);
            }

            // 연출이 끝나는 순간 클릭 가능(Interactable) - 무한 탄젠트로 계단형(0→1)
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(CanvasGroup), "m_Interactable"),
                new AnimationCurve(
                    new Keyframe(0, 0, float.PositiveInfinity, float.PositiveInfinity),
                    new Keyframe(end, 1f, float.PositiveInfinity, float.PositiveInfinity)));

            AssetDatabase.DeleteAsset(ClipPath);
            AssetDatabase.CreateAsset(clip, ClipPath);

            // 2) TimelineAsset: Animation Track 1개(루트 Animator에 바인딩)에 클립 배치
            AssetDatabase.DeleteAsset(TimelinePath);
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, TimelinePath);

            var track = timeline.CreateTrack<AnimationTrack>(null, "Intro");
            TimelineClip tClip = track.CreateClip(clip);
            tClip.start = 0;
            tClip.duration = end + 0.05f; // 마지막 키(Interactable)가 클립 경계에 걸리지 않도록 여유를 둔다
            tClip.displayName = "PerkSelectionIntro";

            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();

            director.playableAsset = timeline;
            director.SetGenericBinding(track, animator);
        }

        private static PerkCardView BuildCard(RectTransform parent, int index, Sprite box)
        {
            RectTransform card = Img("Card" + (index + 1), parent, CardColor, box, true);
            Stretch(card);
            card.gameObject.AddComponent<CanvasGroup>(); // 카드별 알파 연출용

            var outline = card.gameObject.AddComponent<Outline>();
            outline.effectColor = DarkGold;
            outline.effectDistance = new Vector2(3, -3);

            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card.GetComponent<Image>();
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.8f, 1.6f, 2.4f, 1f);
            cb.selectedColor = new Color(1.8f, 1.6f, 2.4f, 1f);
            cb.pressedColor = new Color(2.6f, 2.2f, 3.2f, 1f);
            cb.disabledColor = Color.white; // 페이드 중 흐려 보이는 건 CanvasGroup 알파가 담당
            cb.fadeDuration = 0.1f;
            button.colors = cb;

            var h = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(30, 30, 20, 20);
            h.spacing = 30;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            // 왼쪽: 스프라이트
            RectTransform icon = Img("Icon", card, Gold, box);
            var iconLe = icon.gameObject.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 130;
            iconLe.preferredHeight = 130;
            iconLe.minWidth = 130;
            iconLe.minHeight = 130;
            var iconImg = icon.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // 오른쪽: 텍스트
            var texts = new GameObject("Texts", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement))
                .GetComponent<RectTransform>();
            texts.SetParent(card, false);
            texts.GetComponent<LayoutElement>().flexibleWidth = 1;
            var tv = texts.GetComponent<VerticalLayoutGroup>();
            tv.spacing = 8;
            tv.childAlignment = TextAnchor.MiddleLeft;
            tv.childControlWidth = true;
            tv.childControlHeight = true;
            tv.childForceExpandWidth = true;
            tv.childForceExpandHeight = false;

            RectTransform t = Txt("Title", texts, "Perk Title", 40, Gold, TextAlignmentOptions.Left, FontStyles.Bold);
            RectTransform d = Txt("Description", texts, "Perk description goes here.", 30, Color.white, TextAlignmentOptions.TopLeft, FontStyles.Normal);

            var view = card.gameObject.AddComponent<PerkCardView>();
            var so = new SerializedObject(view);
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_icon").objectReferenceValue = iconImg;
            so.FindProperty("_title").objectReferenceValue = t.GetComponent<TMP_Text>();
            so.FindProperty("_description").objectReferenceValue = d.GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static RectTransform Img(string name, Transform parent, Color color, Sprite sprite, bool sliced = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            if (sliced && sprite != null) img.type = Image.Type.Sliced;
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform Txt(string name, Transform parent, string text, float size, Color color,
            TextAlignmentOptions align, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.raycastTarget = false;
            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rt, float inset = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        private static void Center(RectTransform rt, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
        }
    }
}
