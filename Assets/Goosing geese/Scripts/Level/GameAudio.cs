using UnityEngine;

public class GameAudio : MonoBehaviour
{
    private const int SampleRate = 22050;
    private const float MusicBeatSeconds = 0.3f;
    private const float EffectSeconds = 0.3f;
    private const float ToneLevel = 0.15f;
    private const float LowEnginePitch = 0.7f;
    private const float HighEnginePitch = 1.8f;

    [SerializeField] private RaceController _race;
    [SerializeField] private Racer _player;

    private AudioSource _music;
    private AudioSource _effects;
    private AudioSource _engine;
    private AudioClip _click;
    private AudioClip _win;
    private AudioClip _lose;

    private void Awake()
    {
        _music = gameObject.AddComponent<AudioSource>();
        _effects = gameObject.AddComponent<AudioSource>();
        _engine = gameObject.AddComponent<AudioSource>();
        _music.loop = true;
        _engine.loop = true;
        _music.clip = CreateTone("Music", new[] { 261.6f, 329.6f, 392f, 329.6f, 293.7f, 349.2f, 440f, 349.2f, 329.6f, 392f, 523.3f, 392f, 293.7f, 349.2f, 392f, 293.7f }, MusicBeatSeconds);
        _engine.clip = CreateTone("Skates", new[] { 90f }, EffectSeconds);
        _click = CreateTone("Click", new[] { 550f }, EffectSeconds);
        _win = CreateTone("Finish", new[] { 392f, 523.3f, 659.3f }, EffectSeconds);
        _lose = CreateTone("Overheat", new[] { 220f, 146.8f, 73.4f }, EffectSeconds);
    }

    private void OnEnable()
    {
        _race.RaceEnded += OnRaceEnded;
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
    }

    private void OnDestroy()
    {
        Destroy(_music.clip);
        Destroy(_engine.clip);
        Destroy(_click);
        Destroy(_win);
        Destroy(_lose);
    }

    public void PlayClick()
    {
        _effects.PlayOneShot(_click, _race.Progress.EffectsVolume);
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
        _effects.PlayOneShot(won ? _win : _lose);
    }
}
