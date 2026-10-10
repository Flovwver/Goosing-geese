using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RacerOverheatView : MonoBehaviour
{
    private static readonly int OverheatProgressParameter = Animator.StringToHash("OverheatProgress");
    private static readonly int ExplodedParameter = Animator.StringToHash("Exploded");

    [SerializeField] private Racer _racer;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _racer.OverheatProgressChanged += OnOverheatProgressChanged;
        _racer.Exploded += OnExploded;
        _racer.RaceRestarted += OnRaceRestarted;
    }

    private void OnDisable()
    {
        _racer.OverheatProgressChanged -= OnOverheatProgressChanged;
        _racer.Exploded -= OnExploded;
        _racer.RaceRestarted -= OnRaceRestarted;
    }

    private void OnOverheatProgressChanged(float progress)
    {
        _animator.SetFloat(OverheatProgressParameter, progress);
    }

    private void OnExploded()
    {
        _animator.SetTrigger(ExplodedParameter);
    }

    private void OnRaceRestarted()
    {
        _animator.Rebind();
        _animator.ResetTrigger(ExplodedParameter);
        _animator.SetFloat(OverheatProgressParameter, 0f);
        _animator.Update(0f);
    }
}
