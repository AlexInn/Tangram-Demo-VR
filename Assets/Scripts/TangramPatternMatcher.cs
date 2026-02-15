using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using System.Collections.Generic;

public class TangramPatternMatcher : MonoBehaviour
{
    [System.Serializable]
    public struct PieceGoal
    {
        public string pieceTag;
        public Vector3 relPosition;
        public Quaternion relRotation;
    }

    [Header("Configurazione")]
    public XRGrabInteractable anchorPiece;
    public List<XRGrabInteractable> otherPieces;
    public float positionTolerance = 0.05f;
    public float rotationTolerance = 15f;

    [Header("Soluzione Salvata (Smart)")]
    [SerializeField] private List<PieceGoal> savedSolution = new List<PieceGoal>();

    [Header("Eventi")]
    public UnityEvent OnWin;
    private bool hasWon = false;
    public string levelName = "Livello Tangram Cigno";

    void Start()
    {
        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null) logger.LogData("INFO", "Start_Level: " + levelName, 0f);
    }

    void Update()
    {
        if (hasWon) return;

        if (CheckPattern())
        {
            hasWon = true;
            Debug.Log("VITTORIA! Pattern Corretto.");
            OnWin.Invoke();
            TangramLogger logger = FindObjectOfType<TangramLogger>();
            if (logger != null) logger.LogVictory();
        }
    }

    bool CheckPattern()
    {
        if (anchorPiece.isSelected) return false;

        List<int> availableGoalIndices = new List<int>();
        for (int i = 0; i < savedSolution.Count; i++) availableGoalIndices.Add(i);

        foreach (var piece in otherPieces)
        {
            if (piece.isSelected) return false;

            bool pieceMatched = false;
            Vector3 currentRelPos = anchorPiece.transform.InverseTransformPoint(piece.transform.position);
            Quaternion currentRelRot = Quaternion.Inverse(anchorPiece.transform.rotation) * piece.transform.rotation;

            for (int j = 0; j < availableGoalIndices.Count; j++)
            {
                int goalIdx = availableGoalIndices[j];
                PieceGoal goal = savedSolution[goalIdx];

                if (piece.CompareTag(goal.pieceTag))
                {
                    float dist = Vector3.Distance(currentRelPos, goal.relPosition);
                    float angle = Quaternion.Angle(currentRelRot, goal.relRotation);

                    if (dist <= positionTolerance && angle <= rotationTolerance)
                    {
                        availableGoalIndices.RemoveAt(j);
                        pieceMatched = true;
                        break;
                    }
                }
            }
            if (!pieceMatched) return false;
        }
        return availableGoalIndices.Count == 0;
    }

    [ContextMenu("Bake Solution")]
    public void BakeSolution()
    {
        if (anchorPiece == null) return;
        savedSolution.Clear();
        foreach (var piece in otherPieces)
        {
            PieceGoal newGoal = new PieceGoal();
            newGoal.pieceTag = piece.tag;
            newGoal.relPosition = anchorPiece.transform.InverseTransformPoint(piece.transform.position);
            newGoal.relRotation = Quaternion.Inverse(anchorPiece.transform.rotation) * piece.transform.rotation;
            savedSolution.Add(newGoal);
        }
        Debug.Log("Bake completato con successo!");
    }
}