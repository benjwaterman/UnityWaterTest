using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Codes.Linus.IntVectors;

public class Ditch : Building {
    //Amount of water the ditch should drain before it is full
    public int WaterDrainAmount;
    //Gameobject to raise bottom of ditch to match water level
    public GameObject WaterBase;
    //What depth to sink to
    public float SinkDepth = -0.446f;

    float currentDrainAmount;
    Vector3 originalBasePosition;

    //Special behviour in the form of draining water
    protected override void Update() {
        //Still call base update
        base.Update();
        //If game is not paused and isn't constructing
        if (!GameController.Current.bIsPaused && !bIsConstructing) {
            //If has not filled up
            if (currentDrainAmount < WaterDrainAmount) {
                //For every index, take away the volume from cell
                foreach (Vector2i vec2 in buildingIndicies) {
                    try {
                        //Keep track of water drained
                        currentDrainAmount += WaterController.Current.waterCellArray[vec2.x, vec2.y].volume;
                        //Take amount away from the water
                        WaterController.Current.UpdateCellVolume(vec2.x, vec2.y, 0);
                    }
                    catch (System.Exception E) {
                        Debug.Log(E.Message + " in Ditch.cs");
                        currentDrainAmount = WaterDrainAmount;
                        break;
                    }
                }
            }

            //Move water base up depending on how full it is
            if (WaterBase.transform.position.y < transform.position.y + 0.4) {
                WaterBase.transform.position = Vector3.MoveTowards(WaterBase.transform.position, new Vector3(originalBasePosition.x, originalBasePosition.y + currentDrainAmount / WaterDrainAmount, originalBasePosition.z), 10 * Time.deltaTime);
            }
        }
    }

    //Override original construct as a ditch will place a different way from a normal building
    public override void Construct() {
        //Store current position
        position = transform.position;
        //Depth to sink to
        position.y = SinkDepth;
        //Set is constructing
        bIsConstructing = true;
    }

    public override void FinishedConstruction() {
        base.FinishedConstruction();
        originalBasePosition = WaterBase.transform.position;
    }
}
