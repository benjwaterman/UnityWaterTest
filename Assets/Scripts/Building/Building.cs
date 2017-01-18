using UnityEngine;
using System.Collections;

public enum BuildingType { Sandbags, Wall };

public abstract class Building : MonoBehaviour {

    public string buildingName;
    public int buildingHealth = 100;
    public int buildingCost = 100;
    public float ConstructionSpeed = 1;

    bool bIsConstructing;
    bool bIsDemolishing;
    Vector3 position;

    void Update() {
        //If building has just been placed
        if(bIsConstructing) {
            //Move upwards towards position
            transform.position = Vector3.MoveTowards(transform.position, position, ConstructionSpeed * Time.deltaTime);

            //If reached position, no longer constructing
            if(transform.position == position) {
                bIsConstructing = false;
            }
        }

        //If building is being demolished
        if (bIsDemolishing) {
            //Demolish 3x the speed of construction
            transform.position = Vector3.MoveTowards(transform.position, position, ConstructionSpeed * 5 * Time.deltaTime);

            //If reached position, destroy
            if (transform.position == position) {
                bIsDemolishing = false;
                Destroy(this.gameObject);
            }
        }
    }

    public void Construct() {
        //Store position to move to
        position = transform.position;
        //Start the building under the ground
        float halfHeight = gameObject.GetComponent<Collider>().bounds.extents.y;
        //Move object to be just under the surface of where it has been placed
        transform.position = new Vector3(transform.position.x, transform.position.y - halfHeight * 2 , transform.position.z);
        bIsConstructing = true;
    }

    public void Demolish() {
        float halfHeight = gameObject.GetComponent<Collider>().bounds.extents.y;
        position = new Vector3(transform.position.x, transform.position.y - halfHeight * 2, transform.position.z);
        //Demolishing is true
        bIsDemolishing = true;
        //Constructing is false
        bIsConstructing = false;
    }
}
