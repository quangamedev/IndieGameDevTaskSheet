using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************
Author: Quan Nguyen
Date Made: 3.10.20
Object(s) holding this script: GamePiece
Summary:
- 
***********************************/

public class GamePiece : MonoBehaviour
{

    public int xIndex; //store the current xPos of the game piece
    public int yIndex; //store the current yPos of the game piece
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x - 1, (int)transform.position.y, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x + 1, (int)transform.position.y, 0.5f);
        }
    }

    //sets the x and y index by the arguments passed in 
    //called by PieceManager.PlaceGamePiece()
    public void SetCoord(int x, int y)
    {
        xIndex = x; //set xIndex to the x value passed in by the function call
        yIndex = y; //set the yIndex to the y value passed in by the function call
    }

    //called by
    public void Move(int destX, int destY, float timeToMove)
    {
        //Calls the MoveRoutine() below and pass in a vector 3 and time to move
        StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }

    //caled by Move() when a piece moves
    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        //store our game piece start pos as a var
        Vector3 startPosition = transform.position;

        //bool flag used to determine whether we have arrived at the destination passed in
        bool reachedDestination = false;

        //how many seconds have passed since we started moving
        float elapsedTime = 0f;

        //while the game piece has still not reached its destination
        while (reachedDestination == false)
        {
            //determine whether we have reached our destination yet by checking the distance between our current position & destination
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true; //break the loop

                //set the game piece to exactly the destination
                transform.position = destination;
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
    }
}
