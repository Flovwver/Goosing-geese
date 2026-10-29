using System;

public sealed class Localizer
{
    private readonly PlayerProgress _profile;

    public Localizer(PlayerProgress profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    public string Choose(string russian, string english)
    {
        return _profile.English ? english : russian;
    }
}
