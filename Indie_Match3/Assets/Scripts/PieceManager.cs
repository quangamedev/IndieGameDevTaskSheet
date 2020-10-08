using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************
Author: Quan Nguyen
Date Made: 29.9.20
Object(s) holding this script: Board
Summary:
- Name explanatory
***********************************/

public class PieceManager : MonoBehaviour
{
    public GameObject[] gamePiecePrefabs; //an array of all game pieces stored as gameobjects
    private GamePiece[,] allGamePieces; //a 2-dimensional array holding all the current game pieces' scripts (GamePiece.cs)
    private Board board; //reference to Board class
    private Tile clickedTile; //the tile that the playter clicks on first to move a game piece
    private Tile targetTile; //the tile that the player wants the game piece to move to 
    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.Find("Board").GetComponent<Board>(); //store Board class (script)
        allGamePieces = new GamePiece[board.width, board.height]; //constructs a new array of size width by height

        FillRandom(); //Fills the board with random pieces at the start of the level
    }

    //returns a random GameObject from the gamePiecesPrefabs array 
    //called by FillRandom()
    GameObject GetRandomGamePiece()
    {
        //get a random number between 0 and all the game pieces minus 1 in gamePiecesPrefabs array
        //the .Length property of an array is not inclusive of the final number, same for random.range
        int randomIdx = Random.Range(0, gamePiecePrefabs.Length);

        //make sure that the array is populated (not null) in the inspector panel
        if(gamePiecePrefabs[randomIdx] == null)
        {
            Debug.LogWarning("WARNING: Element " + randomIdx + " in the GamePiecePrefab is reading as NULL!");
        }

        //return selected GamePiece to the function calling it
        return gamePiecePrefabs[randomIdx];
    }

    //place the game piece at destination passed in by function call
    //Called by FillRandom()
    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("PIECEMANAGER: Invalid Game Piece!");
            return; //break out of the method (fucntion) so next lines dont run
        }

        //move the gamePiece passed into the brackets by function call to the x and y passed in
        gamePiece.transform.position = new Vector3(x, y, 0);

        //set the rotation back to 0 in case we have accidentally rotated it
        gamePiece.transform.rotation = Quaternion.identity;

        //call the SetCoord() method to populate GamePiece.xIndex and GamePiece.yIndex vars
        gamePiece.SetCoord(x, y);
    }

    //fill the board with random game pieces
    //Called by Start()
    public void FillRandom()
    {
        for (int row = 0; row < board.width; row++)
        {
            for (int col = 0; col < board.height; col++)
            {
                //instantiates the piece prefab at coordinates row n col
                //Instantiate() constructs an Object, so "as GameObject" casts it instead as a GameObject
                GameObject piece = Instantiate(GetRandomGamePiece()) as GameObject;

                //defensive programming, exit method if someone forgot to place the prefab into the arrays
                if (piece == null)
                {
                    Debug.LogWarning("PIECEMANAGER: Invalid Game Piece!");
                    return; //break out of the method (fucntion) so next lines dont run
                }

                //place the game piece on the current tile
                PlaceGamePiece(piece.GetComponent<GamePiece>(), row, col);

                //set the piece name to it's
                piece.name = "Pieces (" + row + "," + col + ")";

                //store the gamePiecePrefabs GamePiece script at the appropriate position in the array
                allGamePieces[row, col] = piece.GetComponent<GamePiece>();

                //Call the Init method on tile and pass it row and col (which become 
                //Tile.yIndex and pass it a reference to the board which becomes Tile.boardScript;
                //allGamePieces[row, col].Init(row, col, this);

                //set the game pieces sorting layer to Pieces so they appear in front of the tiles
                piece.GetComponent<SpriteRenderer>().sortingLayerName = "Pieces";

                //To keep things tidy, parent the tiles to the Pieces object in the Hierachy
                piece.transform.parent = GameObject.Find("Pieces").transform;
            }
        }
    }

    //sets the clickedTiles var to the tile passed in
    //called by ()
    public void ClickTile(Tile tile)
    {
        //tiles that can be clicked are always = null
        if(clickedTile == null)
        {
            //set the clickedTile to the tile passed in by ()
            clickedTile = tile;
        }
    }

    //if a tile has been clicked, sets the targetTile var to the one passed in
    //called by ()
    public void DragToTile(Tile tile)
    {
        //if there is a tile that has been clicked on
        if (clickedTile != null)
        {
            //sets the target tile to the tile passed in
            targetTile = tile;

            Debug.Log("Clicked tile" + tile.name);
        }
    }

    //called by ()
    public void ReleaseTile()
    {
        //clickedTile and targetTile are both valid tiles
        if (clickedTile != null && targetTile != null)
        {
            //call SwitcheTiles() below to make clickTile and targetTile swap positions.
            SwitchTiles(clickedTile, targetTile);
        }
    }

    //called by ()
    void SwitchTiles(Tile tileClicked, Tile tileTargeted)
    {
        //add code to switch the corresponding GamePieces to the two tiles passed in

        //reset the clickedTile and targetTile so tiles can be clicked again
        clickedTile = null;
        targetTile = null;
    }
}
