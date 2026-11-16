using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InGame.System
{
    public class BloodLuckGameBootstrap : MonoBehaviour
    {
        [SerializeField] private BloodLuckAssetHolder assetHolder;

        [SerializeField] private BloodLuckDataHolder dataHolder;

        [SerializeField] private BloodLuckInGameParameter inGameParameter = new();

        private BloodLuckGameController _controller;

        private void Start()
        {
            var controller = new BloodLuckGameController(
                new BloodLuckManagerParameter(assetHolder, inGameParameter, dataHolder));
            _controller = controller;
            _controller.InitializeGameController().Forget();
        }

        private void Update()
        {
            _controller?.UpdateController(Time.deltaTime);
        }
    }
}
