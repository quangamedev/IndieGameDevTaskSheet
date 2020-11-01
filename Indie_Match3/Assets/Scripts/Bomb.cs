using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a constant value specifying the type of bomb
//declared outside the class so other classes can see it
public enum BombType
{
    None,
    Horizontal,
    Vertical,
    Area
}

public class Bomb : GamePiece
{
    //the type of bomb specified in the enum abobe
    public BombType bombType;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
