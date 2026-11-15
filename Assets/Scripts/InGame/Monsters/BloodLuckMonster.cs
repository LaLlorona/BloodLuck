using InGame.System;
using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonster
    {
        private BloodLuckMonsterComponent _monsterComponent;

        public void UpdateMonster(float deltaTime)
        {
            //ToDo: move towards the player
        }
        public BloodLuckMonster(BloodLuckAssetHolder assetHolder)
        {
            _monsterComponent = GameObject.Instantiate(assetHolder.GetBloodLuckMonsterComponent);
        }

        public void OnMonsterDied()
        {
            GameObject.Destroy(_monsterComponent.gameObject);
        }
    }
}