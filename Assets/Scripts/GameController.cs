using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public static GameController Current;

    [Header("Debug")]
    public bool bIsPaused = false;

    [Header("Game Values")]
    public int DayLength = 10; //How long each day is in seconds
    public int StartingCredits = 1000;
    public int LoadingTime = 2;
    [Header("UI References")]
    public Button ResumeButton;
    public Button[] BuildingButtons;
    public Slider DaySlider;
    public Text CreditsText;
    public Text DayText;
    public Image BackgroundImage;
    public Text ObjectivesText;
    public GameObject LoadingPanel;
    public GameObject CursorText;
    public GameObject ContextPanel;
    [Header("Objective References")]
    public Material DemolishedBuildingMaterial;
    public GameObject[] BarriersToDestory;

    //Store a list of the objective buildings that need to be protected from water
    List<GameObject> ObjectivesList = new List<GameObject>();

    //Current balance
    int currentCredits;
    float timePassed = 0;
    int dayCounter = 0;
    //Number of objective in total
    int objectivesCount;
    //Number of objectives yet to be destroyed
    int objectivesStillActive;
    //How long to display cursor text for
    float cursorTextDisplayTimer;
    //How long to fade cursor text
    float cursorTextFadeSpeed = 1;
    //Are we displaying cursor text
    bool bIsDisplayingCursorText = false;
    //Keep track of time passed for cursor text
    float cursorTimePassed;
    //Cursor position when text was created
    Vector2 cursorTextPosition;

    public GameController() {
        Current = this;
    }

    void Awake() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
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
        //Hide cursor text
        CursorText.GetComponent<CanvasGroup>().alpha = 0;
        //Hide context panel
        HideContextPanel();
    }

    void Update() {
        //While day is = 0, and timepassed is less than 2s, game is loading
        if (dayCounter == 0 && timePassed < LoadingTime) {
            //Increase loading percent 
            LoadingPanel.transform.GetChild(1).GetComponent<Text>().text = Mathf.Round(timePassed / LoadingTime * 100).ToString() + "%";
        }
        //Else if panel is active, disable it, as no longer loading
        else if (LoadingPanel.activeSelf) {
            LoadingPanel.SetActive(false);
            //Start new day
            dayCounter++;
            timePassed = 0;
            DaySlider.value = 0;
            PauseGame();
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
        CreditsText.text = "§" + currentCredits;
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

        if (bIsDisplayingCursorText) {
            //Move towards upper right
            CursorText.transform.position = Vector2.MoveTowards(CursorText.transform.position, cursorTextPosition + new Vector2(200, 200), Time.deltaTime * 50);
            //Start fading after time passed is greater than timer
            if (cursorTimePassed > cursorTextDisplayTimer) {
                CursorText.GetComponent<CanvasGroup>().alpha -= Time.deltaTime * cursorTextFadeSpeed;
                //If text is invisible
                if (CursorText.GetComponent<CanvasGroup>().alpha <= 0) {
                    cursorTimePassed = 0;
                    bIsDisplayingCursorText = false;

                }
            }
            cursorTimePassed += Time.deltaTime;
        }
    }

    public void UpdateCredits(int amount) {
        currentCredits += amount;
        string creditChange;

        //Display change to player
        //If positive amount
        if (amount >= 0) {
            creditChange = "+ §";
        }
        //If negative amount
        else {
            creditChange = "- §";
        }

        creditChange += Mathf.Abs(amount);

        DisplayCursorText(creditChange, 0.5f);
    }

    public int GetCredits() {
        return currentCredits;
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

        //Update decay amounts
        foreach (GameObject building in BuildingController.Current.PlacedBuildings) {
            building.GetComponent<Building>().Decay();
        }

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

        //Check building is still alive
        foreach (GameObject obj in ObjectivesList) {
            if (!obj.GetComponent<Building>().bIsAlive) {
                //Add to has collided list
                hasCollidedList.Add(obj);
            }
        }

        foreach (GameObject obj in hasCollidedList) {
            //Remove from objectives list
            ObjectivesList.Remove(obj);
            //Decrement objectives still active
            objectivesStillActive--;
        }

        //Clear the list
        hasCollidedList.Clear();
    }

    public void DisplayCursorText(string text, float length) {
        CursorText.GetComponent<Text>().text = text;
        bIsDisplayingCursorText = true;
        cursorTextDisplayTimer = length;
        cursorTimePassed = 0;
        CursorText.GetComponent<CanvasGroup>().alpha = 1;
        //MOVE TO CURSOR POSITION
        cursorTextPosition = CursorText.transform.position = Input.mousePosition;
    }

    public void DisplayContextPanel(string title, string description, int cost) {
        //ContextPanel.transform.position = Input.mousePosition;
        //Get title
        ContextPanel.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = title;
        //Get cost
        ContextPanel.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>().text = "§" + cost;
        //Get description
        ContextPanel.transform.GetChild(1).GetComponent<Text>().text = description;
        //Show it
        ContextPanel.SetActive(true);
    }

    public void HideContextPanel() {
        ContextPanel.SetActive(false);
    }
}
