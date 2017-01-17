using UnityEngine;
using System.Collections;

public class BuildingController : MonoBehaviour {

    public static BuildingController Current;

    public Material PlaceMaterial;
    public GameObject SandbagsPrefab;
    public bool bIsPlacing = false;

    public BuildingController() {
        Current = this;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void PlaceSandbags () {
        //If not currently placing a building
        if (!bIsPlacing) {
            GameObject newSandbags = (GameObject)Instantiate(SandbagsPrefab);
            newSandbags.AddComponent<PlaceBuilding>();
            //Instantiate object
            //Attach placing building script
            //Click to place, delete current building, instantiate new one at position
        }
        else {
            Debug.Log("Already placing a building!");
        }
    }
}
