using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Campaign", menuName = "Settings/Campaign")]
public class CampaignSettings : ScriptableObject
{
    [SerializeField] private CampaignStage[] _stages;

    public int Count => _stages != null ? _stages.Length : 0;

    public CampaignStage GetStage(int number)
    {
        if (Count == 0)
            throw new InvalidOperationException("Campaign has no stages.");

        return _stages[Mathf.Clamp(number, 1, Count) - 1];
    }
}
