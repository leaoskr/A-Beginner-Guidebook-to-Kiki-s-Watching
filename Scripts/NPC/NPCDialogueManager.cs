using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum VoiceLine{

    WELCOME_PLAYER, //game manager event
    USE_CAMERA_HINT, //camera event
    FIRST_PHOTO, //camera event
    PHOTO_FAIRY, //notebook event
    LARGE_BUTTERFLY, //butterfly spawn event
    GAME_END_HINT,
    NOTEBOOK_COMPLETE, //notebook event
    GOODBYE, //game manager event

    MORE_PHOTOS,

    STATUE_HINT
}

[RequireComponent(typeof(AudioSource))]
public class NPCDialogueManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] voiceLines;
    
    public static NPCDialogueManager Instance { get; private set; }
    private AudioSource audioSource;
    
    [Header("Subtitle Params")]
    [SerializeField] private GameObject subtitlesPanel;
    [SerializeField] private TMP_Text subtitlesText;
    [SerializeField] private float textSpeed;
    [SerializeField] private Animator fairyAnimator;
    
    private Dictionary<VoiceLine, string[]> voiceLineTexts = new Dictionary<VoiceLine, string[]>{
        {VoiceLine.WELCOME_PLAYER, new[] {"This is sample text! Replace me lol"}},
        {VoiceLine.USE_CAMERA_HINT, new[] {"This is sample text 2! Replace me lol"}},
        {VoiceLine.FIRST_PHOTO, new[] {"This is sample text 3! Replace me lol"}},
        {VoiceLine.PHOTO_FAIRY, new[] {"This is sample text 4! Replace me lol"}},
        {VoiceLine.LARGE_BUTTERFLY, new[] {"This is sample text 5! Replace me lol"}},
        {VoiceLine.GAME_END_HINT, new[] {"This is sample text 6! Replace me lol"}},
        {VoiceLine.NOTEBOOK_COMPLETE, new[] {"This is sample text 7! Replace me lol"}},
        {VoiceLine.GOODBYE, new[] {"This is sample text 8! Replace me lol"}}
    };
        
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start(){
        audioSource = GetComponent<AudioSource>();      
        StartCoroutine(WelcomePlayer());
        ClearSubtitleUI();
    }
    private void Update(){

    }

    private IEnumerator WelcomePlayer(){
        yield return new WaitForSecondsRealtime(20);
        PlaySound(VoiceLine.WELCOME_PLAYER);
    }

    public void PlaySound(VoiceLine sound, float volume=1){

        //wait for other voice lines to be finished
        while (audioSource.isPlaying){
            WaitToSpeak();
        }

        //play requested voiceline
        AudioClip requestedClip = voiceLines[(int)sound];
        
        // fairyAnimator.SetBool("isTalking", true);
        audioSource.PlayOneShot(requestedClip, volume); 
        StartCoroutine(TalkingAnimation(requestedClip.length)); 
        // gameObject.GetComponent<Animator>().Play("GeoFairyTalk");   
        //PlaySubtitle(sound, requestedClip.length);

        
    }

    private IEnumerator TalkingAnimation(float audioClipLength) {
        fairyAnimator.SetBool("isTalking", true);
        yield return new WaitForSeconds(audioClipLength);
        fairyAnimator.SetBool("isTalking", false);
    }

    private IEnumerator WaitToSpeak(){
        yield return null;
    }
    
    //
    // Subtitle UI Functions 
    //
    private void ClearSubtitleUI()
    {
        subtitlesPanel.SetActive(false);
        subtitlesText.text = "";
    }

    private void PlaySubtitle(VoiceLine sound, float voiceLineLength)
    {
        StopAllCoroutines();
        ClearSubtitleUI();
        subtitlesPanel.SetActive(true);
        
        StartCoroutine(ShowSubtitle(sound, voiceLineLength));
    }

    private IEnumerator ShowSubtitle(VoiceLine sound, float voiceLineLength)
    {
        // time to keep subtitles showing to match voice line length
        float timeToWait = voiceLineLength;
        
        string[] linesToDisplay = voiceLineTexts[sound];
        foreach (string line in linesToDisplay)
        {
            subtitlesText.text = line;
            subtitlesText.ForceMeshUpdate();
            int totalVisibleCharacters = subtitlesText.textInfo.characterCount;
            
            for (int counter = 0; counter <= totalVisibleCharacters; counter++)
            {
                subtitlesText.maxVisibleCharacters = counter;
                yield return new WaitForSeconds(textSpeed);
                timeToWait -= textSpeed;
            }
            
            // Time to wait between starting new sentences/sections of script
            yield return new WaitForSeconds(0.5f);
        }
        
        if (timeToWait > 0)
        {
            yield return new WaitForSeconds(timeToWait);
        }
        ClearSubtitleUI();
    }

}
