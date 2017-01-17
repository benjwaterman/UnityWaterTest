using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceBuilding : MonoBehaviour {
    //Bitshift to get layer mask to compare against, 9 is layer of "Floor"
    private int layerMask = 9 << 8;
    //Store list of objects colliding
    List<GameObject> collisionList = new List<GameObject>();
    //Store size of object to check for collisions with
    Vector3 objectBounds;
    //Store this material
    Material thisMaterial;

	// Use this for initialization
	void Start () {
        //Get size of base mesh
        objectBounds = GetComponent<MeshFilter>().mesh.bounds.extents;
        //Multiply it by the scale of the object
        objectBounds.x *= transform.localScale.x;
        objectBounds.y *= transform.localScale.y;
        objectBounds.z *= transform.localScale.z;
        //Disable collisions while placing the object
        gameObject.GetComponent<Collider>().enabled = false;
        //Store this material
        thisMaterial = gameObject.GetComponent<Renderer>().material;
        //Change material to transparent material
        gameObject.GetComponent<Renderer>().material = BuildingController.Current.PlaceMaterial;
    }
	
	// Update is called once per frame
	void Update () { 
        //Move transform with mouse, as long as mouse raycast is hitting something
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.transform != null)
            {
                transform.position = hit.point;
            }
        }

        //Check for any collisions, excluding the floor
        var hitColliders = Physics.OverlapBox(transform.position, new Vector3(objectBounds.x , objectBounds.y, objectBounds.z), Quaternion.identity, ~layerMask);
        //Object is colliding
        if (hitColliders.Length > 0) { 
            gameObject.GetComponent<Renderer>().material.color = new Color(0.8f, 0 , 0, 0.5f);
        }
        //Object is not colliding
        else {
            gameObject.GetComponent<Renderer>().material.color = new Color(0, 0.8f, 0, 0.5f);
        }

        if(Input.GetMouseButtonDown(0)) {
            Place();
        }
    }

    void Place() {
        //Set the colour back to its original
        gameObject.GetComponent<Renderer>().material.color = Color.white;
        //Re enable the collider
        gameObject.GetComponent<Collider>().enabled = true;
        //Assign original material
        gameObject.GetComponent<Renderer>().material = thisMaterial;
        //Destroy this component 
        Destroy(this);
    }

    //void OnCollisionEnter(Collision col)
    //{
    //    //If is not the floor
    //    if(col.gameObject.tag != "Floor") {
    //        //Add to list of collisions
    //        collisionList.Add(col.gameObject);
    //    }
    //}

    //void OnCollisionExit(Collision col) {
    //    //No longer colliding, so remove from list
    //    collisionList.Remove(col.gameObject);
    //}
}
