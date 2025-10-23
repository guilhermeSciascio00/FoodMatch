using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PieceDataSO", menuName = "Scriptable Objects/PieceDataSO")]
public class PieceDataSO : ScriptableObject
{
    [SerializeField] PieceType pieceType;

    [SerializeField] List<Sprite> piecesSprites = new List<Sprite>();

    public List<Sprite> PiecesSprites { get { return piecesSprites; } }
    public PieceType PieceType { get { return pieceType; } }
}
