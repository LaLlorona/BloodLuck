using BloodLuck;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InGame.EditorTools
{
    public static class BloodLuckAudioSetup
    {
        private const string Base = "Assets/Audio/SFX/";

        [MenuItem("Tools/BloodLuck/Connect Audio SFX")]
        public static void Connect()
        {
            ConnectShopPrefab();
            ConnectUiPrefab("Assets/Prefabs/PerkSelectionUI.prefab");
            ConnectUiPrefab("Assets/Prefabs/GameResultUI.prefab");
            ConnectUiPrefab("Assets/Prefabs/SlotMachineUI.prefab");
            ConnectScene("Assets/Scenes/IntroScene.unity");
            ConnectScene("Assets/Scenes/InGameScene.unity");
            ConnectScene("Assets/Scenes/Shop_Prototype.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[BloodLuckAudioSetup] Connected audio clips and UI sounds.");
        }

        private static void ConnectShopPrefab()
        {
            const string path = "Assets/Prefabs/ShopView.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            foreach (var button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                if ((button.name.StartsWith("UpgradeCard") || button.name == "BloodyDiceButton") &&
                    button.GetComponent<ShopCardHoverSound>() == null)
                    button.gameObject.AddComponent<ShopCardHoverSound>();
            ConnectButtonAudio(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ConnectUiPrefab(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            ConnectButtonAudio(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ConnectScene(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            AudioManager existing = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                existing = root.GetComponentInChildren<AudioManager>(true);
                if (existing != null) break;
            }
            var go = existing != null ? existing.gameObject : new GameObject("AudioManager");
            var manager = existing != null ? existing : go.AddComponent<AudioManager>();
            var sources = go.GetComponents<AudioSource>();
            while (sources.Length < 3)
            {
                go.AddComponent<AudioSource>();
                sources = go.GetComponents<AudioSource>();
            }
            sources[0].playOnAwake = false;
            sources[0].loop = true;
            sources[1].playOnAwake = false;
            sources[1].loop = false;
            sources[2].playOnAwake = false;
            sources[2].loop = false;
            Set(manager, "musicSource", sources[0]);
            Set(manager, "transitionMusicSource", sources[2]);
            Set(manager, "sfxSource", sources[1]);
            Set(manager, "swings", Clips("Player/Attack/Bloodluck_Player_Attack_Swing_02_", 3));
            Set(manager, "hits", Clips("Player/Attack/Hit/Bloodluck_Player_Attack_Hit_", 4));
            Set(manager, "footsteps", Clips("Player/Run/Bloodluck_Player_Footstep_", 6));
            Set(manager, "dashes", Clips("Player/Dash/Bloodluck_Player_Dash_", 3));
            Set(manager, "playerDamages", Clips("Player/Damage/Bloodluck_Player_TakeDamage_", 4));
            Set(manager, "shopHovers", Clips("UI/Shop/Hover/Bloodluck_UI_Shop_Hover_", 4));
            Set(manager, "shopPurchase", Clip("UI/Shop/Select/Bloodluck_UI_Shop_Purchase.wav"));
            Set(manager, "diceRoll", Clip("UI/Shop/DiceRoll/Bloodluck_UI_Shop_DicerollSFX.wav"));
            Set(manager, "slotPull", Clip("UI/SlotMachine/Slotpull.wav"));
            Set(manager, "winSfx", Clip("Win/Bloodluck_Game_Win.wav"));
            Set(manager, "perkSelection", Clip("UI/PerkSelect/Bloodluck_PerkSelection.wav"));
            Set(manager, "uiSelect", Clip("UI/BasicSelect/Bloodluck_UI_BasicSelect.wav"));
            SetMusic(manager, "mainMenuMusic", "Bloodluck_Menu.wav");
            SetMusic(manager, "gameplayIntro", "Bloodluck_Gameplay_Intro.wav");
            SetMusic(manager, "gameplayLoop", "Bloodluck_Gameplay_Loop.wav");
            SetMusic(manager, "shopMusic", "Bloodluck_Shop.wav");
            SetMusic(manager, "defeatIntro", "Bloodluck_Defeat_Intro.wav");
            SetMusic(manager, "defeatLoop", "Bloodluck_Defeat_Loop.wav");
            SetMusic(manager, "winLoop", "Bloodluck_Win_Loop.wav");
            ConnectButtonAudio(go.scene.GetRootGameObjects());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static AudioClip Clip(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(Base + path);
        private static void SetMusic(AudioManager manager, string field, string filename) =>
            Set(manager, field, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/" + filename));
        private static AudioClip[] Clips(string prefix, int count)
        {
            var clips = new AudioClip[count];
            for (int i = 0; i < count; i++) clips[i] = Clip(prefix + (i + 1).ToString("00") + ".wav");
            return clips;
        }
        private static void Set(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Set(Object target, string field, AudioClip[] clips)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            property.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConnectButtonAudio(GameObject root)
        {
            ConnectButtonAudio(root.GetComponentsInChildren<UnityEngine.UI.Button>(true));
        }

        private static void ConnectButtonAudio(GameObject[] roots)
        {
            foreach (var root in roots) ConnectButtonAudio(root);
        }

        private static void ConnectButtonAudio(UnityEngine.UI.Button[] buttons)
        {
            foreach (var button in buttons)
            {
                if (button.GetComponent<ShopCardHoverSound>() != null ||
                    button.GetComponentInParent<InGame.UI.PerkCardViewComponent>() != null ||
                    button.GetComponent<UIButtonAudio>() != null)
                    continue;
                button.gameObject.AddComponent<UIButtonAudio>();
            }
        }
    }
}
