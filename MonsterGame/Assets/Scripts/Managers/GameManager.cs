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

    public int playerFuelTotal = 100;
    public int playerFuelCost = 5;

    [HideInInspector] public bool playersTurn = true;
    public float turnDelay = .1f; //How long the delay is between turns
    public float levelStartDelay = 2f; //How long the levelImage shows between levels
    
    [HideInInspector] public List<Enemy> enemies; //List with all the enemy references stored
    //[HideInInspector] public Enemy enemy; //The one enemy in the level

    [HideInInspector] public bool isHiding = false; //Boolean to detect if player is hiding or not
    private float time = 0.0f; //initalizer for time
    private float hideTime = 1f; //Enemies will move every half-second the player is hiding

    private bool enemiesMoving;
    private int level = 1; //Level 1 is when a single enemy will spawn on the board

    private int floorCount = 0; //how many floor tiles are on the board
    private int floorScore = 0; //the amount of tiles the player has cleared/shoveled/explored
    private float snowRate = 0;
    private bool gameOver = false;

    private Text levelText; //the text shown on the level image
    private GameObject levelImage; //store a reference to the level image
    private bool doingSetUp; //a boolean dedicated to making the user not move during the level transition
    [HideInInspector] public bool startMenu = false;

    [HideInInspector] public int[,] board;
    /* The above 2d array is the board state where
     * 0 is a floor, 1 is a wall, 2 is a broken wall and 3 is the exit */

    //Below are containers for the sound effects related to the player
    [HideInInspector] public AudioClip moveSound1;
    [HideInInspector] public AudioClip moveSound2;

    public void SetFloorCount(int count) { floorCount = count; } //set the amount of floor tiles
    public void SetFloorScore() { floorScore++; } //increment the player's total floor score
    public float GetFloorScore() {return (float)floorScore/floorCount; } //return the calculated floor score
    public void ReduceFloorScore() { floorScore = 0; } //set the score to zero 

    //public void SetEnemy(Enemy script) { enemy = script; } //set the enemy that we will be using in this map
    public void SetBoard(int[,] grid) { board = grid; } //store the board layout

    public void CheatFloorScore() { floorScore = floorCount; } //Remove this before game goes live ;) - Testing Function

    public float GetSnowRate() { return snowRate; }
    public bool CanTurnBack() { return (int)snowRate / 5 <= 0 ? false : true; }
    
    public void AddEnemyToList(Enemy script) { enemies.Add(script); }

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

        //enemies = new List<Enemy>(); //initialize the enemy list to be empty
        boardScript = GetComponent<BoardManager>();

        if (level == 1) startMenu = true; //if we're on the first level, then display a startMenu
        SceneManager.sceneLoaded += OnLoadScene; //this will make OnLoadScene the current scene
    }

    private void OnLoadScene(Scene scene, LoadSceneMode sceneMode) {
        level++; //increment the level count
        floorScore = 0; //reset floor score

        //If the game is in startMenu, don't init the game until the player hits enter (check update)
        if(!startMenu) InitGame();
    }

    /*
    //The below function is depricated, but I couldn't find an easy replacement; it still works though.
    //Apart of the Unity UI API; is called whenever a scene is loaded TODO: Replace with scenemanager code
    private void OnLevelWasLoaded(int index) {
        level++; //increment the level count
        floorScore = 0; //reset floor score
        InitGame();
    }
    */

    void InitGame() {
        doingSetUp = true;
        gameOver = false;
        startMenu = false;
        DestroyImmediate(GameObject.Find("StartMenu"));

        player = GameObject.Find("Player").GetComponent<Player>();
        //Change the spotlight to be closer depending on level (darker as game progresses)
        player.transform.GetChild(1).position += new Vector3(0, 0, (level-1)/2);

        //Change the snowfall to come faster depending on level
        //var em = transform.GetChild(0).GetComponent<ParticleSystem>().emission;
        //snowRate += (level * 5); em.rateOverTime = snowRate;

        levelImage = GameObject.Find("LevelImage"); //get the reference for the level image
        levelText = GameObject.Find("LevelText").GetComponent<Text>(); //Similar as above, but getting the component instead
        levelText.text = "Day " + (level-1); //Change the level text to display the current level
        levelImage.SetActive(true); //Display the image
        Invoke("HideLevelImage", levelStartDelay); //Invoke calls the hide level image function after a certain delay

        enemies.Clear(); //Make sure the enemy list is empty when a new level starts
        //enemy = null;
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

    IEnumerator MoveEnemies() {
        enemiesMoving = true;
        if (enemies.Count != 0) yield return new WaitForSeconds(turnDelay / enemies.Count); //'sleep' for turnDelay/enemies.Count (speeds up the wait process)
        else yield return new WaitForSeconds(turnDelay); //if the enemies.Count is zero, we just wait for a full turnDelay

        //Multiple Enemies
        if (enemies.Count == 0) { yield return new WaitForSeconds(turnDelay); /*wait again for a turnDelay, then the player will be able to move*/ }
        else {
            //Issue the move enemy command on every enemy in the list
            //Then wait for an arbitrarily small amount of time at the end of the turn
            for (int i = 0; i < enemies.Count; i++) {
                if (!enemies[i].stunned) { //if the enemy isn't stunned
                    enemies[i].MoveEnemy(); //move him
                    yield return new WaitForSeconds(enemies[i].moveTime / enemies.Count); //wait for a small turn delay
                }
                else { //the enemy is stunned
                    enemies[i].stunLength--; //reduce stun timer
                    if (enemies[i].stunLength == 0) enemies[i].stunned = false; //if stun timer is 0 then enemy is no longer stunned
                    yield return new WaitForSeconds(turnDelay); //wait for turn delay
                }
            }
        }

        //Single Enemy
        /*
        if (enemy == null) yield return new WaitForSeconds(turnDelay);
        else {
            if (enemy.stunned) {
                enemy.stunLength--;
                if (enemy.stunLength == 0) enemy.stunned = false;
                yield return new WaitForSeconds(turnDelay);
            }
            else {
                enemy.MoveEnemy();
                yield return new WaitForSeconds(enemy.moveTime);
            }
        }
        */
        playersTurn = true;
        enemiesMoving = false;
    }
}