using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RacerOverheatView : MonoBehaviour
{
    private static readonly int OverheatProgressParameter = Animator.StringToHash("OverheatProgress");
    private static readonly int ExplosionState = Animator.StringToHash("Base Layer.Explosion");
    private static readonly int ExplodedParameter = Animator.StringToHash("Exploded");

    [SerializeField] private Racer _racer;

    private Animator _animator;
    private bool _isExploding;

    public event Action ExplosionCompleted;

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

    private void Update()
    {
        if (_isExploding == false)
            return;

        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

        if (state.fullPathHash == ExplosionState && state.normalizedTime >= 1f)
        {
            _isExploding = false;
            ExplosionCompleted?.Invoke();
        }
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
        _isExploding = true;
        _animator.SetTrigger(ExplodedParameter);
    }

    private void OnRaceRestarted()
    {
        _isExploding = false;
        _animator.Rebind();
        _animator.ResetTrigger(ExplodedParameter);
        _animator.SetFloat(OverheatProgressParameter, 0f);
        _animator.Update(0f);
    }
}
