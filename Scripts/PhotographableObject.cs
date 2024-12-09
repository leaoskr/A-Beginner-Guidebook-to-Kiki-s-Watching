using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotographableObject : MonoBehaviour
{
    public enum Species
    {
        AFly,
        BFly,
        CFly,
        DFly,
        EFly,
        FFly,
        GFly,
        HFly,
        Fairy,
        BigButterfly
        // Add more species here
    }

    public Species species;

    // For each species, indicate the response when photographed (sound effects, animations, movements, game events, etc.)
    public void ResponseOnBeingPhotographed()
    {
        switch (species)
        {
            case Species.AFly:
                break;
            case Species.BFly:
                break;
            case Species.CFly:
                break;
            case Species.DFly:
                break;
            case Species.EFly:
                break;
            case Species.FFly:
                break;
            case Species.GFly:
                break;
            case Species.HFly:
                break;
            case Species.Fairy:
                NPCDialogueManager.Instance.PlaySound(VoiceLine.PHOTO_FAIRY);
                break;
            case Species.BigButterfly:
                break;
            default:
                break;
        }
    }
}
