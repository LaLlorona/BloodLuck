namespace InGame.Monsters
{
    public interface IBloodLuckMonsterBehaviour
    {
        void UpdateMonsterBehaviour(float deltaTime);

        void ResetMonsterBehaviour();

        void OnMonsterDied();
    }
}
