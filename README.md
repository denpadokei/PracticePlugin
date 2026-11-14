# PracticePlugin

[![Build](https://github.com/denpadokei/PracticePlugin/actions/workflows/build.yml/badge.svg)](https://github.com/denpadokei/PracticePlugin/actions/workflows/build.yml)

A Beat Saber mod that changes playback speed, note jump speed (NJS) and spawn offset in Practice Mode, and adds seeking and section looping to a song. Upstream is [xyonico/PracticePlugin](https://github.com/xyonico/PracticePlugin); this fork is currently maintained for Beat Saber 1.44.1.

![Preview](https://i.imgur.com/vdpHLbR.png)

## Features

- Adds a seek bar and loop controls to the Practice Mode pause menu
- Changes playback speed from 10% to 500% in 5% steps (applied immediately)
- Changes NJS from 1 to 50 and spawn offset from -10 to 20; values are underlined while they match the map defaults
- Drag the seek bar to jump to any position; the new position is applied when you unpause
- Sets a loop section with a start (blue) and an end (red) cursor; the section repeats unless the end cursor sits at the end of the song
- Shows how far you got before failing on the results screen
- Starts Practice Mode with a full energy bar
- Disables NJS and spawn offset editing on Noodle Extensions maps
- Fixes the missing hit detection on already-cut notes when running with ScoreSaber

## Requirements

| Item | Details |
| --- | --- |
| Beat Saber | 1.44.1 |
| Dependencies | BSIPA 4.3.7 or later / SiraUtil 3.3.1 or later / BeatSaberMarkupLanguage 1.14.1 or later / SongCore 3.16.0 or later |

## Installation

1. Set Beat Saber to 1.44.1 (a version manager such as BSManager is recommended)
2. Install BSIPA and the dependencies listed above
3. Download `PracticePlugin-9.1.1-bs1.44.1.zip` from [Releases](https://github.com/denpadokei/PracticePlugin/releases) and copy its `Plugins` folder over the game folder

## Usage

Start a song in Practice Mode and the PracticePlugin UI appears in the pause menu. Practice Mode is entered from the small button next to Play on the song selection screen.

### Seeking

Grab the seek bar in the pause menu and drag it left or right to move the playback position. The position is applied when you unpause. If a loop section is set, the seek position is clamped to that section.

### Loop section

Drag the two cursors above the seek bar to set the section.

- The blue cursor is the loop start
- The red cursor is the loop end
- No loop happens while the red cursor sits at the end of the song
- Playback returns to the start when it reaches the end

### Speed, NJS and spawn offset

Change these from the settings in the pause menu.

| Item | Range | Step |
| --- | --- | --- |
| Speed | 10%–500% | 5% |
| NJS | 1–50 | 1 |
| Spawn Offset | -10–20 | 0.1 |

Speed, NJS and spawn offset return to the map defaults when the song starts or restarts. If you set the Practice Mode start position with the vanilla option, the map begins at that position, so seeking before it finds no notes.

## Configuration

Change these from the `PracticePlugin` tab in the gameplay settings on the song selection screen, or in `UserData/PracticePlugin.json`.

| Item | Type | Default | Description |
| --- | --- | --- | --- |
| StartWithFullEnergy | bool | false | Start Practice Mode with a full energy bar (battery levels excluded) |
| ShowTimeFailed | bool | true | Show the time reached when you failed on the results screen |
| AdjustNJSWithSpeed | bool | false | Adjust NJS along with speed changes |
| DisablePitchCorrection | bool | false | Disable pitch correction when the speed changes |
| EnableGC | bool | false | Explicitly run GC while paused (not shown in the settings UI) |

## Build

```powershell
git clone https://github.com/denpadokei/PracticePlugin.git
cd PracticePlugin
dotnet build .\PracticePlugin\PracticePlugin.csproj -c Release
```

Set `BeatSaberDir` to your Beat Saber install to resolve the game reference assemblies.

The GitHub Actions workflow `build.yml` builds on every push and, when pushing to `master` (or `main`), creates a `PracticePlugin-X.X.X-bs1.44.1` release if that version does not exist yet.

## Credits

Original Plugin made by Xyonico: https://github.com/xyonico/PracticePlugin

## License

MIT License (Copyright (c) 2018 xyonico). See [LICENSE](LICENSE) for details.
