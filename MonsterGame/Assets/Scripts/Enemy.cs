//Authors: Nicholas Marshman - using Unity 2D roguelike tutorial as a base (and geeksforgeeks for DFS)
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
    [HideInInspector] public int[,] board; //the actual board
    [HideInInspector] public bool[,] boolBoard; //a boolean representation of what tiles have been explored
    [HideInInspector] public bool newInfo; //boolean depicting if the object updated it's known board

    //Below are containers for the audio effects
    public AudioClip enemyAttack1; public AudioClip enemyAttack2;

    private Queue<Vector2> path; //vector queue containing the path to player
    private Queue<Vector2> explorePath; //vector queue containing the path to the randomly chosen spot

    private bool chasing = false; //make note if the AI is chasing or not

    //The coordinates of the spot we last saw the player.
    private int lastSeenX = 0; private int lastSeenY = 0;

    //The coordinates of the space randomly chosen to walk to during exploration
    private int rwx; private int rwy;

    protected override void Start() {

        //Depricated: for multiple enemies, we need to add this to the list.  For a singular one, however, we can just use a setter.
        //GameManager.instance.AddEnemyToList(this); //have the enemy add itself to the list in game manager

        GameManager.instance.SetEnemy(this);
        animator = GetComponent<Animator>(); //initalize animator
        target = GameObject.FindGameObjectWithTag("Player").transform; //store the player's location

        base.Start(); //call the super code's base

        path = new Queue<Vector2>(); //init path queue
        explorePath = new Queue<Vector2>(); //init explore queue

        perception = 7; //set the perception stat of the enemy (might need tuned)
        chaseValue = 8; //set the radius the enemy will continue to detect the player when chasing (might need tuned)
        chaseTurns = 12; //the amount of turns the enemy will have an increased detection radius
        chaseCount = 0; //initalize counter

        int col = 2 * GameManager.instance.boardScript.columns;
        int row = 2 * GameManager.instance.boardScript.rows;

        knownBoard = new int[col, row];
        boolBoard = new bool[col, row];

        board = GameManager.instance.board;

        SetInitDirection(); //init the direction the enemy Ai will face
    }
    //update the known board by changing bools if necessary
    public void UpdateGrid() {
        int x = (int)transform.position.x; int y = (int)transform.position.y; //Get the X and Y coordinates of the object
        int max = 2 * GameManager.instance.boardScript.columns; //max value to make sure we don't go OOB.
        List<Vector2> neighbors = InitList(x, y); //Initialize a list to have vectors with the neighbor's coords
        foreach (Vector2 pair in neighbors) { //loop over every neighbor
            if (pair.x <= -1 || pair.x >= max || pair.y >= max || pair.y <= -1) continue; //if a coord is OOB, ignore it
            else { //if we are not OOB
                if (knownBoard[(int)pair.x, (int)pair.y] != board[(int)pair.x, (int)pair.y]) { //if the board hasn't been explored yet
                    //if the only thing being updated is the location of the AI on the board, we can ignore it.  However, if not - we need to
                    //make note of the fact that new information about the maze was found
                    if (knownBoard[(int)pair.x, (int)pair.y] != 4 && board[(int)pair.x, (int)pair.y] != 4) newInfo = true;

                    knownBoard[(int)pair.x, (int)pair.y] = board[(int)pair.x, (int)pair.y]; //update the known board
                    boolBoard[(int)pair.x, (int)pair.y] = true; //set tiles to show they have been explored
                }
            }
        }
    }

    //Make sure we update the grid whenever possible
    private void Update() {
        UpdateGrid();
    }

    //Randomizes the direction the enemy is initally facing
    private void SetInitDirection() {
        int[] direct = new int[4] { 0, 1, 2, 3 };

        int val = Random.Range(0, direct.Length);

        switch (val) {
            case 0: { lastMoveX = 1; lastMoveY = 0; break; } //facing right
            case 1: { lastMoveX = -1; lastMoveY = 0; break; } //facing left
            case 2: { lastMoveX = 0; lastMoveY = 1; break; } //facing up
            case 3: { lastMoveX = 0; lastMoveY = -1; break; } //facing down
        }

        SetDirArrow(lastMoveX, lastMoveY, arrow); //Rotate the arrow indicator respective to where the enemy is facing
    }

    //Enemy AI's move once every two "turns", or once every two steps the player takes.
    protected override void AttemptMove<T>(int xDir, int yDir) {
        //check to see if the enemy can move or not

        base.AttemptMove<T>(xDir, yDir);

        skipMove = true;
    }

    //Method to deep copy a queue
    private Queue<Vector2> DeepCopyQueue(Queue<Vector2> v) {
        if (v == null) {
            print("Error has occured with the queue.");
            //print("There's no easy way to tell what the error is, it might be within AStar");
            //print("However, in all likelyhood the error lies within the newInfo portion of the maze logic");
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

    //TODO: Refactor and make sure the below code actually holds up (specifically the boolBoard portion).
    //A lot of the RandomWalk code was rushed, as we wanted to do IDDFS instead for exploration,
    //but couldn't get it to function in an efficent enough manner and replaced it literally 3 hours before
    //the project was due.  And by "we" I mean "me." - Nick Marshman

    //Choose a random unknown space on the board and then call AStar to get a path to it.
    //AStar will find the shortest path, with the known board data, to the location.
    private void RandomWalk() {
        List<Vector2> list = new List<Vector2>(); //List which will contain the unknown tiles

        //Loop over the board to see what hasn't been explored yet
        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(0); j++) {
                if (board[i, j] == 1 && !boolBoard[i, j]) {
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

    public void MoveEnemy() {
        //Due to the fact the turn check is *before* the AI's detection, the AI is blind to the player's presence when it isn't their turn
        //This allows for some counterplay with the hiding mechanic, so I decided to keep it this way
        //even though logically, it would be better to have the AI still be able to detect the player.

        if (skipMove) { //since we only allow the enemy to move once for every two spaces the player moves
            skipMove = false; //skip the next enemy turn
            return; //don't continue with the rest of the code
        }

        Vector2 move = new Vector2(0, 0); //Initalize the move vector

        //If we can see the player, We'll do Astar
        //Even if we already on the path to the last known location, if we still see him then it'll need to be updated
        //Also, don't need to worry about newInfo here as it's accounted for
        if (CanSeePlayer()) {
            int x = (int)target.position.x; int y = (int)target.position.y;
            lastSeenX = x; lastSeenY = y; //Store the last seen location
            AStar aStar = gameObject.AddComponent<AStar>();
            path = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                (int)transform.position.y, x, y));
            DestroyImmediate(aStar); //Destroy the AStar object on the enemy AI object, if we don't it'll overload memory
            
            chasing = true; //the enemy is now chasing the player
            chaseCount = 0; //reset counter

            if (explorePath.Count != 0) explorePath.Clear(); //clear the explorePath
        }
        else { //enemy can't see player
            //If we were chasing but arrived at the lastSeen location, set it so we aren't chasing anymore
            if (chasing && FinishedChasing()) chasing = false;

            if (!chasing) { //If we aren't chasing a player
                if (explorePath.Count == 0) { RandomWalk(); }
                else if (newInfo) { //we don't need to chose a new tile to explore yet, but received new info while exploring
                    AStar aStar = gameObject.AddComponent<AStar>(); //recalculate the path based on the new info we have
                    explorePath = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                        (int)transform.position.y, rwx, rwy));
                    DestroyImmediate(aStar);
                }
            }
            else { //We are chasing
                if (newInfo || path.Count == 0) { //We got new information in the maze as we moved, so we rerun AStar
                    AStar aStar = gameObject.AddComponent<AStar>();
                    //We don't know the player's current position, so we go to the last place he was seen
                    path = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                        (int)transform.position.y, lastSeenX, lastSeenY));
                    DestroyImmediate(aStar);

                    if(explorePath.Count != 0) explorePath.Clear(); //clear the explorePath
                }
            }
        }

        newInfo = false; //If the newInfo tag changed to true on the last move, change it back to false
        if (chasing) move = path.Dequeue(); //If we're chasing, we use the path queue
        else move = explorePath.Dequeue(); //If we're exploring, we use the explorePath queue

        //The last move is indicative of the direction the Ai is currently facing
        lastMoveX = (int)move.x; lastMoveY = (int)move.y; //update the direction the AI is facing

        SetDirArrow(lastMoveX, lastMoveY, arrow); //Rotate the arrow indicator to depict where the enemy was last facing

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
        //TODO: make hiding useful in the context when the AI can always see the player
        if (GameManager.instance.isHiding) return false;
        //else return true;
        /* Depricated code: this code is the logic that dictates if the enemy can see the player or not
         * however, we decided the enemy should always chase the player, so this code is now depricated.
        */
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
        int direction = GetDirection(lastMoveX, lastMoveY);
        switch (direction) {
            case 0: case 1: { //facing right or facing left
                //If the target's y value is within a block of the enemy's
                if(posy-1 <= tary && tary <= posy+1) {
                    int tileCount = Mathf.Abs(tarx - posx); //calculate how many tiles are between the enemy and the player
                    if (tileCount > perception) return false; //if they are further than the enemy's perception then the enemy can't see them

                    int dir; //calculate the direction in terms of positive/negative
                    if (direction == 0) dir = 1;
                    else dir = -1;
                    
                    //check for walls up until the player's location
                    for (int i = 0; i <= tileCount; i++) {
                        int curPos = posx + (dir*i); //the current tile or position being inspected

                        if (curPos >= board.GetLength(0) || curPos < 0) return false; //out of bounds

                        int tile = board[curPos, tary]; //get the tile
                        if (tile == 1 || tile == 2) return false; //there's a wall tile between the player and the AI
                    }
                    return true; //if they made it out of the loop, then there are no walls between the player and AI
                }
                else return false; //player isn't in the y range of the enemy
            }
            case 2: case 3: { //facing up  or facing down 
                //If the target's x value is within a block of the enemy's
                if (posx-1 <= tarx && tarx <= posx+1) {
                    //check up until perception limit or you hit a wall
                    int tileCount = Mathf.Abs(tary - posy);
                    if (tileCount > perception) return false;

                    int dir; //Positive if facing up, negative if facing down
                    if (direction == 2) dir = 1;
                    else dir = -1;

                    //check for walls up until the player's location
                    for (int i = 0; i <= tileCount; i++) {
                        int curPos = posy + (dir*i); //the current tile or position being inspected

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