using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Axiom
{
    public class AxiomForceDetector : ConstantForceDetector
    {
        [SerializeField]
        private ForceVolume[] _extras = new ForceVolume[0];

        private new void Start()
        {
            _fieldMultiplier = 1;
            _detectableFields = new ForceVolume[2] { Main.NewHorizonsInstance.GetPlanet("Axiom").GetComponentInChildren<GravityVolume>(), Main.NewHorizonsInstance.GetPlanet("Aicale").GetComponentInChildren<GravityVolume>() }.Concat(_extras).ToArray();
            _inheritElement0 = true;
            base.Start();
        }
    }
}
