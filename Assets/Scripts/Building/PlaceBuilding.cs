using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceBuilding : MonoBehaviour {
    //Bitshift to get layer mask to compare against, 9 is layer of "Floor"
    private int layerMask = 9 << 8;
    //Store size of object to check for collisions with
    Vector3 objectExtents;
    //Store this material
    Material thisMaterial;
    //Store renderer component
    Renderer thisRenderer;
    //Bool if building can be placed 
    bool bCanPlace = false;
    //Stores bounds of collider for calculating grid position 
    Vector3 colliderExtents;

    // Use this for initialization
    void Start() {
        //Get size of base mesh
        objectExtents = GetComponent<MeshFilter>().mesh.bounds.extents;
        //Multiply it by the scale of the object
        objectExtents.x *= transform.localScale.x;
        objectExtents.y *= transform.localScale.y;
        objectExtents.z *= transform.localScale.z;
        //Get extents of collider 
        colliderExtents = gameObject.GetComponent<Collider>().bounds.extents;
        //Disable collisions while placing the object
        gameObject.GetComponent<Collider>().enabled = false;
        //Store this material
        thisMaterial = gameObject.GetComponent<Renderer>().material;
        //Change material to transparent material
        gameObject.GetComponent<Renderer>().material = BuildingController.Current.PlaceMaterial;
        //Assign renderer
        thisRenderer = gameObject.GetComponent<Renderer>();
        //Tell building controller we are placing an object
        BuildingController.Current.bIsPlacing = true;
        //Add mesh collider to water so building cant be placed in water
        //WaterController.Current.gameObject.AddComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update() {
        //Move transform with mouse, as long as mouse raycast is hitting something
        RaycastHit hit;
        //Make position same as where mouse is, as long as it is over terrain
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
            if (hit.transform != null) {
                //Lock to grid
                Vector3 position = Vector3.zero;
                //Add half length of object to make sure its aligned with grid
                position.x = Mathf.Round(hit.point.x);// + colliderExtents.x;
                position.y = hit.point.y + colliderExtents.y;
                position.z = Mathf.Round(hit.point.z);// + colliderExtents.z;
                transform.position = position;
            }
        }

        //Check for any collisions, excluding the floor
        var hitColliders = Physics.OverlapBox(transform.position, new Vector3(objectExtents.x - 0.1f, objectExtents.y, objectExtents.z - 0.1f), transform.rotation, ~layerMask);
        //Object is colliding
        if (hitColliders.Length > 0) {
            thisRenderer.material.color = new Color(0.8f, 0, 0, 0.5f);
            //Can't place while colliding
            bCanPlace = false;
        }
        //Object is not colliding
        else {
            thisRenderer.material.color = new Color(0, 0.8f, 0, 0.5f);
            bCanPlace = true;
        }

        //When left mouse is clicked
        if (Input.GetMouseButtonDown(0)) {
            Place();
        }

        //When right mouse button or esc is pressed
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
            CancelPlace();
        }

        //When R is pressed rotate
        if (Input.GetKeyDown(KeyCode.R)) {
            transform.Rotate(0, 90, 0);
        }
    }

    void Place() {
        if (bCanPlace) {
            //Set the colour back to its original
            thisRenderer.material.color = Color.white;
            //Re enable the collider
            gameObject.GetComponent<Collider>().enabled = true;
            //Assign original material
            thisRenderer.material = thisMaterial;
            //Destroy this component 
            Destroy(this);
        }
        else {
            Debug.Log("Can't place here");
        }
    }

    void CancelPlace() {
        Destroy(gameObject);
    }

    //When script is destroyed, no longer placing a building
    void OnDestroy() {
        BuildingController.Current.bIsPlacing = false;
    }
}
