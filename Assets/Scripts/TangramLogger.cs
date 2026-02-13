using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using System;
using System.Collections.Generic;

public class TangramLogger : MonoBehaviour
{
    private string filePath;

    // Codice univoco della sessione (pubblico per essere letto dalla UI)
    public string currentSessionID;

    private bool isLoggingActive = true;

    // <--- NUOVO: Flag per sapere se abbiamo già creato il file fisico
    private bool fileCreated = false;

    private Dictionary<XRGrabInteractable, float> grabStartTimes = new Dictionary<XRGrabInteractable, float>();

    void Start()
    {
        // 1. GENERAZIONE CODICE UNIVOCO (Lo facciamo subito, serve per la UI)
        currentSessionID = UnityEngine.Random.Range(1000, 10000).ToString();

        // 2. Definisce il percorso
        string fileName = $"Tangram_Session_{currentSessionID}.csv";
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        // <--- NOTA: Non creiamo più il file qui! Lo faremo al primo evento.
        Debug.Log($"[CSV] Logger pronto. Sessione: {currentSessionID}. File: {filePath}");

        // 3. AUTOMAZIONE INTERAZIONI
        var interactables = FindObjectsOfType<XRGrabInteractable>();
        foreach (var interactable in interactables)
        {
            interactable.selectEntered.AddListener((args) => OnGrabStart(interactable));
            interactable.selectExited.AddListener((args) => OnGrabEnd(interactable));
        }
    }

    // --- HELPER PRIVATO PER CREARE IL FILE ---
    void CreateFileHeader()
    {
        string header = "Date;Time;Event;ObjectName;Duration\n";
        try
        {
            File.WriteAllText(filePath, header);
            fileCreated = true; // Segniamo che il file esiste
            Debug.Log($"[CSV] File creato fisicamente ora: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore creazione file: {e.Message}");
        }
    }

    void OnGrabStart(XRGrabInteractable item)
    {
        if (!isLoggingActive) return;

        if (!grabStartTimes.ContainsKey(item))
        {
            grabStartTimes.Add(item, Time.time);
        }
        else
        {
            grabStartTimes[item] = Time.time;
        }
    }

    void OnGrabEnd(XRGrabInteractable item)
    {
        if (!isLoggingActive) return;

        if (grabStartTimes.ContainsKey(item))
        {
            float startTime = grabStartTimes[item];
            float duration = Time.time - startTime;
            grabStartTimes.Remove(item);

            LogData("GRAB", item.name, duration);
        }
    }

    public void LogData(string eventType, string objectName, float duration)
    {
        if (!isLoggingActive) return;

        // <--- MODIFICA: Se è la prima volta che scriviamo, crea il file
        if (!fileCreated)
        {
            CreateFileHeader();
        }

        string datePart = System.DateTime.Now.ToString("dd/MM/yyyy");
        string timePart = System.DateTime.Now.ToString("HH:mm:ss.fff");
        string durationStr = duration > 0 ? duration.ToString("F2") : "";

        string line = $"{datePart};{timePart};{eventType};{objectName};{durationStr}\n";

        try
        {
            File.AppendAllText(filePath, line);
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore scrittura CSV: {e.Message}");
        }
    }

    public void LogGaze(string zoneName, float duration)
    {
        LogData("GAZE", zoneName, duration);
    }

    public void LogVictory()
    {
        if (!isLoggingActive) return;

        LogData("FINE", "Tangram completato", 0f);

        isLoggingActive = false;

        Debug.Log($"Vittoria registrata! Codice Sessione: {currentSessionID}");
    }
}