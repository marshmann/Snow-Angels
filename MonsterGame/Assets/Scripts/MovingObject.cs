//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Parent class for objects that move (like the enemies or the player)
public abstract class MovingObject : MonoBehaviour {
    public float moveTime = 0.1f; //the amount of time for an object to move

    protected Transform arrow;

    //A coordinate pair that is used to represent the direction the AI/Player is currently facing
    [HideInInspector] public int lastMoveX;
    [HideInInspector] public int lastMoveY;

    //layer we specified when we created our prefabs in order to check for colisions 
    //We put walls, enemies and the player on this layer
    public LayerMask blockingLayer;

    public LayerMask floorLayer;

    protected BoxCollider2D boxCollider; //boxCollider allows the use of hitboxes
    protected Rigidbody2D rb2d; //store the component reference of the object we're moving
    [HideInInspector] public float inverseMoveTime; //makes movement calculations "more efficent"

    public int[,] knownBoard; //the known board for the moving object
    public int[,] board; //the actual board
    public bool[,] boolBoard; //a boolean representation of what tiles have been explored
    public bool newInfo; //boolean depicting if the object updated it's known board

    public bool checkFloor = true; //boolean depiciting if the floor tiles around the player should be checked

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

    //update the known board by changing bools if necessary
    public void UpdateGrid() {
        if (transform.tag == "Player" || transform.tag == "Bullet") return;
        else { //Enemy Object
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
    }

    //Use this for initialization
    protected virtual void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();

        //Computationally, multiplying is more efficient than dividing, storing the inverse allows us to multiply.
        inverseMoveTime = 1f / moveTime;

        int col = 2 * GameManager.instance.boardScript.columns;
        int row = 2 * GameManager.instance.boardScript.rows;

        knownBoard = new int[col, row];
        boolBoard = new bool[col, row];

        board = GameManager.instance.board;

        arrow = transform.GetChild(0);
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
            UpdateGrid();

            //We only want to check floors to alter once in every two calls (Due to the function being called more than once)
            //hence the checkFloor boolean, which is changed to true in Player.cs
            if (transform.tag == "Player" && checkFloor) {
                checkFloor = false; 
                AlterFloor(end); //Alter the Floor
            }
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
    }

    //Generic class that is called whenever a player or enemy tries to move
    protected virtual void AttemptMove<T>(int xDir, int yDir) where T : Component {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null) return; //if we didn't hit anything as we tried to move   

        //Get the component of whatever we hit as we tried to move
        T hitComponent = hit.transform.GetComponent<T>();
        
        //Can't move and has hit something it can interact with
        if (!canMove && hitComponent != null)
            OnCantMove(hitComponent);
    }

    public void AlterFloor(Vector2 pos) {
        int x = (int)pos.x; int y = (int)pos.y;
        List<Vector2> neighbors = new List<Vector2>(5) {
            pos, new Vector2(x - 1, y), new Vector2(x, y + 1),
            new Vector2(x + 1, y), new Vector2(x, y - 1)
        };

        boxCollider.enabled = false; //Temporarily disable our own box collider so we don't hit ourself as we move

        foreach (Vector2 pair in neighbors) {
            RaycastHit2D hit = Physics2D.Linecast(pair, pair, floorLayer); //calculate if we hit anything as we moved
            if (hit.transform != null) {
                Floor hitComponent = hit.transform.GetComponent<Floor>();
                hitComponent.AlterFloor();

                if(pair == pos) { //Check if the tile we're standing on is trapped
                    if (hitComponent.IsTrapped()) {
                        //do something!
                    }
                }
            } 
        }

        boxCollider.enabled = true; //Re-enable our box collider
    }

    //Change the arrow's rotation depending on the direction the AI is facing
    protected void SetDirArrow(int lastMoveX, int lastMoveY, Transform arrow) {
        int dir = GetDirection(lastMoveX, lastMoveY);
        if (dir == 0) arrow.rotation = right;
        else if (dir == 1) arrow.rotation = left;
        else if (dir == 2) arrow.rotation = up;
        else if (dir == 3) arrow.rotation = down;
    }

    //Return a simple int representing the direction the AI is facing
    /* 0 = right, 1 = left, 2 = up, 3 = down */
    protected int GetDirection(int lastMoveX, int lastMoveY) {
        if (lastMoveX == 1) return 0; //facing right
        else if (lastMoveX == -1) return 1; //facing left
        else if (lastMoveY == 1) return 2; //facing up
        else return 3; //lastMoveY == -1; facing down
    }

    protected abstract void OnCantMove<T>(T component) where T : Component;
}
