using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class NPCController : MonoBehaviour
{

    public Transform player;
    public float followDistance;
    public float maxDistance;
    public float followSpeed = 0.2f;
    public int waitTime = 30;
    private float currentDistance;

    private void Awake()
    {

    }

    private void Update(){

        currentDistance = GetPlayerDistance();
        if (currentDistance > maxDistance){
            FollowPlayer();           
        }
        Wait(waitTime);
        

    }

    private float GetPlayerDistance(){
        //check how far away the player is from the fairy
        return Vector3.Distance(player.transform.position, transform.position);
    }

    private void FollowPlayer(){
        //move fairy so that they are x distance from player
        transform.position = Vector3.MoveTowards(
            transform.position, 
            player.position + new Vector3(followDistance, 0, 0), 
            followSpeed);
    }

    private IEnumerator Wait(int seconds){
        yield return new WaitForSecondsRealtime(seconds);
        
    }




}
