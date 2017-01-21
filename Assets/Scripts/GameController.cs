using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public static GameController Current;

    [Header("Debug")]
    public bool bIsPaused = false;
    public int currentCredits;
    //Store a list of the objective buildings that need to be protected from water
    public List<GameObject> ObjectivesList = new List<GameObject>();
    [Header("Game Values")]
    public int DayLength = 10; //How long each day is in seconds
    public int StartingCredits = 1000;
    [Header("UI References")]
    public Button ResumeButton;
    public Button[] BuildingButtons;
    public Slider DaySlider;
    public Text CreditsText;
    public Text DayText;
    public Image BackgroundImage;
    [Header("Objective References")]
    public Material DemolishedBuildingMaterial;

    float timePassed = 0;
    int dayCounter = 1;

    public GameController() {
        Current = this;
    }

    void Start() {
        //Make buttons not interactable
        SetButtonInteractable(false);
        //Set current credits to starting credits
        currentCredits = StartingCredits;
    }

    void Update() {
        //If game is not paused
        if (!bIsPaused) {
            //Update slider to visualise time passing
            DaySlider.value = timePassed / DayLength;

            if (timePassed > DayLength) {
                //End of day, pause game
                PauseGame();
                //Reset counter
                timePassed = 0;
                //Increase day counter
                dayCounter++;
            }
            timePassed += Time.deltaTime;
        }

        ////////////////Should put in a function
        CreditsText.text = currentCredits + " Cr";
        DayText.text = "Day " + dayCounter;

        //Consider moving to input manager
        //If p is pressed, pause game
        if (Input.GetKeyDown(KeyCode.P)) {
            PauseGame();
        }

        //If press enter, recalculate 
        if (Input.GetKeyDown(KeyCode.Return)) {
            Debug.Log("Refreshing world height array");
            WaterController.Current.RefreshWorld();
        }
    }

    public void PauseGame() {
        bIsPaused = true;
        WaterController.Current.Pause();

        SetButtonInteractable(true);
        UpdateObjectives();
    }

    public void ResumeGame() {
        bIsPaused = false;
        WaterController.Current.Resume();
        WaterController.Current.RefreshWorld();
        BuildingController.Current.TurnOffActiveModes();

        SetButtonInteractable(false);
    }

    void SetButtonInteractable(bool canInteract) {
        ResumeButton.interactable = canInteract;
        foreach (Button button in BuildingButtons) {
            button.interactable = canInteract;
        }
    }

    void UpdateObjectives() {
        List<GameObject> hasCollidedList = new List<GameObject>();

        //Check for collisions
        foreach (GameObject obj in ObjectivesList) {
            if (obj.GetComponent<OnWaterTouch>().CheckForWaterCollision()) {
                //Add to has collided list
                hasCollidedList.Add(obj);
            }
        }

        foreach (GameObject obj in hasCollidedList) {
            //Remove from objectives list
            ObjectivesList.Remove(obj);
            //Call demolish function
            obj.GetComponent<OnWaterTouch>().DemolishSelf();
        }

        //Clear the list
        hasCollidedList.Clear();
    }
}
