using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour
{
    [Header("Nomi delle Scene")]
    [Tooltip("Scrivi qui il nome ESATTO della scena dove si gioca (es. 'TangramGame')")]
    public string gameSceneName = "TangramGame";

    [Tooltip("Scrivi qui il nome ESATTO della scena di menu (es. 'StartMenu')")]
    public string menuSceneName = "StartMenu";

    // Chiama questa funzione dal pulsante "AVVIA SESSIONE"
    public void LoadGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Chiama questa funzione dal pulsante "TORNA AL MENU" (a fine partita)
    public void LoadMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}