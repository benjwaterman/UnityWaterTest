using UnityEngine;
using System.Collections;

public class OnWaterTouch : MonoBehaviour {

    Vector3 colliderExtents;
    //Water layer
    int layerMask;
    //Has collided
    bool bHasCollided = false;
    //Demolishing
    bool bIsDemolishing = false;
    Vector3 demolishedPosition;

    void Start() {
        //Add self to list of buildings that need to be protected
        GameController.Current.ObjectivesList.Add(this.gameObject);

        colliderExtents = gameObject.GetComponent<Collider>().bounds.extents;
        layerMask = LayerMask.NameToLayer("Water") << 8;
    }

    void Update() {
        if (bIsDemolishing) {
            //Move upwards towards position
            transform.position = Vector3.MoveTowards(transform.position, demolishedPosition, 2 * Time.deltaTime);

            //If reached position, no longer constructing
            if (transform.position == demolishedPosition) {
                bIsDemolishing = false;
            }
        }
    }

    public bool CheckForWaterCollision() {
        //Check if colliding with water 
        var hitColliders = Physics.OverlapBox(transform.position, colliderExtents + new Vector3(0.9f, 0.05f, 0), transform.rotation);
        if (hitColliders.Length > 0) {
            foreach (Collider coll in hitColliders) {
                if (coll.tag == ("Water")) {
                    return true;
                }
            }
        }
        return false;
    }

    public void DemolishSelf() {
        //Has collided
        bHasCollided = true;
        //Remove from list of undestroyed buildings
        GameController.Current.ObjectivesList.Remove(this.gameObject);
        //Disable collider so it isnt included in the world height array
        GetComponent<Collider>().enabled = false;
        //Change material to demolished material
        gameObject.GetComponent<Renderer>().material = GameController.Current.DemolishedBuildingMaterial;
        //Move height down 
        bIsDemolishing = true;
        //Set target height to 3/4 beneath surface
        demolishedPosition = transform.position - new Vector3(0, (colliderExtents.x * 1.5f), 0);

        //Update world height array
        //WaterController.Current.RefreshWorld();
        //Destory this
        //Destroy(this.gameObject);
    }
}
