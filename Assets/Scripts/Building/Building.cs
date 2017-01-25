using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Codes.Linus.IntVectors;

public enum BuildingType { Sandbags, Wall };

public abstract class Building : MonoBehaviour {

    public string buildingName;
    public float buildingStrength = 1;
    public int buildingCost = 100;
    public float ConstructionSpeed = 1;

    public GameObject testPrefab;

    protected bool bIsConstructing;
    protected bool bIsDemolishing;
    protected Vector3 position;
    protected Vector3 colliderExtents;

    float halfHeight;
    public List<Vector2> indiciesToCheck;

    void Start() {
        halfHeight = gameObject.GetComponent<Collider>().bounds.extents.y;
        colliderExtents = GetComponent<Collider>().bounds.extents;
    }

    protected virtual void Update() {
        //If building has just been placed
        if (bIsConstructing) {
            //Move upwards towards position, speed scaled by size
            transform.position = Vector3.MoveTowards(transform.position, position, ConstructionSpeed * (halfHeight * 2) * Time.deltaTime);

            //If reached position, no longer constructing
            if (transform.position == position) {
                bIsConstructing = false;
                FinishedConstruction();
            }
        }

        //If building is being demolished
        if (bIsDemolishing) {
            //Demolish 3x the speed of construction
            transform.position = Vector3.MoveTowards(transform.position, position, ConstructionSpeed * (halfHeight * 2) * Time.deltaTime);

            //If reached position, destroy
            if (transform.position == position) {
                bIsDemolishing = false;
                Destroy(this.gameObject);
            }
        }

        //Check water next to this building
        CheckAdjacentWater();
    }

    public virtual void Construct() {
        //Store position to move to
        position = transform.position;
        //Move object to be just under the surface of where it has been placed
        transform.position = new Vector3(transform.position.x, transform.position.y - halfHeight * 2, transform.position.z); //halfHeight * 4
        bIsConstructing = true;
    }

    public virtual void FinishedConstruction() {
        CalculateOuterPoints();
    }

    public virtual void Demolish() {
        position = new Vector3(transform.position.x, transform.position.y - halfHeight * 2, transform.position.z);
        //Demolishing is true
        bIsDemolishing = true;
        //Constructing is false
        bIsConstructing = false;
    }

    void CalculateOuterPoints() {
        //If rotated, invert bounds
        if ((int)transform.localRotation.eulerAngles.y == 90 || (int)transform.localRotation.eulerAngles.y == 270) {
            float temp = colliderExtents.x;
            colliderExtents.x = colliderExtents.z;
            colliderExtents.z = temp;
        }

        //Get length of each axis
        int xSize = (int)(colliderExtents.x - -colliderExtents.x);
        int zSize = (int)(colliderExtents.z - -colliderExtents.z);

        //Get all points touching where water could be. Adding/subtracting one so we get the adjacent points to the building as well
        for (int i = 0; i <= xSize / 2 + 1; i++) {
            for (int j = 0; j <= zSize / 2 + 1; j++) {
                indiciesToCheck.Add(new Vector2i(
                    Mathf.RoundToInt((transform.position.x + colliderExtents.x % 1) + i),
                    Mathf.RoundToInt((transform.position.z + colliderExtents.z % 1) + j)
                    ));

                indiciesToCheck.Add(new Vector2i(
                    Mathf.RoundToInt((transform.position.x - colliderExtents.x % 1) - i),
                    Mathf.RoundToInt((transform.position.z - colliderExtents.z % 1) - j)
                    ));

                indiciesToCheck.Add(new Vector2i(
                    Mathf.RoundToInt((transform.position.x - colliderExtents.x % 1) - i),
                    Mathf.RoundToInt((transform.position.z + colliderExtents.z % 1) + j)
                    ));

                indiciesToCheck.Add(new Vector2i(
                    Mathf.RoundToInt((transform.position.x + colliderExtents.x % 1) + i),
                    Mathf.RoundToInt((transform.position.z - colliderExtents.z % 1) - j)
                    ));
            }
        }

        foreach (Vector2i vec2 in indiciesToCheck) {
            Instantiate(testPrefab, new Vector3(vec2.x, 0, vec2.y), Quaternion.identity);
        }
    }

    //For comparing to water next to this building
    void CheckAdjacentWater() {
        //CHECK WATER LEVELS HERE
        foreach (Vector2i vec2 in indiciesToCheck) {
            //If water is higher than than this building, destroy the building
            try {
                if (WaterController.Current.waterCellArray[vec2.x, vec2.y].volume > buildingStrength) {
                    Demolish();
                }
            }
            catch (System.Exception) { //BLOC KSPAWNING AT 1000 FIX
                Debug.Log(vec2.x + " " + vec2.y);
                throw;
            }

        }
    }
}
