# Change log

## Build 4

## Gameplay & Balance

- Angry Plants will now spawn higher tier weed flowers.
- New Perk: Homing Shuriken. When hitting an angry, it will aim for the nearest angry.
- Homing perk is available when buying the piercing perk.
- The shop have been moderately reworked:
  - The shop will now show up every 8th day (from 4).
  - It is now possible to buy multiple items from a single shop.
- Perk Balance: Longer Shurikens adds 250ms (from 500ms).
- Perk Balance: Speed Increase is now 50% (from 25%).
- Perk Balance: Piercing Shurikens now pierce through 1 enemy per upgrade (from 2).
- Added a new map to the pool.
- Shurikens should now spawn relative to the center of the player.

## UI & Tools

- Camera will now smoothly drag when reaching left and right border. This is to prepare for map navigation.
- Zoomed out the field view to let more space for UI and props.
- Shurikens and the samurai now have shadows.
- Shurikens visual have been reworked.
- Sounds has been split into Master, SFX and Music channels to help with futur sounds settings.
- UI has been heavily reworked, including new fonts.
- Tile growing animation should be a bit more "growy"...
- Some decors added around the field.
- Walls are now obvious around the field.
- Scene transitions should be a bit smoother and indicates clearly a loading is happening.


## Build 3

### Gameplay
- Auto throw is now toggleable by pressing `R`.
- Piercing is now a Tier Perk starting at 0, augmenting by 2.
- Lots of changes on angries, the field and spawn mechanic:
  - Higher level weed tiles now spawn angry at their location.
  - Larger Angries won't collide with obstacle anymore.
  - Some Angries are much faster now.
  - Angry Health follow a curve based on number of days survived.
  - Day length follows a curve based on number of days survived.
  - Larger Angries have a wider weed spread area (about 30% more).

### UI & Tools
- A health bar has been added to track Weed progression on the field to help understand the losing condition.
- Added several debug tools to test stuff:
  - Open a shop by pressing `O`
  - Spawn an Angry Plant by pressing `I`
  - Give yourself money by pressing `U`
- Small improvements to pause menu
- The game is now maximized at start (non webgl version).

### Bug fixes
- Fixed a bug connecting to `timeout` signal too many times causing some lag issues.
- Fixed Restart bug not clearing all items and variables.
