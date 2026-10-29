public sealed class QuestDefinition
{
    public QuestDefinition(QuestId id, int target, int reward, string russianTitle, string englishTitle)
    {
        Id = id;
        Target = target;
        Reward = reward;
        RussianTitle = russianTitle;
        EnglishTitle = englishTitle;
    }

    public QuestId Id { get; }
    public int Target { get; }
    public int Reward { get; }
    public string RussianTitle { get; }
    public string EnglishTitle { get; }
}
