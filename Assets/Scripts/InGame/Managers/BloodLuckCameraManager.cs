using Cysharp.Threading.Tasks;
using InGame.System;
using Unity.Cinemachine;

namespace InGame.Managers
{
    public class BloodLuckCameraManager : IBloodLuckContext
    {
        private readonly CinemachineCamera _inGameCamera;
        private readonly PlayerMovement _playerMovement;

        public BloodLuckCameraManager(BloodLuckManagerParameter parameter)
        {
            _inGameCamera = parameter.AssetHolder.InGameCamera;
            _playerMovement = parameter.AssetHolder.GetPlayerMovementComponent;
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _inGameCamera.Follow = _playerMovement.transform;
        }
    }
}
