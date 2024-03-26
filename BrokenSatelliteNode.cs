using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axiom
{
    public class BrokenSatelliteNode : MonoBehaviour
    {
        public delegate void RepairEvent(BrokenSatelliteNode satelliteNode);

        [SerializeField]
        public Material _repairedMaterial;

        [SerializeField]
        public BrokenSatelliteNodeRepairVolume _repairVolume;

        public event RepairEvent OnRepaired;

        private ReferenceFrameVolume _rfVolume;

        [SerializeField]
        private ParticleSystem _particleSystem;

        [SerializeField]
        private Light _light;

        [SerializeField]
        private MeshRenderer _materialRenderer;

        private bool _damaged;
        public bool isDamaged => _damaged;

        public void Awake()
        {
            _damaged = true;
            _rfVolume = GetComponentInChildren<ReferenceFrameVolume>();
            _repairVolume.OnCompleteRepair += OnCompleteRepair;
        }

        private void OnDestroy()
        {
            _repairVolume.OnCompleteRepair -= OnCompleteRepair;
        }

        private void OnCompleteRepair(BrokenSatelliteNodeRepairVolume repairVolume)
        {
            _damaged = false;

            ReferenceFrameTracker rfTracker = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
            if (rfTracker.GetReferenceFrame() == _rfVolume.GetReferenceFrame())
            {
                rfTracker.UntargetReferenceFrame();
            }

            if (_rfVolume != null)
            {
                _rfVolume.gameObject.SetActive(value: false);
            }

            if (_particleSystem != null)
            {
                _particleSystem.gameObject.SetActive(value: false);
            }

            repairVolume.Deactivate();

            if (_light != null)
            {
                _light.color = Color.green;
                _light.gameObject.name = "GreenLight";
            }

            if (_materialRenderer != null && _repairedMaterial != null)
            {
                _materialRenderer.sharedMaterial = _repairedMaterial;
            }

            if (OnRepaired != null) OnRepaired(this);
        }
    }
}
