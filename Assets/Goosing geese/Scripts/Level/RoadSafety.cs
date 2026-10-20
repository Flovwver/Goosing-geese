using UnityEngine;

public class RoadSafety : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private RaceController _race;
    [SerializeField, Min(0f)] private float _previewDistance = 45f;
    [SerializeField, Min(0f)] private float _roadHalfWidth = 12f;
    [SerializeField, Min(0f)] private float _offRoadGraceTime = 0.75f;

    private RoadPath _path;
    private float _offRoadTimer;
    private GameObject _lines;

    private void OnEnable()
    {
        _level.PathBuilt += OnPathBuilt;
        _player.RaceRestarted += OnRestarted;
    }

    private void Update()
    {
        if (_race.State != RaceState.Racing || _path == null)
            return;

        RoadNavigator navigator = _player.GetComponent<RoadNavigator>();
        Vector3 displacement = _player.transform.position - navigator.ClosestPosition;
        displacement.y = 0f;
        _offRoadTimer = displacement.sqrMagnitude > _roadHalfWidth * _roadHalfWidth ? _offRoadTimer + Time.deltaTime : 0f;

        if (_offRoadTimer >= _offRoadGraceTime)
            _race.EndRace(false, RaceOutcome.OffRoad);
    }

    private void OnDisable()
    {
        _level.PathBuilt -= OnPathBuilt;
        _player.RaceRestarted -= OnRestarted;
    }

    private Color GetRecommendedColor(int segment, float distance)
    {
        bool corner = _path.Pieces[segment].Type != RoadPieceType.Straight;

        if (segment + 1 < _path.Pieces.Count && _path.Curve.GetSegmentEndDistance(segment) - distance <= _previewDistance)
            corner |= _path.Pieces[segment + 1].Type != RoadPieceType.Straight;

        return corner ? Color.green : Color.yellow;
    }

    private void OnPathBuilt(RoadPath path, RoadSettings settings)
    {
        _path = path;
        _offRoadTimer = 0f;

        if (_lines != null)
        {
            _lines.SetActive(false);
            Destroy(_lines);
        }

        _lines = new GameObject("Speed guidance");
        LineRenderer source = _level.GetComponent<LineRenderer>();
        float start = 0f;
        const float sectionLength = 5f;

        for (int segment = 0; segment < path.Curve.SegmentCount; segment++)
        {
            float end = path.Curve.GetSegmentEndDistance(segment);

            for (float distance = start; distance < end; distance += sectionLength)
            {
                var go = new GameObject("Speed section");
                go.transform.SetParent(_lines.transform);
                var line = go.AddComponent<LineRenderer>();
                line.sharedMaterial = source.sharedMaterial;
                line.startWidth = settings.PathLineWidth;
                line.endWidth = settings.PathLineWidth;
                line.startColor = GetRecommendedColor(segment, distance);
                line.endColor = line.startColor;
                line.positionCount = 2;
                line.SetPosition(0, path.Curve.GetPointAtDistance(distance) + Vector3.up * settings.PathLineHeight);
                line.SetPosition(1, path.Curve.GetPointAtDistance(Mathf.Min(end, distance + sectionLength)) + Vector3.up * settings.PathLineHeight);
                line.enabled = settings.ShowPathLine;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            start = end;
        }

        source.enabled = false;
    }

    private void OnRestarted()
    {
        _offRoadTimer = 0f;
    }
}
