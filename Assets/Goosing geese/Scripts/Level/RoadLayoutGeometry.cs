using System.Collections.Generic;
using UnityEngine;

public class RoadLayoutGeometry
{
    private const int MinimumSamples = 24;
    private readonly Dictionary<RoadPieceType, RoadPiece> _prefabs;

    public RoadLayoutGeometry(Dictionary<RoadPieceType, RoadPiece> prefabs)
    {
        _prefabs = prefabs;
    }

    public bool IsValid(RoadPieceType[] layout, Transform start, float minimumSpacing, int samples)
    {
        var curves = new List<List<Vector3>>();
        Vector3 entry = start.position;
        float yaw = start.eulerAngles.y;
        float minimumSquaredDistance = minimumSpacing * minimumSpacing;

        foreach (RoadPieceType type in layout)
        {
            RoadPiece prefab = _prefabs[type];
            float rotationDelta = yaw - prefab.EntryPoint.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0f, rotationDelta, 0f);
            var points = prefab.GetCurvePoints(Mathf.Max(MinimumSamples, samples));
            var worldPoints = new List<Vector3>();

            foreach (Vector3 point in points)
                worldPoints.Add(entry + rotation * (point - prefab.EntryPoint.position));

            for (int previous = 0; previous < curves.Count - 1; previous++)
            {
                foreach (Vector3 point in worldPoints)
                {
                    foreach (Vector3 other in curves[previous])
                    {
                        Vector3 delta = point - other;
                        delta.y = 0f;

                        if (delta.sqrMagnitude < minimumSquaredDistance)
                            return false;
                    }
                }
            }

            curves.Add(worldPoints);
            entry += rotation * (prefab.ExitPoint.position - prefab.EntryPoint.position);
            yaw = prefab.ExitPoint.eulerAngles.y + rotationDelta;
        }

        return layout.Length > 0;
    }
}
