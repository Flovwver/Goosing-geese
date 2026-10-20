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
        if (_level == null)
            return;

        _level.PathBuilt += OnPathBuilt;

        if (_level.TryGetBuiltPath(out RoadPath path, out RoadSettings settings))
            OnPathBuilt(path, settings);
    }

    private void OnTriggerEnter(Collider other)
    {
        Racer racer = other.GetComponentInParent<Racer>();

        if (racer != null)
            racer.FinishRace();
    }

    private void OnDisable()
    {
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
}
