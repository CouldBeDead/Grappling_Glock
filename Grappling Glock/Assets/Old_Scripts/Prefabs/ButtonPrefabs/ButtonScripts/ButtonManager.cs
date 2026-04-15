using UnityEngine;
using UnityEngine.SceneManagement; 

public class ButtonManager : MonoBehaviour
{
    //get the menu canvas data 
    public GameObject menuDropDown; 
    public GameObject menuButton;

   //start buttons logic -- starts game 
   public void StartGame()
    {   
        Debug.Log("Start Game - Level1");
        SceneManager.LoadScene("GameScene"); 
    }

    //quit button logic -- exits the game 
    public void QuitGame()
    {
        Debug.Log("Quit Game - go back to menu");
        Application.Quit();
    }

    //intro button logic -- opens intro scene 
    public void LoadIntro()
    {
        Debug.Log("Load Intr0 - go to intro scene");
        SceneManager.LoadScene("IntroScene");
    }


    //menu Button logic -- opens the menu dropdown Canvas
    public void OpenMenuCanvasDropDown()
    {
        Debug.Log("Open Menu Dropdown");
        menuDropDown.SetActive(true);
        menuButton.SetActive(false);
    }

    //Closes the menu dropdown Canvas
    public void CloseMenuCanvasDropDown()
    {
        Debug.Log("Close Menu Dropdown");
        menuDropDown.SetActive(false);
        menuButton.SetActive(true);
    }

    //Level button logic -- opens level scene 
    public void LoadLevels()
    {
        Debug.Log("Load Levels - go to level scene");
        SceneManager.LoadScene("LevelScene");
    }
}
