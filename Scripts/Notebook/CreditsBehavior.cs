using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CreditsBehavior : MonoBehaviour
{
    public GameObject Credits; // It will be set disabled at the first
    public GameObject photoPrefab;  // Prefab to use for each photo display
    public Transform galleryParent;  // Parent object to contain the photo objects
    public GameObject startPositionObject;  // Start position of the first photo
    public Vector3 spacing = new Vector3(0f, -1.45f, 0f);  // Spacing between photos
    public float scrollSpeed = 1f;  // Speed for scrolling credits
    public float endPositionY = 500f;  // Position to stop scrolling
    private string photoDirectory;

    private bool photosDisplayed = false;  // To track if photos have been displayed

    void Start()
    {
        photoDirectory = GameManager.Instance.PhotoDirectory;
        Credits.SetActive(false);
    }

    void Update()
    {
        //DisplayPhotosAndScrollCredits();
        // If photos are displayed, start scrolling the credits
        if (photosDisplayed)
        {
            ScrollCredits();
        }
    }

    // Public function to trigger displaying photos and scrolling credits
    public void DisplayPhotosAndScrollCredits()
    {
        Debug.Log("Credit display");
        // First, enable Credits, and display all photos
        Credits.SetActive(true);
        DisplayAllPhotos();

        // After displaying photos, set flag to true so credits start scrolling
        photosDisplayed = true;
    }

    // Function to display all photos
    public void DisplayAllPhotos()
    {
        if (Directory.Exists(photoDirectory))
        {
            string[] files = Directory.GetFiles(photoDirectory, "*.png");
            if (files.Length > 0)
            {
                Vector3 currentPosition = startPositionObject.transform.position;

                foreach (string photoPath in files)
                {
                    // Load the image as a texture
                    byte[] fileData = File.ReadAllBytes(photoPath);
                    Texture2D photoTexture = new Texture2D(2, 2);
                    photoTexture.LoadImage(fileData);

                    // Instantiate a new photo object from the prefab
                    GameObject photoInstance = Instantiate(photoPrefab, currentPosition, Quaternion.identity, galleryParent);
                    photoInstance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                    // Assign the loaded texture to the material of the photo object
                    Renderer photoRenderer = photoInstance.transform.Find("Photo").GetComponent<Renderer>();
                    photoRenderer.material.mainTexture = photoTexture;

                    // Update the position for the next photo
                    currentPosition += spacing;
                }
            }
        }
        else
        {
            Debug.LogWarning("Photo directory not found.");
        }
    }

    // Function to scroll the credits upward
    private void ScrollCredits()
    {
        transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);

        // Check if credits have passed the end position and stop scrolling
        if (transform.position.y >= endPositionY)
        {
            enabled = false;  // Stop updating once credits reach the end
        }
    }
}