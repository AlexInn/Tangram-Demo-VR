using UnityEngine;
using UnityEngine.Rendering.Universal; // Necessario per vedere il DecalProjector

public class DecalChanger : MonoBehaviour
{
    [Header("Impostazioni")]
    [Tooltip("L'immagine finale che vuoi mostrare (trascinala qui)")]
    public Texture2D winningTexture;

    [Tooltip("Il nome della proprietà nello Shader. Di solito per URP è '_BaseMap'. Se non funziona, prova '_MainTex'.")]
    public string texturePropertyName = "_BaseMap"; 

    [Header("Riferimenti")]
    public DecalProjector decalProjector;

    // Funzione da collegare all'evento OnWin
    public void SwapDecal()
    {
        if (decalProjector == null)
        {
            decalProjector = GetComponent<DecalProjector>();
        }

        if (winningTexture == null)
        {
            Debug.LogError("ERRORE: Non hai assegnato la 'Winning Texture' nello script!");
            return;
        }

        // 1. Creiamo una copia istantanea del materiale per non modificare i file del progetto
        Material newMat = new Material(decalProjector.material);

        // 2. Impostiamo la nuova texture sulla proprietà specifica
        newMat.SetTexture(texturePropertyName, winningTexture);

        // 3. Riassegniamo il materiale modificato al proiettore
        decalProjector.material = newMat;

        Debug.Log("Decal aggiornato con successo!");
    }
}