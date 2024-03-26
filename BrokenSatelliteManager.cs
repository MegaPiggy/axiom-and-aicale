using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axiom
{
    public class BrokenSatelliteManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _triggerObject;
        private OWTriggerVolume _trigger;

        [SerializeField]
        private AudioSource _audioSource;
        private OWAudioSource _owAudioSource;

        [SerializeField]
        private BrokenSatelliteNode[] _satelliteNodes;

        [SerializeField]
        private Light[] _antennaLights;

        private NoiseImageEffect _cameraNoiseEffect;

        private NotificationData _helmetProgressNotification;

        private int _repairCount;

        private void Awake()
        {
            _cameraNoiseEffect = GetComponentInChildren<NoiseImageEffect>(true);
            _cameraNoiseEffect.enabled = true;
            foreach (var node in _satelliteNodes) node.OnRepaired += OnRepairNode;
            foreach (var light in _antennaLights) light.enabled = false;
            if (_audioSource != null) _owAudioSource = _audioSource.GetComponent<OWAudioSource>();
            if (_triggerObject != null)
            {
                _trigger = _triggerObject.GetComponent<OWTriggerVolume>();
                if (_trigger != null)
                {
                    _trigger.OnEntry += OnEntry;
                    _trigger.OnExit += OnExit;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var node in _satelliteNodes) node.OnRepaired -= OnRepairNode;
            if (_trigger != null)
            {
                _trigger.OnEntry -= OnEntry;
                _trigger.OnExit -= OnExit;
            }
        }

        private void OnRepairNode(BrokenSatelliteNode node)
        {
            _repairCount++;
            UpdateNotification();
            if (_repairCount >= _satelliteNodes.Length)
            {
                DialogueConditionManager.SharedInstance.SetConditionState("PostAxiomBrokenSatellite", conditionState: true);
                Locator.GetShipLogManager().RevealFact("AXION_BROKEN_SATELLITE_X2");
                foreach (var light in _antennaLights) light.enabled = true;
                _cameraNoiseEffect.enabled = false;
                _owAudioSource.PlayOneShot(AudioType.TH_ZeroGTrainingAllRepaired, 2);
            }
        }

        private void UpdateNotification()
        {
            if (_helmetProgressNotification != null)
            {
                NotificationManager.SharedInstance.UnpinNotification(_helmetProgressNotification);
            }
            if (_repairCount < _satelliteNodes.Length)
            {
                _helmetProgressNotification = new NotificationData(NotificationTarget.Player, _repairCount + UITextLibrary.GetString(UITextType.Notification0gTraining));
                NotificationManager.SharedInstance.PostNotification(_helmetProgressNotification, pin: true);
            }
            else
            {
                _helmetProgressNotification = new NotificationData(NotificationTarget.Player, _repairCount + UITextLibrary.GetString(UITextType.Notification0gTraining), 10f);
                NotificationManager.SharedInstance.PostNotification(_helmetProgressNotification, pin: true);
            }
        }

        private void RemoveNotification()
        {
            if (_helmetProgressNotification != null)
            {
                NotificationManager.SharedInstance.UnpinNotification(_helmetProgressNotification);
            }
            _helmetProgressNotification = null;
        }

        private void OnEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                UpdateNotification();
                GlobalMessenger.FireEvent("EnterAxiomBrokenSatellite");
                Locator.GetShipLogManager().RevealFact("AXION_BROKEN_SATELLITE");
            }
        }

        private void OnExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                RemoveNotification();
                GlobalMessenger.FireEvent("ExitAxiomBrokenSatellite");
            }
        }
    }

}
