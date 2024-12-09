using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class HandheldCameraController : MonoBehaviour
{
    [SerializeField] private RawImage liveDisplayImage;
    [SerializeField] private RawImage lastTakenImage;
    [SerializeField] private Camera linkedCamera;
    [SerializeField] private RenderTexture cameraRenderTexture;

    // sound
    [SerializeField] private AudioSource shutterAudioSource;
    [SerializeField] private AudioSource zoomAudioSource;
    [SerializeField] private AudioClip cameraShutterSound;
    [SerializeField] private AudioClip nullShutterSound;

    // photographable objects
    [SerializeField] private GameObject photographableObjects;

    // amount of time to wait before taking another picture
    private float pictureDelayTime = 1.5f;
    private bool canTakePicture;

    // Zoom-related variables
    private int maxZoomInFov = 10;
    private int maxZoomOutFov = 25;
    private int startFov = 20;
    private float zoomSpeed = 10f;

    // number of photos taken
    private int photosTaken = 0;
    private string photoDirectory;

    // scoring related values
    private float maxDistanceAtZoomOut = 20f;
    private float minDistanceNoClipping = 2.5f;

    public enum Score { Bad, Okay, Good, Perfect }

    private void Start()
    {
        // set the render texture of the linked camera to the raw image
        linkedCamera.targetTexture = cameraRenderTexture;
        liveDisplayImage.texture = cameraRenderTexture;
        photosTaken = 0;

        photoDirectory = GameManager.Instance.PhotoDirectory;
        linkedCamera.fieldOfView = startFov;

        liveDisplayImage.gameObject.SetActive(true);
        lastTakenImage.gameObject.SetActive(false);
        canTakePicture = true;

        StartCoroutine(GivePhotoHint());
    }

    // Debug function to avoid having to use VR to test
    // Delete this later
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TakePicture();
        }

        if (Input.GetKey(KeyCode.Q))
        {
            ZoomIn();
        }

        if (Input.GetKey(KeyCode.E))
        {
            ZoomOut();
        }

        // use WASD to  move the camera around
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime);
        }

        // use IJKL to rotate the camera
        if (Input.GetKey(KeyCode.I))
        {
            transform.Rotate(Vector3.right, Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.K))
        {
            transform.Rotate(Vector3.left, Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.J))
        {
            transform.Rotate(Vector3.down, Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.L))
        {
            transform.Rotate(Vector3.up, Time.deltaTime);
        }
    }

    public void TakePicture()
    {
        if (!canTakePicture)
        {
            shutterAudioSource.PlayOneShot(nullShutterSound);
            return;
        }

        shutterAudioSource.PlayOneShot(cameraShutterSound);
        photosTaken++;
        // if (photosTaken == 1) 
        // {
        //     StartCoroutine(FirstPhoto());
        // }

        StartCoroutine(SavePhoto());
    }

    private IEnumerator SavePhoto()
    {
        canTakePicture = false;
        StopZoomSound();
        yield return new WaitForEndOfFrame();

        // save what linked camera sees to a file in persistent data path
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = cameraRenderTexture;

        linkedCamera.Render();

        Texture2D image = new Texture2D(cameraRenderTexture.width, cameraRenderTexture.height, TextureFormat.RGBA32, false, true);
        image.ReadPixels(new Rect(0, 0, image.width, image.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        // Create a copy of the image for display purposes
        Texture2D displayImage = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false, true);
        displayImage.SetPixels(image.GetPixels());
        displayImage.Apply();

        // Display the image on the last taken image raw image
        lastTakenImage.texture = displayImage;
        liveDisplayImage.gameObject.SetActive(false);
        lastTakenImage.gameObject.SetActive(true);

        // Convert the image to gamma space
        Color[] pixels = image.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = pixels[i].gamma;
        }
        image.SetPixels(pixels);
        image.Apply();

        // Save the image to a file
        byte[] bytes = image.EncodeToPNG();

        string photoPath = photoDirectory + "/photo" + photosTaken + ".png";
        File.WriteAllBytes(photoPath, bytes);

        // Update the notebook with the new photo(s) depending on which butterflies were photographed
        HashSet<Tuple<int, Score>> notebookPositionsToUpdate = DetermineObjectsInPhoto();
        foreach ((int position, Score score) in notebookPositionsToUpdate)
        {
            NotebookBehavior.Instance.DisplayNotebookPhoto(photoPath, position, score);
        }

        // delay time before taking another picture and then switch to live display
        yield return new WaitForSeconds(pictureDelayTime);
        canTakePicture = true;
        liveDisplayImage.gameObject.SetActive(true);
        lastTakenImage.gameObject.SetActive(false);
    }

    // Determine which photographable objects in the scene are in the current photo
    // And return a set of which notebook positions need to be updated (i.e. which butterflies are in the photo)\
    private HashSet<Tuple<int, Score>> DetermineObjectsInPhoto()
    {
        HashSet<Tuple<int, Score>> notebookPositionsToUpdate = new HashSet<Tuple<int, Score>>();

        // loop through transforms in photographableObjects children
        foreach (Transform child in photographableObjects.transform)
        {
            // make sure the child has a PhotographableObject component
            PhotographableObject photographableObject = child.GetComponent<PhotographableObject>();
            if (photographableObject == null)
            {
                continue;
            }

            bool inCameraView = IsObjectInCameraView(child);

            // if the child is in view, add it to the set
            if (inCameraView)
            {
                // Determine the score of the photo for this object
                float distanceScore = (photographableObject.species == PhotographableObject.Species.BigButterfly) ? 100f : CalculateDistanceScore(child.position);
                Score score = (distanceScore >= 94) ? Score.Perfect 
                            : (distanceScore >= 80) ? Score.Good 
                            : (distanceScore >= 0.01) ? Score.Okay 
                            : Score.Bad;

                // add the notebook position and score to the set if the score is not bad
                if (score != Score.Bad)
                {
                    Debug.Log($"Took a {score} photo of {photographableObject.species}");
                    // if the child name is in NotebookBehavior.Instance.notebookPositions, add it to the set
                    if (NotebookBehavior.Instance.notebookPositions.ContainsKey(photographableObject.species))
                    {
                        notebookPositionsToUpdate.Add(new Tuple<int, Score>(NotebookBehavior.Instance.notebookPositions[photographableObject.species], score));
                    }

                    // trigger any specific response of the object species when photographed
                    photographableObject.ResponseOnBeingPhotographed();
                }
                else
                {
                    Debug.Log("Bad photo"); // delete this later
                }
            }
        }

        return notebookPositionsToUpdate;
    }

    private bool IsObjectInCameraView(Transform objectTransform)
    {
        // get all renderer components of the object and its children
        Renderer[] renderers = objectTransform.GetComponentsInChildren<Renderer>();

        // loop through each renderer and make sure all of them are in bounds of the camera
        // to make sure you need a good chunk of the object in the photo to count it
        foreach (Renderer objrenderer in renderers)
        {
            // get the bounds of the renderer
            Bounds bounds = objrenderer.bounds;

            // check if the bounds are within the camera's view
            if (!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(linkedCamera), bounds))
            {
                return false;
            }
        }

        // check if another object is blocking the view of the current object by raycasting from linked camera to the object
        RaycastHit hit;
        if (Physics.Raycast(linkedCamera.transform.position, objectTransform.position - linkedCamera.transform.position, out hit, Mathf.Infinity))
        {
            if (hit.transform != objectTransform)
            {
                return false; // if the hit object is not the object, then the object is not in view
            }
        }

        // Check if the object is centered enough in the camera view to be considered in the photo
        Vector3 objectScreenPosition = linkedCamera.WorldToScreenPoint(objectTransform.position);
        // get horizontal and vertical distance from the center of the linkedCamera screen
        float horizontalDistance = Mathf.Abs(objectScreenPosition.x - linkedCamera.pixelWidth / 2);
        float verticalDistance = Mathf.Abs(objectScreenPosition.y - linkedCamera.pixelHeight / 2);
        if (horizontalDistance > ((linkedCamera.pixelWidth / 2) - 52) || verticalDistance > ((linkedCamera.pixelHeight / 2) - 31))
        {
            return false;
        }

        return true;
    }

    private float CalculateDistanceScore(Vector3 objectPosition)
    {
        float maxDistanceCurrentFOV = (maxDistanceAtZoomOut * transform.localScale.z) * (maxZoomOutFov / linkedCamera.fieldOfView);
        // Debug.Log($"Max distance at current FOV is {maxDistanceCurrentFOV}");
        float distance = Vector3.Distance(transform.position, objectPosition);
        // Debug.Log($"Current distance is {distance}");

        // using a curve of the form squareroot(-x + maxDistanceCurrentFOV) to determine the score
        float score = Mathf.Sqrt(Mathf.Max(0, -distance + maxDistanceCurrentFOV));
        float minDistanceScore = Mathf.Sqrt(-(minDistanceNoClipping * transform.localScale.z) + maxDistanceCurrentFOV);

        // Debug.Log($"Score is {score} and minDistanceScore is {minDistanceScore}");

        // find percentage of score out of minDistanceScore to determine the score (round to 100 if over)
        // take score over minDistanceScore and clamp it to between 0 and 1
        float finalScore = Mathf.Max(0, Mathf.Min((score / minDistanceScore), 1)) * 100;
        // Debug.Log($"Final score is {finalScore}");

        return finalScore;
    }

    public void ZoomIn()
    {
        ZoomCamera(maxZoomInFov);
    }

    public void ZoomOut()
    {
        ZoomCamera(maxZoomOutFov);
    }

    private void ZoomCamera(float target)
    {
        linkedCamera.fieldOfView = Mathf.MoveTowards(linkedCamera.fieldOfView, target, zoomSpeed * Time.deltaTime);

        // stop zoom sound if camera is at target zoom
        if (Mathf.Abs(linkedCamera.fieldOfView - target) < 0.01f)
        {
            StopZoomSound();
        }
    }

    public void PlayCameraZoomSound()
    {
        zoomAudioSource.Play();
    }

    public void StopZoomSound()
    {
        zoomAudioSource.Stop();
    }

    private IEnumerator GivePhotoHint()
    {
        yield return new WaitForSecondsRealtime(60);
        if (photosTaken == 0 )
        {
            NPCDialogueManager.Instance.PlaySound(VoiceLine.USE_CAMERA_HINT);
        }
    }

    private IEnumerator FirstPhoto()
    {
        yield return new WaitForSecondsRealtime(2);
        NPCDialogueManager.Instance.PlaySound(VoiceLine.FIRST_PHOTO);
    }
}
