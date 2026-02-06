using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TangramPatternMatcher : MonoBehaviour
{
    // Struttura semplice: salva Chi è, Dove va e Come è girato
    [System.Serializable]
    public struct PieceRelation
    {
        public Transform pieceObject;
        public Vector3 targetLocalPosition;
        public Quaternion targetLocalRotation;
    }

    [Header("Configurazione")]
    [Tooltip("Il pezzo 'Capo'. Tutti gli altri verranno calcolati in base alla posizione di questo.")]
    public Transform anchorPiece;

    [Tooltip("Lista degli altri pezzi del Tangram")]
    public List<Transform> otherPieces;

    [Header("Tolleranza")]
    [Tooltip("Quanto può sbagliare l'utente in metri (es. 0.05 = 5cm)")]
    public float positionThreshold = 0.05f;
    [Tooltip("Quanto può sbagliare la rotazione in gradi")]
    public float rotationThreshold = 20.0f;

    [Header("Dati Salvati (Non toccare a mano)")]
    public List<PieceRelation> solvedPattern;

    [Header("Eventi")]
    public UnityEvent OnWin;

    private bool hasWon = false;

    void Update()
    {
        if (hasWon) return; // Se ha già vinto, fermiamo i controlli

        if (CheckPattern())
        {
            hasWon = true;
            Debug.Log("VITTORIA! Forma riconosciuta.");
            OnWin.Invoke(); // Fa partire Suono e Logger
        }
    }

    // Algoritmo di verifica Semplice (Strict)
    private bool CheckPattern()
    {
        if (anchorPiece == null) return false;

        // Controlliamo ogni pezzo salvato nella soluzione
        foreach (var relation in solvedPattern)
        {
            Transform currentPiece = relation.pieceObject;

            if (currentPiece == null) continue;

            // 1. Calcoliamo dove si trova ORA questo pezzo RISPETTO all'Anchor
            Vector3 currentRelPos = anchorPiece.InverseTransformPoint(currentPiece.position);
            Quaternion currentRelRot = Quaternion.Inverse(anchorPiece.rotation) * currentPiece.rotation;

            // 2. Controlliamo la Distanza
            float dist = Vector3.Distance(currentRelPos, relation.targetLocalPosition);
            if (dist > positionThreshold) return false; // Troppo lontano

            // 3. Controlliamo la Rotazione
            float angle = Quaternion.Angle(currentRelRot, relation.targetLocalRotation);
            if (angle > rotationThreshold) return false; // Ruotato male
        }

        // Se siamo arrivati qui, tutti i pezzi sono al posto giusto!
        return true;
    }

    // --- FUNZIONI PER L'EDITOR ---

    [ContextMenu("SALVA SOLUZIONE")]
    public void BakeSolution()
    {
        solvedPattern.Clear();

        if (anchorPiece == null)
        {
            Debug.LogError("Devi assegnare un Anchor Piece prima di salvare!");
            return;
        }

        foreach (var piece in otherPieces)
        {
            PieceRelation data = new PieceRelation();
            data.pieceObject = piece;

            // Matematica relativa
            data.targetLocalPosition = anchorPiece.InverseTransformPoint(piece.position);
            data.targetLocalRotation = Quaternion.Inverse(anchorPiece.rotation) * piece.rotation;

            solvedPattern.Add(data);
        }

        Debug.Log($"Soluzione salvata con {solvedPattern.Count} pezzi relativi all'Anchor.");
    }
}