# YAL's Dawnsbury Days Mods



## Building
You'll want to set a `DAWNSBURY_DAYS` environment variable to point at where the game folder resides (where the `.exe` and `CREDITS.txt` are, no trailing backslash).

You can then open the Visual Studio (regrettably, it would seem like it has to be VS2026 because of .NET 10) and build the project.

The post-build step should copy the DLL from `bin` to the game's `CustomMods` folder,
but if it doesn't, you may have to do so by hand.

Post-build step will also attempt to update the DLL in the `export` folder that contains the files for publishing on Steam Workshop.

## Credits
Mods by YellowAfterlife

Check individual mods' READMEs for mod-specific notes!