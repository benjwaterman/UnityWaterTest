using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Codes.Linus.IntVectors;

public class Drain : Building {
    //Rate of water the drain can take in per second
    public float WaterDrainRate = 0.5f;
    //Gameobject to raise bottom of ditch to match water level
    public GameObject WaterBase;
    //What depth to sink to
    public float SinkDepth = -0.446f;

    //float currentDrainAmount;
    Vector3 originalBasePosition;

    //Special behviour in the form of draining water
    protected override void Update() {
        //Still call base update
        base.Update();
        //If game is not paused and isn't constructing
        if (!GameController.Current.bIsPaused && !bIsConstructing) {
            //For every index, take away the volume from cell
            foreach (Vector2i vec2 in buildingIndicies) {
                try {
                    float volume = WaterController.Current.waterCellArray[vec2.x, vec2.y].volume;
                    float amountToDrain = WaterDrainRate * Time.deltaTime;
                    if (volume - amountToDrain > 0) {
                        //Take amount away from the water
                        WaterController.Current.UpdateCellVolume(vec2.x, vec2.y, volume - amountToDrain);
                    }
                    else if (volume - amountToDrain < 0) {
                        WaterController.Current.UpdateCellVolume(vec2.x, vec2.y, 0);
                    }
                }
                catch (System.Exception E) {
                    Debug.Log(E.Message + " in Drain.cs");
                    break;
                }
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
