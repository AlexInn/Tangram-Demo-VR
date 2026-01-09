using UnityEngine;
using System.IO;  // Serve per scrivere i file
using System;     // Serve per la data e l'ora

public class GrabTimer : MonoBehaviour
{
    private float _tempoInizio;
    private bool _staiTenendo = false;
    private string _percorsoFile;

    void Start()
    {
        string nomeFile = "ReportTangram.csv";
        string cartellaSalvataggio;

        // Questo blocco "IF" controlla se siamo nell'Editor di Unity o nel gioco finale
#if UNITY_EDITOR
        // SE siamo nell'Editor: Usa il Desktop del tuo PC
        cartellaSalvataggio = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#else
        // SE siamo nel Visore (Build): Usa la cartella sicura dell'applicazione
        cartellaSalvataggio = Application.persistentDataPath;
#endif

        // Unisce la cartella al nome del file
        _percorsoFile = Path.Combine(cartellaSalvataggio, nomeFile);

        // --- Da qui in poi è uguale a prima ---
        if (!File.Exists(_percorsoFile))
        {
            string intestazione = "Nome Pezzo;Durata (secondi);Orario\n";
            File.WriteAllText(_percorsoFile, intestazione);
        }

        Debug.Log($"[CSV] File salvato comodamente qui: {_percorsoFile}");
    }

    public void IniziaTimer()
    {
        _tempoInizio = Time.time;
        _staiTenendo = true;
    }

    public void FermaTimer()
    {
        if (_staiTenendo)
        {
            float durata = Time.time - _tempoInizio;
            _staiTenendo = false;

            // Chiamiamo la funzione che scrive su disco
            SalvaSuCSV(gameObject.name, durata);
        }
    }

    void SalvaSuCSV(string nome, float tempo)
    {
        // 1. Recuperiamo l'ora attuale
        string oraAttuale = DateTime.Now.ToString("HH:mm:ss");

        // 2. Creiamo la riga di testo
        // F2 significa "2 cifre decimali"
        string riga = $"{nome};{tempo.ToString("F2")};{oraAttuale}\n";

        // 3. Aggiungiamo la riga al file (Append) senza cancellare il vecchio contenuto
        try
        {
            File.AppendAllText(_percorsoFile, riga);
            Debug.Log($"[SALVATO] {riga}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore nel salvare il file: {e.Message}");
        }
    }
}