using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Codes.Linus.IntVectors;

public class Ditch : Building {
    //Amount of water the ditch should drain before it is full
    public int WaterDrainAmount;
    //Gameobject to raise bottom of ditch to match water level
    public GameObject WaterBase;

    float currentDrainAmount;
    //Store list of where water could touch
    public List<Vector2i> waterIndices = new List<Vector2i>();
    Vector3 colliderExtents;
    Vector3 originalBasePosition;

    //Special behviour in the form of draining water
    protected override void Update() {
        //Still call base update
        base.Update();
        //If game is not paused
        if (!GameController.Current.bIsPaused) {
            //If has not filled up
            if (currentDrainAmount < WaterDrainAmount) {
                //For every index, take away the volume from cell
                foreach (Vector2i vec2 in waterIndices) {
                    //Keep track of water drained
                    currentDrainAmount += WaterController.Current.waterCellArray[vec2.x, vec2.y].volume;
                    //Take amount away from the water
                    WaterController.Current.UpdateCellVolume(vec2.x, vec2.y, 0);
                }
            }

            if (WaterBase.transform.position.y < transform.position.y + 0.4) {
                WaterBase.transform.position = Vector3.MoveTowards(WaterBase.transform.position, new Vector3(originalBasePosition.x, originalBasePosition.y + currentDrainAmount / WaterDrainAmount, originalBasePosition.z), 10 * Time.deltaTime);
            }
        }
    }

    //Override original construct as a ditch will place a different way from a normal building
    public override void Construct() {
        transform.position = new Vector3(transform.position.x, -0.446f, transform.position.z);

        colliderExtents = GetComponent<Collider>().bounds.extents;
        originalBasePosition = WaterBase.transform.position;
        //Get all points touching where water could be
        for (int i = 1; i <= colliderExtents.x * 2; i++) {
            for (int j = 1; j <= colliderExtents.z * 2; j++) {
                waterIndices.Add(new Vector2i(
                    //X //MAKE THIS WORK
                    Mathf.RoundToInt(transform.position.x + colliderExtents.x % 1 * i), 
                    //Z
                    Mathf.RoundToInt(transform.position.z + colliderExtents.z % 1 * j)
                    ));

                waterIndices.Add(new Vector2i(
                    //X
                    Mathf.RoundToInt(transform.position.x - colliderExtents.x % 1 * i), 
                    //Z
                    Mathf.RoundToInt(transform.position.z - colliderExtents.z % 1 * j)
                    ));
            }
        }
    }
}
