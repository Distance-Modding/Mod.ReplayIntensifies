# Distance Replay Intensifies

Distance mod to extend customization for Ghost and Replay Mode cars.

This started off as a port of Reherc's [Replay Limit Breaker](https://github.com/REHERC/Replay-Limit-Breaker) mod for Spectrum mod loader.

## Current Options

* Maximum number of replay cars can be raised up to 1000.
* (Optional) Separate maximum number of replay cars when selecting from the leaderboards menu.
* Maximum number of saved local replays can be raised up to 10000.
* Maximum number of online leaderboard rankings can be raised up to 10000.
* Change the visual style for Ghost and/or Replay Mode cars:
    * **Ghost** - No solid body.
    * **Networked** - Semi solid body that fades away when colliding with the player.
    * **Replay** - Full solid body that doesn't fade away.
    * **Outline**/**no Outline** - Show or hide the car outline color for any of the 3 above styles.
* Change the minimum and maximum car Level of Detail (LOD), to help improve performance or up the graphics.
* Disable clamping opponent car colors, so that ultrabright color presets can be seen (this affects Online mode cars as well).
* Change whether the data materialization effect for car spawning/finish-despawning is used when racing ghosts.
* Fill remaining auto slots with local replays when there aren't enough online replays to load.

### Steam Rivals

An experimental feature that makes selected players visually stand out when racing, so that you can spot your *true* opponent from far away (rivals are also highlighted differently in the level select leaderboards menu). Steam Rivals can be added or removed from the level select leaderboards menu, or by editing `Settings/Config.json`.

* Enable or disable Steam Rivals (disabled by default).
* Change whether Steam Rivals are highlighted differently in the level select leaderboards menu.
* Change whether Steam Rival car styles are used for ghosts.
* Change whether Steam Rival car styles are used in Replay Mode.
* Change whether Steam Rival car styles are used for your own ghosts/replays.
* Change the visual style for Steam Rival cars.
* Change the outline brightness for Steam Rival cars.

### Randomized Cars

A feature that enables changing up the car types and colors used by other replays. With this you can change all your local replays (that probably use the same car) to something with more variety. Randomness can also be fixed so that you can keep track of the specific replays across multiple runs.

* Enable or disable Randomized Cars (disabled by default).
* Change whether cars are randomized for local and/or online leaderboards replays.
* Change whether cars are randomized for Steam Rival replays (requires Steam Rivals to be enabled).
* Change whether online replays using the Kickstarter backer car will also be randomized (randomization is disabled by default).
* Change how randomness is determined for a car's type/colors:
    * **Always Random** - Randomness is not fixed, and will be different every time.
    * **By Replay** - Randomness is fixed and determined by the replay's data, so a sepecific replay will always use the same random appearance.
    * **By Placement** - Randomness is fixed and determined by the placement out of all the current replays (excluding the car from your current run).
* Add an extra randomness seed that's combined with fixed randomness to change up what you'll normally see.
* Change how random car types are selected:
    * **Off** - Car types won't be randomized.
    * **Car Types** - Default behavior where duplicates are allowed. Choose cars based on individual weighted values.
    * **Car Types (Cycle)** - Cycle through all available car types before choosing duplicates.
* Change how random car colors are selected:
    * **Off** - Car colors won't be randomized.
    * **Color Presets** - Default behavior where duplicates are allowed. Colors are chozen from your car color presets.
    * **Color Presets (Cycle)** - Cycle through all available color presets before choosing duplicates.
    * **HSV** - Colors are generated with random Hue/Saturation/Value channels.
    * **RGB** - Colors are generated with random Red/Green/Blue channels.
    * **Default Colors** - Cars will always use their own default color preset. Useful for custom cars that aren't intended to be recolored.
* Change individual weights of vanilla and custom cars, so that certain cars will be more likely to get chosen.
* Give default weights to all custom cars, so that newly-added cars will automatically be included.
* Prevent choosing random car types that the user hasn't unlocked yet (enabled by default).



## Known Bugs

* Replay/networked style cars can only display somewhere between 12-20 car HUDs. Having more than this many opponents will prevent *any(?)* opponent car HUDs from rendering.
* The NitronicCarController can cause physics instability when too many ghosts are using it at once. So a threshold has been set to disable this for ghosts when there are too many.


## Preview

> #### **[Distance: Rush Hour - Friction (50 Replays at once)](https://www.youtube.com/watch?v=tsvFKG1aANU)**
> [![Distance: Rush Hour - Friction (50 replays at once)](https://img.youtube.com/vi/tsvFKG1aANU/0.jpg)](https://www.youtube.com/watch?v=tsvFKG1aANU)

