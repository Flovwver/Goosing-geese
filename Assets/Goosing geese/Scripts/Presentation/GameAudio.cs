using GoosingGeese.Racing;
using UnityEngine;

namespace GoosingGeese.Presentation
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "GameAudio")]
    public class GameAudio : MonoBehaviour
    {
        private const int SampleRate = 22050;
        private const float MusicBeatSeconds = 0.3f;
        private const float EffectSeconds = 0.3f;
        private const float ToneLevel = 0.15f;
        private const float LowEnginePitch = 0.7f;
        private const float HighEnginePitch = 1.8f;
        private const float HeatWarningThreshold = 0.65f;
        [SerializeField]
        private RaceController _race;
        [SerializeField]
        private Racer _player;
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
            _music.clip = CreateTone("Music",
                new[] { 261.6f, 329.6f, 392f, 329.6f, 293.7f, 349.2f, 440f, 349.2f, 329.6f, 392f, 523.3f, 392f, 293.7f, 349.2f, 392f, 293.7f },
                MusicBeatSeconds);
            _engine.clip = CreateTone("Skates", new[] { 90f }, EffectSeconds);
            _click = CreateTone("Click", new[] { 550f }, EffectSeconds);
            _win = CreateTone("Finish", new[] { 392f, 523.3f, 659.3f }, EffectSeconds);
            _lose = CreateTone("Rival finish", new[] { 294f, 220f, 196f }, EffectSeconds);
            _heatWarning = CreateTone("Heat warning", new[] { 880f, 660f, 880f }, 0.10f);
            _explosion = CreateExplosion();
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

        private void OnDestroy()
        {
            Destroy(_music.clip);
            Destroy(_engine.clip);
            Destroy(_click);
            Destroy(_win);
            Destroy(_lose);
            Destroy(_heatWarning);
            Destroy(_explosion);
        }

        public void PlayClick()
        {
            PlayEffect(_click);
        }

        private static AudioClip CreateExplosion()
        {
            const float Duration = 0.45f;
            int count = Mathf.RoundToInt(SampleRate * Duration);
            var samples = new float[count];
            var random = new System.Random(151);

            for (int index = 0; index < count; index++)
            {
                float age = (float)index / count;
                float decay = (1f - age) * (1f - age);
                float noise = (float)random.NextDouble() * 2f - 1f;
                float bass = Mathf.Sin(2f * Mathf.PI * 28f * (2f * age - age * age));
                samples[index] = (noise * 0.45f + bass * 0.55f) * decay * Mathf.Min(1f, index / 80f) * 0.45f;
            }

            AudioClip clip = AudioClip.Create("Explosion", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateTone(string name, float[] frequencies, float noteDuration)
        {
            int samplesPerNote = Mathf.RoundToInt(SampleRate * noteDuration);
            float[] samples = new float[samplesPerNote * frequencies.Length];

            for (int note = 0; note < frequencies.Length; note++)
            {
                for (int sample = 0; sample < samplesPerNote; sample++)
                {
                    float envelope = Mathf.Sin(Mathf.PI * sample / samplesPerNote);
                    samples[note * samplesPerNote + sample] = Mathf.Sin(2f * Mathf.PI * frequencies[note] * sample / SampleRate) * envelope * ToneLevel;
                }
            }

            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
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
