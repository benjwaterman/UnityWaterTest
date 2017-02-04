using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingController : MonoBehaviour {

    public static BuildingController Current;

    public BuildingController() {
        Current = this;
    }

    void Awake() {
        Current = this;
    }

    //Layers that the building should collide with when placing it
    public LayerMask BuildingCollisionLayers;
    public LayerMask FloorLayerMask;
    public Material PlaceMaterial;
    public GameObject SandbagsPrefab;
    public GameObject DamPrefab;
    public GameObject DitchPrefab;
    public GameObject DrainPrefab;
    public GameObject ConcretePrefab;
    public bool bIsPlacing = false;
    public float RefundPercentage = 0.8f;
    public List<GameObject> PlacedBuildings;

    bool bIsDemolishing = false;
    Color backgroundColour;
    //The prefab od the object we're placing, eg sandbags etc
    GameObject currentlySelectedPrefab;
    //A reference to the actual object we're placing
    GameObject currentlySelectedObject;

    void Start() {
        backgroundColour = GameController.Current.BackgroundImage.color;
    }

    void Update() {
        //If in demolish mode
        if (bIsDemolishing) {
            //If click left mouse
            if (Input.GetMouseButtonDown(0)) {
                //Raycast from mouse to see if hit building object
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    //If gameobject has building component and is not an objective
                    if (hit.transform.gameObject.GetComponent<Building>()) {
                        if (!hit.transform.gameObject.GetComponent<ObjectiveBuilding>()) {
                            //Remove building
                            RemoveBuilding(hit.transform.gameObject);
                        }
                    }
                }
            }

            //If right mouse click or escape
            if (/*Input.GetMouseButtonDown(1) ||*/ Input.GetKeyDown(KeyCode.Escape)) {
                TurnOffActiveModes();
            }
        }
    }

    public void PlaceCurrentlySelected() {
        CreateBuilding(currentlySelectedPrefab);
    }

    public void PlaceSandbags() {
        currentlySelectedPrefab = SandbagsPrefab;
        PlaceCurrentlySelected();
    }

    public void PlaceDam() {
        currentlySelectedPrefab = DamPrefab;
        PlaceCurrentlySelected();
    }

    public void PlaceDitch() {
        currentlySelectedPrefab = DitchPrefab;
        PlaceCurrentlySelected();
    }

    public void PlaceDrain() {
        currentlySelectedPrefab = DrainPrefab;
        PlaceCurrentlySelected();
    }

    public void PlaceConcrete() {
        currentlySelectedPrefab = ConcretePrefab;
        PlaceCurrentlySelected();
    }

    public void DemolishBuilding() {
        //Toggle on and off
        bIsDemolishing = !bIsDemolishing;
        //If demolishing, change bg colour
        if (bIsDemolishing) {
            //Get rid of anything being placed
            TurnOffActiveModes();
            bIsDemolishing = true;
            GameController.Current.BackgroundImage.color = new Color(1, 0, 0, 0.5f);
        }
        //Else change it back to original
        else {
            TurnOffActiveModes();
        }
    }

    //Turn off any active modes, such as demolish or building
    public void TurnOffActiveModes() {
        bIsDemolishing = false;
        GameController.Current.BackgroundImage.color = backgroundColour;

        bIsPlacing = false;
        //Destroy whatever is selected, if something is selected
        if (currentlySelectedObject != null) {
            Destroy(currentlySelectedObject);
        }
    }

    void CreateBuilding(GameObject buildingPrefab) {
        //Make sure we're not demolishing
        TurnOffActiveModes();
        //If not currently placing a building
        if (!bIsPlacing) {
            //Instantiate off screen
            GameObject newBuilding = (GameObject)Instantiate(buildingPrefab, new Vector3(1000, 1000, 1000), Quaternion.identity);
            newBuilding.AddComponent<PlaceBuilding>();
            currentlySelectedObject = newBuilding;
        }
        else {
            Debug.Log("Already placing a building!");
        }
    }

    void RemoveBuilding(GameObject buildingObject) {
        //If not placing
        if (!bIsPlacing) {
            //Refund some of the value, rounded to nearest int
            GameController.Current.UpdateCredits(Mathf.RoundToInt(buildingObject.GetComponent<Building>().buildingCost * RefundPercentage));
            //Destory object
            buildingObject.GetComponent<Building>().Demolish();
        }
    }

    public void HasPlaced() {
        currentlySelectedObject = null;
    }
}
