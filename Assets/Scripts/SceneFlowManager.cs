using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour
{
    [Header("Configurazione")]
    [Tooltip("Nome esatto della scena del Menu Principale")]
    public string menuSceneName = "StartMenu";

    // Funzione per caricare un livello specifico (da collegare ai bottoni del Menu)
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    // Funzione per tornare al menu (da collegare al bottone di fine partita)
    public void LoadMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    // Funzione per uscire dal gioco
    public void QuitGame()
    {
        Debug.Log("Uscita dal gioco...");
        Application.Quit();
    }
}