namespace InGame.System
{
    public abstract record BloodLuckAsyncTask
    {

        public sealed record WaveFinishedAndSelectingPerk : BloodLuckAsyncTask;

        public sealed record WaveFinishedAndVisitingShop : BloodLuckAsyncTask;

        public sealed record LastWaveSlotMachine : BloodLuckAsyncTask;
    }
}
