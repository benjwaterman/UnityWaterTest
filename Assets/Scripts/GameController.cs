using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        //If p is pressed, pause game
        if (Input.GetKeyDown(KeyCode.P)) {
            WaterController.Current.bIsPaused = !WaterController.Current.bIsPaused;
        }

        //If press enter, recalculate 
        if (Input.GetKeyDown(KeyCode.Return)) {
            Debug.Log("Refreshing world height array");
            WaterController.Current.RefreshWorld();
        }
    }
}
