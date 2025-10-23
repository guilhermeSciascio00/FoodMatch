using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    None = 0,
    FastFood = 1,
    Vegetable = 2,
    Fruit = 3,
}

public class Piece : MonoBehaviour
{
    [Header("SpriteHolder")]
    [SerializeField] Sprite pieceSprite;
    public Sprite PieceSprite { get { return pieceSprite; } }

    PieceType pType = PieceType.None;
    public PieceType PieceType { get { return pType; } }
    public Tile CurrentTile { get; set; }

    //References
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] List<PieceDataSO> _pieceCollectionSO;


    //I think that's important to let it be public, before we need to verify in the BoardManager, if we didn't spawn three similar pieces in the neighbors tiles.. If we did, we need to set the piece again.
    public void SetPiece()
    {
        //Set a random type to the piece
        PieceType randomType = (PieceType)Random.Range(1, 4);
        pType = randomType;

        SetPieceName(pType);
        SetPieceSprite(pType);
    }

    //Just so it's easier to indentify them in the hierachy
    private void SetPieceName(PieceType pType)
    {
        switch(pType)
        {
            case PieceType.FastFood:
                gameObject.name = $"FastFood, {CurrentTile}";
                break;
            case PieceType.Vegetable:
                gameObject.name = $"Vegetable, {CurrentTile}";
                break;
            case PieceType.Fruit:
                gameObject.name = $"Fruit, {CurrentTile}";
                break;
        }
    }

    private void SetPieceSprite(PieceType pType)
    {
        switch (pType)
        {
            case PieceType.Vegetable:
                PieceDataSO dataVeg = PickSOByType(pType);
                _spriteRenderer.sprite = dataVeg.PiecesSprites[GetRandomIndex(dataVeg.PiecesSprites)];
                break;
            case PieceType.Fruit:
                PieceDataSO dataFru = PickSOByType(pType);
                _spriteRenderer.sprite = dataFru.PiecesSprites[GetRandomIndex(dataFru.PiecesSprites)];
                break;
            case PieceType.FastFood:
                PieceDataSO dataFastF = PickSOByType(pType);
                _spriteRenderer.sprite = dataFastF.PiecesSprites[GetRandomIndex(dataFastF.PiecesSprites)];
                break;
        }

        pieceSprite = _spriteRenderer.sprite;
    }

    private int GetRandomIndex<T>(List<T> list)
    {
        return Random.Range(0, list.Count);
    }

    private PieceDataSO PickSOByType(PieceType pieceType)
    {
        foreach(PieceDataSO data in _pieceCollectionSO)
        {
            if(data.PieceType == pieceType)
            {
                return data;
            }
        }
        return null;
    }
}
