using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using System;

public class TangramLogger : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        // 1. Definisce il percorso del file
        // Su PC: C:/Users/Tu/AppData/LocalLow/TuoNome/TuoGioco/TangramLog.csv
        // Su Quest: Android/data/com.TuoNome.TuoGioco/files/TangramLog.csv
        filePath = Path.Combine(Application.persistentDataPath, "TangramLog.csv");

        // 2. Scrive l'intestazione solo se il file è nuovo
        if (!File.Exists(filePath))
        {
            // Usa il punto e virgola (;) per Excel italiano
            File.AppendAllText(filePath, "Date;Time;Event;ObjectName\n");
        }
        else
        {
            // Se il file esiste già, inserisce una riga vuota per staccare visivamente la vecchia sessione
            File.AppendAllText(filePath, "\n");
            File.AppendAllText(filePath, $"\n;--- SESSIONE DEL {System.DateTime.Now} ---;\n");
            File.AppendAllText(filePath, "Date;Time;Event;ObjectName\n");
        }

        Debug.Log($"[CSV] Logger avviato. File: {filePath}");

        // 3. AUTOMAZIONE: Trova tutti i pezzi afferrabili nella scena
        // Non devi più trascinarli a mano!
        var interactables = FindObjectsOfType<XRGrabInteractable>();

        foreach (var interactable in interactables)
        {
            // Esclude oggetti che non sono pezzi del tangram (opzionale, basato sui layer o tag se serve)
            // Per ora prende tutto ciò che puoi afferrare.

            // Si iscrive agli eventi di Presa (SelectEntered) e Rilascio (SelectExited)
            interactable.selectEntered.AddListener((args) => LogInteraction("GRAB", args.interactableObject.transform.name));
            interactable.selectExited.AddListener((args) => LogInteraction("RELEASE", args.interactableObject.transform.name));
        }
    }

    // Funzione principale di scrittura
    void LogInteraction(string eventType, string objectName)
    {
        // 1. Data formato italiano (giorno/mese/anno)
        string datePart = System.DateTime.Now.ToString("dd/MM/yyyy");

        // 2. Ora con secondi (24h)
        string timePart = System.DateTime.Now.ToString("HH:mm:ss");

        // 3. Componiamo la riga con 4 colonne: Data ; Ora ; Evento ; Oggetto
        string line = $"{datePart};{timePart};{eventType};{objectName}\n";

        // Scriviamo sul file
        try
        {
            File.AppendAllText(filePath, line);
            Debug.Log($"[LOG] {line.Trim()}"); // Log in console
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore scrittura CSV: {e.Message}");
        }
    }

    // --- FUNZIONE PER LA VITTORIA ---
    // Da collegare all'evento OnWin del TangramPatternMatcher
    public void LogVictory()
    {
        LogInteraction("FINE", "Tangram completato");
        Debug.Log("Vittoria registrata nel CSV!");
    }
}