using Cysharp.Threading.Tasks;

namespace InGame.System
{
    public class BloodLuckGameStateManager : IBloodLuckContext
    {
        public abstract record GameState
        {
            public sealed record Initializing : GameState;

            public sealed record GameProceeding : GameState;

            public sealed record Paused : GameState;

            public sealed record GameFinished : GameState;
        }

        public GameState CurrentGameState { get; private set; } = new GameState.Initializing();

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
        }

        public void PauseGame()
        {
            // Never override a finished game with a paused state.
            if (CurrentGameState is GameState.GameFinished)
            {
                return;
            }

            CurrentGameState = new GameState.Paused();
        }

        public void ResumeGame()
        {
            // Never resume a game that has already finished.
            if (CurrentGameState is GameState.GameFinished)
            {
                return;
            }

            CurrentGameState = new GameState.GameProceeding();
        }

        public void FinishGame()
        {
            CurrentGameState = new GameState.GameFinished();
        }
    }
}
