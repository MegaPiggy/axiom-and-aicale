using UnityEngine;

namespace Axiom
{
    [RequireComponent(typeof(Light))]
    public class PulsingLight : MonoBehaviour
    {
        [SerializeField]
        private float _pulseRate;
        [SerializeField]
        private float _intensityFluctuation;
        [SerializeField]
        private float _rangeFluctuation;
        [SerializeField]
        private float _timeOffset;

        private float _initIntensity;
        private float _initRange;

        private Light _light;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _initIntensity = _light.intensity;
            _initRange = _light.range;
        }

        public void Enable()
        {
            enabled = true;
            _light.enabled = true;
        }

        public void Disable()
        {
            enabled = false;
            _light.enabled = false;
        }

        private void Update()
        {
            _light.intensity = Mathf.Sin((Time.time + _timeOffset) * _pulseRate) * _intensityFluctuation + _initIntensity;
            _light.range = Mathf.Sin((Time.time + _timeOffset) * _pulseRate) * _rangeFluctuation + _initRange;
        }
    }
}
