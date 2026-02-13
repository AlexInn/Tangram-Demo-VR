using UnityEngine;
using TMPro; // Necessario per usare TextMeshPro

public class SessionCodeDisplay : MonoBehaviour
{
    [Tooltip("Trascina qui l'oggetto TextMeshPro (UI o 3D) che deve mostrare il codice")]
    public TMP_Text codeTextLabel;

    [Tooltip("Messaggio opzionale prima del codice (es. 'Codice Sessione: ')")]
    public string prefixText = "ID SESSIONE: ";

    // Questa funzione va chiamata dall'evento OnWin
    public void ShowSessionCode()
    {
        // 1. Trova il logger
        TangramLogger logger = FindObjectOfType<TangramLogger>();

        if (logger != null && codeTextLabel != null)
        {
            // 2. Prende il codice pubblico generato all'avvio
            string code = logger.currentSessionID;

            // 3. Aggiorna il testo
            codeTextLabel.text = prefixText + code;

            // 4. Assicura che l'oggetto sia attivo (se era nascosto)
            codeTextLabel.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Manca il Logger o la Label di testo non è assegnata!");
        }
    }
}