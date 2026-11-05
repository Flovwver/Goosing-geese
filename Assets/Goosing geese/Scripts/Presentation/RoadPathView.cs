using System.Collections.Generic;
using GoosingGeese.Levels;
using GoosingGeese.Roads;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.Presentation
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RoadPathView")]
    public class RoadPathView : MonoBehaviour
    {
        private const float MinimumSampleSpacing = 0.25f;
        [SerializeField]
        private Level _level;
        [SerializeField]
        private Material _material;
        [SerializeField, Min(0f), Tooltip("Total blend length in metres, centered on a straight/turn boundary.")]
        private float _transitionLength = 12f;
        [SerializeField, Min(MinimumSampleSpacing)]
        private float _sampleSpacing = 2f;
        [SerializeField]
        private Color _straightColor = Color.yellow;
        [SerializeField]
        private Color _turnColor = Color.green;
        private GameObject _lines;

        private void OnEnable()
        {
            if (_level == null)
                return;

            _level.PathBuilt += OnPathBuilt;

            if (_level.TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings))
                Show(path, settings);
        }

        private void OnDisable()
        {
            if (_level != null)
                _level.PathBuilt -= OnPathBuilt;

            Clear();
        }

        private void OnPathBuilt(RoadPath path, RoadConfiguration settings)
        {
            Show(path, settings);
        }

        private void Show(RoadPath path, RoadConfiguration settings)
        {
            Clear();

            if (settings.ShowPathLine == false)
                return;

            _lines = new GameObject("Road guidance");
            _lines.transform.SetParent(transform, false);
            Vector3 heightOffset = Vector3.up * settings.PathLineHeight;
            float spacing = Mathf.Max(MinimumSampleSpacing, _sampleSpacing);
            float start = 0f;

            for (int segment = 0; segment < path.Curve.SegmentCount; segment++)
            {
                float end = path.Curve.GetSegmentEndDistance(segment);
                var points = new List<Vector3>();

                for (float distance = start; distance < end; distance += spacing)
                    points.Add(path.Curve.GetPointAtDistance(distance) + heightOffset);

                points.Add(path.Curve.GetPointAtDistance(end) + heightOffset);

                if (points.Count > 1)
                    AddLine(points, BuildGradient(path, segment), settings);

                start = end;
            }
        }

        private Gradient BuildGradient(RoadPath path, int segment)
        {
            Color ownColor = GetPieceColor(path, segment);
            float startBlend = GetBlendHalfLength(path, segment - 1, segment);
            float endBlend = GetBlendHalfLength(path, segment, segment + 1);
            float length = GetSegmentLength(path, segment);
            Color startColor = startBlend > 0f ? Color.Lerp(GetPieceColor(path, segment - 1), ownColor, 0.5f) : ownColor;
            Color endColor = endBlend > 0f ? Color.Lerp(ownColor, GetPieceColor(path, segment + 1), 0.5f) : ownColor;
            var keys = new List<GradientColorKey>
            {
                new GradientColorKey(startColor, 0f)
            };
            float startTime = length > Mathf.Epsilon ? startBlend / length : 0f;
            float endTime = length > Mathf.Epsilon ? 1f - endBlend / length : 1f;

            if (startTime > 0f)
                keys.Add(new GradientColorKey(ownColor, startTime));

            if (endTime < 1f && endTime > startTime)
                keys.Add(new GradientColorKey(ownColor, endTime));

            keys.Add(new GradientColorKey(endColor, 1f));
            var gradient = new Gradient();
            gradient.SetKeys(keys.ToArray(), new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }

        private float GetBlendHalfLength(RoadPath path, int left, int right)
        {
            if (left < 0 || right >= path.Curve.SegmentCount || GetPieceColor(path, left) == GetPieceColor(path, right))
                return 0f;

            float shortest = Mathf.Min(GetSegmentLength(path, left), GetSegmentLength(path, right));
            return Mathf.Min(Mathf.Max(0f, _transitionLength), shortest) * 0.5f;
        }

        private Color GetPieceColor(RoadPath path, int segment)
        {
            return path.Pieces[segment].Type == RoadPieceType.Straight ? _straightColor : _turnColor;
        }

        private float GetSegmentLength(RoadPath path, int segment)
        {
            float start = segment > 0 ? path.Curve.GetSegmentEndDistance(segment - 1) : 0f;
            return path.Curve.GetSegmentEndDistance(segment) - start;
        }

        private void AddLine(List<Vector3> points, Gradient gradient, RoadConfiguration settings)
        {
            var go = new GameObject("Road section");
            go.transform.SetParent(_lines.transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = _material;
            line.startWidth = settings.PathLineWidth;
            line.endWidth = settings.PathLineWidth;
            line.colorGradient = gradient;
            line.positionCount = points.Count;
            line.SetPositions(points.ToArray());
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        private void Clear()
        {
            if (_lines == null)
                return;

            _lines.SetActive(false);
            Destroy(_lines);
            _lines = null;
        }
    }
}
