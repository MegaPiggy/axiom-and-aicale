using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axiom
{
    public class AlignWithJamSun : AlignWithTargetBody
    {
        public override void Start()
        {
            _usePhysicsToRotate = true;
            Transform sunTransform = Main.NewHorizonsInstance.GetPlanet("Jam 3 Sun").transform;
            if (sunTransform != null)
            {
                SetTargetBody(sunTransform.GetAttachedOWRigidbody());
            }
        }
    }
}
