using UnityEngine;
using System.Collections;

public class BuildingController : MonoBehaviour {

    public static BuildingController Current;

    public Material PlaceMaterial;
    public GameObject SandbagsPrefab;
    public bool bIsPlacing = false;
    public float RefundPercentage = 0.8f;

    bool bIsDemolishing = false;
    Color backgroundColour;

    public BuildingController() {
        Current = this;
    }

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
                    //If gameobject has building component
                    if (hit.transform.gameObject.GetComponent<Building>()) {
                        //Remove building
                        RemoveBuilding(hit.transform.gameObject);
                    }

                }
            }

            //If right mouse click or escape
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
                bIsDemolishing = false;
            }
        }
    }

    public void PlaceSandbags() {
        CreateBuilding(SandbagsPrefab);
    }

    public void DemolishBuilding() {
        //Toggle on and off
        bIsDemolishing = !bIsDemolishing;
        //If demolishing, change bg colour
        if (bIsDemolishing) {
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
    }

    void CreateBuilding(GameObject buildingPrefab) {
        //Make sure we're not demolishing
        TurnOffActiveModes();
        //If not currently placing a building
        if (!bIsPlacing) {
            //Instantiate off screen
            GameObject newBuilding = (GameObject)Instantiate(buildingPrefab, new Vector3(1000, 1000, 1000), Quaternion.identity);
            newBuilding.AddComponent<PlaceBuilding>();
        }
        else {
            Debug.Log("Already placing a building!");
        }
    }

    void RemoveBuilding(GameObject buildingObject) {
        //If not placing
        if (!bIsPlacing) {
            //Refund some of the value, rounded to nearest int
            GameController.Current.currentCredits += Mathf.RoundToInt(buildingObject.GetComponent<Building>().buildingCost * RefundPercentage);
            //Destory object
            buildingObject.GetComponent<Building>().Demolish();
        }
    }
}
