using OWML.Common;
using OWML.ModHelper;
using System.Linq;
using UnityEngine;

namespace Axiom
{
    public class Main : ModBehaviour
    {
        private INewHorizons _newHorizons;

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(Axiom)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            _newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            _newHorizons.LoadConfigs(this);

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
            };

            // Wait til next frame so all dependants have run Start
            ModHelper.Events.Unity.FireInNUpdates(IgnoreCompat, 2);
        }

        private void IgnoreCompat()
        {
            var axiom = NewHorizons.Main.BodyDict["Jam3"].First(body => body.Config.name == "Axiom");
            var orbit = axiom.Config.Orbit;
            orbit.inclination = 66.24f;
        }
    }
}