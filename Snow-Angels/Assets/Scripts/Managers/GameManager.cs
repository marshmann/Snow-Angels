﻿//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
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
    [HideInInspector] public int playerScore = 0;
    private bool playerDetected = false;
    
    [HideInInspector] public bool playersTurn = true;
    public float turnDelay = .1f; //How long the delay is between turns
    public float levelStartDelay = 2f; //How long the levelImage shows between levels
    
    [HideInInspector] public List<Enemy> enemies; //List with all the enemy references stored

    [HideInInspector] public bool isHiding = false; //Boolean to detect if player is hiding or not
    private float time = 0.0f; //initalizer for time
    private float hideTime = 1f; //Enemies will move every half-second the player is hiding
    public float pitchChangeTime;

    private bool enemiesMoving; //true if any enemies are still moving, false otherwise
    private int level = 1; //Level 1 is when a single enemy will spawn on the board

    [HideInInspector] public bool tutorial = false; //is the tutorial running?
    [HideInInspector] public GameObject tutTrapTile; //the tile that changes to a trap tile in the tutorial
    [HideInInspector] public GameObject tutWallTile; //the tile that changes to a wall tile in the tutorial

    private int floorCount = 0; //how many floor tiles are on the board
    private int floorScore = 0; //the amount of tiles the player has cleared/shoveled/explored
    private bool gameOver = false; //flag for when the player loses all of his lives

    private Text levelText; //the text shown on the level image
    private Text scoreText; //the text for the score
    private GameObject levelImage; //store a reference to the level image
    private bool doingSetUp; //a boolean dedicated to making the user not move during the level transition

    private bool startMenu = false; //a boolean representing if we should have the start menu showing or not

    private int[,] board; //numerical representation of the board - 0 is a floor, 1 is a wall, 2 is a broken wall, and 3 is exit
    private GameObject[,] boardState; //Array with references to board-objects (floors/walls)

    //Below are containers for the movement sound effects related to the player
    [HideInInspector] public AudioClip moveSound1;
    [HideInInspector] public AudioClip moveSound2;

    public void SetFloorCount(int count) { floorCount = count; } //set the amount of floor tiles
    public void IncFloorScore() { floorScore++; IncreaseScore(1); } //increment the player's total floor score, and the overall score
    public void BoostFloorScore() { floorScore += (floorCount / 4); } //give the player a boost of floorScore
    public float GetFloorScore() {return (float)floorScore/floorCount; } //return the calculated floor score
    public void ReduceFloorScore() { floorScore = 0; } //set the score to zero 
    public void CheatFloorScore(bool full) { if (full) floorScore = floorCount; else floorScore = floorCount / 2; } //set gauge to half or full

    public bool StartMenuShowing() { return startMenu; } //returns true if the startmenu is showing
    
    public bool PlayerDetected() { return playerDetected; } //returns true if the player was detected at all during the level

    public void SetBoard(int[,] board) { this.board = board; } //store the numerical board layout
    public int[,] GetBoard() { return board; } //getter for the boardState arr

    public void SetBoardState(GameObject[,] boardState) { this.boardState = boardState; } //store the gameobject board layout
    public GameObject[,] GetBoardState() { return boardState; } //getter for the boardState arr

    public void AddEnemyToList(Enemy script) { enemies.Add(script); } //function that adds enemies to the list

    //increase the player's overall score by the passed amount
    public void IncreaseScore(int score) {
        playerScore += score; //increase the score
        scoreText.text = "Score: " + playerScore; //update the score on the UI
    }

    private void Awake() {
        //The below code makes sure that only one instance of GameManager is open at a time
        //If there does happen to be more than one instance of it, it'll destroy it
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        /* When a new scene is loaded, normally all objects in the hierarchy will be destroyed
         * DontDestroyOnLoad makes it so this object stays and isn't deleted.
         * We want this since this allows us to keep track of the score between scenes */
        DontDestroyOnLoad(gameObject);

        //We don't want the snoweffect to be destroyed either, so we'll set it to be a child of gamemanager.
        GameObject.Find("SnowEffect").transform.SetParent(transform, false);
        
        boardScript = GetComponent<BoardManager>(); //initalize the boardScript

        if (level == 1) startMenu = true; //if we're on the first level, then display a startMenu
        SceneManager.sceneLoaded += OnLoadScene; //this will make OnLoadScene the current scene
    }

    private void OnLoadScene(Scene scene, LoadSceneMode sceneMode) {
        level++; //increment the level count
        floorScore = 0; //reset floor score
        playerDetected = false;

        if (tutorial) { //if the player just finished the tutorial
            startMenu = true; //show the start menu again
            playerScore = 0; //reset playerScore
            tutorial = false; //set the tutorial bool to false
            level--; //undo the increment that happened above
        }
        else if (gameOver) { //if the player had a gameOver
            startMenu = true; //show the start menu again
            playerScore = 0; //reset playerScore
            gameOver = false; //set the gameOver bool to false
            
            playerLifeTotal = 3; //reset player's life to 3
            GameObject.Find("Player").GetComponent<Player>().SetDefaults(3); //reset player's life to 3
            SoundManager.instance.musicSource.Play(); //reset the music

            level = 2; //reset level counter
        }
        //If the game is in startMenu, don't init the game until the player hits enter (check update)
        if(!startMenu) InitGame();
    }

    //Initalize the game by initializing components and setting up the board
    private void InitGame() {
        doingSetUp = true; //we're setting up the board
        gameOver = false; //reset any gameOver flags

        GetDefaults(); //obtain all the relevant components and destroy the start menu

        GameObject.Find("CenterImage").SetActive(false); //hide the center image

        levelText.text = "Day " + (level-1); //Change the level text to display the current level
        levelImage.SetActive(true); //Display the image
        Invoke("HideLevelImage", levelStartDelay); //Invoke calls the hide level image function after a certain delay

        enemies.Clear(); //Make sure the enemy list is empty when a new level starts
        boardScript.SetupScene(level);
    }

    //Destroy the start menu and initalize components
    private void GetDefaults() { 
        startMenu = false; //set the start menu flag to false
        DestroyImmediate(GameObject.Find("StartMenu")); //delete the start menu object
        player = GameObject.Find("Player").GetComponent<Player>(); //find the player object
        levelImage = GameObject.Find("LevelImage"); //get the reference for the level image
        levelText = GameObject.Find("LevelText").GetComponent<Text>(); //Similar as above, but getting the component instead
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>(); //Get the component for the scoreText
    }

    //Used to turn off the level image
    private void HideLevelImage() {
        levelImage.SetActive(false); //turn the level image off
        doingSetUp = false; //set the setup flag to false
    }

    //Print the game over screen and end the game
    public void GameOver() {
        levelText.text = "You survived for " + (level-1) + " day(s)\n\n"; //print message
        levelText.text += "Your score was: " + playerScore +".\n\n"; //add score message to it
        levelText.text += "Hit Enter to return to the main menu."; //add this to the message text
        levelImage.SetActive(true); //turn the level image on

        gameOver = true; //set the gameOver Flag
    }

    //Update is called once per frame
    private void Update() {
        //when the player hits escape, the game should close
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); //check if the application should close
        //when the player hits enter
        if (Input.GetKeyDown(KeyCode.Return)) {
            if (startMenu) { //if the start Menu is open
                InitGame(); //Initalize and start the game

                //Due to the player object spawning before the board spawn (it's in the inital load), we need to alter the tile the player spawns on to be shoveled
                //like we normally would do in the "start" function in Player.cs;
                player.AlterFloor(new Vector2(0, 0)); //Change the tile the player starts on to be "shoveled"
            }
            //if the player recently triggered gameover
            else if (gameOver) player.Restart(); //restart the scene            
        }

        //Alter the music if the player is being chased
        if (PlayerBeingChased()) { //player is being chased
            playerDetected = true; //the player has been detected at least once

            //Have the music gradually increase in pitch, rather than instantly become 2x speed
            if(SoundManager.instance.musicSource.pitch < 2) { //if it's not at 2x speed yet
                float pitch = SoundManager.instance.musicSource.pitch; //init pitch
                pitch += Time.deltaTime * pitch / pitchChangeTime; //increase it by a small amount
                SoundManager.instance.AlterPitch(pitch); //alter the music's pitch
            }
            else SoundManager.instance.AlterPitch(2); //if it's >= 2, assure it is actually 2x
        }
        else { //player is not being chased
            if (SoundManager.instance.musicSource.pitch > 1) { //if it's still too fast
                float pitch = SoundManager.instance.musicSource.pitch; //init pitch
                pitch -= Time.deltaTime * pitch / pitchChangeTime; //decrease it by a small amount
                SoundManager.instance.AlterPitch(pitch); //alter the music's pitch
            }
            else SoundManager.instance.AlterPitch(1); //if it's <= 1, assure it is actually 1x
        }

        //if the player hits the backspace key when the start menu is open
        if(Input.GetKeyDown(KeyCode.Backspace) && startMenu) {
            tutorial = true; //set tutorial flag
            GetDefaults(); //obtain all the relevant components and destroy the start menu

            levelText.text = "Loading tutorial.."; //let the player know the tutorial is being loaded
            Invoke("HideLevelImage", levelStartDelay); //Invoke calls the hide level image function after a certain delay
            boardScript.SetUpTutorial();

            SetFloorCount(50); //just chose an arbritary number for the floor count in the tutorial.
            player.StartTutorial(); //start the tutorial
        }

        time += Time.deltaTime; //increment time
        //If the it's the player turn and he is hiding, and the elapsed time since he started hiding is greater than hideTime
        if ((playersTurn && isHiding) && (time >= hideTime)) {
            time = 0.0f; //reset the time flag
            playersTurn = false; //make it so it's no longer the players turn
            return; //this return will add a delay to the enemy movement, making it so they don't glide.
        }
        //return if it's the players turn and he isnt hiding, or if the enemies are moving, or if the board is still being set up
        else if ((playersTurn && !isHiding) || enemiesMoving || doingSetUp) return;

        //If it's not the players turn and it is the enemies turn, but the enemies aren't yet moving, then we'll call MoveEnemies()    
        StartCoroutine(MoveEnemies()); //Coroutine allows other things to happen while the function is running
    }

    //Spawn an enemy on the appropriate tile during the tutorial
    public void SpawnTutorialEnemy() { Instantiate(boardScript.enemyTiles[1], new Vector3(4, 1, 0), Quaternion.identity); }

    //A function that returns true if any enemy is chasing the player, false otherwise
    public bool PlayerBeingChased() {
        foreach (Enemy e in enemies) { //loop over every enemy in the list
            if (e.chasing == true)  //check if they are chasing
                return true; //return true if they are 
        }
        return false; //return false, no one is chasing
    }

    //Make sure the enemies are moving 
    private IEnumerator MoveEnemies() {
        enemiesMoving = true; //set the bool
        yield return new WaitForSeconds(turnDelay); //wait for a turn delay
        if (enemies.Count == 0) yield return new WaitForSeconds(turnDelay); //wait again for a turnDelay
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
        enemiesMoving = false; //enemies are done moving
        playersTurn = true; //it is now the player's turn
    }
}