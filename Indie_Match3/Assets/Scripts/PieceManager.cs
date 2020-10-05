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
    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.Find("Board").GetComponent<Board>(); //store Board class (script)
        allGamePieces = new GamePiece[board.width, board.height]; //constructs a new array of size width by height
    }

    //returns a random GameObject from the gamePiecesPrefabs array 
    //called by
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

    public void FillRandom()
    {

    }
}
