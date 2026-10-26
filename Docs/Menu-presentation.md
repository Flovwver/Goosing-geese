# Menu presentation

The starting menu and garage use the user's Crossy Road reference as a visual direction: cyan panels, a bottom navigation dock, white labels, yellow currency accents, a large game title and alternating light upgrade rows. The live race scene remains visible behind the menu. The garage leaves room above its purchase panel to show the selected model.

GameUiTheme supplies the shared palette and loads Poiret One. GameUiFactory builds labels and buttons using that styling. GameScreens owns the starting-menu decorations; GarageView owns model selection and upgrade presentation. No presentation dependencies were added to the racer or garage models.

PoiretOne-Regular.ttf and OFL.txt are bundled under Resources/Fonts. Source: https://github.com/google/fonts/tree/main/ofl/poiretone (SIL Open Font License, included alongside the font).

PedalPromptView displays a centered, pulsing random phrase only on the unobstructed start screen. Reopening chooses another phrase without an immediate repeat. Russian and English variants are paired in the Inspector. PedalHaloGraphic draws the soft halo and requires CanvasRenderer. Neither decoration receives pointer raycasts.

## Verification

- Inspected portrait Game View captures of the starting menu, garage and settings.
- Confirmed the caption has centered alignment, a central pivot, zero offset and no rotation, with an unchanged center during scaling.
- Confirmed the halo renders and the decorations do not intercept pedal input.
- Confirmed Poiret One on all visible UI text and coverage of the used Cyrillic letters, digits and symbols.
- Confirmed all five purchase/back buttons stay within the garage panel.
- Confirmed garage/settings block throttle and hide the pedal decorations.
- Confirmed start chrome is hidden during racing, pause and results.
- Confirmed English garage labels and successful editor compilation.
- No player build or push was performed.
