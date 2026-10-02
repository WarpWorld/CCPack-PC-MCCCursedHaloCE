# Cursed Halo Crowd Control Effect Pack

## Pack metadata
- **Game display name:** MCC Halo Combat Evolved (Cursed Halo mod)
- **Crowd Control game ID:** `MCCCursedHaloCE`
- **Connector type:** `PCConnector`


This is an effect pack for doing Crowd Control on Halo CE and Cursed Halo CE.
The master branch is the pack for Halo CE with the mod Cursed Halo Again. The pack for base halo is on the normalHalo branch.

## Installation
Requires:
- Master Chief Collection on Steam. If you have it elsewhere, like Game Pass, look into how to install Steam Worshop mods on that version and it should work as well.
- Cursed Halo Again, available in the Steam Workshop here: https://steamcommunity.com/sharedfiles/filedetails/?id=2962107814
   - Although many effects still work on the base Halo CE, some other require either parts of the mod, or custom effect scrip bundled with the mod. Use the build in the normalHalo branch if you want to play normal Halo CE.
- And if trying to run it locally, the CrowdControl SDK. https://developer.crowdcontrol.live/sdk/
   - Copy the contents of this repository on \<folder of the CrowdControl SDK\>/PackProject/HaloCE
- Then run the Crowd Control SDK, click Load Package Source, select MCCCursedHaloCE.cs, and then click Connect.

## How it works

This effect pack works through code injection. It uses three kinds of patterns:
1. Injecting code on game instructions that are run once every game frame (or few frames) and access interesting addresses, like the player data base address, and storing those addresses on custom memory caves that we store a pointer for. That way, we can have a constantly updated pointer to info we may want to read or modify, like health and shields.
2. Injecting code that hijacks the behaviour of instructions, like hijacking the damage received function to multiply the damage value by a factor to increase or decrease it.
3. Communicating with H1 scripts. For many effects, this pack uses custom H1 (Halo CE scripting language) scripts, but H1 has no way of communicating with another process or read any external files. To allow the effect pack to communicate with the scripts, I define a global variable with a specific value in the H1 script, surrounded by two other globals with specific values, and then, using the patter 1, hook into the code that reads script variables and look for those three variables, saving a pointer to the first one. Then the pack uses the pointer to change that variable, which the script reads every frame to determine what it should run.

I have already attempted to do simpler injection using pointermaps, but it does not seem to work on Halo CE.
Also, every injection is based on the base address of halo1.dll, which is loaded inside the MCC. If it is not loaded, the pack will wait until it is.

## Project structure
- The entry point is MCCHaloCE.cs. The //ccpragma in the first line includes all the files into the main file so it can be loaded as an effect pack. It also contains the base elements of Crowd Control.
- LifeCycle contains code that makes sure the pack properly connects to the game, sets everything up, and repairs any problem, like a game crash or exiting to the main menu (which overwrites injections).
- Injections contains all the code that injects assembler. In each injection, the replaced code is copied so that if an update breaks the injections, it is easier to find.
- Effects contains implementations for specific effects.
- Utilities contains methods used by all the other code.
- HaloFiles contains the H1 script code relevant to the effect pack. Note that to modify this it needs to be added on each level of the Cursed Halo source code and rebuild such levels.

## How to compile halo maps

You'll need to have the Cursed Halo Again source. To get it, you'll need to ask InfernoPlus.

1. Download the Halo: CE Mod Tools - MCC on steam, and open its folder (On the steam library, Right click->Manage->Browse local files). You should be on a folder named HCEEK.
2. Get Invader (https://github.com/SnowyMouse/invader) and copy all the executables on the main HCEEK folder.
3. Remove the tags and data folders in the HCEEK folder and replace them with the ones from the Cursed Halo Again source.
4. Copy the contents of HaloFiles/ccHaloModifiedSource in HCEEK, so that the data and tags subfolders merge.
5. Copy custom_buildEverything.bat (in HaloFiles) to the HCEEK folder.
6. In data/levels/d20/scripts, cut d20_cinema.hsc and paste it somewhere else.
7. Open tags/levels/d20.scenario in sapien.exe. Select the Game Window, and press Ctrl+Shift+C to compile the scripts. There should be no errors (text in red).
8. Copy d20_cinema.hsc back to its original place.
9. Run custom_buildEverything.bat
10. All the modified maps should be in the maps folder in HCEEK.

Note that the kart levels are not supported, since they are basically a different game, so just the ones from the Cursed Halo Again you can download from the workshop.

## Contributing

If you want to update broken injections or add new effects, feel free to create a pull request.

## Credits
The song used in the Berserker effect is provided by Vertex https://www.youtube.com/watch?v=IX8bAxvUFnc
The song used in the Sick Beats effect is provided by https://www.youtube.com/watch?v=3z4z7oECbG4
The Cursed Halo mod, including a big portion of the crowd control H1 scripts, were created by InfernoPlus
CrowdControl framework is provided by CrowdControl ( https://crowdcontrol.live/ )

## License

[MIT](https://choosealicense.com/licenses/mit/)

## Repository layout

- `MCCCursedHaloCE.cs` defines the pack.
- `Effects/`, `Injections/`, `LifeCycle/`, and `Utilities/` contain its direct source.