using UnityEngine;

public class WorldPresentation : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private RaceController _race;
    [SerializeField] private GameObject[] _scenery;
    [SerializeField] private RacerGarage _garage;
    [SerializeField, Min(1f)] private float _decorationSpacing = 45f;
    [SerializeField, Min(1f)] private float _roadClearance = 20f;
    [SerializeField, Min(1f)] private float _groundSize = 10000f;

    [SerializeField] private SceneryBuilder _builder = new SceneryBuilder();
    private GarageAppearanceBinding _appearanceBinding;
    private RoadPath _path;

    private void OnEnable()
    {
        _appearanceBinding = new GarageAppearanceBinding(_player, _garage);
        _level.PathBuilt += OnPathBuilt;

        if (_level.TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings))
            OnPathBuilt(path, settings);
    }

    private void Start()
    {
        ApplyAppearance();
    }

    private void OnDisable()
    {
        _level.PathBuilt -= OnPathBuilt;
        _appearanceBinding.Dispose();
    }

    private void OnDestroy()
    {
        _builder.Dispose();
    }

    public void ApplyAppearance()
    {
        _appearanceBinding.Apply();
    }

    public void Rebuild()
    {
        if (_path == null)
            return;

        _builder.Build(_path, _race.Progress.Environment, _scenery, _decorationSpacing, _roadClearance, _groundSize);
    }

    private void OnPathBuilt(RoadPath path, RoadConfiguration settings)
    {
        _path = path;
        Rebuild();
    }
}
