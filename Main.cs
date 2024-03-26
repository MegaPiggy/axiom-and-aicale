using Epic.OnlineServices;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom
{
    public class Main : ModBehaviour
    {
        private static Main instance;
        private INewHorizons _newHorizons;
        public static INewHorizons NewHorizonsInstance => instance._newHorizons;

        private void Awake()
        {
            instance = this;
        }

        public static Dictionary<AudioType, string> AudiosToReplace = new Dictionary<AudioType, string>
        {
            {
                AudioType.EndOfTime,
                "audio/End Times (Short) 041013 AP.wav"
            },
            {
                AudioType.ShipCockpitAutopilotActivate,
                "audio/Computer_Interface_Retro.wav"
            },
            {
                AudioType.PlayerSuitJetpackBoost,
                "audio/JetPack.wav"
            },
            {
                AudioType.PlayerSuitCriticalWarning,
                "audio/JetPack_NotificationBeep.wav"
            },
            {
                AudioType.PlayerSuitOxygenRefill,
                "audio/OxygenRefill_Breath.wav"
            },
            {
                AudioType.ShipCockpitProbeLaunch,
                "audio/ProbeLaunch_HighPower.wav"
            },
            {
                AudioType.ShipCockpitProbeLaunchUnderwater,
                "audio/ProbeLaunch_LowPower.wav"
            },
            {
                AudioType.ShipCabinAmbience,
                "audio/SpaceshipAmbience_louder.wav"
            },
            {
                AudioType.PlayerBreathing_LP,
                "audio/SpacesuitAmbience.wav"
            },
            {
                AudioType.PlayerSuitRemoveSuit,
                "audio/SuitOff.wav"
            },
            {
                AudioType.PlayerSuitWearSuit,
                "audio/SuitUp_Faster.wav"
            },
            {
                AudioType.Sun_Ambience_LP,
                "audio/Sun_Surface.wav"
            },
            {
                AudioType.Sun_Explosion,
                "audio/Supernova_Explosion3.wav"
            },
            {
                AudioType.Sun_Collapse,
                "audio/Supernova_Start2_Longer.wav"
            },
            {
                AudioType.Sun_SupernovaWall_LP,
                "audio/Supernova_Wave4.wav"
            },
            {
                AudioType.TH_ZeroGTrainingAllRepaired,
                "audio/SystemBackOnline.wav"
            },
            {
                AudioType.ShipThrustIgnition,
                "audio/ThrusterIgnition_crossfade.wav"
            },
            {
                AudioType.ShipThrustTranslational_LP,
                "audio/Thrusters_HighPower_New.wav"
            },
            {
                AudioType.ShipThrustTranslationalUnderwater_LP,
                "audio/Thrusters_LowPower_New.wav"
            },
            {
                AudioType.Flashback_End,
                "audio/TimeLoop_End_LOUD.wav"
            },
            {
                AudioType.Flashback_Overlay_1_LP,
                "audio/TimeLoop_Middle_LOUD.wav"
            },
            {
                AudioType.Flashback_Overlay_2_LP,
                "audio/TimeLoop_Middle_LOUD.wav"
            },
            {
                AudioType.Flashback_Base_LP,
                "audio/TimeLoop_Middle_LOUD.wav"
            },
            {
                AudioType.GD_UnderwaterAmbient_LP,
                "audio/UnderWaterAmbience.wav"
            },
            {
                AudioType.TH_Observatory,
                "audio/Observatory 062913 AP.wav"
            },
        };
        public const int EndOfTime = (int)AudioType.EndOfTime;

        private void Start()
        {
            _newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            _newHorizons.LoadConfigs(this);

            LoadAllClips(); //load audio

            ModHelper.HarmonyHelper.AddPostfix<AudioManager>(nameof(AudioManager.Awake), typeof(Main), nameof(Main.ReplaceInAudioManager));
            ModHelper.HarmonyHelper.AddPostfix<AudioLibrary>(nameof(AudioLibrary.BuildAudioEntryDictionary), typeof(Main), nameof(Main.ReplaceInAudioLibrary));

            _newHorizons.GetBodyLoadedEvent().AddListener((string name) =>
            {
                if (_newHorizons.GetCurrentStarSystem() == "Jam3")
                {
                    if (name == "Axiom") OnAxiomLoaded(_newHorizons.GetPlanet("Axiom"));
                    else if (name == "Aicale") OnAicaleLoaded(_newHorizons.GetPlanet("Aicale"));
                    else if (name == "Broken Satellite") OnBrokenSatelliteLoaded(_newHorizons.GetPlanet("Broken Satellite"));
                }
            });


            //I was gonna do this but I don't want anything static or bramble related to mess stuff up
            //ModHelper.Events.Unity.FireInNUpdates(IgnoreCompat, 2);
        }

        private void OnAxiomLoaded(GameObject axiom)
        {
            ModHelper.Console.WriteLine("Axiom has loaded", MessageType.Info);
        }

        private void OnAicaleLoaded(GameObject aicale)
        {
            ModHelper.Console.WriteLine("Aicale has loaded", MessageType.Info);
            var astroObject = aicale.GetComponent<NHAstroObject>();
            astroObject._primaryBody._moon = astroObject;
        }

        private void OnBrokenSatelliteLoaded(GameObject brokenSatellite)
        {
            ModHelper.Console.WriteLine("Broken Satellite has loaded", MessageType.Info);
            brokenSatellite.AddComponent<AlignWithJamSun>();
            brokenSatellite.GetComponent<Rigidbody>().mass = 10;
            var astroObject = brokenSatellite.GetComponent<NHAstroObject>();
            astroObject._primaryBody._satellite = astroObject;
            astroObject._type = AstroObject.Type.Satellite;
            var satelliteSnapshot = astroObject._primaryBody.GetComponentInChildren<SatelliteSnapshotController>(true);
            satelliteSnapshot._satelliteCamera = brokenSatellite.GetComponentInChildren<OWCamera>();
            satelliteSnapshot._probeMesh = brokenSatellite.GetComponentInChildren<BrokenSatelliteManager>().transform.Find("BrokenSatellite/center").GetComponent<MeshRenderer>();
            satelliteSnapshot.gameObject.SetActive(true);
        }

        [Obsolete("I was gonna do this but I don't want anything static or bramble related to mess stuff up")]
        private void IgnoreCompat()
        {
            ModHelper.Console.WriteLine("Ignoring inclination lock.", MessageType.Info);
            var axiom = NewHorizons.Main.BodyDict["Jam3"].First(body => body.Config.name == "Axiom");
            var orbit = axiom.Config.Orbit;
            orbit.inclination = 66.24f;
        }

        public static void ReplaceInAudioManager(AudioManager __instance)
        {
            //The audio dictionary is a dictionary containing all of the sounds, matched to the int value of the AudioType enum
            foreach (var pair in AudiosToReplace)
            {
                var type = pair.Key;
                var path = pair.Value;
                if (__instance._audioLibraryDict.ContainsKey((int)type))
                    __instance._audioLibraryDict[(int)type] = new AudioLibrary.AudioEntry(type, GetClips(path), 1f);
                else
                    __instance._audioLibraryDict.Add((int)type, new AudioLibrary.AudioEntry(type, GetClips(path), 1f));
            }
        }

        public static void ReplaceInAudioLibrary(ref Dictionary<int, AudioLibrary.AudioEntry> __result)
        {
            //The audio dictionary is a dictionary containing all of the sounds, matched to the int value of the AudioType enum
            foreach (var pair in AudiosToReplace)
            {
                var type = pair.Key;
                var path = pair.Value;
                if (__result.ContainsKey((int)type))
                    __result[(int)type] = new AudioLibrary.AudioEntry(type, GetClips(path), 1f);
                else
                    __result.Add((int)type, new AudioLibrary.AudioEntry(type, GetClips(path), 1f));
            }
        }

        public static Dictionary<string, AudioClip> LoadedClips = new Dictionary<string, AudioClip>();
        private static AudioClip[] LoadAllClips() => AudiosToReplace.Values.Select(path => GetClip(path)).ToArray();
        private static AudioClip[] GetClips(string path) => new AudioClip[1] { GetClip(path) };
        private static AudioClip GetClip(string path)
        {
            if (LoadedClips.ContainsKey(path)) return LoadedClips[path]; //If it's already loaded, give the one in memory
            AudioClip audioClip = instance.ModHelper.Assets.GetAudio(path); //Else load it
            LoadedClips[path] = audioClip;
            return audioClip;
        }
    }
}