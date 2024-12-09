using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private string photoDirectory;

    // Public read-only property to expose photoDirectory
    public string PhotoDirectory => photoDirectory;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Create a directory to store player photos
            photoDirectory = Path.Combine(Application.persistentDataPath, "Photos");
            if (!Directory.Exists(photoDirectory))
            {
                Directory.CreateDirectory(photoDirectory);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        // Delete the photo directory when the game is closed
        // so that the photos are not stored permanently on the device (only for the current game session)
        if (Directory.Exists(photoDirectory))
        {
            Directory.Delete(photoDirectory, true);
            Debug.Log("Screenshots deleted on application quit.");
        }
    }
}
