using System;
using UnityEngine;

[Serializable]
public sealed class OpponentSnapshot
{
    public string Stats;
    public string Navigation;
    public Vector3 Position;
    public Quaternion Rotation;
    public float CruiseSpeed;
    public float CornerSpeed;
    public float LaneOffset;
    public float BrakingPreviewDistance;
    public GameObject Vehicle;

    public OpponentSnapshot(OpponentDefinition definition)
    {
        Stats = definition.Racer.Stats.ToJson();
        Navigation = definition.Racer.Navigation.ToJson();
        Position = definition.Racer.Position;
        Rotation = definition.Racer.Rotation;
        CruiseSpeed = definition.CruiseSpeed;
        CornerSpeed = definition.CornerSpeed;
        LaneOffset = definition.LaneOffset;
        BrakingPreviewDistance = definition.BrakingPreviewDistance;
        Vehicle = definition.Vehicle;
    }

    public OpponentDefinition Restore()
    {
        var stats = ScriptableObject.CreateInstance<RacerStats>();
        var navigation = ScriptableObject.CreateInstance<NavigationSettings>();

        try
        {
            JsonUtility.FromJsonOverwrite(Stats, stats);
            JsonUtility.FromJsonOverwrite(Navigation, navigation);
            var racer = new RacerDefinition(new RacerConfiguration(stats), new NavigationConfiguration(navigation), Position, Rotation);
            return new OpponentDefinition(racer, CruiseSpeed, CornerSpeed, LaneOffset, BrakingPreviewDistance, Vehicle);
        }
        finally
        {
            UnityEngine.Object.Destroy(stats);
            UnityEngine.Object.Destroy(navigation);
        }
    }
}
