namespace GoosingGeese.Infrastructure
{
    public interface IGamePlatform
    {
        void ReportGameplay(bool isReady, bool isActive);
    }
}
