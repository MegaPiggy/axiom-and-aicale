using Epic.OnlineServices;
using HarmonyLib;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Axiom
{
    public class Main : ModBehaviour
    {
        private static Main instance;
        private INewHorizons _newHorizons;
        public static IModConsole ConsoleInstance => instance.ModHelper.Console;
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

        public static bool AlphaSFX = true;

        private void Start()
        {
            _newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            _newHorizons.LoadConfigs(this);

            LoadAllClips(); //load audio

            new HarmonyLib.Harmony("MegaPiggy.Axiom").PatchAll(Assembly.GetExecutingAssembly());
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
        }

        public override void Configure(IModConfig config)
        {
            AlphaSFX = config.GetSettingsValue<bool>("alphaSFX");
        }

        private void OnAxiomLoaded(GameObject axiom)
        {
            ModHelper.Console.WriteLine("Axiom has loaded", MessageType.Info);
            axiom.transform.Find("Sector/IcePlanet/Interior/QuietTown/TwoStoryCabin (1)/AirCurrent").transform.localPosition = new Vector3(-4.5452f, 9.061f, 1.25f);
            axiom.transform.Find("Sector/IcePlanet/Interior/Details/TreeHouseIsland").gameObject.SetActive(false); // Disable for now until I can make it less laggy
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
            //brokenSatellite.AddComponent<AlignWithJamSun>();
            brokenSatellite.GetComponent<Rigidbody>().mass = 10;
            var astroObject = brokenSatellite.GetComponent<NHAstroObject>();
            astroObject._primaryBody._satellite = astroObject;
            astroObject._type = AstroObject.Type.Satellite;
            brokenSatellite.GetComponentInChildren<Camera>().fieldOfView = 40;
            brokenSatellite.GetComponentInChildren<NoiseImageEffect>()._strength = 0.25f;
            var satelliteSnapshot = astroObject._primaryBody.GetComponentInChildren<SatelliteSnapshotController>(true);
            satelliteSnapshot._satelliteCamera = brokenSatellite.GetComponentInChildren<OWCamera>();
            satelliteSnapshot._probeMesh = brokenSatellite.GetComponentInChildren<BrokenSatelliteManager>().transform.Find("BrokenSatellite/center").GetComponent<MeshRenderer>();
            satelliteSnapshot.gameObject.SetActive(true);
        }

        public static void ReplaceInAudioManager(AudioManager __instance)
        {
            if (!AlphaSFX) return;
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
            if (!AlphaSFX) return;
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

    [HarmonyPatch(typeof(OWExtensions), nameof(OWExtensions.Assert), new Type[] { typeof(Collider), typeof(LayerMask), typeof(bool) })]
    public static class AssertPatch
    {
        public static void Prefix(Collider collider, LayerMask layerMask, bool isTrigger)
        {
            if (!OWLayerMask.IsLayerInMask(collider.gameObject.layer, layerMask))
            {
                Main.ConsoleInstance.WriteLine("\"" + collider.transform.GetPath() + "\" collider is not in the \"" + LayerMask.LayerToName(layerMask.value) + "\" | #" + layerMask.value + " LayerMask!", MessageType.Error);
            }
            if (collider.isTrigger != isTrigger)
            {
                Main.ConsoleInstance.WriteLine("\"" + collider.transform.GetPath() + "\" isTrigger should be set to " + isTrigger, MessageType.Error);
            }
        }
    }

    [HarmonyPatch(typeof(Sector), nameof(Sector.Awake))]
    public static class SectorPatch
    {
        public static void Prefix(Sector __instance)
        {
            var triggerRoot = __instance._triggerRoot;
            if (triggerRoot == null)
            {
                Main.ConsoleInstance.WriteLine(__instance.transform.GetPath() + " does not have a trigger root. Defaulting to self.", MessageType.Warning);
                triggerRoot = __instance.gameObject;
            }
            var proximityTrigger = triggerRoot.GetComponent<ProximityTrigger>();
            if (proximityTrigger != null)
            {
                return;
            }
            var owTriggerVolume = triggerRoot.GetComponent<OWTriggerVolume>();
            if (owTriggerVolume != null)
            {
                return;
            }

            Main.ConsoleInstance.WriteLine("Could not find any triggers for Sector " + __instance.transform.GetPath(), MessageType.Error);
        }
    }
}