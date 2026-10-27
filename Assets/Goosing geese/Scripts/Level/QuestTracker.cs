using System;

public static class QuestTracker
{
    public const int QuestCount = 3;
    private const int DailyRaceTarget = 3;
    private const int DailyWinTarget = 1;
    private const int GlobalWinTarget = 10;
    private const int DailyRaceReward = 50;
    private const int DailyWinReward = 75;
    private const int GlobalWinReward = 300;

    public static void RefreshDay(PlayerProgress progress, DateTime date)
    {
        string today = date.ToString("yyyy-MM-dd");

        if (progress.DailyDate == today)
            return;

        progress.DailyDate = today;
        progress.DailyRaces = 0;
        progress.DailyWins = 0;
        progress.DailyRacesClaimed = false;
        progress.DailyWinsClaimed = false;
    }

    public static void RecordRace(PlayerProgress progress, bool won, DateTime date)
    {
        RefreshDay(progress, date);
        progress.Races++;
        progress.DailyRaces++;

        if (won)
        {
            progress.Wins++;
            progress.DailyWins++;
        }
    }

    public static int GetProgress(PlayerProgress progress, int quest, DateTime date)
    {
        Validate(quest);
        bool currentDay = progress.DailyDate == date.ToString("yyyy-MM-dd");
        return quest == 0 ? (currentDay ? progress.DailyRaces : 0)
            : quest == 1 ? (currentDay ? progress.DailyWins : 0) : progress.Wins;
    }

    public static int GetTarget(int quest)
    {
        Validate(quest);
        return quest == 0 ? DailyRaceTarget : quest == 1 ? DailyWinTarget : GlobalWinTarget;
    }

    public static bool IsClaimed(PlayerProgress progress, int quest, DateTime date)
    {
        Validate(quest);

        if (quest == 2)
            return progress.GlobalWinsClaimed;

        return progress.DailyDate == date.ToString("yyyy-MM-dd")
            && (quest == 0 ? progress.DailyRacesClaimed : progress.DailyWinsClaimed);
    }

    public static bool TryClaim(PlayerProgress progress, int quest, DateTime date)
    {
        if (quest < 0 || quest >= QuestCount)
            return false;

        RefreshDay(progress, date);

        if (IsClaimed(progress, quest, date) || GetProgress(progress, quest, date) < GetTarget(quest))
            return false;

        int reward = quest == 0 ? DailyRaceReward : quest == 1 ? DailyWinReward : GlobalWinReward;
        progress.Points += reward;
        progress.Wallet += reward;

        if (quest == 0)
            progress.DailyRacesClaimed = true;
        else if (quest == 1)
            progress.DailyWinsClaimed = true;
        else
            progress.GlobalWinsClaimed = true;

        return true;
    }

    private static void Validate(int quest)
    {
        if (quest < 0 || quest >= QuestCount)
            throw new ArgumentOutOfRangeException(nameof(quest));
    }
}
