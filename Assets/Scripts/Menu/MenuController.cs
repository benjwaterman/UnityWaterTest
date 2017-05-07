using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    public static MenuController Current;
    public GameObject HelpPanel;

    public MenuController() {
        Current = this;
    }

    public void LoadLevel(int levelIndex) {
        SceneManager.LoadScene(levelIndex);
    }

    public void QuitGame() {
        Application.Quit();
    }

    void Update() {
        //If h is pressed, display or hide help
        if(Input.GetKeyDown(KeyCode.H)) {
            HelpPanel.SetActive(!HelpPanel.activeSelf);
        }
    }
}
