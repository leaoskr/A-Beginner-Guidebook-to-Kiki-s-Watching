using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBehavior : MonoBehaviour
{
    public Material waterMaterial; // Assign water material in the inspector
    public float speedX = 0.1f;    // Speed at which the water moves in the X direction

    private float offsetX = 0.0f;

    void Update()
    {
        offsetX += Time.deltaTime * speedX;
        waterMaterial.SetTextureOffset("_BaseMap", new Vector2(offsetX, 0.0f));
    }
}
