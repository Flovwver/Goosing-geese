using System;
using GoosingGeese.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    public sealed class LeaderboardView
    {
        private readonly PlayerProgress _progress;
        private readonly Localizer _text;
        private readonly GameUiFactory _ui;

        public LeaderboardView(PlayerProgress progress, GameUiFactory ui)
        {
            _progress = progress;
            _text = new Localizer(progress);
            _ui = ui;
        }

        public void Render(Transform parent, System.Collections.Generic.List<LeaderboardEntry> entries, Action back)
        {
            string rows = "";

            for (int i = 0; i < entries.Count; i++)
            {
                bool currentPlayer = entries[i].PlayerId == _progress.PlayerId;

                if (i < 5 || currentPlayer)
                    rows += GetRank(entries, i) + ". " + entries[i].Name + " — " + entries[i].Score + (currentPlayer ? " •" : "") + "\n";
            }

            if (entries.Find(entry => entry.PlayerId == _progress.PlayerId) == null)
                rows += (entries.Count + 1) + ". " + _progress.PlayerName + " — " + _progress.Points + " •";

            _ui.CreateLabel(parent, rows, 24).GetComponent<LayoutElement>().preferredHeight = 170f;
            _ui.CreateLabel(parent,
                _text.Choose("Рекорд этого устройства. Онлайн-рейтинг пока не подключён.",
                "Device record. Online rankings are not connected."),
                23);
            _ui.CreateButton(parent, _text.Choose("Назад", "Back"), back);
        }

        private int GetRank(System.Collections.Generic.List<LeaderboardEntry> entries, int index)
        {
            int rank = 1;

            for (int previous = 0; previous < index; previous++)
            {
                if (entries[previous].Score > entries[index].Score)
                    rank++;
            }

            return rank;
        }
    }
}
