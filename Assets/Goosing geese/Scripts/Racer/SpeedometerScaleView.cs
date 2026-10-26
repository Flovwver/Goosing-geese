using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(SpeedometerDialGraphic))]
public class SpeedometerScaleView : MonoBehaviour
{
    private const int MarkCapacity = 4;

    private readonly float[] _marks = new float[MarkCapacity];
    private readonly Vector2[] _positions = new Vector2[MarkCapacity];
    private readonly float[] _widths = new float[MarkCapacity];

    [SerializeField] private Text[] _labels = new Text[MarkCapacity];
    [SerializeField, Min(0f)] private float _labelGap = 6f;
    [SerializeField, Min(0f)] private float _labelPadding = 4f;
    [SerializeField, Min(1f)] private float _labelHeight = 24f;

    private SpeedometerDialGraphic _dial;
    private float _maximumSpeed;
    private float _greenEnd;
    private float _yellowEnd;
    private Vector2 _size;
    private int _markCount;
    private bool _hasValues;

    private void OnEnable()
    {
        _dial = GetComponent<SpeedometerDialGraphic>();
        _hasValues = false;
    }

    public void SetValues(float maximumSpeed, float greenEnd, float yellowEnd)
    {
        maximumSpeed = Mathf.Max(0f, maximumSpeed);
        greenEnd = Mathf.Clamp01(greenEnd);
        yellowEnd = Mathf.Clamp(yellowEnd, greenEnd, 1f);
        Vector2 size = _dial.rectTransform.rect.size;

        if (_hasValues && Mathf.Approximately(_maximumSpeed, maximumSpeed)
            && Mathf.Approximately(_greenEnd, greenEnd)
            && Mathf.Approximately(_yellowEnd, yellowEnd) && _size == size)
            return;

        _hasValues = true;
        _maximumSpeed = maximumSpeed;
        _greenEnd = greenEnd;
        _yellowEnd = yellowEnd;
        _size = size;
        _markCount = 0;
        AddMark(0f);
        AddMark(greenEnd);
        AddMark(yellowEnd);
        AddMark(1f);
        _dial.SetScaleMarks(_marks, _markCount);
        LayoutLabels();
    }

    private void AddMark(float value)
    {
        if (_markCount > 0)
        {
            float previous = _marks[_markCount - 1];

            if (Mathf.Approximately(previous, value)
                || Mathf.RoundToInt(previous * _maximumSpeed) == Mathf.RoundToInt(value * _maximumSpeed))
                return;
        }

        _marks[_markCount++] = value;
    }

    private void LayoutLabels()
    {
        for (int index = 0; index < _labels.Length; index++)
        {
            Text label = _labels[index];

            if (label == null)
                continue;

            label.gameObject.SetActive(index < _markCount);

            if (index >= _markCount)
                continue;

            label.text = Mathf.RoundToInt(_marks[index] * _maximumSpeed).ToString();
            _widths[index] = label.preferredWidth + _labelPadding;
            label.rectTransform.sizeDelta = new Vector2(_widths[index], _labelHeight);
            _positions[index] = _dial.GetScalePoint(_marks[index]);

            if (index > 0 && Mathf.Abs(_positions[index].y - _positions[index - 1].y) < _labelHeight)
            {
                float minimumX = _positions[index - 1].x
                    + (_widths[index - 1] + _widths[index]) * 0.5f + _labelGap;
                _positions[index].x = Mathf.Max(_positions[index].x, minimumX);
            }
        }

        float overflow = _positions[_markCount - 1].x + _widths[_markCount - 1] * 0.5f - _dial.rectTransform.rect.xMax;
        float shift = Mathf.Max(0f, overflow);

        for (int index = 0; index < _markCount; index++)
            _labels[index].rectTransform.anchoredPosition = _positions[index] - Vector2.right * shift;
    }
}
