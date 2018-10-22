﻿//Authors: Nicholas Marshman - using Unity 2D roguelike tutorial as a base (and geeksforgeeks for DFS)
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {
    [HideInInspector] public bool stunned = false; //is the enemy stunned?
    [HideInInspector] public int stunLength; //how long is he stunned for?

    //Depricated idea: player damage
    public int playerDamage; //Amount of food damage the player loses when hit by this enemy
    //The player damage value is set in Unity as a variable in the Enemy1 and Enemy2 prefab under the Enemy component

    private Animator animator; //the animator for the enemy
    private Transform target; //used to store player position (where the enemies will move toward)
    private bool skipMove; //enemies move every other turn

    private int perception; //how the enemy can see ahead of them
    private int chaseValue; //radius the enemy will detect during chasing
    private int chaseTurns; //amount of turns the increased detect radius lasts
    private int chaseCount; //counter for chase turns

    [HideInInspector] public int[,] knownBoard; //the known board for the moving object
    private int[,] board; //the actual board
    private bool[,] boolBoard; //a boolean representation of what tiles have been explored
    [HideInInspector] public bool newInfo; //boolean depicting if the object updated it's known board

    private int knownTileCount = 0;

    //Below are containers for the audio effects
    public AudioClip enemyAttack1; public AudioClip enemyAttack2;

    private Queue<Vector2> path; //vector queue containing the path to player
    //private Queue<Vector2> explorePath; //vector queue containing the path to the randomly chosen spot

    private bool chasing = false; //make note if the AI is chasing or not

    //The coordinates of the spot we last saw the player.
    private int lastSeenX = 0; private int lastSeenY = 0;

    //The coordinates of the space randomly chosen to walk to during exploration
    private int rwx; private int rwy;

    protected override void Start() {
        
        GameManager.instance.AddEnemyToList(this); //have the enemy add itself to the list in game manager
        //GameManager.instance.SetEnemy(this); //if there is only going to be one enemy in the game

        animator = GetComponent<Animator>(); //initalize animator
        target = GameObject.FindGameObjectWithTag("Player").transform; //store the player's location

        base.Start(); //call the super code's base

        path = new Queue<Vector2>(); //init path queue
        //explorePath = new Queue<Vector2>(); //init explore queue

        perception = 7; //set the perception stat of the enemy (might need tuned)
        chaseValue = 8; //set the radius the enemy will continue to detect the player when chasing (might need tuned)
        chaseTurns = 12; //the amount of turns the enemy will have an increased detection radius
        chaseCount = 0; //initalize counter

        ResetBoard();

        board = GameManager.instance.board;

        lastMove = RandomDirection();
        SetDirArrow(lastMove, arrow); //Rotate the arrow indicator respective to where the enemy is facing
    }

    private void ResetBoard() {
        int col = 2 * GameManager.instance.boardScript.columns;
        int row = 2 * GameManager.instance.boardScript.rows;

        knownBoard = new int[col, row];
        boolBoard = new bool[col, row];
    }

    //update the known board by changing bools if necessary
    public int UpdateGrid() {
        int newCount = 0;

        int x = (int)transform.position.x; int y = (int)transform.position.y; //Get the X and Y coordinates of the object
        int max = 2 * GameManager.instance.boardScript.columns; //max value to make sure we don't go OOB.
        List<Vector2> neighbors = InitList(x, y); //Initialize a list to have vectors with the neighbor's coords
        foreach (Vector2 pair in neighbors) { //loop over every neighbor
            if (pair.x <= -1 || pair.x >= max || pair.y >= max || pair.y <= -1) continue; //if a coord is OOB, ignore it
            else { //if we are not OOB
                if (knownBoard[(int)pair.x, (int)pair.y] != board[(int)pair.x, (int)pair.y]) { //if the board hasn't been explored yet
                    //if the only thing being updated is the location of the AI on the board, we can ignore it.  However, if not - we need to
                    //make note of the fact that new information about the maze was found
                    if (knownBoard[(int)pair.x, (int)pair.y] != 4 && board[(int)pair.x, (int)pair.y] != 4) {
                        newCount++; newInfo = true;
                    }

                    knownBoard[(int)pair.x, (int)pair.y] = board[(int)pair.x, (int)pair.y]; //update the known board
                    boolBoard[(int)pair.x, (int)pair.y] = true; //set tiles to show they have been explored
                }
            }
        }

        return newCount;
    }

    //Make sure we update the grid whenever possible
    private void Update() {
        if (!skipMove) {
            knownTileCount += UpdateGrid();

            if (knownTileCount >= 40) {
                ResetBoard();
                knownTileCount = 0;
            }
        }
    }

    //Randomizes the direction the enemy is initally facing
    private Vector2 RandomDirection() {
        int[] direct = new int[4] { 0, 1, 2, 3 };

        int val = Random.Range(0, direct.Length);

        switch (val) {
            case 0: { return new Vector2(1, 0); } //facing right
            case 1: { return new Vector2(-1, 0); } //facing left
            case 2: { return new Vector2(0, 1); } //facing up
            case 3: { return new Vector2(0, -1); } //facing down

            default: return new Vector2(0, 0); //Will never be called, is here to clear warnings
        }
    }

    //Enemy AI's move once every two "turns", or once every two steps the player takes.
    protected override void AttemptMove<T>(int xDir, int yDir) {
        base.AttemptMove<T>(xDir, yDir);
        skipMove = true;
    }

    //Method to deep copy a queue
    private Queue<Vector2> DeepCopyQueue(Queue<Vector2> v) {
        if (v == null) {
            print("Error has occured with the queue.");
            return new Queue<Vector2>(0);
        }
        Queue<Vector2> cp = new Queue<Vector2>(v.Count);
        for (int i = 0; i < v.Count; i++) {
            cp.Enqueue(v.Dequeue());
        }
        return cp;
    }

    //A simple getter function
    public int[,] GetBoard() {
        return board;
    }
    
    /*
    //Choose a direction and walk until we run into a wall
    private void RandomWalk() {
        List<Vector2> list = new List<Vector2>(); //List which will contain the unknown tiles

        //Loop over the board to see what hasn't been explored yet
        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(0); j++) {
                if (board[i, j] == 0 && !boolBoard[i, j]) {
                    list.Add(new Vector2(i, j)); //add unexplored tiles to the list
                }
            }
        }

        int rand = Random.Range(0, list.Count - 1); //get a random number
        Vector2 chosenTile = list[rand]; //use the random number to choose an unexplored tile
        rwx = (int)chosenTile.x; rwy = (int)chosenTile.y; //store the coordinates for global use

        AStar aStar = gameObject.AddComponent<AStar>(); //Create the Astar Object        

        //Store the path given by A* to the randomly chosen coordinate
        explorePath = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
            (int)transform.position.y, rwx, rwy));

        DestroyImmediate(aStar); //delete the Astar Object, as we don't need to keep it around
    }
    */

    public void MoveEnemy() {
        //Due to the fact the turn check is *before* the AI's detection, the AI is blind to the player's presence when it isn't their turn
        //This allows for some counterplay with the hiding mechanic, so I decided to keep it this way
        //even though logically, it would be better to have the AI still be able to detect the player.

        if (skipMove) { //since we only allow the enemy to move once for every two spaces the player moves
            skipMove = false; //skip the next enemy turn
            return; //don't continue with the rest of the code
        }

        Vector2 move = new Vector2(0, 0); //Initalize the move vector

        if (CanSeePlayer()) {
            int x = (int)target.position.x; int y = (int)target.position.y;
            lastSeenX = x; lastSeenY = y; //Store the last seen location
            AStar aStar = gameObject.AddComponent<AStar>();
            path = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                (int)transform.position.y, x, y));
            DestroyImmediate(aStar); //Destroy the AStar object on the enemy AI object, if we don't it'll overload memory

            chasing = true; //the enemy is now chasing the player
            chaseCount = 0; //reset counter
        }
        else { //enemy isn't in line of sight

            //If we were chasing but arrived at the lastSeen location, set it so we aren't chasing anymore
            if (chasing && FinishedChasing()) chasing = false;

            if (chasing && (newInfo || path.Count == 0)) {
                AStar aStar = gameObject.AddComponent<AStar>();
                //We don't know the player's current position, so we go to the last place he was seen
                path = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                    (int)transform.position.y, lastSeenX, lastSeenY));
                DestroyImmediate(aStar);
            }
            else {

                //TODO: Alter this to allow random breaking, in other words to make it so the enemy AI
                //doesn't always need to reach a wall to change direction.

                bool oob = false; //flag that is triggered if the potential move spot is out of bounds
                int tile = 0; //initalize the tile slot to be a floor tile
                int x; int y;
                Vector2 dir = lastMove;

                //while the tile isn't a floor (or exit) tile, or while the move leads to an oob spot
                do {
                    x = (int)(dir.x + transform.position.x); y = (int)(dir.y + transform.position.y);

                    //if x or y is out of bounds, set the flag and redo the loop
                    if (x >= board.GetLength(0) || y >= board.GetLength(0) || x < 0 || y < 0) oob = true;
                    else {
                        oob = false; //it's not out of bounds
                        move = dir;
                        tile = board[x, y];
                    }

                    dir = RandomDirection(); //choose a random direction
                }
                while ((tile != 0 && tile != 3) || oob);
            }
        }

        newInfo = false; //If the newInfo tag changed to true on the last move, change it back to false
        if (chasing) move = path.Dequeue(); //If we're chasing, but can't see the player, then we'll use the path
        //else move = explorePath.Dequeue(); //If we're exploring, we use the explorePath queue

        //The last move is indicative of the direction the Ai is currently facing
        lastMove = move; //update the direction the AI is facing

        SetDirArrow(lastMove, arrow); //Rotate the arrow indicator to depict where the enemy was last facing

        AttemptMove<Player>((int)move.x, (int)move.y); //tell the enemy to move
    }

    //TODO: figure out what happens when an enemy can't move due to it running into another enemy
    //If the enemy can't move, then it's likely because it ran into a player
    //Potential way of figuring this out:
    /*
     * If the player is detected and is being chased by the enemy, attempt to move with the player in mind,
     * if the player isn't detected, attempt to move with the enemy in mind, in other words if the enemy is in the way, retry movement
     * Alternatively, we can make it so enemies can share a tile, though that might be hard to show in a script.
     */
    protected override void OnCantMove<T>(T component) {
        Player hitPlayer = component as Player; //cast the component to be player
        animator.SetTrigger("enemyAttack"); //have the enemy visually attack the player
        SoundManager.instance.RandomizeSFX(enemyAttack1, enemyAttack2); //play a random attack sound
        hitPlayer.LoseALife(playerDamage); //hit the player
    }

    //Detect if the player can be seen or not by the enemy.
    private bool CanSeePlayer() {
        //If the player is hiding, he can't be detected - or the AI is stunned
        if (GameManager.instance.isHiding) return false;

        //Simple short-hands for the position coordinates of the enemy AI and the Player
        int posx = (int)transform.position.x; int posy = (int)transform.position.y;
        int tarx = (int)target.position.x; int tary = (int)target.position.y;
        //print(transform.position + " , " + target.position);

        if (chasing && (chaseTurns > chaseCount)) {
            chaseCount++;
            //If the target's coords fall into range of the enemy's detection radius while chasing, continue having an increased detection radius
            if (((tary - chaseValue) <= posy && posy <= (tary + chaseValue)) && ((tarx - chaseValue) <= posx && posx <= (tarx + chaseValue)))
                return true;
            else return false; //else return false and reduce the detection radius back to normal
        }

        //If the target is on one of the blocks surrounding the AI then he's going to be detected
        foreach (Vector2 pair in InitList(posx, posy)) if (tarx == pair.x && tary == pair.y) return true;

        //Get the direction the target is moving and detect accordingly
        int direction = GetDirection(lastMove);
        switch (direction) {
            case 0:
            case 1: { //facing right or facing left
                //If the target's y value is within a block of the enemy's
                if (posy - 1 <= tary && tary <= posy + 1) {
                    int tileCount = Mathf.Abs(tarx - posx); //calculate how many tiles are between the enemy and the player
                    if (tileCount > perception) return false; //if they are further than the enemy's perception then the enemy can't see them

                    int dir; //calculate the direction in terms of positive/negative
                    if (direction == 0) dir = 1;
                    else dir = -1;

                    //check for walls up until the player's location
                    for (int i = 0; i <= tileCount; i++) {
                        int curPos = posx + (dir * i); //the current tile or position being inspected

                        if (curPos >= board.GetLength(0) || curPos < 0) return false; //out of bounds

                        int tile = board[curPos, tary]; //get the tile
                        if (tile == 1 || tile == 2) return false; //there's a wall tile between the player and the AI
                    }
                    return true; //if they made it out of the loop, then there are no walls between the player and AI
                }
                else return false; //player isn't in the y range of the enemy
            }
            case 2:
            case 3: { //facing up  or facing down 
                //If the target's x value is within a block of the enemy's
                if (posx - 1 <= tarx && tarx <= posx + 1) {
                    //check up until perception limit or you hit a wall
                    int tileCount = Mathf.Abs(tary - posy);
                    if (tileCount > perception) return false;

                    int dir; //Positive if facing up, negative if facing down
                    if (direction == 2) dir = 1;
                    else dir = -1;

                    //check for walls up until the player's location
                    for (int i = 0; i <= tileCount; i++) {
                        int curPos = posy + (dir * i); //the current tile or position being inspected

                        if (curPos >= board.GetLength(0) || curPos < 0) return false; //out of bounds

                        int tile = board[tarx, curPos]; //get the tile
                        if (tile == 1 || tile == 2) return false; //there's a wall tile between the player and the AI
                    }
                    return true; //if they made it out of the loop, then there are no walls between the player and AI
                }
                else return false; //player isn't in the x range of the enemy
            }
        }

        return false;
    }

    private bool FinishedChasing() {
        if (lastSeenX == transform.position.x && lastSeenY == transform.position.y) {
            //print("Enemy has arrived at the player's last seen location");
            return true;
        }
        else return false;
    }
}