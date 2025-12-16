#Sea Sailer

Did you know BONELAB contains anti-piracy measures?

This mod lets you toggle them on and off at runtime, allowing you to experience exactly what a pirate with a broken crack (or no crack at all) would, at any given moment!


![Sea Sailer preview](https://raw.githubusercontent.com/deathride58/BL-SeaSailer/refs/heads/master/Media/seasailer.gif)

The full implications of this are up for you to discover.

<details>
<summary>Spoiler</summary>
(It's severely underwhelming. But hey, it'd probably make for amusing content!)
</details>


## Compatibility notes

Fusion is completely untested. Due to Fusion lacking its own networking for the `EntitledChild` component (the component that enables GameObjects when the user is a pirate), I suspect this may, at worst, cause desyncs for content that contains the `EntitledChild` component. At best, other users in Fusion simply won't see the box covering Skeleton Pirate's left eye.

The Quest and Meta Link versions of the game are also completely untested by me (I only own the Steam version). This mod's safeguards are a bit lighter for any build that isn't the Steam version, so they *might* work. Due to code surrounding the relevant code being very platform/store-specific, it's hard to guarantee compatibility.

Additionally, this mod does not work on some pirated copies of the game. Yes, rather meta, I know! It's debatable as to whether this is an actual issue.

## Known issues

Sadly, in the latest Steam build of Patch 6, Skeleton Pirate's eye-blocking effect doesn't seem to be properly functional, despite the blocker showing in mirrors. I'm unsure if this is the case in other versions of Patch 6. You'll want to don an eyepatch IRL if you want the full intended experience.

Additionally, the function this mod relies on can sometimes end up reporting false to this mod even when it should be true. This applies to the entire playsession. This prevents the mod from working at all, as the mod relies on this function returning true at least once to verify that you actually do own the game. If this function never returns true, you'll receive a notification within a few seconds of starting the game, advising you to restart.

## Technical details

This mod is primarily concerned with overriding what the game's entitlement check returns. Almost everything that checks the legitimacy of your copy relies on that entitlement check (with the only notable exception being leaderboards, which use a server-side check for a game license). 

This means that, while this mod is enabled in the Bone Menu, most of the game will see you the same as any ordinary pirate who failed to crack the game!

This mod also employs a few safeguards to ensure it doesn't accidentally crack an illegitimate copy of the game. The exact specifics of such will go undetailed. Those familiar with C# are encouraged to check out the source code on GitHub if they're *really* curious.

## Credits

Mod icon - Wikipedia user RootOfAllLight. [Joli_Rouge_icon.svg](https://en.wikipedia.org/wiki/File:Joli_Rouge_icon.svg) used under the Creative Commons Attribution-Share Alike 4.0 International license. [Read more here](https://creativecommons.org/licenses/by-sa/4.0/deed.en).

