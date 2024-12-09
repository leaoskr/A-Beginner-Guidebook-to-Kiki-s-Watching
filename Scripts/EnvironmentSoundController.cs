using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSoundController : MonoBehaviour
{


    public AudioSource river;
    public AudioSource bgm;
    public AudioSource lakeBgm;

    private bool firstTime = true;

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player"){
            if (firstTime){
                NPCDialogueManager.Instance.PlaySound(VoiceLine.STATUE_HINT);
                firstTime = false;
            }
            bgm.volume = 0;
            lakeBgm.volume = river.volume = 1;
        }

        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player"){
            bgm.volume = 1;
            lakeBgm.volume = river.volume = 0;
        }
        
    }


    public void ChangeAreaBGM(){

        if (bgm.volume > 0 ){
            bgm.volume = 0;
            lakeBgm.volume = river.volume = 1;
        }
        else {
            bgm.volume = 0.7f;
            lakeBgm.volume = river.volume = 0;
        }
        


    }
}
