namespace InGame.Monsters
{
    public interface IBloodLuckMonsterBehaviour
    {
        void UpdateMonsterBehaviour(float deltaTime);

        void OnMonsterDied();
    }
}
