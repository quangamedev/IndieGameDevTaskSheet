using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
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
    //called by FindVerticalMatches() to search for vertical matches from the startPiece
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
            //searchDirection is a Vector2, (1,0), (-1,0), (0,1) or (0,-1)
            //so if searchDirection.x = 1 or -1, and searchDirection.y = 0 multiplying by i
            //will search each piece in the same row as the startPiece.
            //if searchDirection.x = 0, and searchDirection.y = 1 or -1, multiplying by it by i
            //will search up and down and not change the x
            //we clamp it as a safety check to make sure it never goes outside of -1 and 1
            //and will still searches one piece at a time in either direction
            nextX = startX + (int) Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int) Mathf.Clamp(searchDirection.y, -1, 1) * i;

            //if the nextX and nextY is outside the board, our search is finished so break out of the loop
            if(pieceManager.IsWithinBounds(nextX,nextY) == false)
            {
                break;
            }

            //if the next piece is within the bounds of the board create a var to store the next piece
            GamePiece nextPiece = pieceManager.allGamePieces[nextX, nextY];

            //if the nextPiece matches the startPiece
            //make sure that List does not contain nextPiece
            if(nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
            {
                //we have a match!
                //add the next game piece to our list of matches
                matches.Add(nextPiece);
            }
            else //the next piece is not a match
            {
                break;
            }
        }

        //if the number of matches meets or exceeds the required amount
        if (matches.Count >= minLength)
        {
            //return list of matches
            return matches;
        }
        else
        {
            //return a null list (all pahs must return something)
            return null;
        }
    }

    //Calles FindMatches() and searches in an upwards and downwards direction
    //return a list of all vertical matches
    //called by 
    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        //calls FindMatches() and passess in a direction to search upwards and then downwards
        //we pass in a min length of 2 (not 3) because it is possible to match 3
        //by matching one above the start piece and one below (we will combine these 2 lists later)
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);
    }
}
