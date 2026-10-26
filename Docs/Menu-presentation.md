# Menu presentation

The starting menu and garage use the user's Crossy Road reference as a visual direction: cyan panels, a bottom navigation dock, white labels, yellow currency accents, a large game title and alternating light upgrade rows. The live race scene remains visible behind the menu. The garage leaves room above its purchase panel to show the selected model.

GameUiTheme supplies the shared palette and loads the heading/body font pair. GameUiFactory builds labels and buttons using that styling. GameScreens owns the starting-menu decorations; GarageView owns model selection and upgrade presentation. No presentation dependencies were added to the racer or garage models.

The active pair is Rubik Mono One for headings and the pedal prompt, and Russo One for buttons, prices and body text. Both font files and their SIL OFL licenses are bundled in Resources/Fonts/RubikMonoOne and Resources/Fonts/RussoOne. Sources:
- https://github.com/google/fonts/tree/main/ofl/rubikmonoone
- https://github.com/google/fonts/tree/main/ofl/russoone

Poiret One remains available as the previous alternative.

PedalPromptView displays a centered, pulsing random phrase only on the unobstructed start screen. Reopening chooses another phrase without an immediate repeat. Russian and English variants are paired in the Inspector. PedalHaloGraphic draws the soft halo and requires CanvasRenderer. Neither decoration receives pointer raycasts.

## Verification

- Inspected portrait Game View captures of the starting menu, garage and settings.
- Confirmed the caption has centered alignment, a central pivot, zero offset and no rotation, with an unchanged center during scaling.
- Confirmed the halo renders and the decorations do not intercept pedal input.
- Confirmed the Rubik Mono One / Russo One pairing and coverage of Cyrillic letters, digits and UI symbols. All twelve Russian/English pedal prompts fit their block; garage controls stay within the panel in both languages.
- Confirmed all five purchase/back buttons stay within the garage panel.
- Confirmed garage/settings block throttle and hide the pedal decorations.
- Confirmed start chrome is hidden during racing, pause and results.
- Confirmed English garage labels and successful editor compilation.
- No player build or push was performed.
## Semicircular speedometer

SpeedometerDialGraphic draws a fixed 180-degree arc and needle with UI geometry. Zero speed points left, half of the selected maximum points up, and the selected maximum points right. Values are clamped at both ends. Racer thresholds drive the green/yellow/red arc boundaries, including handling upgrades; the numbered scale follows the selected model's upgraded speed. The gauge does not receive pointer raycasts.

Verified needle directions at 0%, 50% and 100%, negative/over-limit values, empty/full color zones, live handling and maximum-speed bindings, portrait layout, and one correctly bound dial after script reload. The old linear Slider components were removed. Unity reported no errors; no player build was made.
## Numeric gauge scale

SpeedometerScaleView replaces the MAX caption with 0, the green/yellow boundary speed, the yellow/red boundary speed and the selected maximum. Displayed speeds are rounded to integers. Coincident boundaries and identical rounded labels share a single numbered tick; nearby labels are separated horizontally when their vertical ranges overlap. Minor ticks divide the same fixed semicircle into ten intervals and are omitted near numbered ticks. Scale spacing and geometry are configurable on the view components.

Verified the default 0/50/75/100 scale, collapsed yellow zone (0/75/100), empty edge zones, zero maximum, upgraded maximum, close boundaries near the center and either end, and valid mesh geometry with merged ticks. Portrait Game View and normal Play/Stop checks reported no errors.
