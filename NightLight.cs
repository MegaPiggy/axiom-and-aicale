using System;
using UnityEngine;

namespace Axiom
{
    [RequireComponent(typeof(Light))]
    public class NightLight : MonoBehaviour
    {
        [SerializeField]
        private float _dayIntensityMultiplier = 0.5f;

        private DayNightTracker _dayNightTracker;
        private Light _light;
        private float _nightIntensity;
        private float _startIntensity;
        private float _targetIntensity;
        private float _initFadeTime;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _dayNightTracker = GetComponentInParent<DayNightTracker>();
            _nightIntensity = _light.intensity;
            enabled = false;
        }

        private void OnDestroy()
        {
            if (_dayNightTracker != null)
            {
                _dayNightTracker.OnSunrise -= OnSunrise;
                _dayNightTracker.OnSunset -= OnSunset;
            }
        }

        public void SetDayNightTracker(DayNightTracker dayNightTracker)
        {
            if (_dayNightTracker == null)
            {
                _dayNightTracker = dayNightTracker;
                _dayNightTracker.OnSunrise += OnSunrise;
                _dayNightTracker.OnSunset += OnSunset;
            }
            else
            {
                Debug.LogError("We've already set up a day-night tracker!");
                Debug.Break();
            }
        }

        private void Update()
        {
            float num = Mathf.Clamp01((Time.time - _initFadeTime) / 5f);
            if (num < 1f)
            {
                _light.intensity = _startIntensity + (_targetIntensity - _startIntensity) * num;
            }
            else
            {
                enabled = false;
            }
        }

        private void StartFade()
        {
            _startIntensity = _light.intensity;
            _initFadeTime = Time.time;
            enabled = true;
        }

        private void OnSunrise()
        {
            _targetIntensity = _nightIntensity * _dayIntensityMultiplier;
            StartFade();
        }

        private void OnSunset()
        {
            _targetIntensity = _nightIntensity;
            StartFade();
        }
    }
}
