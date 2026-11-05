using System;
using GoosingGeese.Racing;
using UnityEngine;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(Animator))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RacerOverheatView")]
    public class RacerOverheatView : MonoBehaviour
    {
        private static readonly int OverheatProgressParameter = Animator.StringToHash("OverheatProgress");
        private static readonly int ExplosionState = Animator.StringToHash("Base Layer.Explosion");
        private static readonly int ExplodedParameter = Animator.StringToHash("Exploded");
        [SerializeField]
        private Racer _racer;
        private Animator _animator;
        private ParticleSystem[] _explosionParticles;
        private bool _explosionActive;

        public event Action ExplosionCompleted;

        public bool HasExploded { get; private set; }

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
            _racer.Exploded += PlayExplosion;
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
            _racer.Exploded -= PlayExplosion;
            _racer.RaceRestarted -= OnRaceRestarted;
        }

        public void PlayExplosion()
        {
            if (HasExploded)
                return;

            HasExploded = true;
            ResetExplosionParticles();
            _explosionActive = true;
            _animator.Play(ExplosionState, 0, 0f);
            _animator.Update(0f);

            foreach (ParticleSystem particles in _explosionParticles)
            {
                if (particles.gameObject.activeInHierarchy)
                    particles.Play(false);
            }
        }

        private void OnOverheatProgressChanged(float progress)
        {
            _animator.SetFloat(OverheatProgressParameter, progress);
        }

        private void OnRaceRestarted()
        {
            HasExploded = false;
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
}
