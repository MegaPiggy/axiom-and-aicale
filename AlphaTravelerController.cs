using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axiom
{
    public class AlphaTravelerController : TravelerController
    {
        [Header("Animation")]
        [SerializeField]
        protected Animation _animation;
        [Tooltip("Playing animation clip for any animation components (excluding animators)")]
        [SerializeField]
        protected AnimationClip playingClip;
        [Tooltip("Talking animation clip for any animation components (excluding animators)")]
        [SerializeField]
        protected AnimationClip talkingClip;

        public override void Awake()
        {
            _delayToRestartAudio = 0;
            if (_dialogueSystem == null) _dialogueSystem = GetComponentInChildren<CharacterDialogueTree>();
            if (_audioSource == null) _audioSource = GetComponentInChildren<AudioSource>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_animation == null) _animation = GetComponentInChildren<Animation>();
            base.Awake();
        }

        public override void OnSectorOccupantsUpdated()
        {
            if (_animator != null)
            {
                _animator.enabled = true;
            }
            if (_animation != null)
            {
                _animation.enabled = true;
            }
        }

        public override void StartConversation()
        {
            if (_animator != null && _animator.enabled)
            {
                _animator.SetTrigger("Talking");
            }
            if (_animation != null && _animation.enabled)
            {
                _animation.PlayQueued(talkingClip.name, QueueMode.PlayNow, PlayMode.StopAll);
            }
            Locator.GetTravelerAudioManager().StopAllTravelerAudio();
        }

        public override void EndConversation(float audioDelay)
        {
            if (_animator != null && _animator.enabled)
            {
                _animator.SetTrigger("Playing");
            }
            if (_animation != null && _animation.enabled)
            {
                _animation.PlayQueued(playingClip.name, QueueMode.PlayNow, PlayMode.StopAll);
            }
            Locator.GetTravelerAudioManager().PlayAllTravelerAudio(audioDelay);
        }

        public override void OnUnpause()
        {
            if (!_talking)
            {
                if (_animator != null && _animator.enabled)
                {
                    _animator.SetTrigger("Playing");
                }
                if (_animation != null && _animation.enabled)
                {
                    _animation.PlayQueued(playingClip.name, QueueMode.PlayNow, PlayMode.StopAll);
                }
            }
        }

        public override void OnStartFastForward()
        {
            if (_animator != null)
            {
                _animator.enabled = false;
            }
            if (_animation != null)
            {
                _animation.enabled = false;
            }
        }

        public override void OnEndFastForward()
        {
            if (_animator != null)
            {
                _animator.enabled = true;
                if (_animator.enabled)
                {
                    _animator.SetTrigger("Playing");
                }
            }
            if (_animation != null)
            {
                _animation.enabled = true;
                if (_animation.enabled)
                {
                    _animation.PlayQueued(playingClip.name, QueueMode.PlayNow, PlayMode.StopAll);
                }
            }
        }
    }
}