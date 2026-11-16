using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Title
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private void Awake()
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(LoadInGameScene);
        }

        private void LoadInGameScene()
        {
            SceneManager.LoadScene("InGameScene");
        }
    }
}