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
    //The amount the building decays per day
    public float DecaySpeed = 0.1f;
    public bool bIsObjectiveBuilding;
    public bool bIsDrain;
    public bool bIsAlive = true;

    public GameObject testPrefab;

    protected bool bIsConstructing;
    protected bool bIsDemolishing;
    protected Vector3 position;
    protected Vector3 colliderExtents;
    //Amount building has decayed
    protected float decayAmount = 0;

    float halfHeight;
    //List of indicies to check for water
    public List<Vector2i> indiciesToCheck;
    //Indicies that the building is on
    public List<Vector2i> buildingIndicies;
    Renderer thisRenderer;
    int daysAlive = 0;

    void Start() {
        halfHeight = gameObject.GetComponent<Collider>().bounds.extents.y;
        colliderExtents = GetComponent<Collider>().bounds.extents;
        thisRenderer = GetComponent<Renderer>();

        if (bIsObjectiveBuilding) {
            //Add self to list of buildings that need to be protected
            GameController.Current.AddObjective(this.gameObject);
            //Calculate bounds
            CalculateOuterPoints();
            CalculateInnerPoints();
            bIsConstructing = false;
            bIsDemolishing = false;
        }
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
            //Demolish
            transform.position = Vector3.MoveTowards(transform.position, position, ConstructionSpeed * Time.deltaTime);

            //If reached position, destroy
            if (transform.position == position) {
                bIsDemolishing = false;
                //Do not destroy objective buildings
                if (!bIsObjectiveBuilding) {
                    Destroy(this.gameObject);
                }
            }
        }

        //If not constructing or demolishing
        if (bIsAlive && !bIsConstructing && !bIsDemolishing) {
            //If not a drain
            if (!bIsDrain) {
                //Check water next to this building
                CheckAdjacentWater();
            }
        }
    }

    public void Decay() {
        //If is a drain, don't do anything
        if(bIsDrain) {
            return;
        }

        //If this is not the first day the building has been placed for
        if (daysAlive > 0) {
            //Increase decay amount
            decayAmount = DecaySpeed * buildingStrength;
            //Decrease building strength
            if (decayAmount < buildingStrength) {
                buildingStrength -= decayAmount;
            }
        }

        //Increment days alive
        daysAlive++;
    }

    public virtual void Construct() {
        //Only for defenses
        if (!bIsObjectiveBuilding) {
            //Store position to move to
            position = transform.position;
            //Move object to be just under the surface of where it has been placed
            transform.position = new Vector3(transform.position.x, transform.position.y - halfHeight * 2, transform.position.z); //halfHeight * 4
            bIsConstructing = true;
        }
    }

    public virtual void FinishedConstruction() {
        BuildingController.Current.PlacedBuildings.Add(this.gameObject);
        CalculateOuterPoints();
        CalculateInnerPoints();
    }

    public virtual void Demolish() {
        bIsAlive = false;
        if (bIsObjectiveBuilding) {
            position = new Vector3(transform.position.x, transform.position.y - halfHeight * 0.5f, transform.position.z);
            //Change material to demolished material
            gameObject.GetComponent<Renderer>().material = GameController.Current.DemolishedBuildingMaterial;
            //Set isAlive to false
            GetComponent<Building>().bIsAlive = false;
        }
        else {
            position = new Vector3(transform.position.x, transform.position.y - halfHeight * 2, transform.position.z);
        }
        //Demolishing is true
        bIsDemolishing = true;
        //Constructing is false
        bIsConstructing = false;
        //Disable collider
        GetComponent<Collider>().enabled = false;
        //For each point set world height array to 0
        WaterController.Current.UpdateWorldHeightArray(buildingIndicies.ToArray(), 0);
        if (!bIsObjectiveBuilding) {
            BuildingController.Current.PlacedBuildings.Remove(this.gameObject);
        }
    }

    void CalculateOuterPoints() {
        //Adding / subtracting one so we get the adjacent points to the building as well
        CalculatePoints(1, out indiciesToCheck);
    }

    void CalculateInnerPoints() {
        CalculatePoints(0, out buildingIndicies);
    }

    void CalculatePoints(int reach, out List<Vector2i> outList) {
        List<Vector2i> list = new List<Vector2i>();
        Vector3 adjustedExtents = colliderExtents;

        //If rotated, invert bounds
        if ((int)transform.localRotation.eulerAngles.y == 90 || (int)transform.localRotation.eulerAngles.y == 270) {
            adjustedExtents.x = colliderExtents.z;
            adjustedExtents.z = colliderExtents.x;
        }

        //Get length of each axis
        int xSize = (int)(adjustedExtents.x - -adjustedExtents.x);
        int zSize = (int)(adjustedExtents.z - -adjustedExtents.z);

        //Get all points 
        for (int i = 0; i <= xSize / 2 + reach; i++) {
            for (int j = 0; j <= zSize / 2 + reach; j++) {
                list.Add(new Vector2i(
                    Mathf.RoundToInt((transform.position.x + adjustedExtents.x % 1) + i),
                    Mathf.RoundToInt((transform.position.z + adjustedExtents.z % 1) + j)
                    ));

                list.Add(new Vector2i(
                    Mathf.RoundToInt((transform.position.x - adjustedExtents.x % 1) - i),
                    Mathf.RoundToInt((transform.position.z - adjustedExtents.z % 1) - j)
                    ));

                list.Add(new Vector2i(
                    Mathf.RoundToInt((transform.position.x - adjustedExtents.x % 1) - i),
                    Mathf.RoundToInt((transform.position.z + adjustedExtents.z % 1) + j)
                    ));

                list.Add(new Vector2i(
                    Mathf.RoundToInt((transform.position.x + adjustedExtents.x % 1) + i),
                    Mathf.RoundToInt((transform.position.z - adjustedExtents.z % 1) - j)
                    ));
            }
        }

        outList = list;
        /*if (reach == 1) {
            foreach (Vector2i vec2 in list) {
                Instantiate(testPrefab, new Vector3(vec2.x, 0, vec2.y), Quaternion.identity);
            }
        }*/
    }

    //For comparing to water next to this building
    void CheckAdjacentWater() {
        float highestVolume = 0;
        foreach (Vector2i vec2 in indiciesToCheck) {
            float volume = 0;
            try {
                volume = WaterController.Current.waterCellArray[vec2.x, vec2.y].volume;
            }
            catch (System.Exception) {
                Debug.Log(vec2.x + " " + vec2.y);
                throw;
            }

            //If is objective, any water destroys it
            if (bIsObjectiveBuilding) {
                if (volume > 0.015f) {
                    Demolish();
                    break;
                }
                continue;
            }

            else {
                //Record highest volume
                if (volume > highestVolume) {
                    highestVolume = volume;
                }

                //If water is higher than than this building, destroy the building
                if (volume > buildingStrength) {
                    //If not already demolishing
                    if (!bIsDemolishing && !bIsConstructing) {
                        Demolish();
                        break;
                    }
                }

                //Increase red depending on volume in relation to strength
                Color color = thisRenderer.material.color;
                thisRenderer.material.color = new Color(highestVolume / buildingStrength, color.g, color.b);
            }
        }
    }
}
