using UnityEngine;
using UnityEngine.SceneManagement; 

public class ButtonManager : MonoBehaviour
{
    //start buttons logic -- starts game 
    public void StartGame()
    {
        Debug.Log("Start Game - Level1");
        SceneManager.LoadScene("Level_1");
    }
    public void RestartGame()
    {
        Debug.Log("Restart Game - go to start screne");
        SceneManager.LoadScene("StartScrene");
    }

    //quit button logic -- exits the game 
    public void QuitGame()
    {
        Debug.Log("Quit Game - go back to menu");
        Application.Quit();
    }

}
