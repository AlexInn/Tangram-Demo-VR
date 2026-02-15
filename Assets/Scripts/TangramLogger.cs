using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using System;
using System.Collections.Generic;

public class TangramLogger : MonoBehaviour
{
    private string filePath;
    public string currentSessionID;

    private bool isLoggingActive = true;
    private bool fileCreated = false;

    private Dictionary<XRGrabInteractable, float> grabStartTimes = new Dictionary<XRGrabInteractable, float>();

    // SPOSTATO IN AWAKE: Viene eseguito PRIMA di qualsiasi Start() degli altri script
    void Awake()
    {
        // 1. GENERAZIONE CODICE
        currentSessionID = UnityEngine.Random.Range(1000, 10000).ToString();
        string fileName = $"Tangram_Session_{currentSessionID}.csv";

        // 2. DEFINIZIONE PERCORSO
        string folderPath = "";

#if UNITY_EDITOR
        folderPath = Path.Combine(Application.dataPath, "Logs"); 
#elif UNITY_ANDROID
        folderPath = "/storage/emulated/0/Documents/TangramVR_Logs";
#else
        folderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "TangramVR_Logs");
#endif

        // 3. CREAZIONE CARTELLA
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore cartella: {e.Message}");
            folderPath = Application.persistentDataPath;
        }

        filePath = Path.Combine(folderPath, fileName);

        Debug.Log($"[CSV] Logger inizializzato in Awake. ID: {currentSessionID}");
    }

    void Start()
    {
        // 4. AUTOMAZIONE INTERAZIONI (Questo può restare in Start)
        var interactables = FindObjectsOfType<XRGrabInteractable>();
        foreach (var interactable in interactables)
        {
            interactable.selectEntered.AddListener((args) => OnGrabStart(interactable));
            interactable.selectExited.AddListener((args) => OnGrabEnd(interactable));
        }
    }

    // --- DA QUI IN GIU' E' TUTTO UGUALE A PRIMA ---

    void CreateFileHeader()
    {
        string header = "Date;Time;Event;ObjectName;Duration\n";
        try
        {
            File.WriteAllText(filePath, header);
            fileCreated = true;
            Debug.Log($"[CSV] File creato fisicamente: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore creazione file CSV: {e.Message}");
        }
    }

    void OnGrabStart(XRGrabInteractable item)
    {
        if (!isLoggingActive) return;

        if (!grabStartTimes.ContainsKey(item))
            grabStartTimes.Add(item, Time.time);
        else
            grabStartTimes[item] = Time.time;
    }

    void OnGrabEnd(XRGrabInteractable item)
    {
        if (!isLoggingActive) return;

        if (grabStartTimes.ContainsKey(item))
        {
            float duration = Time.time - grabStartTimes[item];
            grabStartTimes.Remove(item);
            LogData("GRAB", item.name, duration);
        }
    }

    public void LogData(string eventType, string objectName, float duration)
    {
        if (!isLoggingActive) return;

        // Se è la prima volta che scriviamo qualcosa (anche INFO), crea il file
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
            // Protezione extra nel caso il path fosse ancora null (non dovrebbe più succedere)
            if (string.IsNullOrEmpty(filePath))
                Debug.LogError("FilePath è null! Awake non ha funzionato correttamente.");
            else
                Debug.LogError($"Errore scrittura: {e.Message}");
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
        Debug.Log($"Vittoria! Sessione conclusa: {currentSessionID}");
    }
}