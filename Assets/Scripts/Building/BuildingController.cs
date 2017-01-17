using UnityEngine;
using System.Collections;

public class BuildingController : MonoBehaviour {

    public static BuildingController Current;

    public Material PlaceMaterial;
    public GameObject SandbagsPrefab;

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
        GameObject newSandbags = (GameObject)Instantiate(SandbagsPrefab);
        newSandbags.AddComponent<PlaceBuilding>();
        //Instantiate object
        //Attach placing building script
        //Click to place, delete current building, instantiate new one at position
    }
}
