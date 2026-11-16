using Cysharp.Threading.Tasks;
using InGame.Managers;

namespace InGame.System
{
    public class BloodLuckGameController
    {
        private readonly BloodLuckContextMap _bloodLuckContext;
        private readonly BloodLuckGameUpdateLoopManager _bloodLuckGameUpdateManager;
        private bool _isControllerInitializationFinished = false;

        public BloodLuckGameController(BloodLuckManagerParameter parameter)
        {
            _bloodLuckContext = new BloodLuckContextMap();
            _bloodLuckGameUpdateManager = new BloodLuckGameUpdateLoopManager();

            _bloodLuckContext.Register(_bloodLuckGameUpdateManager);
            _bloodLuckContext.Register(new BloodLuckGameStateManager());
            _bloodLuckContext.Register(new BloodLuckMonsterManager(parameter));
            _bloodLuckContext.Register(new BloodLuckWaveManager(parameter));
            _bloodLuckContext.Register(new BloodLuckPerkSelectionManager(parameter));
            _bloodLuckContext.Register(new BloodLuckEffectManager(parameter));
            _bloodLuckContext.Register(new BloodLuckMonsterProjectileManager(parameter));
            _bloodLuckContext.Register(new BloodLuckPlayerStateManager(parameter));
            _bloodLuckContext.Register(new BloodLuckUIManager(parameter));
            _bloodLuckContext.Register(new BloodLuckSlotMachineManager(parameter));
            _bloodLuckContext.Register(new BloodLuckShopManager(parameter));
            _bloodLuckContext.Register(new BloodLuckGameResultManager(parameter));
            _bloodLuckContext.Register(new BloodLuckCheatManager());
            _bloodLuckContext.Register(new BloodLuckPlayerManager(parameter));
            _bloodLuckContext.Register(new BloodLuckCameraManager(parameter));
            // Register new systems (IBloodLuckContext) here.
        }

        public async UniTask InitializeGameController()
        {
            await _bloodLuckContext.InitializeAllContextAsync();
            _bloodLuckContext.WireContextDependencyAfterInit();
            _bloodLuckContext.GetRegisteredInstance<BloodLuckGameStateManager>().ResumeGame();
            _isControllerInitializationFinished = true;
        }

        public void UpdateController(float deltaTime)
        {
            if (!_isControllerInitializationFinished)
            {
                return;
            }
            _bloodLuckGameUpdateManager.UpdateInGameLoop(deltaTime);
        }
    }
}
