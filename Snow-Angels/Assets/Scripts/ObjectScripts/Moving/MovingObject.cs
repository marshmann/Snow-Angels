//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Parent class for objects that move (like the enemies or the player)
public abstract class MovingObject : MonoBehaviour {
    public float moveTime = 0.1f; //the amount of time for an object to move
    
    

    //layers we specified when we created our prefabs in order to check for colisions
    public LayerMask blockingLayer; //layer for walls, enemies, and player    
    public LayerMask floorLayer; //layer for floor tiles

    protected BoxCollider2D boxCollider; //boxCollider allows the use of hitboxes
    protected Rigidbody2D rb2d; //store the component reference of the object we're moving

    [HideInInspector] public float inverseMoveTime; //makes movement calculations "more efficent"
    [HideInInspector] public bool checkFloor = true; //boolean depiciting if the floor tiles around the player should be checked
    protected Vector2 lastMove; //A coordinate pair that is used to represent the direction the MO is facing
    protected Transform arrow; //the arrow that rotates around the enemy/player depending on the direction they are facing

    //Calculate the angles necessary for the arrow indicator's rotation and store them locally
    //Do this now so we don't have to calculate it everytime the player/enemy moves
    private Quaternion up = Quaternion.AngleAxis(90, Vector3.forward);
    private Quaternion down = Quaternion.AngleAxis(-90, Vector3.forward);
    private Quaternion left = Quaternion.AngleAxis(180, Vector3.forward);
    private Quaternion right = Quaternion.AngleAxis(0, Vector3.forward);

    //returns a list with coordinates of a given tile's neighbors
    public List<Vector2> InitList(int x, int y) {
        List<Vector2> list = new List<Vector2>(8) {
            new Vector2(x - 1, y - 1), new Vector2(x - 1, y),
            new Vector2(x - 1, y + 1), new Vector2(x, y + 1),
            new Vector2(x + 1, y + 1), new Vector2(x + 1, y),
            new Vector2(x + 1, y - 1), new Vector2(x, y - 1)
        };

        return list;
    }

    //Use this for initialization
    protected virtual void Start() {
        boxCollider = GetComponent<BoxCollider2D>(); //init the box collider
        rb2d = GetComponent<Rigidbody2D>(); //init the rigidbody 
        inverseMoveTime = 1f / moveTime; //inverse the moveTime for efficiency
        arrow = transform.GetChild(0); //get the arrow object
    }

    /*
    * The out keyword allows an object to be passed by reference
    * In other words, we can alter an object in this function and then return that same object
    * while also returning the boolean value we want to return
    * This means we can return a boolean on if we can move or not
    * as we also return a RaycastHit2D object
    */
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit) {
        Vector2 start = transform.position; //the current position

        //the end position, calculated by moving in the direction passed
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false; //Temporarily disable our own box collider so we don't hit ourself as we move
        hit = Physics2D.Linecast(start, end, blockingLayer); //calculate if we hit anything as we moved
        boxCollider.enabled = true; //Re-enable our box collider

        if (hit.transform == null) { //if we don't collide with anything
            StartCoroutine(SmoothMovement(end));
            //We only want to check floors to alter once in every two calls (Due to the function being called more than once)
            //hence the checkFloor boolean, which is changed to true in Player.cs
            if (transform.tag == "Player") AlterFloor(end); //Alter the Floor            
            return true; //we can move
        }
        else //if we collide with something
            return false; //we can't move
    }

    //Use this to move units from one space to the next.  Pass it a Vector3 of where to move to (end).
    //It creates the movement animation (sliding from a tile to the next)
    protected IEnumerator SmoothMovement(Vector3 end) {
        //calculate the remaining distance to move based on the square magnitude
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        //While the remaining distance is greater than a number that is basically zero (float.Epsilon)
        while (sqrRemainingDistance > float.Epsilon) {
            //move toward the end position from the start position, in a straight line
            //inverseMoveTime*Time.deltaTime represents how many units closer the object is to it's destination spot
            Vector3 newPosition = Vector3.MoveTowards(rb2d.position, end, inverseMoveTime * Time.deltaTime);
            rb2d.MovePosition(newPosition); //move the actual object to the new Position
            sqrRemainingDistance = (transform.position - end).sqrMagnitude; //recalculate the remaining distance
            yield return null; //Wait for a frame before re-evaluating the condition in the while loop (sleep for a frame)
        }

        if(transform.tag == "Player") transform.position = new Vector2((int)end.x, (int)end.y);
    }

    //Generic class that is called whenever a player or enemy tries to move
    protected virtual void AttemptMove<T>(int xDir, int yDir) where T : Component {
        RaycastHit2D hit; //create a raycast object
        bool canMove = Move(xDir, yDir, out hit); //see if the player successfully moved, returning a bool and a raycast object
        if (hit.transform == null) return; //if we didn't hit anything as we tried to move   
        T hitComponent = hit.transform.GetComponent<T>(); //Get the component of whatever we hit as we tried to move
        if (!canMove && hitComponent != null) OnCantMove(hitComponent); //Can't move and has hit something it can interact with
    }

    //Alter the floor tiles as the player walks over them
    public void AlterFloor(Vector2 pos) {
        int x = (int)pos.x; int y = (int)pos.y;

        //Create a list of potential neighbor tiles at the pos location
        List<Vector2> neighbors = new List<Vector2>(5) {
            pos, new Vector2(x - 1, y), new Vector2(x, y + 1),
            new Vector2(x + 1, y), new Vector2(x, y - 1)
        };

        boxCollider.enabled = false; //Temporarily disable our own box collider so we don't hit ourself as we move

        foreach (Vector2 pair in neighbors) { //for every pos in neighbors
            RaycastHit2D hit = Physics2D.Linecast(pair, pair, floorLayer); //calculate if the tile is an actual floor tile
            if (hit.transform != null) { //if we hit something
                Floor hitComponent = hit.transform.GetComponent<Floor>(); //get the floor component
                if (hitComponent != null) { //if we hit a floor tile
                    hitComponent.AlterFloor(); //call the alterfloor function in the floor script

                    if (pair == pos) { //Check if the tile we're standing on is trapped (on spawn)
                        string trapType = hitComponent.IsTrapped(); //check if its trapped
                        Player pl = transform.GetComponent<Player>(); //get the player object
                        if (trapType != "") { //if there is a trap on the tile
                            if (trapType == "Pain") { //if there is a pain trap on the tile
                                pl.LoseALife(1); //Player will lose a life
                                SoundManager.instance.RandomizeSFX(pl.hitSound1, pl.hitSound2); //play a random hit sound
                            }
                        }
                        else { //there is no trap on the tile
                            if (pl.spawn) pl.spawn = false; //set the spawn flag to false if the player was just spawning
                            else SoundManager.instance.RandomizeSFX(pl.moveSound1, pl.moveSound2); //play a random move sound     
                        }
                    }
                }
            } 
        }
        boxCollider.enabled = true; //Re-enable our box collider
    }

    //Change the arrow's rotation depending on the direction the AI is facing
    protected void SetDirArrow(Vector2 lastMove, Transform arrow) {
        int dir = GetDirection(lastMove); //get the direction
        if (dir == 0) arrow.rotation = right; //rotate to the right
        else if (dir == 1) arrow.rotation = left; //rotate to the left
        else if (dir == 2) arrow.rotation = up; //rotate up
        else if (dir == 3) arrow.rotation = down; //rotate down
    }

    //Return a simple int representing the direction the AI is facing
    /* 0 = right, 1 = left, 2 = up, 3 = down */
    protected int GetDirection(Vector2 lastMove) {
        if ((int)lastMove.x == 1) return 0; //facing right
        else if ((int)lastMove.x == -1) return 1; //facing left
        else if ((int)lastMove.y == 1) return 2; //facing up
        else return 3; //lastMoveY == -1; facing down
    }

    //anything that derives from this class will need to override this function
    protected abstract void OnCantMove<T>(T component) where T : Component;
}
