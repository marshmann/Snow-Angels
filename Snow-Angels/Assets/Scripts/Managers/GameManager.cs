//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance = null;
    //HidesInInspector makes it so this can't be viewed.. in the inspector.. self explanatory :P
    [HideInInspector] public BoardManager boardScript;

    private Player player; //store reference to player object

    public int playerLifeTotal = 3; //how much life the player defaultly has
    
    [HideInInspector] public bool playersTurn = true;
    public float turnDelay = .1f; //How long the delay is between turns
    public float levelStartDelay = 2f; //How long the levelImage shows between levels
    
    [HideInInspector] public List<Enemy> enemies; //List with all the enemy references stored

    [HideInInspector] public bool isHiding = false; //Boolean to detect if player is hiding or not
    private float time = 0.0f; //initalizer for time
    private float hideTime = 1f; //Enemies will move every half-second the player is hiding

    private bool enemiesMoving;
    private int level = 1; //Level 1 is when a single enemy will spawn on the board

    [HideInInspector] public bool tutorial = false;
    [HideInInspector] public GameObject tutTrapTile;
    [HideInInspector] public GameObject tutWallTile;

    private int floorCount = 0; //how many floor tiles are on the board
    private int floorScore = 0; //the amount of tiles the player has cleared/shoveled/explored
    private bool gameOver = false;

    private Text levelText; //the text shown on the level image
    private GameObject levelImage; //store a reference to the level image
    private bool doingSetUp; //a boolean dedicated to making the user not move during the level transition

    private bool startMenu = false; //a boolean representing if we should have the start menu showing or not

    private int[,] board; //numerical representation of the board - 0 is a floor, 1 is a wall, 2 is a broken wall, and 3 is exit
    private GameObject[,] boardState; //Array with references to board-objects (floors/walls)

    //Below are containers for the sound effects related to the player
    [HideInInspector] public AudioClip moveSound1;
    [HideInInspector] public AudioClip moveSound2;

    public void SetFloorCount(int count) { floorCount = count; } //set the amount of floor tiles
    public void SetFloorScore() { floorScore++; } //increment the player's total floor score
    public float GetFloorScore() {return (float)floorScore/floorCount; } //return the calculated floor score
    public void ReduceFloorScore() { floorScore = 0; } //set the score to zero 
    public void CheatFloorScore(bool full) { if (full) floorScore = floorCount; else floorScore = floorCount / 2; } //Remove this before game goes live ;) - Testing Function

    public bool StartMenuShowing() { return startMenu; } //returns true if the startmenu is showing

    public void SetBoard(int[,] board) { this.board = board; } //store the numerical board layout
    public int[,] GetBoard() { return board; } //getter for the boardState arr

    public void SetBoardState(GameObject[,] boardState) { this.boardState = boardState; } //store the gameobject board layout
    public GameObject[,] GetBoardState() { return boardState; } //getter for the boardState arr

    public void AddEnemyToList(Enemy script) { enemies.Add(script); } //function that adds enemies to the list

    void Awake() {
        //The below code makes sure that only one instance of GameManager is open at a time
        //If there does happen to be more than one instance of it, it'll destroy it
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        /* When a new scene is loaded, normally all objects in the hierarchy will be destroyed
         * DontDestroyOnLoad makes it so this object stays and isn't deleted.
         * We want this since this allows us to keep track of score between scenes
         */
        DontDestroyOnLoad(gameObject);

        //We don't want the snoweffect to be destroyed either, so we'll set it to be a child of gamemanager.
        GameObject.Find("SnowEffect").transform.SetParent(transform, false);
        
        boardScript = GetComponent<BoardManager>();

        if (level == 1) startMenu = true; //if we're on the first level, then display a startMenu
        SceneManager.sceneLoaded += OnLoadScene; //this will make OnLoadScene the current scene
    }

    private void OnLoadScene(Scene scene, LoadSceneMode sceneMode) {
        level++; //increment the level count
        floorScore = 0; //reset floor score

        if (tutorial) { //if the player just finished the tutorial
            startMenu = true; //show the start menu again
            tutorial = false; //set the tutorial bool to false
        }
        //If the game is in startMenu, don't init the game until the player hits enter (check update)
        if(!startMenu) InitGame();
    }

    void InitGame() {
        doingSetUp = true;
        gameOver = false;
        startMenu = false;
        DestroyImmediate(GameObject.Find("StartMenu"));

        player = GameObject.Find("Player").GetComponent<Player>();
        //Change the spotlight to be closer depending on level (darker as game progresses)
        //player.transform.GetChild(1).position += new Vector3(0, 0, (level-1)/2);        

        levelImage = GameObject.Find("LevelImage"); //get the reference for the level image
        levelText = GameObject.Find("LevelText").GetComponent<Text>(); //Similar as above, but getting the component instead
        levelText.text = "Day " + (level-1); //Change the level text to display the current level
        levelImage.SetActive(true); //Display the image
        Invoke("HideLevelImage", levelStartDelay); //Invoke calls the hide level image function after a certain delay

        enemies.Clear(); //Make sure the enemy list is empty when a new level starts
        boardScript.SetupScene(level);
    }

    //Function dedicated to printing the board for testing.
    public void PrintIt<T>(T[,] x) {
        string str = "";
        for (int i = x.GetUpperBound(0); i >= 0; i--) {
            for (int j = x.GetUpperBound(0); j >= 0; j--) {
                str = x[j, i] + str;
            }
            print(str);
            str = "";
        }
        print("------------");
    }

    //Used to turn off the level image
    private void HideLevelImage() {
        levelImage.SetActive(false);
        doingSetUp = false;
    }

    //Print the game over screen and end the game
    public void GameOver() {
        levelText.text = "You survived for " + (level-1) + " day(s)\n\n";
        levelText.text += "Hit Enter to Play Again";
        levelImage.SetActive(true);

        gameOver = true;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); //check if the application should close

        if (Input.GetKeyDown(KeyCode.Return)) {
            if (startMenu) { //Start Menu is open
                InitGame(); //Initalize the game

                //Due to the player object spawning before the board spawn (it's in the inital load), we need to alter the tile the player spawns on to be shoveled
                //like we normally would do in the "start" function in Player.cs;
                player.AlterFloor(new Vector2(0, 0)); //Change the tile the player starts on to be "shoveled"
            }
            else if (gameOver) { //End Screen is open               
                player.SetDefaults(3); //Reset the player's stats to their defaults (passing 3 representing the default amount of lives)
                player.Restart(); //Restart the scene
                level = 1; //set the player level indicator back to 0
                player.enabled = true; //Make it so the player can move again
                SoundManager.instance.musicSource.Play();
            }
        }
        if(Input.GetKeyDown(KeyCode.Backspace) && startMenu) {
            startMenu = false; tutorial = true;
            DestroyImmediate(GameObject.Find("StartMenu"));

            player = GameObject.Find("Player").GetComponent<Player>();

            player.letPlayerAttack = false;
            player.letPlayerMove = false;
            player.letPlayerSneak = false;

            levelImage = GameObject.Find("LevelImage"); //get the reference for the level image
            levelText = GameObject.Find("LevelText").GetComponent<Text>(); //Similar as above, but getting the component instead

            levelText.text = "Loading tutorial..";

            Invoke("HideLevelImage", levelStartDelay); //Invoke calls the hide level image function after a certain delay
            boardScript.SetUpTutorial();

            SetFloorCount(50);
            level--; 
            StartCoroutine(player.MoveTutorialP1());
        }

        time += Time.deltaTime; 
        if ((playersTurn && isHiding) && (time >= hideTime)) {
            time = 0.0f;
            playersTurn = false;
            return;
        }
        else if ((playersTurn && !isHiding) || enemiesMoving || doingSetUp) return;

        //If it's not the players turn and it should be the enemies turn, call the move enemies function        
        StartCoroutine(MoveEnemies());
    }

    public void SpawnTutorialEnemy() {
        Instantiate(boardScript.enemyTiles[1], new Vector3(4, 1, 0), Quaternion.identity);
    }

    private IEnumerator MoveEnemies() {
        enemiesMoving = true;

        yield return new WaitForSeconds(turnDelay); //wait for a turn delay

        if (enemies.Count == 0)  yield return new WaitForSeconds(turnDelay); //wait again for a turnDelay
        else {
            //Issue the move enemy command on every enemy in the list
            //Then wait for an arbitrarily small amount of time at the end of the turn
            for (int i = 0; i < enemies.Count; i++) {
                if (!enemies[i].stunned) {//if the enemy isn't stunned
                    //let other enemies know his old tile is able to be walked on
                    foreach (Enemy e in enemies) e.knownBoard[(int)enemies[i].transform.position.x, (int)enemies[i].transform.position.y] = 0;
                    enemies[i].MoveEnemy(); //move him
                    foreach (Enemy e in enemies) { //let other enemies know the tile he moved to is taken
                        e.knownBoard[(int)enemies[i].transform.position.x, (int)enemies[i].transform.position.y] = 1;
                        e.newInfo = true; //this does mean that newInfo will *always* be true, so it's redundant and isn't necessary
                        //however it's currently a bandaid fix to the issue of enemies going into the same spot as other enemies.
                    }
                }
                else { //the enemy is stunned
                    enemies[i].stunLength--; //reduce stun timer
                    if (enemies[i].stunLength == 0) {
                        enemies[i].ps.Stop(); //stop the stun particle effects
                        enemies[i].stunned = false; //if stun timer is 0 then enemy is no longer stunned
                    }
                }
                yield return new WaitForSeconds(enemies[i].moveTime / enemies.Count); //wait for a small turn delay
            }
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}