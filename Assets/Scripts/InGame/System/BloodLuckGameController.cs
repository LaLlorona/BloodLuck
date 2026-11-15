using Cysharp.Threading.Tasks;
using InGame.Managers;

namespace InGame.System
{
    public class BloodLuckGameController
    {
        private readonly BloodLuckContextMap _bloodLuckContext;
        private readonly BloodLuckGameUpdateLoopManager _bloodLuckGameUpdateManager;
        private readonly BloodLuckAssetHolder _assetHolder;
        private bool _isControllerInitializationFinished = false;

        public BloodLuckGameController(BloodLuckAssetHolder assetHolder)
        {
            _assetHolder = assetHolder;
            _bloodLuckContext = new BloodLuckContextMap();
            _bloodLuckGameUpdateManager = new BloodLuckGameUpdateLoopManager();

            _bloodLuckContext.Register(_bloodLuckGameUpdateManager);
            _bloodLuckContext.Register(new BloodLuckGameStateManager());
            _bloodLuckContext.Register(new BloodLuckMonsterManager(assetHolder));
            _bloodLuckContext.Register(new BloodLuckWaveManager());
            _bloodLuckContext.Register(new BloodLuckPerkSelectionManager(assetHolder));
            _bloodLuckContext.Register(new BloodLuckCheatManager());
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
