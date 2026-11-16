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
            ConnectScene("Assets/Scenes/InGameScene.unity");
            ConnectScene("Assets/Scenes/Shop_Prototype.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[BloodLuckAudioSetup] Connected SFX in gameplay and shop scenes.");
        }

        private static void ConnectShopPrefab()
        {
            const string path = "Assets/Prefabs/ShopView.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            foreach (var button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                if ((button.name.StartsWith("UpgradeCard") || button.name == "BloodyDiceButton") &&
                    button.GetComponent<ShopCardHoverSound>() == null)
                    button.gameObject.AddComponent<ShopCardHoverSound>();
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
            while (sources.Length < 2)
            {
                go.AddComponent<AudioSource>();
                sources = go.GetComponents<AudioSource>();
            }
            sources[0].playOnAwake = false;
            sources[0].loop = true;
            sources[1].playOnAwake = false;
            sources[1].loop = false;
            Set(manager, "musicSource", sources[0]);
            Set(manager, "sfxSource", sources[1]);
            Set(manager, "swings", Clips("Player/Attack/Bloodluck_Player_Attack_Swing_Miss_", 3));
            Set(manager, "hits", Clips("Player/Attack/Hit/Bloodluck_Player_Attack_Hit_", 4));
            Set(manager, "footsteps", Clips("Player/Run/Bloodluck_Player_Footstep_", 6));
            Set(manager, "shopHovers", Clips("UI/Shop/Hover/Bloodluck_UI_Shop_Hover_", 4));
            Set(manager, "death", Clip("Player/Death/Bloodluck_Player_Death.wav"));
            Set(manager, "shopPurchase", Clip("UI/Shop/Select/Bloodluck_UI_Shop_Purchase.wav"));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static AudioClip Clip(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(Base + path);
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
    }
}
