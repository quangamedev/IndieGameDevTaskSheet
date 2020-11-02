using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using System.Linq; //used to we can combine lists into a single list using the Union() method
using System.Runtime.CompilerServices;
using UnityEditor;

/***********************************
Author: Quan Nguyen
Date Made: 29.9.20
Object(s) holding this script: PieceManager
Summary:
- Initialises a tile and assigns it an x and y index, and the board it is on
- Handles the Mouse input events and sends them to the PieceManager class
- Adds game pieces to the allGamePieces array at the start and after movement
- Checks whether game pieces are within the bounds of the board
- Clears pieces from the board when we find a match
- Clears all pieces from the board when
***********************************/

public class PieceManager : MonoBehaviour
{
    public GameObject[] gamePiecePrefabs; //an array of all game pieces stored as gameobjects
    public GamePiece[,] allGamePieces; //a 2-dimensional array holding all the current game pieces' scripts (GamePiece.cs)
    private Board board; //reference to Board class
    private MatchManager matchManager; //reference to the MatchManager class
    private Tile clickedTile; //the tile that the playter clicks on first to move a game piece
    private Tile targetTile; //the tile that the player wants the game piece to move to
    public float swapTime = 0.5f; //the amount of time in seconds it takes for pieces to swap places
    private bool playerInputEnabled = true; //used to disable player input while pieces are collapsing
    public StartingObject[] startingGamePieces; //an array of pieces that use the nested class StartingObject
    public int fillYOffset = 10; //how high pieces drop down
    public float fillMoveTime = 0.1f; //how fast pieces drop off screen
    public GameObject[] areaBombs;
    public GameObject[] horizontalBombs;
    public GameObject[] verticalBombs;

    public bool foundHorizontalMatches = false; //whether the horizontal matches were found this time
    public bool foundVerticalMatches = false;//whether the vertical matches were found this time

    //Nested class that defines specific game pieces we want on the board
    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;
        public int x;
        public int y;
        public int z;
    }

    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.Find("Board").GetComponent<Board>(); //store Board class (script)
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>(); //store the MatchManager class
        allGamePieces = new GamePiece[board.width, board.height]; //constructs a new array of size width by height

        SetupGamePieces();
        FillBoard(fillYOffset, fillMoveTime); //Fills the board with random pieces at the start of the level
    }

    //returns a random GameObject from the gamePiecesPrefabs array 
    //called by FillBoard()
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
    //Called by FillBoard()
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
    //Called by PlaceGamePiece() above when adding a piece to the allGamePieces array
    //called by MatchManager.FindMatches() to check the piece we start the search from is whithin the board
    //called by MatchManager.FindMatches() to check whether the next piece we are checking for matches is within the board
    //called by MakeGamePiece() when a new piece is placed
    //called by FillRandomAt() when instantiating a game piece
    public bool IsWithinBounds(int x, int y)
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
    //called by RefillRoutine() when pieces have finished collapsing
    public void FillBoard(int yOffset = 0, float moveTime = 0.1f)
    {
        int maxLoops = 100; //the maximum amount of times we let our while loop go round
        int loops = 0; //the number of times while has looped so far

        //loop through the board
        for (int row = 0; row < board.width; row++)
        {
            for (int col = 0; col < board.height; col++)
            {
                //checks wether there is a piece at the postion in the array
                if (allGamePieces[row, col] == null)
                {
                    GamePiece piece = FillRandomAt(row, col, yOffset, moveTime);

                    //keep looping until the game piece at row, col has no matches
                    //HasMatchOnFill() returns true when the random piece made has matches
                    while (matchManager.HasMatchOnFill(row, col) == true)
                    {
                        //clear the starting piece that has matches
                        ClearPieceAt(row, col);

                        //place a new random game piece with FillRandomAt()
                        piece = FillRandomAt(row, col, yOffset, moveTime);

                        //add 1 to the number of loops
                        loops++;

                        //if we have done more than 100 loops
                        if (loops > maxLoops)
                        {
                            //set the loops to 0 and break out of the loop
                            loops = 0;
                            Debug.LogWarning("WARNING: Fill board has exceeded the maximum loops!");
                            break;
                        }
                    }
                }
            }
        }
    }

    //Instatiates a random piece and then calls MakeGamePiece() to place it
    //called by FillBoard() when the board is filled at the start of the game
    private GamePiece FillRandomAt(int row, int col, int yOffset = 0, float moveTime = 0.1f)
    {
        if(IsWithinBounds(row, col) == true)
        {
        //instantiates the piece prefab at coordinates row n col
        //Instantiate() constructs an Object, so "as GameObject" casts it instead as a GameObject
        GameObject randomPiece = Instantiate(GetRandomGamePiece()) as GameObject;

        //place the random piece at the row and col passed in
        return MakeGamePiece(randomPiece, row, col, yOffset, moveTime);
        }
        return null;
    }

    //Instantiates pieces from the startingGamePieces array then calls MakeGamePiece() to place them at the x and y defined in the inspector
    //called by Start()
    void SetupGamePieces()
    {
        //loop through the startGamePieces array defined in the Inspector panel
        foreach(StartingObject sPiece in startingGamePieces)
        {
            if(sPiece != null && IsWithinBounds(sPiece.x, sPiece.y) == true)
            {
                //instantiate the next piece in the array
                GameObject startingPiece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity) as GameObject;

                //call MakeGamePiece() to place it on the board
                MakeGamePiece(startingPiece, sPiece.x, sPiece.y, fillYOffset, fillMoveTime);
            }
        }
    }

    //puts a game piece passed in at the coordinates also passed in as argunments
    //called by FillRandomAt() to fill the whole board at the start of the game
    private GamePiece MakeGamePiece(GameObject prefab, int row, int col, int yOffset = 0, float moveTime = 0.1f)
    {
        //defensive programming to make sure that randomPiece is a valid GamePiece
        if (prefab != null && IsWithinBounds(row, col) == true)
        {
            //set the piece name to it's
            prefab.name = "Pieces (" + row + "," + col + ")";

            //store the gamePiecePrefabs GamePiece script at the appropriate position in the array
            allGamePieces[row, col] = prefab.GetComponent<GamePiece>();

            //set the game pieces sorting layer to Pieces so they appear in front of the tiles
            prefab.GetComponent<SpriteRenderer>().sortingLayerName = "Pieces";

            //To keep things tidy, parent the tiles to the randomPieces object in the Hierachy
            prefab.transform.parent = GameObject.Find("Pieces").transform;

            //initialises the GamePiece to give it access to the PieceManager
            prefab.GetComponent<GamePiece>().Init(this);

            //place the game piece on the tile with row and col passed in
            PlaceGamePiece(prefab.GetComponent<GamePiece>(), row, col);

            //if we have passed in a yOffset
            if (yOffset != 0)
            {
                //move the piece up in the y by the amount of yOffset
                prefab.transform.position = new Vector3(row, col + yOffset, 0);

                //move the piece back down to its proper tile using the pieces Move() method
                prefab.GetComponent<GamePiece>().Move(row, col, moveTime);
            }

            //return the GamePiece to the function calling this
            return prefab.GetComponent<GamePiece>();
        }
        return null;
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

            //Debug.Log("Clicked tile" + tile.name);
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
    
    //Calles SwitchTilesRoutine() below to move pieces and highlight matches
    //called by ReleaseTile()
    void SwitchTiles(Tile tileClicked, Tile tileTargeted)
    {
        StartCoroutine(SwitchTilesRoutine(tileClicked, tileTargeted));
    }

    //switches the places of the clickedTile and targetTile
    //if they dont match, switches the pieces back to their earlier position
    //highlights matching pieces from the two pieces moving
    //called by SwitchTiles() above when two pieces are swapped
    IEnumerator SwitchTilesRoutine(Tile tileClicked, Tile tileTargeted)
    {
        if (playerInputEnabled == true)
        {
            //set the pieces positions to the positions of the tiles
            GamePiece clickedPiece = allGamePieces[tileClicked.xIndex, tileClicked.yIndex];
            GamePiece targetPiece = allGamePieces[tileTargeted.xIndex, tileTargeted.yIndex];

            //defensive programming to make sure that the pieces are valid
            if (clickedPiece != null & targetPiece != null)
            {
                //commits the swap by moving the two chosen pieces
                clickedPiece.Move(targetPiece.xIndex, targetPiece.yIndex, swapTime);
                targetPiece.Move(clickedPiece.xIndex, clickedPiece.yIndex, swapTime);

                //yield so the pieces can move and the array updates with the new positions
                yield return new WaitForSeconds(swapTime);

                //return a list of matches for the clicked and targeted piece
                List<GamePiece> tileClickedMatches = matchManager.FindMatchesAt(tileClicked.xIndex, tileClicked.yIndex);
                List<GamePiece> tileTargetedMatches = matchManager.FindMatchesAt(tileTargeted.xIndex, tileTargeted.yIndex);

                //if neither of the lists have anything in them, we havent a match
                if (tileClickedMatches.Count == 0 && tileTargetedMatches.Count == 0)
                {
                    //move the tiles back to their orginal position
                    clickedPiece.Move(tileClicked.xIndex, tileClicked.yIndex, swapTime);
                    targetPiece.Move(tileTargeted.xIndex, tileTargeted.yIndex, swapTime);
                }
                else
                {
                    //call ClearAndRefillBoard() and pass it a combined list of tileClickedMatches and tileTargetedMatches to clear
                    ClearAndRefillBoard(tileClickedMatches.Union(tileTargetedMatches).ToList());
                }

                //yield so the pieces can move back and the array updates with the new positions
                yield return new WaitForSeconds(swapTime);
            }
        }
        //after the pieces have moved, highlight the tiles of any matches

    }

    //clears matched pieces at the location passed in
    //called by ClearBoard() to clear all pieces on the board
    //called by the overloaded ClearPieceAt(List<GamePiece>) when clearing out a list of matches
    //called by FillBoard when matches are made at the start of the level
    public void ClearPieceAt(int x, int y)
    {
        //store the location at the x and y arguments in a variable
        GamePiece pieceToClear = allGamePieces[x, y];

        //check if we have a game piece that is valid at that location
        if(pieceToClear != null)
        {
            //destroy the piece to clear
            Destroy(pieceToClear.gameObject);

            //set the location of the allGamePieces array to null
            allGamePieces[x, y] = null;
        }
    }

    //Overloaded version of ClearPieceAt(int x, int y) above
    //our programm will know which one to use, depending upon whether we pass in two ints or a List of GamePieces
    //clears out a List of GamePieces passed in as an argument
    //called by ClearAndCollapseRoutine() to clear matches after a move
    void ClearPieceAt(List<GamePiece> gamePieces)
    {
        //loop through the List passed in
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
            //call the original ClearPieceAt() and pass in the x and y of each piece in the List
            ClearPieceAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    //clears the whole board of pieces
    //called by
    void ClearBoard()
    {
        //loop through the whole board
        for(int row = 0; row < board.width; row++)
        {
            for(int col = 0; col < board.height; row++)
            {
                //clear every piece on the board
                ClearPieceAt(row, col);
            }
        }
    }

    //called by overloaded CollapseColumn() function
    List<GamePiece> CollapseColumn (int column, float collapseTime = 0.1f)
    {
        //create a new List of game pieces that we will reurn from this method
        List<GamePiece> movingPieces = new List<GamePiece>();

        //loop from 0 in the y all the way to height - 1
        for (int i = 0; i < board.height; i++)
        {
            //i is looking for spaces in the column (pieces that are null)
            if (allGamePieces[column, i] == null)
            {
                //we have found an empty space, so leave i at the y value of the first empty space and begin looping j, starting at the square above i
                for (int j = i + 1; j < board.height; j++)
                {
                    //j is looking for actual game pieces (pieces that are valid)
                    if(allGamePieces[column, j] != null)
                    {
                        //move the first valid game piece j finds to the y value of the first empty space stored in i
                        allGamePieces[column, j].Move(column, i, collapseTime * (j - i));

                        //in order to add the right piece into the List, we cant wair for GamePiece.MoveRoutine() to update the array and call SetCoord(),
                        //which happens after the move is finished. So we do it manually here
                        allGamePieces[column, i] = allGamePieces[column, j];
                        allGamePieces[column, i].SetCoord(column, i);

                        //check that the game piece we moved isnt already in the List we return
                        if(!movingPieces.Contains(allGamePieces[column, i]))
                        {
                            //add  the piece we just moved to the List we return
                            movingPieces.Add(allGamePieces[column, i]);
                        }

                        //set the empty sapce just created to null
                        allGamePieces[column, j] = null;

                        //break out of j and hand back over to i to start the process over again
                        break;
                    }
                }
            }
        }
        //return the List of pieces that have moved to the function calling this
        return movingPieces;
    }

    //overloaded version of CollapseColum() which receives a List of gamepieces and returns a List of gamepieces containing the pieces that have moved
    //called by ClearAndCollapseRoutine() after pieces are cleared
    List<GamePiece> CollapseColumn (List<GamePiece> gamePieces)
    {
        //make the List of moving game pieces we will return
        List<GamePiece> movingPieces = new List<GamePiece>();

        //use GetColumns() to return a List of columns we want to collapse
        List<int> columnsToCollapse = GetColumns(gamePieces);

        //loops through the List of ints representing columns to collapse
        foreach (int column in columnsToCollapse)
        {
            //keep a running List of every piece that has moved by combining the Lists the ORIGINAL Collapse Column returns together
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }
        //return the List of pieces that have moved to the function calling tihs
        return movingPieces;
    }

    //receives a list of game pieces and return their column numbers
    //called by overloaded CollapseColumn()
    List <int> GetColumns(List<GamePiece> gamePieces)
    {
        //the list of integers this function returns
        List<int> columns = new List<int>();

        //loop throught the List of game pieces passed in
        foreach (GamePiece piece in gamePieces)
        {
            //make sure our List of columns doesnt already contain the pieces x
            if (!columns.Contains(piece.xIndex))
            {
                //add the xIndex (column number) to the columns List
                columns.Add(piece.xIndex);
            }
        }

        //return the List of column numbers to the function calling this
        return columns;
    }

    //calls ClearAndRefillBoardRoutine() below to clear the board and refill it again after matches are made
    //called by SwitchTilesRoutine() when matches are made
    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        //call the coroutine below
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    //called by ClearAndRefillBoard() when all the collapsing has ended
    IEnumerator RefillRoutine()
    {
        FillBoard(fillYOffset, fillMoveTime);
        yield return null;
    }

    //called by ClearAndRefillBoard() above when collapsing columns
    //called by itself recursively when collapsing makes more matches
    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        //disable player input
        playerInputEnabled = false;

        ////set in MatchManager.FindMatchesAt()
        //if (foundHorizontalMatches == true)
        //{
        //    //if there are 4 or more matches we make a horizontal bomb
        //    if(gamePieces.Count > 4)
        //    {
        //        //make horizontal bomb
        //        MakeBomb("horizontal");
        //    }
        //}

        ////set in MatchManager.FindMatchesAt()
        //if (foundVerticalMatches == true)
        //{
        //    //if there are 4 or more matches we make a vertical bomb
        //    if (gamePieces.Count > 4)
        //    {
        //        //make vertical bomb
        //        MakeBomb("vertical");
        //    }
        //}

        //create a var for to check its match value in the matched game pieces
        GamePiece piece = gamePieces[0].GetComponent<GamePiece>();

        //a string to store the matchvalue of the pieces
        string matchValue = "";

        //switch statement to check the matchvalue of the pieces
        switch (piece.matchValue)
        {
            case GamePiece.MatchValue.blue:
                matchValue = "blue";
                break;
            case GamePiece.MatchValue.green:
                matchValue = "green";
                break;
            case GamePiece.MatchValue.orange:
                matchValue = "orange";
                break;
            case GamePiece.MatchValue.purple:
                matchValue = "purple";
                break;
            case GamePiece.MatchValue.red:
                matchValue = "red";
                break;
            case GamePiece.MatchValue.yellow:
                matchValue = "yellow";
                break;
        }

        //if there are 5 or more matches we make an area bomb
        if (gamePieces.Count > 4)
        {
            MakeBomb("area", matchValue);
        }
        //if there are 4 or more matches we make a vertical or a horizontal bomb
        else if (gamePieces.Count == 4)
        {
            //set in MatchManager.FindMatchesAt()
            if (foundHorizontalMatches == true)
            {
                //make horizontal bomb
                MakeBomb("horizontal", matchValue);
            }
            //set in MatchManager.FindMatchesAt()
            else if (foundVerticalMatches == true)
            {
                //make vertical bomb
                MakeBomb("vertical", matchValue);
            }
        }

        //clear and collapse
        //adding yield return ensures the coroutine is finished before continuing on with this function
        yield return StartCoroutine(ClearAndCollapseRoutine(gamePieces));
        yield return null; //wait until it is finished

        //refill the board
        //adding yield return ensures that the coroutine is finished before continuing on with this fucntion
        yield return StartCoroutine(RefillRoutine());

        //re enable input
        playerInputEnabled = true;
    }

    //clears pieces from the board then collapses the board and checks for new matches
    //called by ClearAndRefillBoardRoutine() above
    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        //Pieces that have moved in this collapse
        List<GamePiece> movingPieces = new List<GamePiece>();

        //A list of matches formed by our moving pieces after collapsing
        List<GamePiece> matches = new List<GamePiece>();

        //a small delay in between switching tiles and clearing game pieces
        yield return new WaitForSeconds(0.25f);

        //has the collapsing of tiles finding matches finished (no more matches)
        bool isFinished = false;

        //keep clearing, collapsing and checking for matches until no further matches are made
        while(isFinished == false)
        {
            //find the pieces affected by bombs
            List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);

            //merge the list of matches to clear and the list of bombed pieces to clear
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            //check the total List to see if our bomb has triggered a second bomb
            bombedPieces = GetBombedPieces(gamePieces);

            //merge the list of matches to clear and the list of bombed pieces to clear
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            //check the total List to see if our bomb has triggered a third bomb
            bombedPieces = GetBombedPieces(gamePieces);

            //merge the list of matches to clear and the list of bombed pieces to clear
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            //clear the pieces in the List passed in as an argument
            ClearPieceAt(gamePieces);

            //reset the bools ready for the next turn
            foundVerticalMatches = false;
            foundHorizontalMatches = false;

            //a small delay in between clearing and collapsing so our player can see what is going on
            yield return new WaitForSeconds(0.25f);

            //call the overloaded version of CollapseColumns() to move the pieces down and set movingPieces to the list of moving pieces it returns
            movingPieces = CollapseColumn(gamePieces);

            //check if the pieces have finished collapsing
            while (IsCollapsed(movingPieces) == false)
            {
                //if not, wait for 1 frame
                yield return null;
            }

            //a small delay to give pieces time to collapse before looking for new matches on the board
            yield return new WaitForSeconds(0.25f);

            //find new matches at the position of the pieces that collapsed and store them in matches List
            matches = matchManager.FindMatchesAt(movingPieces);

            //if FindMatchesAt() returns an empty List
            if(matches.Count == 0)
            {
                //break out of the loop and stop clearing and collapsing
                isFinished = true;
                break;
            }
            else //collapsing created new matches
            {
                //small pause so they player can see that there are new matches
                yield return new WaitForSeconds(0.25f);

                //have this function call itself and pass it a new List of matching pieces for it to clear and collapse
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }

        }
        yield return null;
    }

    //finds out if pieces are at their destination yet and returns true if they have stopped moving
    //called by ClearAndCollapseRoutine() to delay looking for matches until collapsing has finished
    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                //check the piece's y position to see if its different from its xIndex by subtracting one from another
                if(piece.transform.position.y - (float) piece.yIndex > 0.0001f)
                {
                    //the piece is  still moving
                    return false;
                }
            }
        }
        return true;
    }

    //returns a List of all game pieces in a row passed in as argument
    //called by GetBombedPieces() when a vertical bomb is activated
    List<GamePiece> GetRowPieces(int row)
    {
        //create the List that we will return
        List<GamePiece> gamePieces = new List<GamePiece>();

        //loop through the width of the board
        for (int i = 0; i < board.width; i++)
        {
            //safety check to make sure there is a piece on the tile
            if(allGamePieces[i, row] != null)
            {
                //add the piece to the List of game pieces on that row
                gamePieces.Add(allGamePieces[i, row]);
            }
        }
        return gamePieces;
    }

    //returns a List of all game pieces in a column passed in as argument
    //called by GetBombedPieces() when a horizontal bomb is activated
    List<GamePiece> GetColumnPieces(int column)
    {
        //create the List that we will return
        List<GamePiece> gamePieces = new List<GamePiece>();
        
        //loop through the height of the board
        for (int i = 0; i < board.height; i++)
        {
            //safety check to make sure there is a piece on the tile
            if(allGamePieces[column, i] != null)
            {
                //add the piece to the List of game pieces on that column
                gamePieces.Add(allGamePieces[column, i]);
            }
        }
        return gamePieces;
    }

    //returns a List of all pieces adjacent to the bombX and bombY passed in
    //called by GetBombedPieces() when an area bomb is activated
    List<GamePiece> GetAdjacentPieces (int bombX, int bombY, int bombRange = 1)
    {
        //create the List that we will return
        List<GamePiece> gamePieces = new List<GamePiece>();

        //create a for loop that goes from -1 (default value) in the x to +1 in the x
        //and -1 in the y to +1 in the y. If we change bombRange, there will be more adjacent tiles to check
        for (int x = bombX - bombRange; x <= bombX + bombRange; x++)
        {
            for (int y = bombY - bombRange; y <= bombY + bombRange; y++)
            {
                //defensive programming to make sure all adjacent pieces are on the board
                if (IsWithinBounds(x, y) == true)
                {
                    //if they are on the board, add them to the List we return
                    gamePieces.Add(allGamePieces[x, y]);
                }
            }
        }
        return gamePieces;
    }

    //receives a List of matching pieces and checks to see if any are bombs
    //if it finds bombs it returns a List of which pieces are affected by them
    //called by ClearAndCollapseRoutine()
    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        //The List of all pieces that ClearAndCollapseRoutine() will clear passed in as arguments affected by bombs
        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        //loop through the List of game pieces that ClearAndCollapseRoutine() is about to clear, passed in as an argument to this function
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                //creates a List of bombed pieces on this individual piece
                List<GamePiece> piecesToClear = new List<GamePiece>();

                //create a var to see if it is a bomb so we can find out its type of bomb
                Bomb bomb = piece.GetComponent<Bomb>();

                //if bomb isnt null, the piece is a bomb
                if (bomb != null)
                {
                    //check bomb type
                    switch (bomb.bombType)
                    {
                        case BombType.Vertical:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Horizontal:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Area:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 2);
                            break;
                    }
                    //merge our pieces to clear List with our master List of allPiecesToClear
                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                }
            }
        }
        return allPiecesToClear;
    }

    //clears a piece and puts a bomb on the board in its place
    //called by ClearAndRefillBoardRoutine()
    void MakeBomb(string bombToMake, string matchValue)
    {
        //get random coordinates to place the bomb
        int randomX = Random.Range((int)0, (int)board.width);
        int randomY = Random.Range((int)0, (int)board.height);

        //safety check for our random cooridnates
        if (IsWithinBounds(randomX, randomY) == true)
        {
            //destroys the game piece at the x and y coordinates and removes it from the array
            ClearPieceAt(randomX, randomY);
        }

        //an Index to store the matchValue of the bomb that we will create
        int bombIndex = 0;

        //checks the matchValue of the pieces that made a match
        switch (matchValue)
        {
            case "blue":
                bombIndex = 0;
                break;
            case "green":
                bombIndex = 1;
                break;
            case "orange":
                bombIndex = 2;
                break;
            case "purple":
                bombIndex = 3;
                break;
            case "red":
                bombIndex = 4;
                break;
            case "yellow":
                bombIndex = 5;
                break;
        }

        //checks whether the string passed in indicates a horizontal bomb
        if(bombToMake == "horizontal")
        {
            //instantiate a random horizontal bomb at the random x and y
            GameObject bomb = Instantiate(horizontalBombs[bombIndex], new Vector3(randomX, randomY, 0), Quaternion.identity) as GameObject;

            //pass the bomb the PieceManager script using the Init on GamePiece it inherits
            bomb.GetComponent<Bomb>().Init(this);

            //call SetCoord to pass it an xIndex and a yIndex
            bomb.GetComponent<Bomb>().SetCoord(randomX, randomY);

            //parent the bomb to the Pieces game object in the hierarchy
            bomb.transform.parent = GameObject.Find("Pieces").transform;

            //put the bomb on the Pieces sorting layer in front of the tile
            bomb.GetComponent<SpriteRenderer>().sortingLayerName = "Pieces";

            //add the bomb to the allGamePieces array
            allGamePieces[randomX, randomY] = bomb.GetComponent<GamePiece>();

            Debug.Log("Made a horizontal bomb at " + randomX + ":" + randomY);
        }
        else if (bombToMake == "vertical")
        {
            //instantiate a random vertical bomb at the random x and y
            GameObject bomb = Instantiate(verticalBombs[bombIndex], new Vector3(randomX, randomY, 0), Quaternion.identity) as GameObject;

            //pass the bomb the PieceManager script using the Init on GamePiece it inherits
            bomb.GetComponent<Bomb>().Init(this);

            //call SetCoord to pass it an xIndex and a yIndex
            bomb.GetComponent<Bomb>().SetCoord(randomX, randomY);

            //parent the bomb to the Pieces game object in the hierarchy
            bomb.transform.parent = GameObject.Find("Pieces").transform;

            //put the bomb on the Pieces sorting layer in front of the tile
            bomb.GetComponent<SpriteRenderer>().sortingLayerName = "Pieces";

            //add the bomb to the allGamePieces array
            allGamePieces[randomX, randomY] = bomb.GetComponent<GamePiece>();

            Debug.Log("Made a vertical bomb at " + randomX + ":" + randomY);
        }
        else if (bombToMake == "area")
        {
            //instantiate a random area bomb at the random x and y
            GameObject bomb = Instantiate(areaBombs[bombIndex], new Vector3(randomX, randomY, 0), Quaternion.identity) as GameObject;

            //pass the bomb the PieceManager script using the Init on GamePiece it inherits
            bomb.GetComponent<Bomb>().Init(this);

            //call SetCoord to pass it an xIndex and a yIndex
            bomb.GetComponent<Bomb>().SetCoord(randomX, randomY);

            //parent the bomb to the Pieces game object in the hierarchy
            bomb.transform.parent = GameObject.Find("Pieces").transform;

            //put the bomb on the Pieces sorting layer in front of the tile
            bomb.GetComponent<SpriteRenderer>().sortingLayerName = "Pieces";

            //add the bomb to the allGamePieces array
            allGamePieces[randomX, randomY] = bomb.GetComponent<GamePiece>();

            Debug.Log("Made a area bomb at " + randomX + ":" + randomY);
        }
    }
}
