using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************
Author: Quan Nguyen
Date Made: 29.9.20
Object(s) holding this script: Board
Summary:
Initialises a tile and assigns it an x, y INdex and the board it is on
***********************************/

public class Tile : MonoBehaviour
{
    public int xIndex; //location of x tile
    public int yIndex; //location of y tile

    private PieceManager pieceManager; //reference to PieceManager class
    private Board boardScript; // the particular Boards script of this tile
    // Start is called before the first frame update
    void Start()
    {
        //get a reference to the PieceManager class
        pieceManager = GameObject.Find("PieceManager").GetComponent<PieceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Sets the xIndex, yIndex and boardScript vars to the ones passed in
    //We are passing in a boardScript in case later we want more than one boardScript in our game
    //Called by Board.SetupTiles
    public void Init(int x, int y, Board board) //initialises a tile
    {
        xIndex = x; //store the x of the tile as the x var passed in by Board.SetupTiles()
        yIndex = y; //store the y of the tile as the x var passed in by Board.SetupTiles()
        boardScript = board; //store the Board script of the tile as the boardScripot var passed in by Board.SetupTiles()
    }
}
