using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class ButterflyPlaceHolder : MonoBehaviour
{
    // Start is called before the first frame update
    private InteractableUnityEventWrapper interactableUnityEventWrapper;

    private void Awake()
    {
        interactableUnityEventWrapper = GetComponent<InteractableUnityEventWrapper>();
        interactableUnityEventWrapper.WhenSelect.AddListener(Select);
    }

    private void Select()
    {
        Debug.Log("Butterfly 1 captured");
        NPCDialogueManager.Instance.PlaySound(VoiceLine.FIRST_PHOTO);
    }

}
