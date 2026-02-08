using UnityEngine;

public class InterestZone : MonoBehaviour
{
    [Tooltip("Il nome che apparirà nel file CSV (es. 'Tavolo', 'Finestra')")]
    public string zoneName = "Zona Generica";

    // Variabili per contare le statistiche interne (opzionale, utile per debug)
    [HideInInspector] public int gazeCount = 0;
    [HideInInspector] public float totalGazeDuration = 0f;
}