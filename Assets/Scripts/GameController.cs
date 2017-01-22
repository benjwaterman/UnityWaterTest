using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public static GameController Current;

    [Header("Debug")]
    public bool bIsPaused = false;
    public int currentCredits;
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
    public Text ObjectivesText;
    public GameObject LoadingPanel;
    [Header("Objective References")]
    public Material DemolishedBuildingMaterial;
    public GameObject[] BarriersToDestory;

    //Store a list of the objective buildings that need to be protected from water
    List<GameObject> ObjectivesList = new List<GameObject>();

    float timePassed = 0;
    int dayCounter = 0;
    //Number of objective in total
    int objectivesCount;
    //Number of objectives yet to be destroyed
    int objectivesStillActive;

    public GameController() {
        Current = this;
    }

    void Start() {
        //Make buttons not interactable
        SetButtonInteractable(false);
        //Set current credits to starting credits
        currentCredits = StartingCredits;
        //Make sure objectives list is empty
        ObjectivesList.Clear();
        //Enable panel
        LoadingPanel.SetActive(true);
    }

    void Update() {
        //While day is = 0, game is loading
        if(dayCounter == 0) {
            //Increase loading percent 
            LoadingPanel.transform.GetChild(1).GetComponent<Text>().text = Mathf.Round(timePassed / DayLength * 100).ToString() + "%";
        }
        //Else if panel is active, disable it
        else if(LoadingPanel.activeSelf) {
            LoadingPanel.SetActive(false);
        }

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
                //Reset slider
                DaySlider.value = 0;
            }
            timePassed += Time.deltaTime;
        }

        ////////////////Should put in a function
        CreditsText.text = currentCredits + " Cr";
        DayText.text = "Day " + dayCounter;
        ObjectivesText.text = objectivesStillActive + "/" + objectivesCount + " buildings standing";

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

    public void AddObjective(GameObject objective) {
        ObjectivesList.Add(objective);
        objectivesCount++;
        objectivesStillActive++;
    }

    public void PauseGame() {
        bIsPaused = true;
        WaterController.Current.Pause();

        SetButtonInteractable(true);
        UpdateObjectives();
    }

    public void ResumeGame() {
        //If first day, destory the barriers
        if (dayCounter == 1) {
            foreach (GameObject barrier in BarriersToDestory) {
                barrier.SetActive(false);
            }
        }

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
            //Decrement objectives still active
            objectivesStillActive--;
        }

        //Clear the list
        hasCollidedList.Clear();
    }
}
