using UnityEngine;

public class PedalPulse : MonoBehaviour
{
    [SerializeField] private RaceController _race;
    [SerializeField, Min(0f)] private float _amplitude = 0.06f;
    [SerializeField, Min(0f)] private float _frequency = 3f;
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        float pulse = _race.State == RaceState.Waiting ? 1f + Mathf.Sin(Time.unscaledTime * _frequency) * _amplitude : 1f;
        transform.localScale = _baseScale * pulse;
    }
}
