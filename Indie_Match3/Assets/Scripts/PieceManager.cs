using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************
Author: Quan Nguyen
Date Made: 29.9.20
Object(s) holding this script: Board
Summary:
- Initialises a tile and assigns it an x and y index, and the board it is on
- Handles the Mouse input events and sends them to the PieceManager class
- Adds game pieces to the allGamePieces array at the start and after movement
- Checks whether game pieces are within the bounds of the board
***********************************/

public class PieceManager : MonoBehaviour
{
    public GameObject[] gamePiecePrefabs; //an array of all game pieces stored as gameobjects
    private GamePiece[,] allGamePieces; //a 2-dimensional array holding all the current game pieces' scripts (GamePiece.cs)
    private Board board; //reference to Board class
    private Tile clickedTile; //the tile that the playter clicks on first to move a game piece
    private Tile targetTile; //the tile that the player wants the game piece to move to
    private GamePiece clickedPiece;
    private GamePiece targetPiece;
    public float swapTime = 0.5f; //the amount of time in seconds it takes for pieces to swap places
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
    //adds the game piece passed in to the allGamePieces array
    //Called by FillRandom()
    //called by GamePiece.MoveRoutine() when a piece has finished moving
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

        //calls the function below to see if the x and y are within the bounds of the board 
        //IsWithtinBounds(x, y) returns true or false
        if (IsWithinBounds(x,y) == true)
        {
            //assign the gamePiece to the correct place in the allGamePieces array
            allGamePieces[x, y] = gamePiece;
        }

        //call the SetCoord() method to populate GamePiece.xIndex and GamePiece.yIndex vars
        gamePiece.SetCoord(x, y);
    }

    //Function that returns true or false depending on if the x, y coordinates passed in are within the boundaries of the board
    //Called by PlaceGamePiece above when adding a piece to the allGamePieces array
    bool IsWithinBounds(int x, int y)
    {
        //checks to make sure x is between 0 between 0 and the width -1 and y is whitin 0 and height -1
        //return a bool
        if (x >= 0 && x < board.width && y >= 0 && y < board.height)
        {
            return true;
        }
        return false;
    }

    bool IsNextTo(Tile start, Tile end)
    {
        //start = clickedTile;
        //end = targetTile;
        //Debug.Log(Mathf.Abs(start.xIndex - end.xIndex));
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && Mathf.Abs(start.yIndex - end.yIndex) == 0)
        {
            return true;
        }
        else if (Mathf.Abs(start.xIndex - end.xIndex) == 0 && Mathf.Abs(start.yIndex - end.yIndex) == 1)
        {
            return true;
        }
            return false;
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
                GameObject randomPiece = Instantiate(GetRandomGamePiece()) as GameObject;

                //defensive programming, exit method if someone forgot to place the prefab into the arrays
                if (randomPiece == null)
                {
                    Debug.LogWarning("PIECEMANAGER: Invalid Game Piece!");
                    return; //break out of the method (fucntion) so next lines dont run
                }


                //place the game piece on the current tile
                PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), row, col);

                //set the piece name to it's
                randomPiece.name = "Pieces (" + row + "," + col + ")";

                //store the gamePiecePrefabs GamePiece script at the appropriate position in the array
                allGamePieces[row, col] = randomPiece.GetComponent<GamePiece>();

                //Call the Init method on tile and pass it row and col (which become 
                //Tile.yIndex and pass it a reference to the board which becomes Tile.boardScript;
                //allGamePieces[row, col].Init(row, col, this);

                //initialises the GamePiece to give it access to the PieceManager
                randomPiece.GetComponent<GamePiece>().Init(this);

                //set the game pieces sorting layer to Pieces so they appear in front of the tiles
                randomPiece.GetComponent<SpriteRenderer>().sortingLayerName = "Pieces";

                //To keep things tidy, parent the tiles to the randomPieces object in the Hierachy
                randomPiece.transform.parent = GameObject.Find("Pieces").transform;
            }
        }
    }

    //sets the clickedTiles var to the tile passed in
    //called by Tile.OnMouseDown() wqhen a tile is clicked
    public void ClickTile(Tile tile)
    {
        //tiles that can be clicked are always = null
        if(clickedTile == null)
        {
            //set the clickedTile to the tile passed in by ()
            clickedTile = tile;

            Debug.Log("Clicked tile" + tile.name);
        }
    }

    //if a tile has been clicked, sets the targetTile var to the one passed in
    //called by Tile.OnMouseOver() when a tile has been entered by the mouse
    public void DragToTile(Tile tile)
    {
        //if there is a tile that has been clicked on
        if (clickedTile != null && IsNextTo(clickedTile, tile) == true)
        {
            //sets the target tile to the tile passed in
            targetTile = tile;

            Debug.Log("Clicked tile" + tile.name);
        }
    }

    //checks if the clickedTile and targetTile are valid tiles and calls SwitchTiles() below to swap their places
    //resets both clickedTile and targetTile to null so another switch can be made
    //called by Tile.OnMouseUp() when the mouse button is released over a tile
    public void ReleaseTile()
    {
        //clickedTile and targetTile are both valid tiles
        if (clickedTile != null && targetTile != null)
        {
            //call SwitcheTiles() below to make clickTile and targetTile swap positions.
            SwitchTiles(clickedTile, targetTile);
        }

        //resets the clickedTile and targetTile so tiles can be clicked again
        clickedTile = null;
        targetTile = null;
    }
    //switches the places of the clickedTile and the targetTile and resets both to null so another switch can be made
    //called by ReleaseTile()
    void SwitchTiles(Tile tileClicked, Tile tileTargeted)
    {
        //set the pieces positions to the positions of the tiles
        clickedPiece = allGamePieces[tileClicked.xIndex, tileClicked.yIndex];
        targetPiece = allGamePieces[tileTargeted.xIndex, tileTargeted.yIndex];

        //commits the swap by moving the two chosen pieces
        clickedPiece.Move(targetPiece.xIndex, targetPiece.yIndex, 0.5f);
        targetPiece.Move(clickedPiece.xIndex, clickedPiece.yIndex, 0.5f);
    }

}
