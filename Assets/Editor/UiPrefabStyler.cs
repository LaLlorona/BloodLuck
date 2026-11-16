using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.EditorTools
{
    /// <summary>Tools/BloodLuck/Style UI Prefabs applies the Gui_parts look and gothic fonts to the in-game UI prefabs.</summary>
    public static class UiPrefabStyler
    {
        private const string HeadingFontPath = "Assets/Fonts/Cinzel SDF.asset";
        private const string BodyFontPath = "Assets/Fonts/CormorantGaramond SDF.asset";

        private static readonly Color Cream = new Color(0.85f, 0.80f, 0.69f, 1f);
        private static readonly Color BodyCream = new Color(0.80f, 0.76f, 0.67f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.57f, 0.47f, 1f);
        private static readonly Color Gold = new Color(0.92f, 0.68f, 0.21f, 1f);

        private static readonly Color CardRest = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color CardHover = new Color(1f, 0.85f, 0.5f, 1f);
        private static readonly Color CardPressed = new Color(0.6f, 0.45f, 0.25f, 1f);

        private static readonly string[] SlotMachineIntroItems =
        {
            "Panel/TitlePlaque", "Panel/Reel1", "Panel/Reel2", "Panel/Reel3", "Panel/Payline",
            "Panel/PullsBox", "Panel/ResultBox", "Panel/SpinButton"
        };

        private static TMP_FontAsset _heading;
        private static TMP_FontAsset _body;

        [MenuItem("Tools/BloodLuck/Style UI Prefabs")]
        public static void ApplyAll()
        {
            ShopViewPrefabStyler.SetBorder("Frame_mid", 100);
            ShopViewPrefabStyler.SetBorder("mid_background", 40);
            ShopViewPrefabStyler.SetBorder("Mini_frame1", 60);
            ShopViewPrefabStyler.SetBorder("Mini_background", 40);
            ShopViewPrefabStyler.SetBorder("button", 60);

            _heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HeadingFontPath);
            _body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFontPath);

            Edit("Assets/Prefabs/PerkSelectionUI.prefab", StylePerkSelection);
            Edit("Assets/Prefabs/HudView.prefab", StyleHud);
            Edit("Assets/Prefabs/GameResultUI.prefab", StyleGameResult);
            Edit("Assets/Prefabs/SlotMachineUI.prefab", StyleSlotMachine);
            Debug.Log("[UiPrefabStyler] Styled PerkSelectionUI, HudView, GameResultUI, SlotMachineUI");
        }

        private static void Edit(string path, global::System.Action<Transform> style)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                style(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void StylePerkSelection(Transform root)
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                Image frame = button.transform.Find("CardFrame").GetComponent<Image>();
                ShopViewPrefabStyler.SetFrameHover(button, frame, CardRest, CardHover, CardPressed);
            }

            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name == "Description") StyleBody(text);
                else StyleHeading(text, Cream);
            }
        }

        private static void StyleHud(Transform root)
        {
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                StyleHeading(text, Cream);
        }

        private static void StyleGameResult(Transform root)
        {
            Transform panel = root.Find("Panel");
            ShopViewPrefabStyler.Slice(panel, "mid_background", 1f);
            ShopViewPrefabStyler.AddFrame(panel, "PanelFrame", "Frame_mid", 2.5f, true);
            panel.Find("Inner").GetComponent<Image>().enabled = false;

            ShopViewPrefabStyler.Slice(panel.Find("VictoryRoot"), "button", 2f);
            panel.Find("VictoryRoot").GetComponent<Image>().color = new Color(1f, 0.82f, 0.4f, 1f);
            ShopViewPrefabStyler.Slice(panel.Find("DefeatRoot"), "button", 2f);

            StyleHeading(panel.Find("TitleText").GetComponent<TMP_Text>(), Cream);
            foreach (string label in new[] { "WaveLabel", "PullsLabel" })
                StyleHeading(panel.Find(label).GetComponent<TMP_Text>(), Gold);
            foreach (string value in new[] { "WaveText", "PullsText" })
                StyleHeading(panel.Find(value).GetComponent<TMP_Text>(), Cream);

            StyleTextButton(panel.Find("RestartButton"));
        }

        private static void StyleSlotMachine(Transform root)
        {
            Transform panel = root.Find("Panel");
            ShopViewPrefabStyler.Slice(panel, "mid_background", 1f);
            ShopViewPrefabStyler.AddFrame(panel, "PanelFrame", "Frame_mid", 2.5f, true);
            panel.Find("Inner").GetComponent<Image>().enabled = false;

            for (int i = 1; i <= 3; i++)
            {
                Transform reel = panel.Find("Reel" + i);
                ShopViewPrefabStyler.Slice(reel, "Mini_frame1", 4f);
                ShopViewPrefabStyler.Slice(reel.Find("Window"), "Mini_background", 4f);
            }

            Transform plaque = panel.Find("TitlePlaque");
            ShopViewPrefabStyler.Slice(plaque, "button", 3f);
            StyleHeading(plaque.Find("Text").GetComponent<TMP_Text>(), Cream);

            StyleInfoBox(panel.Find("PullsBox"), 4f);
            StyleInfoBox(panel.Find("ResultBox"), 4f);
            StyleHeading(panel.Find("PullsBox/PullsText").GetComponent<TMP_Text>(), Cream);
            StyleHeading(panel.Find("ResultBox/ResultText").GetComponent<TMP_Text>(), Gold);

            StyleTextButton(panel.Find("SpinButton"));

            ShopViewPrefabStyler.BuildIntro(root.gameObject, SlotMachineIntroItems, "SlotMachineIntro");
        }

        private static void StyleInfoBox(Transform box, float headMultiplier)
        {
            ShopViewPrefabStyler.Slice(box, "Mini_background", 3f);
            ShopViewPrefabStyler.AddFrame(box, "BoxFrame", "Mini_frame1", 3f, false);
            Transform head = box.Find("Head");
            ShopViewPrefabStyler.Slice(head, "button", headMultiplier);
            head.SetAsLastSibling();
            StyleHeading(head.Find("Text").GetComponent<TMP_Text>(), Cream);
        }

        private static void StyleTextButton(Transform button)
        {
            ShopViewPrefabStyler.StyleButton(button);
            Transform outline = button.Find("Outline");
            if (outline != null) outline.GetComponent<Image>().enabled = false;
            StyleHeading(button.Find("Text").GetComponent<TMP_Text>(), Cream);
        }

        private static void StyleHeading(TMP_Text text, Color color)
        {
            float size = text.fontSize;
            text.font = _heading;
            text.color = color;
            text.enableAutoSizing = true;
            text.fontSizeMax = size;
            text.fontSizeMin = Mathf.Min(size, 16f);
        }

        private static void StyleBody(TMP_Text text)
        {
            text.font = _body;
            text.fontStyle = FontStyles.Bold;
            text.color = BodyCream;
        }
    }
}
