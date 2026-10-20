using UnityEngine;

[DefaultExecutionOrder(100)]
public class RacerLoadout : MonoBehaviour
{
    [SerializeField] private Racer _racer;
    [SerializeField] private RacerGarage _garage;

    private RacerStats _runtimeStats;

    private void OnEnable()
    {
        _garage.SelectionChanged += Apply;
        _garage.UpgradesChanged += Apply;
        Apply();
    }

    private void OnDisable()
    {
        _garage.SelectionChanged -= Apply;
        _garage.UpgradesChanged -= Apply;
    }

    private void OnDestroy()
    {
        if (_racer != null && Application.isPlaying)
            _racer.ConfigureStats(_racer.BaseStats);

        if (_runtimeStats != null)
            Destroy(_runtimeStats);
    }

    private void Apply()
    {
        RacerStats source = _garage.BaseStats;

        if (_runtimeStats == null)
        {
            _runtimeStats = Instantiate(source);
            _runtimeStats.name = "Selected racer stats";
            _runtimeStats.hideFlags = HideFlags.DontSave;
        }

        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), _runtimeStats);
        _garage.ApplyStats(_runtimeStats);
        _racer.ConfigureStats(_runtimeStats);
    }
}
