using Cysharp.Threading.Tasks;
using InGame.System;

namespace InGame.Managers
{
    public class BloodLuckWaveManager : IBloodLuckContext
    {
        private BloodLuckGameUpdateLoopManager _updateLoopManager;
        private BloodLuckMonsterManager _monsterManager;
        private BloodLuckUIManager _uiManager;
        private readonly BloodLuckWaveTable _waveTable;
        private int _currentWaveNumber;

        public int CurrentWaveNumber => _currentWaveNumber;

        public BloodLuckWaveManager(BloodLuckManagerParameter parameter)
        {
            _waveTable = parameter.DataHolder.WaveTable;
        }

        public UniTask InitAsync()
        {
            _currentWaveNumber = 1;
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _updateLoopManager = contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>();
            _monsterManager = contextMap.GetRegisteredInstance<BloodLuckMonsterManager>();
            _uiManager = contextMap.GetRegisteredInstance<BloodLuckUIManager>();
            OnWaveStarted();
        }

        public void OnWaveFinished()
        {
            _currentWaveNumber++;
            _uiManager.RefreshHud();
            if (_currentWaveNumber % BloodLuckDefine.SHOP_APPEAR_WAVE_NUM == 0)
            {
                _updateLoopManager.AddAsyncTask(new BloodLuckAsyncTask.WaveFinishedAndVisitingShop());
            }

            else if (_currentWaveNumber == BloodLuckDefine.FINAL_WAVE_NUM)
            {
                _updateLoopManager.AddAsyncTask(new BloodLuckAsyncTask.LastWaveSlotMachine());
            }
            else
            {
                _updateLoopManager.AddAsyncTask(new BloodLuckAsyncTask.WaveFinishedAndSelectingPerk());
            }
        }

        public void OnWaveStarted()
        {
            _monsterManager.SpawnMonsters(_waveTable.GetWave(_currentWaveNumber));
        }
    }
}