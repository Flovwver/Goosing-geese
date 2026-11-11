using GoosingGeese.Settings;

namespace GoosingGeese.Levels
{
    public interface ILevelSession
    {
        CampaignStage CurrentStage { get; }

        int SelectedLevelNumber { get; }

        int SelectedSceneryIndex { get; }

        bool IsRacing { get; }

        bool IsWaiting { get; }

        void ValidatePreparation();

        void PrepareAttempt();
    }
}
