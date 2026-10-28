using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Campaign", menuName = "Settings/Campaign")]
public class CampaignSettings : ScriptableObject
{
    private const int GroupSize = 5;
    private const int SeedStride = 7919;

    private readonly Dictionary<int, CampaignStage> _generatedStages = new Dictionary<int, CampaignStage>();

    [SerializeField] private CampaignStage[] _stages;
    [Header("After fixed stages")]
    [SerializeField] private int _proceduralSeed = 2601026;
    [SerializeField, Min(1)] private int _maximumPieces = 60;
    [SerializeField, Min(0f)] private float _speedPerLevel = 1.5f;
    [SerializeField, Min(1f)] private float _maximumOpponentSpeed = 220f;

    public int Count => _stages != null ? _stages.Length : 0;

    private void OnValidate()
    {
        _generatedStages.Clear();
    }

    public CampaignStage GetStage(int number)
    {
        if (Count == 0)
            throw new InvalidOperationException("Campaign has no stages.");

        number = Mathf.Max(1, number);

        if (number <= Count)
            return CloneStage(_stages[number - 1]);

        if (_generatedStages.TryGetValue(number, out CampaignStage generated) == false)
        {
            generated = GenerateStage(number);
            _generatedStages.Add(number, generated);
        }

        return CloneStage(generated);
    }

    private CampaignStage CloneStage(CampaignStage stage)
    {
        return JsonUtility.FromJson<CampaignStage>(JsonUtility.ToJson(stage));
    }

    private CampaignStage GenerateStage(int number)
    {
        int offset = number - Count - 1;
        int group = offset / GroupSize;
        var bag = new[] { LevelDifficulty.Easy, LevelDifficulty.Easy, LevelDifficulty.Medium, LevelDifficulty.Medium, LevelDifficulty.Hard };
        var random = new System.Random(unchecked(_proceduralSeed + group * SeedStride));

        for (int index = bag.Length - 1; index > 0; index--)
        {
            int next = random.Next(index + 1);
            LevelDifficulty swap = bag[index];
            bag[index] = bag[next];
            bag[next] = swap;
        }

        LevelDifficulty difficulty = bag[offset % GroupSize];
        CampaignStage last = _stages[Count - 1];
        float multiplier = difficulty == LevelDifficulty.Easy ? 0.92f : difficulty == LevelDifficulty.Hard ? 1.08f : 1f;
        int baseReward = difficulty == LevelDifficulty.Easy ? 60 : difficulty == LevelDifficulty.Medium ? 100 : 160;
        float rewardFactor = Mathf.Min(5f, 1f + ((number - 4) / GroupSize) * 0.25f);
        return new CampaignStage
        {
            Seed = unchecked(_proceduralSeed + number * SeedStride),
            PieceCount = Mathf.Min(_maximumPieces, last.PieceCount + (group + 1) * 2),
            Difficulty = difficulty,
            Reward = Mathf.RoundToInt(baseReward * rewardFactor),
            FirstClearBonus = last.FirstClearBonus,
            OpponentMaxSpeed = Mathf.Min(_maximumOpponentSpeed, (last.OpponentMaxSpeed + (offset + 1) * _speedPerLevel) * multiplier),
            OpponentAcceleration = Mathf.Min(200f, last.OpponentAcceleration + offset + 1),
            CruiseSpeed = last.CruiseSpeed,
            CornerSpeed = last.CornerSpeed,
            Layout = null
        };
    }
}
