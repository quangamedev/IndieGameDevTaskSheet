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
        
    }
    
    //sets the x and y index by the arguments passed in 
    //called by 
    public void SetCoord(int x, int y)
    {
        xIndex = x; //set xIndex to the x value passed in by the function call
        yIndex = y; //set the yIndex to the y value passed in by the function call
    }
}
