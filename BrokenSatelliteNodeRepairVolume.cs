using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Axiom
{
    [RequireComponent(typeof(Collider))]
    public class BrokenSatelliteNodeRepairVolume : MonoBehaviour
    {
        public delegate void RepairEvent(BrokenSatelliteNodeRepairVolume volume);

        [SerializeField]
        private float _repairDistance = 3f;
        public float repairDistance
        {
            get
            {
                return _repairDistance;
            }
            set
            {
                _repairDistance = value;
            }
        }

        [SerializeField]
        private float _secondsToRepair = 3f;
        public float secondsToRepair
        {
            get
            {
                return _secondsToRepair;
            }
            set
            {
                _secondsToRepair = value;
            }
        }

        private float _repairFraction;

        private bool _isRepairing;
        public bool isRepairing => _isRepairing;

        private InteractReceiver _interactReceiver;

        private PlayerAudioController _playerAudioController;

        private ScreenPrompt _repairScreenPrompt;

        private ScreenPrompt _repairDetailsScreenPrompt;

        private StringBuilder _repairDetailsStringBuilder;

        public event RepairEvent OnCompleteRepair;

        private void Awake()
        {
            _interactReceiver = gameObject.AddComponent<InteractReceiver>();
            _interactReceiver.OnGainFocus += OnGainFocus;
            _interactReceiver.OnLoseFocus += OnLoseFocus;
            _interactReceiver.OnPressInteract += OnPressInteract;
            _interactReceiver.OnReleaseInteract += OnReleaseInteract;
            _repairScreenPrompt = new ScreenPrompt(InputLibrary.interact, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.ShipRepairPrompt));
            _repairDetailsScreenPrompt = new ScreenPrompt("");
            _isRepairing = false;
            _repairDetailsStringBuilder = new StringBuilder(128);
        }

        private void Start()
        {
            _interactReceiver.SetPromptText(UITextType.HoldPrompt, UITextType.RepairPrompt);
            _interactReceiver.SetInteractRange(_repairDistance);
            _playerAudioController = Locator.GetPlayerTransform().GetComponentInChildren<PlayerAudioController>();
            Locator.GetPromptManager().AddScreenPrompt(_repairScreenPrompt, PromptPosition.Center);
            Locator.GetPromptManager().AddScreenPrompt(_repairDetailsScreenPrompt, PromptPosition.Center);
            enabled = false;
        }

        private void OnEnable()
        {
            SetPromptVisibility(isVisible: true);
        }

        private void OnDisable()
        {
            SetPromptVisibility(isVisible: false);
        }

        private void SetPromptVisibility(bool isVisible)
        {
            if (_interactReceiver != null && )
            {
                if (_interactReceiver._screenPrompt != null)
                {
                    _interactReceiver._screenPrompt.SetVisibility(isVisible: false);
                    Locator.GetPromptManager().RemoveScreenPrompt(_interactReceiver._screenPrompt);
                }
                if (_interactReceiver._noCommandIconPrompt != null)
                {
                    _interactReceiver._noCommandIconPrompt.SetVisibility(isVisible: false);
                    Locator.GetPromptManager().RemoveScreenPrompt(_interactReceiver._noCommandIconPrompt);
                }
            }
            _repairScreenPrompt.SetVisibility(isVisible);
            _repairDetailsScreenPrompt.SetVisibility(isVisible);
        }

        private void OnDestroy()
        {
            _interactReceiver.OnGainFocus -= OnGainFocus;
            _interactReceiver.OnLoseFocus -= OnLoseFocus;
            _interactReceiver.OnPressInteract -= OnPressInteract;
            _interactReceiver.OnReleaseInteract -= OnReleaseInteract;
        }

        public InteractReceiver GetInteractReceiver()
        {
            return _interactReceiver;
        }

        public void Activate()
        {
            gameObject.SetActive(value: true);
        }

        public void Deactivate()
        {
            enabled = false;
            _interactReceiver.DisableInteraction();
            SetPromptVisibility(isVisible: false);
            gameObject.SetActive(value: false);
        }

        public float GetRepairFraction()
        {
            return _repairFraction;
        }

        public void ResetVolume()
        {
            _repairFraction = 0f;
            _interactReceiver.DisableInteraction();
            SetPromptVisibility(isVisible: false);
        }


        private void Update()
        {
            if (!OWTime.IsPaused())
            {
                _repairDetailsStringBuilder.Length = 0;
                _repairDetailsStringBuilder.Append(UITextLibrary.GetString(UITextType.SatelliteNodeName));
                _repairDetailsStringBuilder.Append(": ");
                _repairDetailsStringBuilder.Append(_repairFraction.ToString("P0"));
                _repairDetailsScreenPrompt.SetText(_repairDetailsStringBuilder.ToString());
                SetPromptVisibility(isVisible: true);
            }
            else if (_repairScreenPrompt.IsVisible())
            {
                SetPromptVisibility(isVisible: false);
            }
            if (_isRepairing)
            {
                if (_repairFraction < 1f)
                {
                    _repairFraction += Time.deltaTime / _secondsToRepair;
                    _repairFraction = Mathf.Clamp01(_repairFraction);
                }
                if (_repairFraction >= 1f && OnCompleteRepair != null)
                {
                    GlobalMessenger<BrokenSatelliteNodeRepairVolume>.FireEvent("FinishRepairing", this);
                    OnCompleteRepair(this);
                    _isRepairing = false;
                    SetPromptVisibility(isVisible: false);
                    _playerAudioController.StopRepairTool();
                    _playerAudioController.PlayRepairCompleteOneShot();
                }
            }
        }

        private void OnGainFocus()
        {
            enabled = true;
        }

        private void OnLoseFocus()
        {
            if (_repairFraction < 1f)
            {
                _interactReceiver.ResetInteraction();
            }
            SetPromptVisibility(isVisible: false);
            if (_isRepairing)
            {
                GlobalMessenger<BrokenSatelliteNodeRepairVolume>.FireEvent("StopRepairing", this);
            }
            _isRepairing = false;
            _playerAudioController.StopRepairTool();
            enabled = false;
        }

        private void OnPressInteract()
        {
            _isRepairing = true;
            _playerAudioController.PlayRepairTool();
            GlobalMessenger<BrokenSatelliteNodeRepairVolume>.FireEvent("StartRepairing", this);
        }

        private void OnReleaseInteract()
        {
            if (_repairFraction < 1f)
            {
                _interactReceiver.ResetInteraction();
            }
            _isRepairing = false;
            _playerAudioController.StopRepairTool();
            GlobalMessenger<BrokenSatelliteNodeRepairVolume>.FireEvent("StopRepairing", this);
        }
    }
}
