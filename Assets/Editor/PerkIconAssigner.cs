using System.Linq;
using InGame.System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace InGame.EditorTools
{
    public static class PerkIconAssigner
    {
        private const string IconFolder = "Assets/RPG_inventory_icons";

        [MenuItem("Tools/BloodLuck/Assign Perk Icons To AssetHolder")]
        private static void Assign()
        {
            var holders = Object.FindObjectsByType<BloodLuckAssetHolder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (holders.Length == 0)
            {
                Debug.LogWarning("No BloodLuckAssetHolder in the open scene.");
                return;
            }

            var icons = AssetDatabase.FindAssets("t:Sprite", new[] { IconFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(sprite => sprite != null)
                .ToArray();

            foreach (var holder in holders)
            {
                var serialized = new SerializedObject(holder);
                var property = serialized.FindProperty("_perkIcons");
                property.arraySize = icons.Length;
                for (int i = 0; i < icons.Length; i++)
                    property.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(holder);
                EditorSceneManager.MarkSceneDirty(holder.gameObject.scene);
            }

            Debug.Log($"Assigned {icons.Length} perk icons to {holders.Length} AssetHolder(s). Save the scene.");
        }
    }
}
