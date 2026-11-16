using Cysharp.Threading.Tasks;
using UnityEngine;

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
            if (CurrentGameState is GameState.GameFinished)
            {
                return;
            }

            Time.timeScale = 0f;
            CurrentGameState = new GameState.Paused();
        }

        public void ResumeGame()
        {
            if (CurrentGameState is GameState.GameFinished)
            {
                return;
            }

            Time.timeScale = 1f;
            CurrentGameState = new GameState.GameProceeding();
        }

        public void FinishGame()
        {
            CurrentGameState = new GameState.GameFinished();
        }
    }
}
