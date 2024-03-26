using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axiom
{
    public class QuantumStatue : QuantumObject
    {
        private MeshRenderer[] _meshRenderers;

        public override void Awake()
        {
            base.Awake();
            _meshRenderers = base.GetComponentsInChildren<MeshRenderer>();
        }

        public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
        {
            foreach (MeshRenderer meshRenderer in this._meshRenderers)
            {
                if (UnityEngine.Random.value < 0.2f)
                {
                    meshRenderer.enabled = true;
                }
                else
                {
                    meshRenderer.enabled = false;
                }
            }
            return true;
        }
    }

}
