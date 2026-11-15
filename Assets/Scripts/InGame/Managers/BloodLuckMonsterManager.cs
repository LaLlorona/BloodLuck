using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using InGame.Monsters;
using InGame.System;


namespace InGame.Managers
{
    public class BloodLuckMonsterManager : IBloodLuckContext, IBloodLuckPriorityUpdatable
    {
        private BloodLuckWaveManager _bloodLuckWaveManager;
        private readonly List<BloodLuckMonster> _monsters = new();
        private BloodLuckAssetHolder _assetHolder;

        public BloodLuckMonsterManager(BloodLuckAssetHolder assetHolder)
        {
            _assetHolder = assetHolder;
        }
        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _bloodLuckWaveManager = contextMap.GetRegisteredInstance<BloodLuckWaveManager>();
            var updateLoopManager = contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>();
            updateLoopManager.RegisterUpdatable(this);

        }

        public BloodLuckUpdatePriority UpdatePriority => BloodLuckUpdatePriority.MonsterUpdate;
        public void GameUpdate(float deltaTime)
        {
            foreach (var monster in _monsters)
            {
                monster.UpdateMonster(deltaTime);
            }
        }

        public void OnMonsterDied(BloodLuckMonster monster)
        {
            monster.OnMonsterDied();
            _monsters.Remove(monster);

            if (_monsters.Count == 0)
            {
                OnAllMonsterDied();
            }
        }

        private void OnAllMonsterDied()
        {
            _bloodLuckWaveManager.OnWaveFinished();
        }

        public void SpawnMonsters()
        {
            //ToDo: spawn monsters logic goes here
            for (int i = 0; i < 10; i++)
            {
                _monsters.Add(new BloodLuckMonster(_assetHolder));
            }
        }

        public void CheatKillAllMonsters()
        {
            foreach (var monster in _monsters.ToList())
            {
                OnMonsterDied(monster);
            }
        }
    }
}