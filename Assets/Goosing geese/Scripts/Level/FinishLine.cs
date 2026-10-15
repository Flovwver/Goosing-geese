using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FinishLine : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Racer _racer;

    private BoxCollider _trigger;

    private void Awake()
    {
        _trigger = GetComponent<BoxCollider>();
        _trigger.isTrigger = true;
    }

    private void OnEnable()
    {
        _racer.RaceRestarted += OnRaceRestarted;

        if (_level == null)
            return;

        _level.PathBuilt += OnPathBuilt;

        if (_level.TryGetBuiltPath(out RoadPath path, out RoadSettings settings))
            OnPathBuilt(path, settings);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Racer>() != _racer)
            return;

        _racer.FinishRace();

        if (_racer.IsFinished)
            _trigger.enabled = false;
    }

    private void OnDisable()
    {
        _racer.RaceRestarted -= OnRaceRestarted;

        if (_level != null)
            _level.PathBuilt -= OnPathBuilt;
    }

    private void OnPathBuilt(RoadPath path, RoadSettings settings)
    {
        _trigger.enabled = path.FinishPoint != null;

        if (path.FinishPoint == null)
            return;

        transform.SetPositionAndRotation(path.FinishPoint.position, path.FinishPoint.rotation);
    }

    private void OnRaceRestarted()
    {
        _trigger.enabled = _level != null && _level.TryGetBuiltPath(out RoadPath path, out _) && path.FinishPoint != null;
    }
}
