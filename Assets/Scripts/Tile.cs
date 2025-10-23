using UnityEngine;

public enum TileState
{
    Empty = 0,
    Locked = 1,
    HoldingAPiece = 2,
}


public class Tile : MonoBehaviour
{
    //Still thinking if Tile needs to have MonoBehaviour or not.
    public Vector3Int TilePosition { get; set; }
    public Piece PieceReference { get; set; }

    TileState _tileState = TileState.Empty;
    public TileState TileState {  get { return _tileState; } set {  _tileState = value; } }
    
    private void Start()
    {
        string tileCoords = $"x: {TilePosition.x}, y: {TilePosition.y}";
        gameObject.name = tileCoords;

        //Debug.Log($"{tileCoords} holds the piece: {PieceReference.gameObject.name} and its status is : {TileState}");
    }

}
