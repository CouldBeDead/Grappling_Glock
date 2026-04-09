using UnityEngine;
using UnityEngine.SceneManagement; 

public class ButtonManager : MonoBehaviour
{
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
}
