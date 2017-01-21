using UnityEngine;
using System.Collections;

public class OnWaterTouch : MonoBehaviour {

    Vector3 colliderExtents;
    //Water layer
    int layerMask;
    //Has collided
    bool bHasCollided = false;

    void Start() {
        //Add self to list of buildings that need to be protected
        GameController.Current.ObjectivesList.Add(this.gameObject);

        colliderExtents = gameObject.GetComponent<Collider>().bounds.extents;
        layerMask = LayerMask.NameToLayer("Water") << 8;
    }

    void Update() {
        if (!bHasCollided) {
            //Check if colliding with water 
            var hitColliders = Physics.OverlapBox(transform.position, colliderExtents + new Vector3(0.1f, 0.05f, 0), transform.rotation);
            if (hitColliders.Length > 0) {
                foreach (Collider coll in hitColliders) {
                    if (coll.tag == ("Water")) {
                        DemolishSelf();
                    }
                }
            }
        }
    }

    void DemolishSelf() {
        //Has collided
        bHasCollided = true;
        //Remove from list of undestroyed buildings
        GameController.Current.ObjectivesList.Remove(this.gameObject);
        //Disable collider so this isnt called again
        GetComponent<Collider>().enabled = false;
        //Update world height array
        WaterController.Current.RefreshWorld();
        //Destory this
        Destroy(this.gameObject);
    }
}
