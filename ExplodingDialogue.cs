using UnityEngine;

namespace Axiom
{
    public class ExplodingDialogue : MonoBehaviour
    {
        private CharacterDialogueTree _dialogueTree;

        private void Start()
        {
            _dialogueTree = GetComponent<CharacterDialogueTree>();
            _dialogueTree.OnEndConversation += OnEndConversation;
        }

        private void OnDestroy()
        {
            _dialogueTree.OnEndConversation -= OnEndConversation;
        }

        private void OnEndConversation()
        {
            if (DialogueConditionManager.SharedInstance.GetConditionState("20YearComa"))
            {
                Locator.GetDeathManager().KillPlayer(DeathType.Energy);
            }
        }
    }

}
