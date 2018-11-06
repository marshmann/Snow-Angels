//Author: Nicholas Marshman, with the aid of a roguelike 2d unity tutorial. 
using System.Collections.Generic; //Allows the use of lists
using System; 
using UnityEngine;
using Random = UnityEngine.Random;

//This code sets up the board/level everytime a new level needs to be generated
public class BoardManager : MonoBehaviour {

    [Serializable]
    public class Count {
        public int minimum; public int maximum;

        public Count(int min, int max) {
            minimum = min; maximum = max;
        }
    }

    //Specify the size of the board, we can change them as the levels progress.  Default is 20x20.
    public int columns = 20;
    public int rows = 20;
    //This creates a Count object that is used to specify how many walls are created in the level 
    //(not the border walls, just the internal ones).

    public Count gemCount = new Count(3, 3); //The amount of gems that'll appear in the level

    //Below are variables that hold the prefabs that are created in the Unity Engine (check the prefab folder)
    //Create the exit block
    public GameObject exit;
    //Create the other objects 
    //(we use arrays so we can pass in the multiple types of tiles and randomly choose which is displayed)
    public GameObject[] floorTiles;
    //public GameObject[] wallTiles; //Currently depricated since we don't use the destructable walls
    public GameObject[] gemTiles; //used to unlock the exit tile
    public GameObject[] healthTiles; //heals the player on pickup
    public GameObject[] enemyTiles; //contains the two enemy prefabs
    public GameObject[] wallTiles; //all walls but the last are undestructable
    
    //used to keep the hierarchy clean (the list of game objects on the left of unity)
    //basically going to just put all the board objects in this so there isn't as much clutter.
    [HideInInspector] public Transform boardHolder;
    //Track all the different possible positions on the gameboard, and track if an object has spawned there or not
    //It's a list of Vector3, meaning it'll take 3 floats - the x, y, and z coordinates.  
    //Z is always 0 since we're working in 2d.
    private List<Vector3> gridPositions = new List<Vector3>();

    private GameObject[,] boardState; //contains the instantiated floor tiles

    //Maze generation was provided by: http://tutorials.daspete.at/unity3d/maze-runner
    public class Maze {
        private int width; private int height; //width and height of the board
        private bool[,] grid; //grid (true = no wall, false = wall)
        private System.Random random; //Random object
        private int startX; private int startY; //start pos of the player

        //constructor
        public Maze(int width, int height, System.Random random) {
            this.width = width; //init width
            this.height = height; //init height
            this.random = random; //init random
        }

        //Getter for grid
        public bool[,] Grid() { return grid; }

        //Generate the grid
        public void Generate() {
            grid = new bool[width, height];
            startX = 0; startY = 0;

            grid[startX, startY] = true;
            CreateMaze(startX, startY);
        }

        //Depth first search maze generator
        private void CreateMaze(int x, int y) {
            //Create a direction array in one of four directions and shuffle it
            int[] directions = new int[] { 1, 2, 3, 4 };
            Tools.Shuffle(directions, random); //shuffle the directions
            //Loop through every direction and create 2 cells that aren't going to be walls 
            for (int i = 0; i < directions.Length; i++) {
                if (directions[i] == 1) {
                    if (y - 1 <= 0) continue;
                    //If the two cells are not already a path
                    if (!grid[x, y - 2]) {
                        grid[x, y - 2] = true; grid[x, y - 1] = true;
                        CreateMaze(x, y - 2);
                    }
                }
                if (directions[i] == 2) {
                    if (x - 1 <= 0) continue;
                    if (!grid[x - 2, y]) {
                        grid[x - 2, y] = true; grid[x - 1, y] = true;
                        CreateMaze(x - 2, y);
                    }
                }
                if (directions[i] == 3) {
                    if (x + 2 >= width - 1) continue;
                    if (!grid[x + 2, y]) {
                        grid[x + 2, y] = true; grid[x + 1, y] = true;
                        CreateMaze(x + 2, y);
                    }
                }
                if (directions[i] == 4) {
                    if (y + 2 >= height - 1) continue;
                    if (!grid[x, y + 2]) {
                        grid[x, y + 2] = true; grid[x, y + 1] = true;
                        CreateMaze(x, y + 2);
                    }
                }
            }
        }
    }

    //Function to empty the gridPositions list and re-initalize it
    private void InitializeList() {
        gridPositions.Clear();

        for (int x = 1; x < 2 * columns - 1; x++) {
            for (int y = 1; y < 2 * rows - 1; y++) {
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    //Function to set up outerwalls and the floor of the game board
    private int[,] BoardSetup(bool[,] grid) {
        int col = 2 * columns; int row = 2 * rows;
        int[,] board = new int[col, row];
        boardState = new GameObject[col, row];
        boardHolder = new GameObject("Board").transform; //initalize the boardHolder
        bool exitPlaced = false;   
        int floorCount = 0;
        /* The reason this loop is from -1 to columns (same for rows)
         * is because we are building a border around the gameboard for the outerwall */
        for (int x = -1; x <= col; x++) {
            for (int y = -1; y <= row; y++) {
                GameObject chosenTile; //The tile that we will randomly choose to put on the board

                //The grid is smaller than the board, due to the fact the maze generation creates small corridors and not big ones
                //thus, we scale the board by doing int division, allowing us to have two-four tiles on the board be associated
                //with one tile on the grid.
                int xVal = x / 2; int yVal = y / 2;

                /* In the if statements below, we take our chosen tile and instantiate it.  
                 * In other words, we clone it for use on our board.
                 * Instantiate wants three things:
                 * 1) the original object
                 * 2) the position to put the object
                 * 3) the rotation of the object (Quaternion.identity just means that it won't rotate it)
                 * Then, we cast it to be a GameObject (with "as GameObject")
                 * Finally we set the parent of the instantiated object to be the board
                 */
                 
                if (x == -1 || y == -1) { //If the tile is on the edge, choose a random outer wall tile
                    chosenTile = wallTiles[Random.Range(0, wallTiles.Length-1)];
                    GameObject instance = Instantiate(chosenTile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
                else if (!exitPlaced && x >= columns && y >= columns && (grid[xVal, yVal])) { //if the exit is not placed and we are around the center of the board
                    GameObject instance = Instantiate(exit, new Vector3(x, y, 0f), Quaternion.identity) as GameObject; //set the exit
                    instance.transform.SetParent(boardHolder);
                    exitPlaced = true; //we only set the exit once

                    board[x, y] = 3; //indicate that there is an exit
                    boardState[x, y] = instance; //store the instance in the boardState
                }
                else if (grid[xVal, yVal]) { //else choose a random floor tile from the floorTile array
                    chosenTile = floorTiles[Random.Range(0, floorTiles.Length)];
                    GameObject instance = Instantiate(chosenTile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                    if (x == 0 && y == 0) instance.GetComponent<Floor>().SetNotTrapped(); //force the tile where the player spawns to not be a trap

                    //if the floor is trapped we won't count it as having the posibilty of being shoveled
                    if (instance.GetComponent<Floor>().IsTrapped() == "") floorCount++; //increment a counter on how many floor tiles are generated

                    boardState[x, y] = instance; //store the instance in the boardState

                    instance.transform.SetParent(boardHolder);
                }
            }
        }
        GameManager.instance.SetFloorCount(floorCount); //Set the amount of floor tiles generated in the GameManager.
        return board;
    }

    //Returns a random position on the grid/gameboard 
    private Vector3 RandomPosition() {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPos = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPos;
    }

    //Spawn tiles for a given GameObject array at a randomly chosen position
    public void LayoutObjectAtRandom(GameObject[] tileArray, int min, int max) {
        int count = Random.Range(min, max + 1);
        for (int i = 0; i < count; i++) {
            Vector3 randomPos; //choose a random position

            //Find a non-wall tile
            bool floorTile = false;

            do { 
                randomPos = RandomPosition();
                GameObject tile = boardState[(int)randomPos.x, (int)randomPos.y];

                if (tile != null && tile.GetComponent<Floor>() != null) floorTile = true;
                else floorTile = false;
            }
            while (!floorTile);

            boardState[(int)randomPos.x, (int)randomPos.y].GetComponent<Floor>().SetNotTrapped();

            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)]; //choose a random tile
            Instantiate(tileChoice, randomPos, Quaternion.identity); //Instantiate the random tile we chose at the random position (with no rotation)
        }
    }

    public void SetUpTutorial() {
        int col = 5; int row = 5;
        for (int x = -1; x <= col+1; x++) {
            for (int y = -1; y <= row+1; y++) {
                GameObject chosenTile; //The tile that we will randomly choose to put on the board

                if (x == -1 || y == -1 || x == col+1 || y == row+1) { //If the tile is on the edge, choose a random outer wall tile
                    chosenTile = wallTiles[Random.Range(0, wallTiles.Length - 1)];
                    GameObject instance = Instantiate(chosenTile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
                else { //else choose a random floor tile from the floorTile array                  
                    chosenTile = floorTiles[Random.Range(0, floorTiles.Length)];
                    GameObject instance = Instantiate(chosenTile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    if (x == 1 && y == 2) GameManager.instance.tutTrapTile = instance;
                    else if (x == 4 && y == 1) GameManager.instance.tutWallTile = instance;
                    instance.transform.SetParent(boardHolder);
                }
            }
        }
    }

    //Function that is called by the game manager to set up the board
    public void SetupScene(int level) {
        Maze maze = new Maze(columns + 1, rows + 1, new System.Random()); //Create a maze object
        maze.Generate(); //Generate the maze
        bool[,] grid = maze.Grid(); //get the maze's grid
        int[,] board = BoardSetup(grid); //set up the outerwall and the floor
        int twocol = 2 * columns; int tworow = 2 * rows; //scale the board to be twice the size of the grid
        for (int i = 0; i <= twocol; i++) {
            for (int j = 0; j <= tworow; j++) {
                if (!grid[i / 2, j / 2]) { //associate one grid tile with four board tiles
                    int rand; //randomly generated number

                    //If the position we're looking at isn't on the to  p or right edge of the board
                    if (i != twocol && j != tworow) {
                        rand = Random.Range(0, wallTiles.Length); //randomly generate a number
                        if (rand == 3) board[i, j] = 2; //if rand == 3 then the wall is destructable
                        else board[i, j] = 1; //else it's just a regular wall
                    }
                    else rand = Random.Range(0, wallTiles.Length-1); //else randomly generate an outer-wall tile (no destructable wall)
                    
                    //Generate a wall based on rand
                    GameObject wallChoice = wallTiles[rand]; //randomly choose an outer-wall tile
                    GameObject instance = Instantiate(wallChoice, new Vector3(i, j, 0f), Quaternion.identity); //place it on the board
                    instance.transform.SetParent(boardHolder); //for organization sake, make it a child of the boardHolder object            
                }
            }
        }

        InitializeList(); //reset the gridPos list
        board = FixTrappedTiles(board); //ensure the board doesn't have unrestricted paths due to traps
        GameManager.instance.SetBoard(board); //use GetManager's setter for the board layout

        LayoutObjectAtRandom(gemTiles, gemCount.minimum, gemCount.maximum); //randomly put the gem tiles

        int enemyCount = (int)Mathf.Log(level+3, 2f); //monster amount is based on a logarithmic distribution
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount); //put the specified amount of enemies on the board

        GameManager.instance.SetBoardState(boardState); //store boardState in the gamemanager
    }

    //Make sure no two-alike traps are right next to each other
    private int[,] FixTrappedTiles(int[,] board) {
        //look at each floor tile to make sure traps aren't going to be impassable
        for (int i = 0; i <= boardState.GetUpperBound(0); i++) {
            for(int j = 0; j <= boardState.GetUpperBound(0); j++) {
                GameObject go = boardState[i, j];
                if (go != null) { //ignore null tiles
                    Floor tile = go.GetComponent<Floor>(); //get the floor component
                    if (tile != null && tile.IsTrapped() != "") { //if it's trapped  (and if it's a floor tile)          

                        //look at the neighboring tiles to make sure they aren't also trapped
                        foreach (GameObject temp in GetNeighbors((int)tile.transform.position.x, (int)tile.transform.position.y)) {
                            if (temp != null && temp.GetComponent<Floor>() != null) {
                                Floor neighbor = temp.GetComponent<Floor>();
                                //if the type of trap neighbor has is the same as the trap on the center tile, remove the trap on the neighbor tile
                                if (neighbor.IsTrapped() == tile.IsTrapped()) neighbor.SetNotTrapped();
                            }
                        }

                        if (tile.IsTrapped() == "Wall") { //if the trapped tile is a wall, we'll need to spawn it
                            Vector3 pos = tile.transform.position; //get the tile's pos
                            GameObject chosenTile = wallTiles[Random.Range(0, wallTiles.Length - 1)]; //chose a random non-destructable tile
                            GameObject instance = Instantiate(chosenTile, pos, Quaternion.identity) as GameObject; //instantiate it
                            instance.transform.SetParent(boardHolder); //put it in the boardHolder

                            board[(int)pos.x, (int)pos.y] = 1; //update the numerical representation
                            boardState[i, j] = instance; //store the instance in the boardState array
                            Destroy(go); //destroy the floor tile, which isn't needed anymore.
                        }
                    }
                }
            }
        }

        return board;
    }

    //Do bounds checks and return a list containing neighboring floor tiles 
    private List<GameObject> GetNeighbors(int x, int y) {
        List<GameObject> list = new List<GameObject>(8); //init list
        int high = boardState.GetUpperBound(0); //get upperbound 

        //if x - 1 is in bounds
        if (x - 1 >= 0) {
            list.Add(boardState[x - 1, y]); //add the left neighbor to the list
            if (y - 1 >= 0) list.Add(boardState[x - 1, y - 1]); //if y-1 is in bounds, add the bot left neighbor
            if (y + 1 <= high) list.Add(boardState[x - 1, y + 1]); //if y+1 is in bounds, add the top left neighbor
        }
        //if x + 1 is in bounds
        if (x + 1 <= high) {
            list.Add(boardState[x + 1, y]); //add the right neighbor to the list
            if (y - 1 >= 0) list.Add(boardState[x + 1, y - 1]); //if y-1 is in bounds, add the bot right neighbor
            if (y + 1 <= high) list.Add(boardState[x + 1, y + 1]); //if y+1 is in bounds, add the top right neighbor
        }

        if (y - 1 >= 0) list.Add(boardState[x, y - 1]); //if y-1 is in bounds, add the bot neighbor
        if (y + 1 <= high) list.Add(boardState[x, y + 1]); //if y+1 is in bounds, add the top neighbor

        return list;
    }
}
