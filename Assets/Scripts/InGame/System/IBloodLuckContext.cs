using Cysharp.Threading.Tasks;

namespace InGame.System
{
    public interface IBloodLuckContext
    {
        UniTask InitAsync();
        void WireContextAfterInit(BloodLuckContextMap contextMap);

    }
}