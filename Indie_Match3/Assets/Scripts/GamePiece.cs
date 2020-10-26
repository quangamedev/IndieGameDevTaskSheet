using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************
Author: Quan Nguyen
Date Made: 3.10.20
Object(s) holding this script: GamePiece
Summary:
- Initialises a game piece and assigns it an xIndex and yIndex
- Moves game pieces using Vector3.Lerp
***********************************/

public class GamePiece : MonoBehaviour
{

    public int xIndex; //store the current xPos of the game piece
    public int yIndex; //store the current yPos of the game piece
    private bool isMoving = false; //checks whether the game pieces are moving
    private PieceManager pieceManager; //a reference to the piece manager class

    //the colour of the piece, defined in the Inspector using the enum below
    //the type of this var must be the same as the name of the enum used to link them
    public MatchValue matchValue;

    //assign a constant to each colour of piece
    //this is used to determine whether pieces match, even if they have different sprites
    public enum MatchValue
    {
        blue,
        green,
        orange,
        purple,
        red,
        yellow
    };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    Move((int)transform.position.x - 1, (int)transform.position.y, 0.5f);
        //}
        //if (Input.GetKeyDown(KeyCode.RightArrow))
        //{
        //    Move((int)transform.position.x + 1, (int)transform.position.y, 0.5f);
        //}
    }

    //initialises the GamePiece to give it access to the PieceManager class
    //called by
    public void Init(PieceManager pm)
    {
        //set the pieceManager var to the one passed in to the function
        pieceManager = pm;
    }

    //sets the x and y index by the arguments passed in 
    //called by PieceManager.PlaceGamePiece()
    //called by MoveRoutine() when a game piece is moved
    public void SetCoord(int x, int y)
    {
        xIndex = x; //set xIndex to the x value passed in by the function call
        yIndex = y; //set the yIndex to the y value passed in by the function call
    }

    //calls the MoveRoutine below to move game pieces
    //called by PieceManager.SwitchTilesRoutine() to find whether swapped tiles are a match
    public void Move(int destX, int destY, float timeToMove)
    {
        if (isMoving == false) //game pieces are not currently moving
        {
            //Calls the MoveRoutine() below and pass in a vector 3 and time to move
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
        }
    }

    //Lerps the game pieces to the destination passed in
    //over the amount of time passes in in timeToMove
    //caled by Move() when a piece moves
    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        //store our game piece start pos as a var
        Vector3 startPosition = transform.position;

        //bool flag used to determine whether we have arrived at the destination passed in
        bool reachedDestination = false;

        //the move has started so set isMoving to true
        isMoving = true;

        //how many seconds have passed since we started moving
        float elapsedTime = 0f;

        //while the game piece has still not reached its destination
        while (reachedDestination == false)
        {
            //determine whether we have reached our destination yet by checking the distance between our current position & destination
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true; //break the loop

                //call the PlaceGamePiece function to set the pieces final pos, to set it's x and y index and to add it to the allGamePieces array
                //PlaceGamePiece receives 2 ints, so recast the destination x and y as ints
                pieceManager.PlaceGamePiece(this, (int)destination.x, (int)destination.y);

                //break out of the while loop immediately to not run all the remaining code
                break;
            }

            //increment elapsedTime by the amount of time since
            //the last time frame so we can track the total movement tile
            elapsedTime += Time.deltaTime;

            //gives us a number clamped between 0 n 1 representing progress of the move
            float t = elapsedTime / timeToMove;

            //moves the game piece to it's new destination based on the current value of t
            transform.position = Vector3.Lerp(startPosition, destination, t);

            //wait until next frame
            yield return null;
        }

        isMoving = false; //game piece is no longer moving
    }
}
