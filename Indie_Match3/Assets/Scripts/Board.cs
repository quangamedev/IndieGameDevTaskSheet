using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************
Author: Quan Nguyen
Date Made: 29.9.20
Object(s) holding this script: Board
Summary:
- Create the boards and instantiate the tiles as child objects
***********************************/

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject tileNormalPrefab; //standard sprite with sprite attached
    private Tile[,] allTiles; //a 2 dimensional array holding all the boards Tiles scripts
    
    // Start is called before the first frame update
    void Start()
    {
        allTiles = new Tile[width, height]; //makes a new array of size width & height
        SetupTiles();
    }


    //Instantiates a grid of tiles, renames the tiles, parents them to the Boards>Tiles object
    //in the Hierachy and adds theirs Tiles scripts to the allTiles array
    //Called in start
    void SetupTiles()
    {
        for (int row = 0; row < width; row++)
        {
            for (int col = 0; col < height; col++)
            {
                //instantiates the tile prefab at coordinates row n col
                //Instantiate() constructs an Object, so "as GameObject" casts it instead as a GameObject
                GameObject tile = Instantiate(tileNormalPrefab, new Vector3(row, col, 0), Quaternion.identity) as GameObject;

                //set the tile name to it's
                tile.name = "Tile (" + row + "," + col + ")";

                //store the tilePrefabs Tile script ar the appropriate position in the array
                allTiles[row, col] = tile.GetComponent<Tile>();

                //Call the Init method on tile and pass it row and col (which become 

                //To keep things tidy, parent the tiles to the Pieces object in the Hierachy
                tile.transform.parent = GameObject.Find("Tiles").transform;
            }
        }
    }
}
