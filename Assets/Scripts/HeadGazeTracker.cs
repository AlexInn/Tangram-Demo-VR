using UnityEngine;

public class HeadGazeTracker : MonoBehaviour
{
    [Header("Impostazioni Raycast")]
    public float maxDistance = 20f; // Quanto lontano vede il giocatore
    public LayerMask layerMask;     // Quali layer può colpire (es. 'Default' o crea un layer 'Gaze')

    [Header("Debug")]
    [SerializeField] private string currentLookingAt = "Niente";
    [SerializeField] private float currentGazeTimer = 0f;

    // Variabili interne per la logica
    private InterestZone currentZone = null;
    private float gazeStartTime;

    // Riferimento al Logger (lo script che scrive il CSV)
    private TangramLogger logger;

    void Start()
    {
        // Trova automaticamente il logger nella scena
        logger = FindObjectOfType<TangramLogger>();

        // Imposta la maschera su "Tutto" se ti sei dimenticato di settarla
        if (layerMask == 0) layerMask = LayerMask.GetMask("Default");
    }

    void Update()
    {
        // 1. Spara il raggio dal centro della telecamera in avanti
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Se colpiamo qualcosa
        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            // Cerchiamo se l'oggetto colpito è una "Zona di Interesse"
            InterestZone hitZone = hit.collider.GetComponent<InterestZone>();

            // CASO A: Stiamo guardando una NUOVA zona
            if (hitZone != null && hitZone != currentZone)
            {
                // Se stavamo guardando un'altra zona prima, chiudiamo quella sessione
                if (currentZone != null)
                {
                    StopGaze(currentZone);
                }

                // Iniziamo a tracciare la nuova zona
                StartGaze(hitZone);
            }
            // CASO B: Abbiamo colpito un oggetto che NON è una zona (es. il pavimento generico)
            else if (hitZone == null && currentZone != null)
            {
                StopGaze(currentZone);
            }
        }
        else
        {
            // CASO C: Non stiamo colpendo nulla (guardiamo il cielo vuoto)
            if (currentZone != null)
            {
                StopGaze(currentZone);
            }
        }

        // Aggiorna il timer per debug nell'Inspector
        if (currentZone != null)
        {
            currentGazeTimer = Time.time - gazeStartTime;
        }
    }

    // Funzione chiamata appena inizi a fissare qualcosa
    void StartGaze(InterestZone zone)
    {
        currentZone = zone;
        gazeStartTime = Time.time;
        currentLookingAt = zone.zoneName;
        currentGazeTimer = 0f;

        // Opzionale: Logga l'inizio (se vuoi un file molto dettagliato)
        // logger.LogInteraction("GAZE_START", zone.zoneName);

        // Incrementa il contatore interno della zona
        zone.gazeCount++;
    }

    // Funzione chiamata quando distogli lo sguardo
    void StopGaze(InterestZone zone)
    {
        float duration = Time.time - gazeStartTime;

        // Aggiorna il totale della zona
        zone.totalGazeDuration += duration;

        // --- SCRITTURA NEL CSV ---
        // Scriviamo: Evento="GAZE", Oggetto="NomeZona", Extra="Durata"
        // Nota: Dovrai aggiornare il tuo Logger per accettare la durata, vedi sotto.
        if (logger != null)
        {
            logger.LogGaze(zone.zoneName, duration);
        }

        // Reset variabili
        currentZone = null;
        currentLookingAt = "Niente";
        currentGazeTimer = 0f;
    }

    // Disegna il raggio nell'editor per vedere dove guardi (Gizmo rosso)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * maxDistance);
    }
}