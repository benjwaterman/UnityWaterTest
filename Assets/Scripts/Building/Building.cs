using UnityEngine;
using System.Collections;

public enum BuildingType { Sandbags, Wall };

public abstract class Building : MonoBehaviour {

    public string buildingName;
    public float buildingStrength = 1;
    public int buildingCost = 100;
    public float ConstructionSpeed = 1;

    protected bool bIsConstructing;
    bool bIsDemolishing;
    protected Vector3 position;
    float halfHeight;

    void Start() {
        halfHeight = gameObject.GetComponent<Collider>().bounds.extents.y;
    }

    protected virtual void Update() {
        //If building has just been placed
        if(bIsConstructing) {
            //Move upwards towards position, speed scaled by size
            transform.position = Vector3.MoveTowards(transform.position, position, ConstructionSpeed * (halfHeight * 2) * Time.deltaTime);

            //If reached position, no longer constructing
            if(transform.position == position) {
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
    }

    public virtual void Construct() {
        //Store position to move to
        position = transform.position;
        //Move object to be just under the surface of where it has been placed
        transform.position = new Vector3(transform.position.x, transform.position.y - halfHeight * 2, transform.position.z); //halfHeight * 4
        bIsConstructing = true;
    }

    public virtual void FinishedConstruction() {

    }

    public virtual void Demolish() {
        position = new Vector3(transform.position.x, transform.position.y - halfHeight * 2, transform.position.z);
        //Demolishing is true
        bIsDemolishing = true;
        //Constructing is false
        bIsConstructing = false;
    }
}
