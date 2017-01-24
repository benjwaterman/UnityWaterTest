﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceBuilding : MonoBehaviour {
    //Bitshift to get layer mask to compare against, 9 is layer of "Floor"
    private int layerMask = 9 << 8;
    //Store size of object to check for collisions with
    Vector3 objectExtents;
    //Store materials
    Material[] thisMaterial;
    //Store renderer components
    Renderer[] thisRenderer;
    //Store building that we're placing
    Building thisBuilding;
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

        //Assign renderers
        thisRenderer = gameObject.GetComponentsInChildren<Renderer>();
        //Initialise array with number of renderes
        thisMaterial = new Material[thisRenderer.Length];
        //Store materials and change material to place material
        for (int i = 0; i < thisRenderer.Length; i++) {
            thisMaterial[i] = thisRenderer[i].material;
            thisRenderer[i].material = BuildingController.Current.PlaceMaterial;
        }

        //Store building
        thisBuilding = gameObject.GetComponent<Building>();
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
                position.x = Mathf.Round(hit.point.x) + colliderExtents.x % 1;// + colliderExtents.x;
                position.y = hit.point.y + colliderExtents.y;
                position.z = Mathf.Round(hit.point.z) + colliderExtents.z % 1;// + colliderExtents.z;
                transform.position = position;
            }
        }

        //Check for any collisions, excluding the floor. We reduce the extents by 0.1f in order to allow buildings to be placed directly next to each other, as otherwise a collision it detected
        var hitColliders = Physics.OverlapBox(transform.position, new Vector3(objectExtents.x - 0.1f, objectExtents.y, objectExtents.z - 0.1f), transform.rotation, ~layerMask);
        //Object is colliding
        if (hitColliders.Length > 0) {
            SetColour(new Color(0.8f, 0, 0, 0.5f));
            //Can't place while colliding
            bCanPlace = false;
        }
        //Object is not colliding
        else {
            SetColour(new Color(0, 0.8f, 0, 0.5f));
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
        //If player has enough credits
        if (GameController.Current.currentCredits >= thisBuilding.buildingCost) {
            //If can place
            if (bCanPlace) {
                //Update credits
                GameController.Current.currentCredits -= thisBuilding.buildingCost;
                //Set the colour back to its original
                SetColour(Color.white);
                //Re enable the collider
                gameObject.GetComponent<Collider>().enabled = true;
                //Assign original material
                for (int i = 0; i < thisRenderer.Length; i++) {
                    thisRenderer[i].material = thisMaterial[i];
                }
                //Tell the building it has been placed
                thisBuilding.Construct();
                //If shift is held
                if (Input.GetKey(KeyCode.LeftShift)) {
                    //Create another building to place
                    if (gameObject.GetComponent<Sandbags>()) {
                        BuildingController.Current.PlaceSandbags();
                    }
                    else if(gameObject.GetComponent<Dam>()) {
                        BuildingController.Current.PlaceDam();
                    }
                    else if (gameObject.GetComponent<Ditch>()) {
                        BuildingController.Current.PlaceDitch();
                    }
                }
                //Destroy this component 
                Destroy(this);
            }
            //Not a suitable location
            else {
                Debug.Log("Can't place here");
            }
        }
        //Not enough credits
        else {
            Debug.Log("Not enough credits");
        }
    }

    void CancelPlace() {
        Destroy(gameObject);
    }

    void SetColour(Color color) {
        foreach (Renderer ren in thisRenderer) {
            ren.material.color = color;
        }
    }

    //When script is destroyed, no longer placing a building
    void OnDestroy() {
        BuildingController.Current.bIsPlacing = false;
        //Destroy mesh collider attached to water, this is for performance reasons
        //Destroy(WaterController.Current.gameObject.GetComponent<MeshCollider>());
    }
}
