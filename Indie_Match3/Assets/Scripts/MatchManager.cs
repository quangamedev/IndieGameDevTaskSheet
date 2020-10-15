using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************
Author: Quan Nguyen
Date Made: 29.9.20
Object(s) holding this script: Manager
Summary:
Initialises a tile and assigns it an x, y INdex and the board it is on
***********************************/

public class MatchManager : MonoBehaviour
{
    private PieceManager pieceManager; //a reference to the PieceManager class
    private Board board;

    // Start is called before the first frame update
    void Start()
    {
        //gets access to the PieceManager class
        pieceManager = GameObject.Find("PieceManager").GetComponent<PieceManager>();

        //gets access to the Board class
        board = GameObject.Find("Board").GetComponent<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Looks for matches and then stores matching pieces in a list which it then returns to the function calling this
    //minLength has a default value (of 3) which makes it an optional parameter.
    //called by
    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        //create a list that will be returned  if an appropriate match is found
        List<GamePiece> matches = new List<GamePiece>();

        //if we are using a starting x and y position that is outside the bounds of the board
        if (pieceManager.IsWithinBounds(startX, startY) == false)
        {
            //return a null list and break our of the function
            return null;
        }

        //get a reference to the game piece we will be starting our check for matches from
        GamePiece startPiece = pieceManager.allGamePieces[startX, startY];

        //defensive programming to check that the startPiece is a valid piece
        if (startPiece != null)
        {
            //add the startPiece as the first element of the matches list
            matches.Add(startPiece);
        }
        else
        {
            //return a null list and break out of the function
            return null;
        }

        //create two variables that will hold the next position we will search in
        int nextX; //the x-coordinates of the tile to search
        int nextY; //the y-coordinates of the tile to search
        int maxSearches; //the maximum number of searches we will do

        //set the value of the maximum number of searches we will do to either
        //the width or height of the board (whichever is greater)
        if (board.width > board.height)
        {
            maxSearches = board.width;
        }
        else
        {
            maxSearches = board.height;
        }

        //create a loop to start the search for matches
        //we dont need to start at zero because the startPiece is zero
        //we also dont need to go all the way to maxSearches because the startPiece is one of them
        for (int i = 1; i < maxSearches - 1; i++)
        {

        }
    }
}
