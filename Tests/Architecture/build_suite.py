"""Generate one in-memory Unity test entry point without importing tests into Assets."""
from pathlib import Path
import re

root = Path(__file__).resolve().parents[2]
usings, bodies, calls = set(), [], []
for source in sorted(Path(__file__).parent.glob('*Tests.cs')):
    text = source.read_text(encoding='utf-8-sig')
    lines = text.splitlines()
    usings.update(line for line in lines if line.startswith('using '))
    bodies.append('\n'.join(line for line in lines if not line.startswith('using ')))
    prefix = 'await ' if re.search(r'async\s+Task<string>\s+Main', text) else ''
    calls.append(f'            reports.Add({prefix}{source.stem}.Main());')

runner = r'''
public static class ArchitectureSuite
{
    public static async System.Threading.Tasks.Task<string> Main()
    {
        string[] keys = { "GoosingGeese.Profile.v1", "GoosingGeese.Leaderboard.v1" };
        bool[] exists = System.Array.ConvertAll(keys, UnityEngine.PlayerPrefs.HasKey);
        string[] values = System.Array.ConvertAll(keys, key => UnityEngine.PlayerPrefs.GetString(key));
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        string profile = race.Progress.ToJson();
        float timeScale = UnityEngine.Time.timeScale;
        var reports = new System.Collections.Generic.List<string>();

        try
        {
CALLS
            return string.Join("\n", reports);
        }
        finally
        {
            race.enabled = true;
            race.Progress.RestoreJson(profile, UnityEngine.Resources.Load<RacerAppearanceCatalog>("RacerAppearanceCatalog"));
            UnityEngine.Object.FindFirstObjectByType<RacerGarage>().EndPreview();
            UnityEngine.Object.FindFirstObjectByType<Level>().RestartRace();
            UnityEngine.Time.timeScale = timeScale;

            for (int index = 0; index < keys.Length; index++)
            {
                if (exists[index])
                    UnityEngine.PlayerPrefs.SetString(keys[index], values[index]);
                else
                    UnityEngine.PlayerPrefs.DeleteKey(keys[index]);
            }

            UnityEngine.PlayerPrefs.Save();
        }
    }
}
'''.replace('CALLS', '\n'.join(calls))
output = root / 'Temp' / 'ArchitectureSuite.cs'
output.parent.mkdir(exist_ok=True)
output.write_text('\n'.join(sorted(usings)) + '\n' + '\n'.join(bodies) + runner, encoding='utf-8')
print(output)
