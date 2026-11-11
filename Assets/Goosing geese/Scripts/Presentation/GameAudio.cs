using GoosingGeese.Racing;
using UnityEngine;

namespace GoosingGeese.Presentation
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "GameAudio")]
    public class GameAudio : MonoBehaviour
    {
        private const string AudioResources = "RaceAudio/";
        private const float LowEnginePitch = 0.7f;
        private const float HighEnginePitch = 1.8f;
        private const float HeatWarningThreshold = 0.65f;

        [SerializeField] private RaceController _race;
        [SerializeField] private Racer _player;
        private AudioSource _music;
        private AudioSource _effects;
        private AudioSource _engine;
        private AudioClip _click;
        private AudioClip _win;
        private AudioClip _lose;
        private AudioClip _heatWarning;
        private AudioClip _explosion;
        private bool _heatWarningPlayed;

        private void Awake()
        {
            _music = gameObject.AddComponent<AudioSource>();
            _effects = gameObject.AddComponent<AudioSource>();
            _engine = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _engine.loop = true;
            _music.clip = Resources.Load<AudioClip>(AudioResources + "Music");
            _engine.clip = Resources.Load<AudioClip>(AudioResources + "Skates");
            _click = Resources.Load<AudioClip>(AudioResources + "Click");
            _win = Resources.Load<AudioClip>(AudioResources + "Finish");
            _lose = Resources.Load<AudioClip>(AudioResources + "RivalFinish");
            _heatWarning = Resources.Load<AudioClip>(AudioResources + "HeatWarning");
            _explosion = Resources.Load<AudioClip>(AudioResources + "Explosion");
        }

        private void OnEnable()
        {
            _race.RaceEnded += OnRaceEnded;
            _player.OverheatProgressChanged += OnHeatProgressChanged;
        }

        private void Start()
        {
            _music.Play();
            _engine.Play();
        }

        private void Update()
        {
            bool paused = _race.State == RaceState.Paused;
            _music.volume = paused ? 0f : _race.Progress.MusicVolume;
            _effects.volume = _race.Progress.EffectsVolume;
            _engine.volume = _race.State == RaceState.Racing ? _race.Progress.EffectsVolume * _player.NormalizedSpeed : 0f;
            _engine.pitch = Mathf.Lerp(LowEnginePitch, HighEnginePitch, Mathf.Clamp01(_player.NormalizedSpeed));
        }

        private void OnDisable()
        {
            _race.RaceEnded -= OnRaceEnded;
            _player.OverheatProgressChanged -= OnHeatProgressChanged;
        }

        public void PlayClick()
        {
            PlayEffect(_click);
        }

        private void OnRaceEnded(bool won)
        {
            if (_race.Outcome == RaceOutcome.Overheated || _race.Outcome == RaceOutcome.OffRoad)
                PlayEffect(_explosion);
            else
                PlayEffect(won ? _win : _lose);
        }

        private void OnHeatProgressChanged(float progress)
        {
            if (progress <= HeatWarningThreshold * 0.5f)
                _heatWarningPlayed = false;

            if (_heatWarningPlayed || progress < HeatWarningThreshold || progress >= 1f || _race.State != RaceState.Racing)
                return;

            _heatWarningPlayed = true;
            PlayEffect(_heatWarning);
        }

        private void PlayEffect(AudioClip clip)
        {
            _effects.volume = _race.Progress.EffectsVolume;
            _effects.PlayOneShot(clip);
        }
    }
}
