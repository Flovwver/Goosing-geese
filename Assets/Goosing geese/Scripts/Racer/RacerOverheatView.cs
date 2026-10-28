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
    private ParticleSystem[] _explosionParticles;
    private bool _explosionActive;

    public event Action ExplosionCompleted;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _explosionParticles = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particles in _explosionParticles)
        {
            ParticleSystem.MainModule main = particles.main;
            main.stopAction = ParticleSystemStopAction.None;
        }
    }

    private void OnEnable()
    {
        _racer.OverheatProgressChanged += OnOverheatProgressChanged;
        _racer.Exploded += OnExploded;
        _racer.RaceRestarted += OnRaceRestarted;
    }

    private void Update()
    {
        if (_explosionActive == false)
            return;

        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

        if (state.fullPathHash == ExplosionState && state.normalizedTime >= 1f)
        {
            _explosionActive = false;
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
        ResetExplosionParticles();
        _explosionActive = true;
        _animator.SetTrigger(ExplodedParameter);
    }

    private void OnRaceRestarted()
    {
        _explosionActive = false;
        ResetExplosionParticles();
        _animator.Rebind();
        _animator.ResetTrigger(ExplodedParameter);
        _animator.SetFloat(OverheatProgressParameter, 0f);
        _animator.Update(0f);
    }

    private void ResetExplosionParticles()
    {
        foreach (ParticleSystem particles in _explosionParticles)
        {
            particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
