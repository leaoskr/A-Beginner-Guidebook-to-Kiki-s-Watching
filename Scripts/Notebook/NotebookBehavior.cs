using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static PhotographableObject;
using static HandheldCameraController;
using System;
using UnityEngine.Rendering.UI;

public class NotebookBehavior : MonoBehaviour
{
    public static NotebookBehavior Instance { get; private set; }

    // Assign this in the Inspector to the leftEyeAnchor object
    //public Transform leftEyeAnchor;

    // Offset of the notebook relative to the player
    //public Vector3 positionOffset = new Vector3(0.48f, 1f, -0.18f);

    // Store the initial height of the notebook
    //private float initialHeight;
    private int photoCount = 0;

    public GameObject photoOne;
    public GameObject photoTwo;
    public GameObject photoThree;
    public GameObject photoFour;
    public GameObject photoFive;
    public GameObject photoSix;
    public GameObject photoSeven;
    public GameObject photoEight;
    //public GameObject photoNine; // fairy
    public GameObject photoTen; // big butterfly
    private GameObject photoObject = null; 
    private bool[] hasSetPhotos;
    private Score[] photoScores;

    private string photoDirectory;

    public CreditsBehavior credits;
    public GameObject finalButterfly;
    public GameObject photographable;

    public Dictionary<Species, int> notebookPositions = new Dictionary<Species, int>()
    {
        {Species.AFly, 2},
        {Species.BFly, 1},
        {Species.CFly, 3},
        {Species.DFly, 7},
        {Species.EFly, 4},
        {Species.FFly, 5},
        {Species.GFly, 6},
        {Species.HFly, 8},
        {Species.Fairy, 9},
        {Species.BigButterfly, 10}
    };

    private AudioSource audioSource;
    [SerializeField] private AudioClip perfectSound;
    [SerializeField] private AudioClip goodSound;
    [SerializeField] private AudioClip okaySound;


    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // Set the position of notebook relative to the player, and store the y value (height)
        //transform.position = leftEyeAnchor.position + positionOffset;
        //initialHeight = transform.position.y;

        // Disable the entire Photos object
        photoOne.SetActive(false);
        photoTwo.SetActive(false);
        photoThree.SetActive(false);
        photoFour.SetActive(false);
        photoFive.SetActive(false);
        photoSix.SetActive(false);
        photoSeven.SetActive(false);
        photoEight.SetActive(false);
        //photoNine.SetActive(false);
        photoTen.SetActive(false);
        hasSetPhotos = new[] { false, false, false, false, false, false, false, false, false, false };
        photoScores = new[] { Score.Bad, Score.Bad, Score.Bad, Score.Bad, Score.Bad, Score.Bad, Score.Bad, Score.Bad, Score.Bad, Score.Bad };

        photoDirectory = GameManager.Instance.PhotoDirectory;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // When player moves, we want the notebook stays at the same height, but moves with the player.
        // So, notebook's y value doesn't change, x and z value changes with the player.
        //Vector3 newPosition = leftEyeAnchor.position + positionOffset;
        //newPosition.y = initialHeight;
        //transform.position = newPosition;
    }

    // Display photo in the placeholder on the notebook
    // Example: DisplayNotebookPhoto("photo1.png", 1); This call will show one of the small butterfly
    public void DisplayNotebookPhoto(string photoPath, int photoPosition, Score photoScore)
    {
        bool isBetterScore = photoScores[photoPosition - 1] < photoScore;
        bool isNewPhoto = !hasSetPhotos[photoPosition - 1];

        if (isBetterScore) Debug.Log("Better score for photo " + photoPosition + ": " + photoScore);

        // Use only the first photo for each butterfly by returning if the photo for that position has already been set
        if (!isNewPhoto && !isBetterScore) { return; }

        // Assign the photoObject based on the input fileName
        photoObject = photoPosition switch
        {
            1 => photoOne,
            2 => photoTwo,
            3 => photoThree,
            4 => photoFour,
            5 => photoFive,
            6 => photoSix,
            7 => photoSeven,
            8 => photoEight,
            //9 => photoNine,
            10 => photoTen,
            _ => null
        };

        if (File.Exists(photoPath) && photoObject != null)
        {
            // Load the photo file as a Texture2D
            byte[] fileData = File.ReadAllBytes(photoPath);
            Texture2D photoTexture = new Texture2D(2, 2);  // Create a new Texture2D
            photoTexture.LoadImage(fileData);  // Load the image data into the texture

            // Flip the texture vertically because otherwise the photo will be up-side-down
            // For the interim scene, when I test it, it looks like we don't need it, so I comment it out.
            //FlipTextureVertically(photoTexture);

            // Get the Renderer component of the Photo object
            Renderer photoRenderer = photoObject.transform.Find("Photo").GetComponent<Renderer>();

            if (photoRenderer != null)
            {
                // Apply the loaded texture to the material's main texture
                photoObject.SetActive(true);
                photoRenderer.material.mainTexture = photoTexture;
                photoRenderer.GetComponent<AudioSource>().Play(); // TODO: Change this to play sound effect based on the score

                // Show the stamps 
                if (photoPosition != 10 && photoPosition != 9)
                {
                    if (photoScore == Score.Okay)
                    {
                        GameObject stamp_ok = photoObject.transform.Find("ok").gameObject;
                        if (stamp_ok != null) stamp_ok.SetActive(true);
                        else Debug.Log("ok stamp is null");

                    }
                    else if (photoScore == Score.Good)
                    {
                        GameObject stamp_good = photoObject.transform.Find("good").gameObject;
                        if (stamp_good != null) stamp_good.SetActive(true);
                        else Debug.Log("good stamp is null");
                    }
                    else if (photoScore == Score.Perfect)
                    {
                        GameObject stamp_perfect = photoObject.transform.Find("perfect").gameObject;
                        if (stamp_perfect != null) stamp_perfect.SetActive(true);
                        else Debug.Log("perfect stamp is null");
                    }
                }

                photoObject = null;

                // update values
                if (isNewPhoto)
                {
                    photoCount++; // Increment the photo count only if the photo is new
                    hasSetPhotos[photoPosition - 1] = true;
                }
                if (isBetterScore)
                {
                    photoScores[photoPosition - 1] = photoScore;
                    switch (photoScore)
                    {
                        case Score.Perfect:
                            audioSource.PlayOneShot(perfectSound);
                            break;
                        case Score.Good:
                            audioSource.PlayOneShot(goodSound);
                            break;
                        case Score.Okay:
                            audioSource.PlayOneShot(okaySound);
                            break;
                        default:
                            break;
                    }
                }

                // Debug.Log("Photo successfully applied: " + fileName);


                if (photoPosition == 9){
                    NPCDialogueManager.Instance.PlaySound(VoiceLine.PHOTO_FAIRY);
                }

                if (photoPosition == 10)
                {
                    StartCoroutine(EndGame());                  
                }

                //if(photoCount == 4){
                //    StartCoroutine(SpawnButterfly());
                //}

                if (photoCount == 9){
                    NPCDialogueManager.Instance.PlaySound(VoiceLine.NOTEBOOK_COMPLETE);
                }
            }
            else
            {
                Debug.LogError("No Renderer found on the Photo object.");
            }
        }
        else
        {
            Debug.Log("Photo file not found or Not notebook photo: " + photoPath);
        }
    }

    void FlipTextureVertically(Texture2D original)
    {
        int width = original.width;
        int height = original.height;

        // Create a new array to hold the pixels
        Color[] pixels = original.GetPixels();

        // Flip the pixels array vertically
        for (int y = 0; y < height / 2; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color temp = pixels[y * width + x];
                pixels[y * width + x] = pixels[(height - 1 - y) * width + x];
                pixels[(height - 1 - y) * width + x] = temp;
            }
        }

        // Apply the flipped pixels to the texture
        original.SetPixels(pixels);
        original.Apply();
    }

    private IEnumerator EndGame(){
        yield return new WaitForSecondsRealtime(1); //5
        NPCDialogueManager.Instance.PlaySound(VoiceLine.GOODBYE);
        credits.DisplayPhotosAndScrollCredits();
    }

    public IEnumerator SpawnButterfly(){
        Instantiate(finalButterfly, finalButterfly.transform.position, finalButterfly.transform.rotation, photographable.transform);
        // Instantiate(finalButterfly, new Vector3(5, 2, -6), Quaternion.identity, photographable.transform);
        
        yield return new WaitForSecondsRealtime(1);
        NPCDialogueManager.Instance.PlaySound(VoiceLine.LARGE_BUTTERFLY);
        
        //NPCDialogueManager.Instance.PlaySound(VoiceLine.GAME_END_HINT);

    }

    public int GetPhotoCount()
    {
        return photoCount;
    }
}
