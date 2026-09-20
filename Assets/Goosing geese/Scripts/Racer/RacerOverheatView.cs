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
    }

    private void OnDisable()
    {
        _racer.OverheatProgressChanged -= OnOverheatProgressChanged;
        _racer.Exploded -= OnExploded;
    }

    private void OnOverheatProgressChanged(float progress)
    {
        _animator.SetFloat(OverheatProgressParameter, progress);
    }

    private void OnExploded()
    {
        _animator.SetTrigger(ExplodedParameter);
    }
}
