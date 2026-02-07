using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using System;
using System.Collections.Generic;

public class TangramLogger : MonoBehaviour
{
    private string filePath;

    // <--- AGGIUNTO: Interruttore per fermare il log dopo la vittoria
    private bool isLoggingActive = true;

    private Dictionary<XRGrabInteractable, float> grabStartTimes = new Dictionary<XRGrabInteractable, float>();

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
            File.AppendAllText(filePath, "Date;Time;Event;ObjectName;Duration\n");
        }
        else
        {
            // Se il file esiste già, inserisce una riga vuota per staccare visivamente la vecchia sessione
            File.AppendAllText(filePath, "\n");
            File.AppendAllText(filePath, $"\n;--- SESSIONE DEL {System.DateTime.Now} ---;\n");
            // Ripetiamo l'header per chiarezza nella nuova sessione
            File.AppendAllText(filePath, "Date;Time;Event;ObjectName;Duration\n");
        }

        Debug.Log($"[CSV] Logger avviato. File: {filePath}");

        // 3. AUTOMAZIONE: Trova tutti i pezzi afferrabili nella scena
        // Non devi più trascinarli a mano!
        var interactables = FindObjectsOfType<XRGrabInteractable>();

        foreach (var interactable in interactables)
        {
            // Esclude oggetti che non sono pezzi del tangram (opzionale, basato sui layer o tag se serve)
            // Per ora prende tutto ciò che puoi afferrare.

            // Quando afferri -> Fa partire il timer interno
            interactable.selectEntered.AddListener((args) => OnGrabStart(interactable));

            // Quando rilasci -> Calcola durata e scrive nel CSV
            interactable.selectExited.AddListener((args) => OnGrabEnd(interactable));
        }
    }

    // <--- NUOVI METODI PER GESTIRE LA DURATA DEL GRAB ---

    void OnGrabStart(XRGrabInteractable item)
    {
        // Se il gioco è finito, ignoriamo nuovi grab
        if (!isLoggingActive) return;

        if (!grabStartTimes.ContainsKey(item))
        {
            grabStartTimes.Add(item, Time.time);
        }
        else
        {
            // Se per caso era rimasto appeso, resettiamo il tempo
            grabStartTimes[item] = Time.time;
        }
    }

    void OnGrabEnd(XRGrabInteractable item)
    {
        // Se il gioco è finito, ignoriamo il rilascio (logica di scrittura)
        if (!isLoggingActive) return;

        if (grabStartTimes.ContainsKey(item))
        {
            float startTime = grabStartTimes[item];
            float duration = Time.time - startTime;

            // Rimuoviamo dal dizionario
            grabStartTimes.Remove(item);

            // Scriviamo il log usando la funzione unificata.
            LogData("GRAB", item.name, duration);
        }
    }

    // <--- FUNZIONE UNIFICATA (Sostituisce le vecchie LogInteraction e LogGaze) ---
    // Questa funzione viene chiamata sia dallo script del Gaze sia dal Grab qui sopra
    public void LogData(string eventType, string objectName, float duration)
    {
        // <--- BLOCCO DI SICUREZZA: Se il gioco è finito, non scrive nulla (neanche Gaze)
        if (!isLoggingActive) return;

        // 1. Data formato italiano (giorno/mese/anno)
        string datePart = System.DateTime.Now.ToString("dd/MM/yyyy");

        // 2. Ora con secondi (24h)
        string timePart = System.DateTime.Now.ToString("HH:mm:ss");

        // 3. Durata formattata (se duration è 0, scriviamo "0.00" o puoi lasciare vuoto se preferisci)
        string durationStr = duration > 0 ? duration.ToString("F2") : "";

        // 4. Componiamo la riga con 5 colonne: Data ; Ora ; Evento ; Oggetto ; Durata
        string line = $"{datePart};{timePart};{eventType};{objectName};{durationStr}\n";

        // Scriviamo sul file
        try
        {
            File.AppendAllText(filePath, line);
            // Debug.Log($"[LOG] {line.Trim()}"); // Decommenta se vuoi vedere il log in console
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore scrittura CSV: {e.Message}");
        }
    }

    // Mantengo questo metodo per compatibilità con il GazeTracker se lo chiama direttamente
    public void LogGaze(string zoneName, float duration)
    {
        LogData("GAZE", zoneName, duration);
    }

    // --- FUNZIONE PER LA VITTORIA ---
    // Da collegare all'evento OnWin del TangramPatternMatcher
    public void LogVictory()
    {
        // Evita doppi log se chiamato più volte per errore
        if (!isLoggingActive) return;

        // Passiamo 0 come durata
        LogData("FINE", "Tangram completato", 0f);

        // <--- STOP: Da ora in poi, niente viene più scritto nel file
        isLoggingActive = false;

        Debug.Log("Vittoria registrata nel CSV! Logging disattivato.");
    }
}