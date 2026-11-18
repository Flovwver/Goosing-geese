namespace GoosingGeese.Profile
{
    public sealed class QuestDefinition
    {
        public QuestDefinition(QuestId id, int target, int reward, string titleKey)
        {
            Id = id;
            Target = target;
            Reward = reward;
            TitleKey = titleKey;
        }

        public QuestId Id { get; }
        public int Target { get; }
        public int Reward { get; }
        public string TitleKey { get; }
    }
}
