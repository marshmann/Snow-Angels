//Author: Nicholas Marshman, with the aid of a roguelike 2d unity tutorial. 
//This class, along with the enemy class and the assets, recieved the most changes.
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
    public int columns;
    public int rows;
    //This creates a Count object that is used to specify how many walls are created in the level (not the border walls, just the internal ones).

    //The amount of gems that'll appear in the level - min and max actually set in unity (the values here don't matter)
    public Count gemCount = new Count(3, 3);

    //Below are variables that hold the prefabs that are created in the Unity Engine (check the prefab folder)
    //Create the exit block
    public GameObject exit;
    //Create the other objects (we use arrays so we can pass in the multiple types of tiles and randomly choose which is displayed)
    public GameObject[] floorTiles;
    //public GameObject[] wallTiles; //Currently depricated since we don't use the destructable walls
    public GameObject[] gemTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outerWallTiles; //undestructable, currently using them for the maze, not just the outerwall

    //used to keep the hierarchy clean (the list of game objects on the left of unity)
    //basically going to just put all the board objects in this so there isn't as much clutter.
    private Transform boardHolder;
    //Track all the different possible positions on the gameboard, and track if an object has spawned there or not
    //It's a list of Vector3, meaning it'll take 3 floats - the x, y, and z coordinates.  Z is always 0 since we're working in 2d.
    //NOTE: We should just use Vector2 instead.  There's no point in using Vector3.
    private List<Vector3> gridPositions = new List<Vector3>();

    //Maze generation was provided by: http://tutorials.daspete.at/unity3d/maze-runner
    public class Maze {
        int width; int height; //width and height of the board
        bool[,] grid; //grid (true = no wall, false = wall)
        System.Random random; 
        int startX; int startY; //start pos of the player

        //constructor
        public Maze(int width, int height, System.Random random) {
            this.width = width; 
            this.height = height;
            this.random = random;
        }

        //Getter for grid
        public bool[,] Grid(){
            return grid;
        }

        //Generate the grid
        public void Generate() {
            grid = new bool[width, height];
            startX = 0;
            startY = 0;

            grid[startX, startY] = true;
            CreateMaze(startX, startY);
        }

        //Depth first search maze generator
        void CreateMaze(int x, int y) {
            
            //Create a direction array in one of four directions and shuffle it
            int[] directions = new int[] { 1, 2, 3, 4 };
            Tools.Shuffle(directions, random); //shuffle the directions

            //Loop through every direction and create 2 cells that aren't going to be walls 
            for (int i = 0; i < directions.Length; i++) {
                if (directions[i] == 1) {
                    if (y - 1 <= 0)
                        continue;

                    //If the two cells are not already a path
                    if (grid[x, y - 2] == false) {
                        grid[x, y - 2] = true;
                        grid[x, y - 1] = true;

                        CreateMaze(x, y - 2);
                    }
                }

                if (directions[i] == 2) {
                    if (x - 1 <= 0)
                        continue;

                    if (grid[x - 2, y] == false) {
                        grid[x - 2, y] = true;
                        grid[x - 1, y] = true;

                        CreateMaze(x - 2, y);
                    }
                }

                if (directions[i] == 3) {
                    if (x + 2 >= width - 1)
                        continue;

                    if (grid[x + 2, y] == false) {
                        grid[x + 2, y] = true;
                        grid[x + 1, y] = true;

                        CreateMaze(x + 2, y);
                    }
                }

                if (directions[i] == 4) {
                    if (y + 2 >= height - 1)
                        continue;

                    if (grid[x, y + 2] == false) {
                        grid[x, y + 2] = true;
                        grid[x, y + 1] = true;

                        CreateMaze(x, y + 2);
                    }
                }
            }
        }
    }

    //Function to empty the gridPositions list and re-initalize it
    void InitializeList() {
        gridPositions.Clear(); 

        for(int x = 1; x < 2*columns - 1; x++) {
            for(int y = 1; y < 2*rows - 1; y++) {
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    //Function to set up outerwalls and the floor of the game board
    void BoardSetup(bool[,] grid) {
        /* The reason this loop is from -1 to columns +1 (same for rows)
         * is because we are building a border around the gameboard for the outerwall 
         */
        boardHolder = new GameObject("Board").transform; //initalize the boardHolder
        bool exitPlaced = false;
        for(int x = -1; x < 2*columns + 1; x++) {
            for(int y = -1; y < 2*rows + 1; y++) {

                GameObject chosenTile; //The tile that we will randomly choose to put on the board

                /* In the if statements below, we take our chosen tile and instantiate it.  
                 * In other words, we clone it for use on our board.
                 * Instantiate wants three things:
                 * 1) the original object
                 * 2) the position to put the object
                 * 3) the rotation of the object (Quaternion.identity just means that it won't rotate it)
                 * Then, we cast it to be a GameObject (with "as GameObject")
                 * Finally we set the parent of the instantiated object to be the board
                 */
                
                //If the tile is on the edge, choose a random outer wall tile
                if (x == -1 || x == 2 * columns || y == -1 || y == 2 * rows) {
                    chosenTile = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                    GameObject instance = Instantiate(chosenTile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
                else if(!exitPlaced && x >= columns && y >= columns && (grid[(int)x / 2, (int)y / 2])) {
                    GameObject instance = Instantiate(exit, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                    exitPlaced = true; //we only set the exit once
                }
                else if (grid[(int)x/2, (int)y/2]) {
                    //else choose a random floor tile from the floorTile array
                    chosenTile = floorTiles[Random.Range(0, floorTiles.Length)];
                    GameObject instance = Instantiate(chosenTile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
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
    void LayoutObjectAtRandom(GameObject[] tileArray, int min, int max, bool[,] grid) {
        int count = Random.Range(min, max + 1);
        for (int i = 0; i < count; i++) {
            Vector3 randomPos = RandomPosition(); //choose a random position
            //Find a non-wall tile
            while(!grid[(int)randomPos.x/2, (int)randomPos.y/2]) {
                randomPos = RandomPosition();
            }
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)]; //choose a random tile
            //Instantiate the random tile we chose at the random position (with no rotation)
            Instantiate(tileChoice, randomPos, Quaternion.identity); 
        }
    }

    //Function that is called by the game manager to set up the board
    public void SetupScene(int level) {
        //Generate the maze
        Maze maze = new Maze(columns + 1, rows + 1, new System.Random());
        maze.Generate();
        bool[,] grid = maze.Grid();

        BoardSetup(grid); //set up the outerwall and the floor
        
        for(int i = 0; i<= 2*columns; i++) {
            for(int j = 0; j<= 2*rows; j++) {
                if (!grid[(int)i/2, (int)j/2]) {
                    GameObject wallChoice = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                    Instantiate(wallChoice, new Vector3(i, j, 0f), Quaternion.identity);
                }
            }
        }

        InitializeList(); //reset the gridPos list and the grid list

        //monster amount is based on a logarithmic distribution, we do (level+1) so an enemy appears in the first level
        int enemyCount = (int)Mathf.Log((level+1), 2f);
        LayoutObjectAtRandom(gemTiles, gemCount.minimum, gemCount.maximum, grid); //randomly put the gem tiles
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount,grid); //put the specified amount of enemies on the board
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  