
# Escape the Reaper

A 3D hyper-casual lane-runner built in Unity. Run, jump, and slide through a haunted world while a reaper-like stalker trails you the entire way — collect coins, survive as long as you can, and spend your coins on new character skins.

Built as a game development course project (CS443), with AI assistance (Claude, by Anthropic) used throughout for scripting, level design tooling, and game systems.

---

## Gameplay

- **Endless 3-lane runner** — the character runs forward automatically; you switch lanes, jump, and slide to avoid obstacles.
- **Gradual speed ramp** — each run starts at a manageable pace and gently speeds up the longer you survive.
- **2-hit health system** — take two hits and the run ends. No lives, no hearts, just a health bar.
- **The Stalker** — a reaper-like enemy follows you throughout the run. He's hidden most of the time but appears at the start, after every hit, and dramatically during your death — carrying your body off before the end screen shows.
- **3 themed levels** — a haunted mansion, a blood-red dungeon, and a toxic crypt, each with increasing difficulty.
- **Coins & Skins Shop** — coins collected during runs are saved permanently. Spend them in the main menu shop to unlock and equip different character skins (used across all three levels).
- **Settings** — master volume control, saved between sessions.

## Controls

| Action | Keyboard | Mobile |
|---|---|---|
| Switch lanes | Arrow keys / A, D | Swipe left / right |
| Jump | Space | Swipe up |
| Slide | S | Swipe down |
| Pause | Esc | — |

## Tech Stack

- **Engine:** Unity 6.5 (6000.5.0f1)
- **Render Pipeline:** Built-In Render Pipeline
- **Language:** C#
- **Character models & animations:** [Adobe Mixamo](https://www.mixamo.com) (Humanoid rigs, retargeted animations)
- **Audio:** Original synthesized background music and sound effects

## Project Structure

```
Assets/
  Scripts/
    Managers/   - GameManager, HealthManager, ScoreManager, AudioManager, LevelSettings, SkinManager
    Player/     - PlayerController, PlayerSpawner, CameraFollow, MobileInput
    Enemies/    - IHazard, GhostEnemy, SpikeTrap, MovingHazard, Stalker
    Level/      - Coin, LevelGenerator
    UI/         - UIManager, MainMenuController, SettingsController, CoinDisplay, ShopController
  Scenes/       - MainMenu, Level1, Level2, Level3
  Prefabs/      - Player skins, Stalker, hazards, tiles
  Animations/   - Character FBX models and shared animation clips
  Audio/        - Background music and sound effects
```

## Key Systems

- **State-machine driven** — the overall game (`GameManager`) and the Stalker enemy both use enum-based state machines (e.g. `MainMenu / Playing / Paused / Dying / GameOver`) to keep behavior predictable and avoid conflicting flags.
- **Interface-based hazards** — every hazard (spikes, ghost, moving obstacles) implements a shared `IHazard` interface, so the player's collision code doesn't need to know the specific type of what hit it.
- **Endless tile-based level generation** — the track is built from short, recyclable segments spawned ahead of the player and removed behind them, grouped into difficulty pools that unlock further into a run.
- **Persistent progression** — coins, owned skins, the equipped skin, and high score are all saved locally between sessions via `PlayerPrefs`.

## Running the Project

1. Install **Unity 6000.5.0f1** (or a compatible Unity 6.5 build) via Unity Hub.
2. Clone this repository.
3. Open the project folder in Unity Hub (**Projects → Add → Add project from disk**).
4. Open the `MainMenu` scene under `Assets/Scenes` and press Play.

## Credits

- Character models and animations: [Mixamo](https://www.mixamo.com) (Adobe) — free for personal, academic, and commercial use.
- Game design, programming, and content built with the assistance of **Claude (Anthropic)**.

## License

This project was created for educational purposes as part of a university course assignment.
