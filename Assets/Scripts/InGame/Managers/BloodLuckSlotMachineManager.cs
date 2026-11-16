using System;
using Cysharp.Threading.Tasks;
using InGame.System;
using Object = UnityEngine.Object;

namespace InGame.Managers
{
    public class BloodLuckSlotMachineManager : IBloodLuckContext
    {
        private const float EndDelaySeconds = 1.2f;

        private readonly BloodLuckAssetHolder _assetHolder;
        private readonly BloodLuckInGameParameter _parameter;
        private BloodLuckPlayerStateManager _playerStateManager;

        public BloodLuckSlotMachineManager(BloodLuckManagerParameter parameter)
        {
            _assetHolder = parameter.AssetHolder;
            _parameter = parameter.InGameParameter;
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _playerStateManager = contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>();
        }
        
        public async UniTask<bool> PlaySlotMachineAsync()
        {
            var view = Object.Instantiate(_assetHolder.SlotMachineViewPrefab);
            var ct = view.destroyCancellationToken;
            try
            {
                view.Init();
                while (true)
                {
                    view.SetPulls(_playerStateManager.CurrentPulls, _playerStateManager.MaxPulls);
                    if (_playerStateManager.CurrentPulls <= 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(EndDelaySeconds), DelayType.UnscaledDeltaTime, cancellationToken: ct);
                        return false;
                    }

                    view.SetSpinInteractable(true);
                    await view.WaitSpinClickAsync(ct);

                    _playerStateManager.SetCurrentPulls(_playerStateManager.CurrentPulls - 1);
                    view.SetPulls(_playerStateManager.CurrentPulls, _playerStateManager.MaxPulls);

                    bool isLastPull = _playerStateManager.CurrentPulls == 0;
                    bool isJackpot = isLastPull || UnityEngine.Random.value < _parameter.SlotJackpotChance;
                    int[] result = BuildResult(view.ReelCount, view.SymbolCount, isJackpot);
                    await view.PlaySpinAsync(result, ct);

                    if (isJackpot)
                    {
                        view.SetResultText("JACKPOT!");
                        await UniTask.Delay(TimeSpan.FromSeconds(EndDelaySeconds), DelayType.UnscaledDeltaTime, cancellationToken: ct);
                        return true;
                    }

                    view.SetResultText("MISS");
                }
            }
            finally
            {
                if (view != null)
                {
                    Object.Destroy(view.gameObject);
                }
            }
        }

        private static int[] BuildResult(int reelCount, int symbolCount, bool isJackpot)
        {
            var result = new int[reelCount];
            if (isJackpot)
            {
                Array.Fill(result, UnityEngine.Random.Range(0, symbolCount));
                return result;
            }

            bool allSame;
            do
            {
                allSame = true;
                for (int i = 0; i < reelCount; i++)
                {
                    result[i] = UnityEngine.Random.Range(0, symbolCount);
                    allSame &= result[i] == result[0];
                }
            } while (allSame && symbolCount > 1);

            return result;
        }
    }
}
