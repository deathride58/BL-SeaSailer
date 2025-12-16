using MelonLoader;
using HarmonyLib;
using Il2CppSLZ.Marrow.Utilities;
using BoneLib.BoneMenu;
using System.Reflection;
using Il2CppCysharp.Threading.Tasks;
using BoneLib;
using BoneLib.Notifications;
using MelonLoader.Utils;
using System.Security.Cryptography;

[assembly: MelonInfo(typeof(SeaSailer.SeaSailer), "SeaSailer", "1.0.0", "Bhijn", null)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
[assembly: MelonAuthorColor(255, 198, 119, 230)]
[assembly: MelonAdditionalDependencies("BoneLib")]

[assembly: AssemblyDescription("Allows toggling Bonelab's anti-piracy features at runtime")]

namespace SeaSailer;

public class SeaSailer : MelonMod
{
    internal static MelonLogger.Instance Logger;

    // a lot of this boilerplate is a copypaste from virtualstockdelete because i do NOT need to reinvent the wheel here lmao
    public static MelonPreferences_Category SailorCategory;
    public static MelonPreferences_Entry<bool> MelonSailingSeas;

    public static BoneLib.BoneMenu.Page SailorPage;
    public static BoolElement BonedSailingSeas;

    public static bool SailingSeas;

    public static bool ProperlyInitialized = false;
    public static bool GenuinelyPassedCheck = false;
    public static bool CheckFailureWarned = false;
    public static bool GameInitialized = false;
    public static bool PotentiallyPirated = false;

    public override void OnInitializeMelon()
    {
        SailorCategory = MelonPreferences.CreateCategory("SeaSailer");
        MelonSailingSeas = SailorCategory.CreateEntry("SailingSeas", false, "Anti-piracy measures enabled?");
        SailingSeas = MelonSailingSeas.Value;
        Logger = LoggerInstance;

        SailorPage = BoneLib.BoneMenu.Page.Root.CreatePage("Sea Sailer", UnityEngine.Color.white, createLink: false);
        
        BonedSailingSeas = SailorPage.CreateBool("Sail the seven seas?", UnityEngine.Color.white, SailingSeas, YarrMatey);

        Hooking.OnMarrowGameStarted += HoistSail;
        Hooking.OnLevelLoaded += SanityCheck;

#if DEBUG
        Logger.Msg("Game name: " + MelonEnvironment.GameExecutableName);
#endif
        if (!BoneLib.HelperMethods.IsAndroid() && MelonEnvironment.GameExecutableName == "BONELAB_Steam_Windows64")
        {
            // for a legitimate copy of the steam version, it can be assumed that the user is running the latest build of patch 6
            // there's no reason to be running an older version of steam patch 6 because old versions are borderline bricked on the latest builds of steamVR
            // additionally, there's not much reason to be modifying the game's executables or steamapi dll directly-- either being modified is insanely fishy
            // there's not even any DLC for DLC unlockers to be an excuse!
            string gameversion = MelonLoader.InternalUtils.UnityInformationHandler.GameVersion;
            if (gameversion == "1.744.58126")
            {
                MD5 md5 = MD5.Create();
                FileStream game = File.OpenRead(MelonEnvironment.GameExecutablePath);
                FileStream steamlib = File.OpenRead(MelonEnvironment.UnityGameDataDirectory + "\\Plugins\\x86_64\\steam_api64.dll");
                byte[] gamehash = md5.ComputeHash(game);
                byte[] steamlibhash = md5.ComputeHash(steamlib);

                // this is a very simple, non-obfuscated checksum because it honestly doesn't need to be any more complicated than this
                // with this mod being open source, the threat model here only really cares about nagging the average pirate who doesn't know much beyond downloading repacks
                // as a sidenote this is a hell of a lot more robust than just caring about where the game's installed to x3
                long combinedchecksum = BitConverter.ToInt64(gamehash) - BitConverter.ToInt64(gamehash, 8) + BitConverter.ToInt64(steamlibhash) - BitConverter.ToInt64(steamlibhash, 8);
#if DEBUG
                Logger.Msg("Game MD5: " + Convert.ToHexString(gamehash));
                Logger.Msg("SteamAPI MD5: " + Convert.ToHexString(steamlibhash));
                Logger.Msg("Combined hash: " + combinedchecksum.ToString());
#endif
                game.Dispose();
                steamlib.Dispose();

                if (combinedchecksum != 768354071345851907)
                    PotentiallyPirated = true;
#if DEBUG
                Logger.Msg("Piracy detected? " + (PotentiallyPirated ? "Yeah" : "Nah"));
#endif
            }
            else if (gameversion == "1.702.57346")
            {
                // this is the old version that's bricked under steamVR. there is no legitimate reason to be running this
                // not to be confused with the latest version of the oculus win64 build, 1.703.57485
                PotentiallyPirated = true;
            }
        }

        ProperlyInitialized = true;

#if DEBUG
        Logger.Msg("Initialized.");
#endif
    }

    public override void OnPreferencesLoaded()
    {
        if (!ProperlyInitialized)
            return;

        SailingSeas = MelonSailingSeas.Value;
        BonedSailingSeas.Value = SailingSeas;
    }

    public static void YarrMatey(bool value)
    {
        SailingSeas = value;
        MelonSailingSeas.Value = value;
        SailorCategory.SaveToFile(false);
    }

    public static void HoistSail()
    {
#if DEBUG
        Logger.Msg("Marrow started; Sea Sailer now active");
#endif
        GameInitialized = true;

        if (PotentiallyPirated)
        {
            Task.Run(async () =>
            {
#if DEBUG
                await Task.Delay(3000);
#else
                await Task.Delay(30 * 60 * 1000);
#endif
                Notifier.Send(new Notification()
                {
                    Title = "Notice",
                    Message = "You seem to enjoy this game!\nYou should support the developers, and buy it.",
                    ShowTitleOnPopup = true,
                    PopupLength = 30f,
                    Type = NotificationType.Information
                });
            });
        }
    }

    // sometimes CheckEntitlementAsync returns false to the melon despite the UniTask being finished, for the entirety of a playsession
    // except it responds to changes to what it's returned despite being false as far as this melon is concerned???????
    // the kicker is that this happens so rarely that properly debugging this is an absolute pain in the ass
    // i get that async is funky but seriously what's up with this higgs bugson
    // is it a melonloader bug?? is it il2cpp jank?? is it a hidden layer of the antipiracy?? is it just a skill issue??
    // who the fuck knows!!!!
    public static void SanityCheck(LevelInfo info)
    {

        if (GenuinelyPassedCheck || CheckFailureWarned)
        {
            return;
        }

        // since avatar is always set after loading into a level, it can be assumed that there will always be an attempt to check entitlement some time after a level load
        Task.Run(async () =>
        {
#if DEBUG
            Logger.Msg("Didn't pass sanity check; waiting");
#endif

            await Task.Delay(5000);

            if (GenuinelyPassedCheck || CheckFailureWarned)
            {
#if DEBUG
                Logger.Msg("Passed sanity check!");
#endif
                return;
            }

            Logger.Msg("Entitlement check failed or is inactive this session.");
            Logger.Msg("Sea Sailer will be inactive until restart or check passes.");
            Notifier.Send(new Notification()
            {
                Title = "Sea Sailer",
                Message = "Entitlement check failed or is bugged this session.\nSea Sailer will be inactive until restart or check passes.",
                ShowTitleOnPopup = true,
                PopupLength = 10f,
                Type = NotificationType.Warning
            });

            CheckFailureWarned = true;
        });
    }

    // this should only be called if the game itself has verified the player owning a copy of the game at least once, through any method that can verify it
    public static void AllowPiracy() 
    {
        GenuinelyPassedCheck = true;
        BoneLib.BoneMenu.Page.Root.CreatePageLink(SailorPage);
    }

    [HarmonyPatch(typeof(MarrowEntitlement), "CheckEntitlementAsync")]
    public static class EntitlementCheck
    {
        static void Postfix(ref UniTask<bool> __result)
        {
#if DEBUG
            // psst, you. that comment above sanitycheck? here's the debug info showing that i'm not shitting you here
            Logger.Msg("True entitlement is: " + (__result.result ? "true" : "false"));
            Logger.Msg("Status is: " + (int)__result.Status);
#endif
            if (__result.result == true && !GenuinelyPassedCheck)
                AllowPiracy();

            if (!GameInitialized)
                return;

            // it's incredibly trivial to crack the steam version, but the quest and rift store versions have DRM that's actually worth a damn
            // so hey, might as well ensure that this funny DLL isn't going to accidentally behave as a standalone crack for the quest skiddies lmao
            // this isn't gonna stop those that know how to recompile dotnet projects, but those that do probably already know how to crack games
            // though of course getting lemonloader working on a pirated copy of bonelab is apparently a challenge to begin with
            if (GenuinelyPassedCheck)
                __result.result = !SailingSeas;
        }
    }
}