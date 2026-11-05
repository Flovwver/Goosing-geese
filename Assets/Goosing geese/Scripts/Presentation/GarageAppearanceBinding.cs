using System;
using GoosingGeese.Garage;
using GoosingGeese.Racing;
using GoosingGeese.Settings;

namespace GoosingGeese.Presentation
{
    public sealed class GarageAppearanceBinding : IDisposable
    {
        private readonly Racer _player;
        private readonly RacerAppearance _appearance;
        private readonly RacerGarage _garage;

        public GarageAppearanceBinding(Racer player, RacerGarage garage)
        {
            _player = player;
            _appearance = player.GetComponent<RacerAppearance>();
            _garage = garage;
            _player.RaceRestarted += Apply;
            _garage.PreviewChanged += Apply;
        }

        public void Dispose()
        {
            _player.RaceRestarted -= Apply;
            _garage.PreviewChanged -= Apply;
        }

        public void Apply()
        {
            RacerAppearanceDefinition appearance = _garage.PreviewEntry;

            if (appearance == null)
                return;

            if (appearance.Model != null)
                _appearance.ShowModel(appearance.Model);
            else
                _appearance.ShowOriginal(appearance.Tint);
        }
    }
}
