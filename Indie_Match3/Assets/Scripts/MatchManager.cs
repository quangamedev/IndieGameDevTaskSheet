using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; //used so we can combine lists into a single list using the Union() method

/***********************************
Author: Quan Nguyen
Date Made: 29.9.20
Object(s) holding this script: MatchManager
Summary:
- Find Matches using FindMatchesAt()
- Highlights matches using HighlightMatches() or HighlightMatchesAt()
- Turns off highlights using HighlightTilesOff()
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

        Debug.Log("Reading IsWithinBounds() as " + pieceManager.IsWithinBounds(0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Looks for matches and then stores matching pieces in a list which it then returns to the function calling this
    //minLength has a default value (of 3) which makes it an optional parameter.
    //called by FindVerticalMatches() to search for vertical matches from the startPiece
    //called by FindHorizontalMatches() to search for horizontal matches from the startPiece
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

            //check to see if our game piece is null
            if(nextPiece == null)
            {
                break; //break out of the search
            }
            else //the game piece is valid
            {
                //if the nextPiece matches the startPiece
                //make sure that List does not contain nextPiece
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
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

    //Calls FindMatches() and searches in an upwards and downwards direction
    //return a list of all vertical matches
    //called by FindMatchesAt()
    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        //calls FindMatches() and passess in a direction to search upwards and then downwards
        //we pass in a min length of 2 (not 3) because it is possible to match 3
        //by matching one above the start piece and one below (we will combine these 2 lists later)
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        //we cannot use the System.Linq.Union() method to combine to Lists if any of them are null
        //so if they are null, we need to set them to empty Lists
        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        //use the System.Linq.Union() method to combine the two Lists
        //var will default to the first data type that is put in it (this case, a List)
        //Union() returns an IEnumerable, so we use ToList() to convert combined List to a List (instead of IEnumerable)
        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        //check whether the combined matches List is long enough for a winning match (default of 3)
        if (combinedMatches.Count >= minLength)
        {
            return combinedMatches;
        }
        else //we dont have enough matches so return a null list
        {
            return null;
        }
    }

    //Calls FindMatches() and searches in an left and right
    //return a list of all vertical matches
    //called by FindMatchesAt()
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        //calls FindMatches() and passess in a direction to search left and then right
        //we pass in a min length of 2 (not 3) because it is possible to match 3
        //by matching one to the left of start piece and one to the right (we will combine these 2 lists later)
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);

        //we cannot use the System.Linq.Union() method to combine to Lists if any of them are null
        //so if they are null, we need to set them to empty Lists
        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        //use the System.Linq.Union() method to combine the two Lists
        //var will default to the first data type that is put in it (this case, a List)
        //Union() returns an IEnumerable, so we use ToList() to convert combined List to a List (instead of IEnumerable)
        List<GamePiece> combinedMatches = leftMatches.Union(rightMatches).ToList();

        //check whether the combined matches List is long enough for a winning match (default of 3)
        if (combinedMatches.Count >= minLength)
        {
            return combinedMatches;
        }
        else //we dont have enough matches so return a null list
        {
            return null;
        }
    }

    //checks if FindMatches() works by looping through the board and changing matching tiles to red
    //called by Start()
    void HighlightMatches()
    {
        //loops through the entire board
        for (int row = 0; row < board.width; row++)
        {
            for (int col = 0; col < board.height; col++)
            {
                //makes sure row n col are within bounds
                if (pieceManager.IsWithinBounds(row, col) == false)
                {
                    break; //break out of loop if row and col not within bounds
                }

                HighlightMatchesAt(row, col);
            }
        }
    }

    //checks for matches at the coordinates passed in as arguments, and if it finds them, calls HighlightTilesOn() to highlight the match
    //called by HighlightMatches() when checking the whole board for matches
    //called by PieceManager.SwitchTilesRoutine() when 2 game pieces are switched
    public void HighlightMatchesAt(int row, int col)
    {
        //turn off highlights on the whole board so we are ready to highlight the current matches
        HighlightTilesOff(row, col);

        //return a list of all matches
        List<GamePiece> allMatches = FindMatchesAt(row, col);

        //for each matching GamePiece found in allMatches, change their color to red
        foreach (GamePiece matchedPiece in allMatches)
        {
            //highlight the tile at the matching pieces x and y index
            HighlightTilesOn(matchedPiece.xIndex, matchedPiece.yIndex);
        }
    }

    //resets the color of the tile at the coordinates passed in
    //called by HighlightMatchesAt()
    void HighlightTilesOff(int x, int y)
    {
        //change the colour of the tiles back to white and set transparency
        board.allTiles[x, y].GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 0.625f);
    }

    //Turns the tile at the coordinate passed in to red to show they are matches
    //called by HighlightMatchesAt()
    void HighlightTilesOn(int x, int y)
    {
        //change the colour of the tiles to red
        board.allTiles[x, y].GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, 0.625f);
    }

    //Finds horizontal and vertical matches and combine them into an allMatches List that is then returned.
    //called by HighlightMatches() to highlight matching pieces.
    //called by PieceManager.SwitchTilesRoutine() to find whether the swapped tiles are a match
    public List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        //declares and populate the lists to hold horizontal and vertical matches
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, 3);
        List<GamePiece> vertiMatches = FindVerticalMatches(x, y, 3);

        //we cannot use the System.Linq.Union() method to combine to Lists if any of them are null
        //so if they are null, we need to set them to empty Lists
        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }
        if (vertiMatches == null)
        {
            vertiMatches = new List<GamePiece>();
        }

        //use the System.Linq.Union() method to combine the two Lists
        //var will default to the first data type that is put in it (this case, a List)
        //Union() returns an IEnumerable, so we use ToList() to convert combined List to a List (instead of IEnumerable)
        List<GamePiece> allMatches = horizMatches.Union(vertiMatches).ToList();
        return allMatches;
    }

    //checks the piece at x and y passed in and returns true if there is a match
    //called by
    public bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        //call FindMatches() to populate 2 Lists of matches to the left and down
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        //we cannot use List.Count on a null List, so if we didnt find matches, set our List to a new empty List so we dont get errors
        if(leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }
    }
}
