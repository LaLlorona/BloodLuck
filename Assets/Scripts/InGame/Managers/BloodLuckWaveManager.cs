using Cysharp.Threading.Tasks;
using InGame.System;

namespace InGame.Managers
{
    public class BloodLuckWaveManager : IBloodLuckContext
    {
        private BloodLuckGameUpdateLoopManager _updateLoopManager;
        private BloodLuckMonsterManager _monsterManager;
        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _updateLoopManager = contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>();
            _monsterManager = contextMap.GetRegisteredInstance<BloodLuckMonsterManager>();
            OnWaveStarted();
        }

        public void OnWaveFinished()
        {
            _updateLoopManager.AddAsyncTask(new BloodLuckAsyncTask.WaveFinishedAndSelectingPerk());
        }

        public void OnWaveStarted()
        {
            _monsterManager.SpawnMonsters();
        }
    }
}