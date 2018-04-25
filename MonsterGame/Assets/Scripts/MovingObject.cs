//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Parent class for objects that move (like the enemies or the player)
public abstract class MovingObject : MonoBehaviour {
    public float moveTime = 0.1f; //the amount of time for an object to move

    //layer we specified when we created our prefabs in order to check for colisions 
    //We put walls, enemies and the player on this layer
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2d; //store the component reference of the object we're moving
    private float inverseMoveTime; //makes movement calculations "more efficent"
    [HideInInspector] public int[,] knownBoard;
    [HideInInspector] public int[,] board;

    private List<Vector2> InitList(int x, int y) {
        List<Vector2> list = new List<Vector2>(8) {
            new Vector2(x - 1, y - 1), new Vector2(x - 1, y),
            new Vector2(x - 1, y + 1), new Vector2(x, y + 1),
            new Vector2(x + 1, y + 1), new Vector2(x + 1, y),
            new Vector2(x + 1, y - 1), new Vector2(x, y - 1)
        };
        return list;
    }

    private void UpdateGrid() {
        Vector2 position = transform.position;
        int x = (int)position.x; int y = (int)position.y;
        List<Vector2> neighbors = InitList(x, y);
        int max = 2 * GameManager.instance.boardScript.columns + 1;
        foreach (Vector2 pair in neighbors) {
            int xVal = (int)pair.x; int yVal = (int)pair.y;
            if (xVal <= -1 || xVal >= max || yVal >= max || yVal <= -1) continue;
            else knownBoard[xVal, yVal] = board[xVal, yVal];
        }
        //GameManager.instance.PrintIt<int>(knownBoard);
    }

    //Use this for initialization
    protected virtual void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();

        //Computationally, multiplying is more efficient than dividing, storing the inverse allows us to multiply.
        inverseMoveTime = 1f / moveTime;

        int col = 2 * GameManager.instance.boardScript.columns + 1;
        int row = 2 * GameManager.instance.boardScript.rows + 1;
        knownBoard = new int[col, row];
        board = GameManager.instance.board;
    }

    /*
    * The out keyword allows an object to be passed by reference
    * In other words, we can alter an object in this function and then return that same object
    * while also returning the boolean value we want to return
    * In other words, we return a boolean on if we can move or not
    * and we also return a RaycastHit2D object
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
            return true; //we can move
        }
        else //if we collide with something
            return false; //we can't move
    }

    //Use this to move units from one space to the next.  Pass it a Vector3 of where to move to (end).
    //It creates the movement animation (sliding from a tile to the next)
    protected IEnumerator SmoothMovement (Vector3 end) {
        //calculate the remaining distance to move based on the square magnitude
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //While the remaining distance is greater than a number that is basically zero (float.Epsilon)
        while(sqrRemainingDistance > float.Epsilon) {
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

        if (hit.transform == null) //if we didn't hit anything as we tried to move
            return;

        //Get the component of whatever we hit as we tried to move
        T hitComponent = hit.transform.GetComponent<T>();

        //Can't move and has hit something it can interact with
        if (!canMove && hitComponent != null)
            OnCantMove(hitComponent);
    }

    protected abstract void OnCantMove<T> (T component) where T : Component;
}
