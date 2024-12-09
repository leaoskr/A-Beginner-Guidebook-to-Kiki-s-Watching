using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatueBehavior : MonoBehaviour
{
    public NotebookBehavior notebook;
    public GameObject stoneText;
    private bool triggeredBigButterfly;
    private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;

    void Start()
    {
        if (notebook == null)
        {
            notebook = FindObjectOfType<NotebookBehavior>();
        }
        triggeredBigButterfly = false;
        audioSource = GetComponent<AudioSource>();

        stoneText.SetActive(false);
    }

    // Trigger when the notebook is placed on the statue
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object interacting with the statue is the notebook
        if (!triggeredBigButterfly && other.gameObject == notebook.gameObject)
        {
            // Check if the photoCount in the notebook has reached 4
            if (notebook.GetPhotoCount() >= 4)
            {
                stoneText.SetActive(true);
                stoneText.GetComponent<TextMeshPro>().text = "You collected enough butterfly photos, \nTake a photo of the butterfly king!";
                StartCoroutine(notebook.SpawnButterfly());
                triggeredBigButterfly = true;
                if (audioSource) audioSource.PlayOneShot(activationSound);
            }
            else if (notebook.GetPhotoCount() < 4)
            {
                stoneText.SetActive(true);
                stoneText.GetComponent<TextMeshPro>().text = "Take at least 4 photos, \nthen come back!";
                Debug.Log("Take at least 4 photos, then come back!");
                NPCDialogueManager.Instance.PlaySound(VoiceLine.MORE_PHOTOS);

            }
        }
    }
}
