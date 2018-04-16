using System.Collections.Generic; //Allows the use of lists
using System; //Allows the use of "serializable", which allows us to modify how variables appear in the unity editor (and to show/hide them)
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * This code sets up the board/level everytime a new level needs to be generated
 */
public class BoardManager : MonoBehaviour {

    [Serializable]
    public class Count {
        public int minimum;
        public int maximum;

        public Count(int min, int max) {
            this.minimum = min; 
            this.maximum = max;
        }       
    }

    //Specify the size of the board, we can change them as the levels progress.  Default is 8x8.
    public int columns = 8;
    public int rows = 8;
    //This creates a Count object that is used to specify how many walls are created in the level (not the border walls, just the internal ones).
    //The code below specifies that there will be between 5 and 9 walls
    public Count wallCount = new Count(5, 9);
    //Same as wallCount, but for the food items in the level.  There are between 1 and 5 that can spawn.
    public Count foodCount = new Count(1, 5);

    //Below are variables that hold the prefabs that are created in the Unity Engine (check the prefab folder)
    //Create the exit block
    public GameObject exit;
    //Create the other objects (we use arrays so we can pass in the multiple types of tiles and randomly choose which is displayed)
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outerWallTiles;

    //used to keep the hierarchy clean (the list of game objects on the left of unity)
    //basically going to just put all the board objects in this so there isn't as much clutter.
    private Transform boardHolder;
    //Track all the different possible positions on the gameboard, and track if an object has spawned there or not
    //It's a list of Vector3, meaning it'll take 3 floats - the x, y, and z coordinates.  Z is always 0 since we're working in 2d.
    //NOTE: We should just use Vector2 instead.  There's no point in using Vector3.
    private List<Vector3> gridPositions = new List<Vector3>();

    //Function to empty the gridPositions list and re-initalize it
    void InitializeList() {
        gridPositions.Clear(); 

        //Re-initalize the gridPositions list
        /* The reason to loop from 1 to columns-1 (same for rows) 
         * is so there is always a space on both sides of the board.
         * In other words, there will always be a path to the exit.
         * For our maze: this isn't good, do something else :D
         */
        for(int x = 1; x < columns - 1; x++) {
            for(int y = 1; y < rows - 1; y++) {
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    //Function to set up outerwalls and the floor of the game board
    void BoardSetup() {
        boardHolder = new GameObject("Board").transform; //initalize the boardHolder

        /* The reason this loop is from -1 to columns +1 (same for rows)
         * is because we are building a border around the gameboard for the outerwall 
         */
        for(int x = -1; x < columns + 1; x++) {
            for(int y = -1; y < rows +1; y++) {
                //Choose a random floor tile from the floorTile array
                GameObject chosenTile = floorTiles[Random.Range(0, floorTiles.Length)];

                //If the tile is on the edge, choose a random outer wall tile instead
                if(x == -1 || x == columns || y == -1 || y == rows) 
                    chosenTile = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                
                /*Here we take our chosen tile and instantiate it.  In other words, we clone it for use on our board.
                 * Instantiate wants three things:
                 * 1) the original object
                 * 2) the position to put the object
                 * 3) the rotation of the object (Quaternion.identity just means that it won't rotate it)
                 * Finally we cast it to be a GameObject (with "as GameObject")
                 */
                GameObject instance = Instantiate(chosenTile, new Vector3(x,y,0f), Quaternion.identity) as GameObject;

                //set the parent of the newly instatiated game object to be our boardHolder
                instance.transform.SetParent(boardHolder);                
            }
        }
    }

    //Returns a random position on the grid/gameboard 
    Vector3 RandomPosition() {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPos = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPos;
    }

    //Spawn tiles for a given GameObject array at a randomly chosen position
    void LayoutObjectAtRandom(GameObject[] tileArray, int min, int max) {
        int count = Random.Range(min, max + 1);
        for (int i = 0; i < count; i++) {
            Vector3 randomPos = RandomPosition(); //choose a random position
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)]; //choose a random tile

            //Instantiate the random tile we chose at the random position (with no rotation)
            Instantiate(tileChoice, randomPos, Quaternion.identity); 
        }
    }

    //Function that is called by the game manager to set up the board
    public void SetupScene(int level) {
        BoardSetup(); //set up the board
        InitializeList(); //reset the gridPosition list
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum); //randomly put wall tiles
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum); //randomly put food tiles

        /* Create a logarithmic difficulty progression
         * In other words, the higher the level, the more enemies that spawn
         */
        int enemyCount = (int)Mathf.Log((level+1), 2f);
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount); //put the specified amount of enemies on the board
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity); //put the exit block in the top right corner
    }
}
