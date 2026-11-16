using UnityEngine;

namespace InGame.System
{
    public class BloodLuckDataHolder : MonoBehaviour
    {
        [SerializeField] private BloodLuckWaveTable _waveTable;

        public BloodLuckWaveTable WaveTable => _waveTable;
    }
}
