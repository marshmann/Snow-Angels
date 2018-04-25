//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager instance = null;
    //HidesInInspector makes it so this can't be viewed.. in the inspector.. self explanatory :P
    [HideInInspector] public BoardManager boardScript;
    public int playerLifeTotal = 3; //how much food the player defaultly has
    [HideInInspector] public bool playersTurn = true;
    public float turnDelay = .1f; //How long the delay is between turns
    public float levelStartDelay = 2f; //How long the levelImage shows between levels

    [HideInInspector] public List<Enemy> enemies; //List with all the enemy references stored
    [HideInInspector] public bool isHiding = false; //Boolean to detect if player is hiding or not

    private bool enemiesMoving;
    private int level = 1; //Level 2 is when a single enemy will spawn on the board

    private Text levelText; //the text shown on the level image
    private GameObject levelImage; //store a reference to the level image
    private bool doingSetUp; //a boolean dedicated to making the user not move during the level transition

    [HideInInspector] public int[,] board; 
    /* The above 2d array is the board state where
     * 0 is a floor, 1 is a wall, 2 is a broken wall and 3 is the exit */

    void Awake () {
        //The below code makes sure that only one instance of GameManager is open at a time
        //If there does happen to be more than one instance of it, it'll destroy it
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        /* When a new scene is loaded, normally all objects in the hierarchy will be destroyed
         * DontDestroyOnLoad makes it so this object stays and isn't deleted.
         * We want this since this allows us to keep track of score between scenes
         */
        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    //The below function is depricated, but I couldn't find an easy replacement; it still works though.
    //Apart of the Unity UI API; is called whenever a scene is loaded
    private void OnLevelWasLoaded(int index) {
        level++; //increment the level count
        InitGame();
    }

    void InitGame() {
        doingSetUp = true;

        levelImage = GameObject.Find("LevelImage"); //get the reference for the level image
        levelText = GameObject.Find("LevelText").GetComponent<Text>(); //Similar as above, but getting the component instead
        levelText.text = "Day " + level; //Change the level text to display the current level
        levelImage.SetActive(true); //Display the image
        Invoke("HideLevelImage", levelStartDelay); //Invoke calls the hide level image function after a certain delay

        enemies.Clear(); //Make sure the enemy list is empty when a new level starts
        boardScript.SetupScene(level);
    }
    public void PrintIt<T>(T[,] x) {
        string str = "";
        for (int i = x.GetUpperBound(0)-1; i >= 0; i--) {
            for (int j = x.GetUpperBound(0) - 1; j >= 0; j--) {
                str = x[j, i] + str;
            }
            print(str);
            str = "";
        }
    }

    //Used to turn off the level image
    private void HideLevelImage() {
        levelImage.SetActive(false);
        doingSetUp = false;
    }

    public void GameOver() {
        levelText.text = "You survived for " + level + " days";
        levelImage.SetActive(true);
        enabled = false;
    }

    // Update is called once per frame
    void Update () {
        if (playersTurn || enemiesMoving || doingSetUp)
            return;

        //If it's not the players turn and it should be the enemies turn, call the move enemies function
        StartCoroutine(MoveEnemies());
    }

    public void AddEnemyToList(Enemy script) {
        enemies.Add(script);
    }

    public void SetBoard(int[,] grid) {
        board = grid;
    }

    IEnumerator MoveEnemies() {
        enemiesMoving = true; 
        yield return new WaitForSeconds(turnDelay); //'sleep' for turnDelay

        if(enemies.Count == 0) {
            yield return new WaitForSeconds(turnDelay);
        }

        //Issue the move enemy command on every enemy in the list
        //Then wait for an arbitrarily small amount of time at the end of the turn
        for(int i = 0; i< enemies.Count; i++) {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}