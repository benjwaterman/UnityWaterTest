using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static GameController Current;

    public GameController() {
        Current = this;
    }

    void Awake() {
        Current = this;
        
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = -1;
    }

    [Header("Debug")]
    public bool bIsPaused = false;

    [Header("Game Values")]
    public int NumberOfDays = 7;
    public int DayLength = 10; //How long each day is in seconds
    public int StartingCredits = 1000;
    public int CreditsPerDay = 200; //How many credits the player gets each day
    public int LoadingTime = 2;
    public int BuildingsToProtect = 5;
    [Header("UI References")]
    public Canvas InGameUICanvas;
    public Button ResumeButton;
    public Button[] BuildingButtons;
    public Slider DaySlider;
    public Text CreditsText;
    public Text DayText;
    public Image BackgroundImage;
    public Text ObjectivesText;
    public GameObject LoadingPanel;
    public GameObject IntroPanel;
    public GameObject CursorText;
    public GameObject ContextPanel;
    public GameObject SliderMarker;
    public Text TaskText;
    public GameObject GameOverPanel;
    public GameObject GameWinPanel;
    [Header("Objective References")]
    public Material DemolishedBuildingMaterial;
    public Material RedTransparentMaterial;
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
        //Set day slider max value to number of days
        DaySlider.maxValue = NumberOfDays;
        //Disable intro panel
        IntroPanel.SetActive(false);

        //Create the markers on the slider signifying each day
        float width = DaySlider.GetComponent<RectTransform>().rect.width;
        float widthIncrement = width / NumberOfDays;
        Transform mainPanel = GameObject.Find("MainPanel").transform;
        for (int i = 0; i <= NumberOfDays; i++) {
            GameObject newUI = (GameObject)Instantiate(SliderMarker, mainPanel);
            newUI.transform.position = DaySlider.transform.position - new Vector3(width / 2, 0) + new Vector3(widthIncrement * i, 10);
        }

        //Hide in game UI
        InGameUICanvas.enabled = false;

        //Set task text
        TaskText.text = "Protect at least <b>" + BuildingsToProtect + "</b> buildings";

        //Update UI
        UpdateCredits(0);
    }

    void Update() {
        //While day is = 0, and timepassed is less than 2s, game is loading
        if (dayCounter == 0 && timePassed < LoadingTime) {
            //Increase loading percent 
            LoadingPanel.transform.GetChild(1).GetComponent<Text>().text = Mathf.Round(timePassed / LoadingTime * 100).ToString() + "%";
        }
        //Else if panel is active, disable it, as no longer loading
        else if (LoadingPanel.activeSelf) {
            StartFirstDay();
        }

        //Has made it to the end of the required day
        if(dayCounter == NumberOfDays + 1) {
            //Level has been beat
            GameWin();
            dayCounter = -1;
        }

        //If game is not paused
        if (!bIsPaused) {
            //Update slider to visualise time passing
            DaySlider.value = (dayCounter - 1) + timePassed / DayLength;

            if (timePassed > DayLength) {
                EndDay();
            }

            timePassed += Time.deltaTime;
        }

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

        //Return to main menu
        if(Input.GetKeyDown(KeyCode.Backspace)) {
            MenuController.Current.LoadLevel(0);
        }

        //If mouse is over a UI element
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

    public void CloseIntroPanel() {
        //Disable intro panel
        IntroPanel.SetActive(false);
        //Enable in game UI
        InGameUICanvas.enabled = true;
    }

    void StartFirstDay() {
        //Disable loading panel
        LoadingPanel.SetActive(false);
        //Enable intro panel
        IntroPanel.SetActive(true);
        //Update objectives UI
        UpdateObjectives();

        //Reset day
        dayCounter++;
        timePassed = 0;
        DaySlider.value = 0;
        PauseGame();
    }

    //What happens when day ends
    void EndDay() {
        //End of day, pause game
        PauseGame();
        //Reset counter
        timePassed = 0;
        //Increase day counter
        dayCounter++;
        //Reset slider
        //DaySlider.value = 0;

        UpdateCredits(CreditsPerDay);
    }

    //Update player credits
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

        //Update UI
        CreditsText.text = "§" + currentCredits;
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

        //Update UI
        DayText.text = "Day " + dayCounter;
    }

    void SetButtonInteractable(bool canInteract) {
        ResumeButton.interactable = canInteract;
        foreach (Button button in BuildingButtons) {
            button.interactable = canInteract;
        }
    }

    public void UpdateObjectives() {
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

        //Update UI
        ObjectivesText.text = objectivesStillActive + "/" + objectivesCount + " buildings standing";

        //Check if game has ended
        if(objectivesStillActive < BuildingsToProtect) {
            GameOver();
        }
    }

    //When game is lost
    void GameOver() {
        //Pause game
        PauseGame();
        //Remove ability to play game
        ResumeButton.interactable = false;
        //Enable panel
        GameOverPanel.SetActive(true);
        //Disable game ui
        InGameUICanvas.enabled = false;
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //When level is won
    void GameWin() {
        //Pause game
        PauseGame();
        //Remove ability to play game
        ResumeButton.interactable = false;
        //Enable game win screen
        GameWinPanel.SetActive(true);
        //Disable main UI
        InGameUICanvas.enabled = false;
    }

    public void NextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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

    public int GetDay() {
        return dayCounter;
    }
}
