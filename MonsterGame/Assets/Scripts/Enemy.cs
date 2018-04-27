//Authors: Nicholas Marshman - using Unity 2D roguelike tutorial as a base (and geeksforgeeks for DFS)
//In addition: Kevin Bechman and Dave Kelly, due to this class being where the AI mostly is
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {
    public int playerDamage; //Amount of food damage the player loses when hit by this enemy
    //The player damage value is set in Unity as a variable in the Enemy1 and Enemy2 prefab under the Enemy component

    private Animator animator; //the animator for the enemy
    private Transform target; //use to store player position (where the enemies will move toward)
    private bool skipMove; //enemies move every other turn

    private int perception; //Changed the initialization to happen in Start(), as we get null reference error otherwise.

    //Below are containers for the audio effects
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;

    private Queue<Vector2> path;
    private Queue<Vector2> explorePath;
    private bool brokeExploring = false;
    private int lastSeenX = 0;
    private int lastSeenY = 0;

    //Randomize these in start
    private int lastMoveX;
    private int lastMoveY;

    //Testing
    // private int num = 0;

    private bool omitRight = false;
    private bool omitLeft = false;

    protected override void Start() {
        GameManager.instance.AddEnemyToList(this); //have the enemy add itself to the list in game manager
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        base.Start();

        path = new Queue<Vector2>();
        explorePath = new Queue<Vector2>();
        perception = board.GetLength(0) / 2;

        SetInitDirection();
    }

    private void Update() {
        UpdateGrid();
    }

    private void SetInitDirection() {
        int[] direct = new int[4] { 0, 1, 2, 3 };

        int val = Random.Range(0, direct.Length);

        switch (val) {
            case 0: { lastMoveX = 1; lastMoveY = 0; break; }
            case 1: { lastMoveX = -1; lastMoveY = 0; break; }
            case 2: { lastMoveX = 0; lastMoveY = 1; break; }
            case 3: { lastMoveX = 0; lastMoveY = -1; break; }
        }
    }

    protected override void AttemptMove<T>(int xDir, int yDir) {
        //check to see if the enemy can move or not
        if (skipMove) {
            skipMove = false;
            return;
        }

        base.AttemptMove<T>(xDir, yDir);

        skipMove = true;
    }

    private Queue<Vector2> DeepCopyQueue(Queue<Vector2> v) {
        Queue<Vector2> cp = new Queue<Vector2>(v.Count);
        for (int i = 0; i < v.Count; i++) {
            cp.Enqueue(v.Dequeue());
        }
        return cp;
    }

    public int[,] GetBoard() {
        return board;
    }

    public int GetNewlyExploredInt(int[,] newBoard, int x, int y) {
        List<Vector2> neighbors = InitList(x, y);
        int max = 2 * GameManager.instance.boardScript.columns;

        int count = 0;
        foreach (Vector2 pair in neighbors) {
            int xVal = (int)pair.x; int yVal = (int)pair.y;
            if (xVal <= -1 || xVal >= max || yVal >= max || yVal <= -1) continue;
            else {
                if ((newBoard[xVal, yVal] != board[xVal, yVal]) && newBoard[xVal, yVal] != 4) {
                    newBoard[xVal, yVal] = board[xVal, yVal];
                    count++;
                }
            }
        }
        return count;
    }

    private bool ValidNeighbor(int x, int y, int[,] state) {
        if (x < 0 || y < 0 || y >= state.GetLength(0) || x >= state.GetLength(0))
            return false;
        else if (state[x, y] != 0) {
            return false;
        }
        else return true;
    }

    private int[,] Clone(int[,] board) {
        int[,] copy = new int[board.GetLength(0), board.GetLength(0)];

        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(0); j++) {
                copy[i, j] = board[i, j];
            }
        }
        return copy;
    }

    private int[,] Swap(Node n, int x, int y) {
        int[,] cp = Clone(n.state);

        int t = cp[x, y];
        cp[x, y] = cp[n.aix, n.aiy];
        cp[n.aix, n.aiy] = t;

        return cp;
    }

    private List<State> GetNeighbors(Node n) {
        List<State> neighbors = new List<State>();
        if (ValidNeighbor(n.aix - 1, n.aiy, n.state)) {
            State child = new State(Swap(n, n.aix - 1, n.aiy));
            child.newlyExploredTiles = GetNewlyExploredInt(child.board, n.aix-1, n.aiy);
            //print(child.newlyExploredTiles);
            neighbors.Add(child);
        }
        if (ValidNeighbor(n.aix + 1, n.aiy, n.state)) {
            State child = new State(Swap(n, n.aix + 1, n.aiy));
            child.newlyExploredTiles = GetNewlyExploredInt(child.board, n.aix + 1, n.aiy);
            //print(child.newlyExploredTiles);
            neighbors.Add(child);
        }
        if (ValidNeighbor(n.aix, n.aiy - 1, n.state)) {
            State child = new State(Swap(n, n.aix, n.aiy - 1));
            child.newlyExploredTiles = GetNewlyExploredInt(child.board, n.aix, n.aiy-1);
            //print(child.newlyExploredTiles);
            neighbors.Add(child);
        }
        if (ValidNeighbor(n.aix, n.aiy + 1, n.state)) {
            State child = new State(Swap(n, n.aix, n.aiy + 1));
            child.newlyExploredTiles = GetNewlyExploredInt(child.board, n.aix, n.aiy + 1);
            //print(child.newlyExploredTiles);
            neighbors.Add(child);
        }
        return neighbors;
    }

    //Function that initializes all unknown spaces to -1 
    private int[,] CreateInitDFSBoard() {
        int size = board.GetLength(0);
        int[,] tempBoard = new int[size - 1, size - 1];
        print(size);
        for (int i = 0; i < size - 1; i++) {
            for (int j = 0; j < size - 1; j++) {
                if (boolBoard[i, j]) {
                    tempBoard[i, j] = knownBoard[i, j];
                }
                else tempBoard[i, j] = -1;
            }
        }
        return tempBoard;
    }

    public void MoveEnemy() {
        //If we can see the player, We'll do Astar
        //Even if we already on the path to the last known location, if we still see him then it'll need to be updated
        //Also, don't need to worry about newInfo here as it's accounted for
        bool exploring = false;
        Vector2 move = new Vector2(0, 0); //Initalize the move vector
        if (2 > 3 && CanSeePlayer(lastMoveX, lastMoveY)) {
            int x = (int)target.position.x;
            int y = (int)target.position.y;
            lastSeenX = x; lastSeenY = y; //Store the last seen location

            AStar aStar = gameObject.AddComponent<AStar>();
            path = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                (int)transform.position.y, x, y));
            print("Regular Astar");
            DestroyImmediate(aStar);

            brokeExploring = true;
        }
        else { //enemy can't see player
            // haven't seen player, and there is no current explore path or we recently stopped chasing

            print(path.Count + " " + explorePath.Count);

            if (path.Count == 0 && (explorePath.Count == 0 || brokeExploring)) {
                exploring = true;
                print("Creating Explore Path");
                ExplorationAI(CreateInitDFSBoard(), (int)transform.position.x, (int)transform.position.y);
            }
            // haven't seen player, and there is a current path we are exploring AND we haven't stopped to chase a player yet
            else if (path.Count == 0 && explorePath.Count != 0 && !brokeExploring) {
                exploring = true;
            }
            else { //We are on the path to the last place the player was seen
                if (2>3 && newInfo && explorePath.Count == 0) { //We got new information in the maze as we moved, so we rerun AStar
                    AStar aStar = gameObject.AddComponent<AStar>();
                    //We don't know the player's current position, so we go to the last place he was seen
                    path = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                        (int)transform.position.y, lastSeenX, lastSeenY));

                    print("AStar with new Info");
                    DestroyImmediate(aStar);
                    brokeExploring = true;
                }
                //Else if we got no new information and we are still on the path to the player
            }
        }
        newInfo = false; //If the newInfo tag changed to true on the last move, change it back to false
        if (!exploring) {
            move = path.Dequeue();
        }
        else if (exploring) {
            move = explorePath.Dequeue();
        }
        //The enemy should never be standing still now, so if this is 0,0 then something's wrong
        lastMoveX = (int)move.x; lastMoveY = (int)move.y;
        AttemptMove<Player>((int)move.x, (int)move.y);
    }

    protected override void OnCantMove<T>(T component) {
        Player hitPlayer = component as Player; //cast the component to be player
        animator.SetTrigger("enemyAttack"); //have the enemy visually attack the player
        SoundManager.instance.RandomizeSFX(enemyAttack1, enemyAttack2); //play a random attack sound
        hitPlayer.LoseALife(playerDamage); //hit the player
    }

    public class Node {
        public Node parent;
        public int[,] state;
        public int aix;
        public int aiy;
        public int exploredTiles;

        public Node(int[,] state, int aix, int aiy, Node parent, int exploredTiles) {
            this.state = state;
            this.aix = aix;
            this.aiy = aiy;
            this.parent = parent;
            this.exploredTiles = exploredTiles;
        }

        public override string ToString() {
            return aix + ", " + aiy + " | " + exploredTiles;
        }
    }

    public class State {
        public int[,] board;
        public int newlyExploredTiles;

        public State(int[,] board) {
            this.board = board;
        }

        public override int GetHashCode() {
            int hash = 0;

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(0); j++) {
                    hash = hash * 31 + board[i, j];
                }
            }
            return hash;
        }
    }

    private Vector2 FindAi(State state) {
        for (int i = 0; i < state.board.GetLength(0); i++) {
            for (int j = 0; j < state.board.GetLength(0); j++) {
                if (state.board[i, j] == 4) return new Vector2(i, j);
            }
        }
        return new Vector2(-1, -1);
    }

    private void SetPath(Node n, Queue<Vector2> explorePath) {
        if (n.parent == null) {
            print(n);
            print("hey it returned null");
            return;
        }
        SetPath(n.parent, path);
        Vector2 vec = new Vector2(n.aix - n.parent.aix, n.aiy - n.parent.aiy);
        explorePath.Enqueue(vec);
    }

    private void ExplorationAI(int[,] rootBoard, int aix, int aiy) {
        Dictionary<State, Node> hm = new Dictionary<State, Node>();
        rootBoard[aix, aiy] = 4;
        Node root = new Node(rootBoard, aix, aiy, null, 0);
        Node max = ModifiedDFS(root, 3, hm);

        explorePath = new Queue<Vector2>(); //Empty the original queue
        SetPath(max, explorePath);
    }

    //It will ALWAYS finish it's path, which means we should cut at an early depth
    private Node ModifiedDFS(Node root, int depth, Dictionary<State, Node> hm) {
        Stack<Node> stack = new Stack<Node>();
        Node max = root;
        stack.Push(root);
        while (stack.Count != 0 && depth != 0) {
            Node parent = stack.Pop();
            if (parent.exploredTiles > max.exploredTiles) {
                max = parent;
            }
            foreach (State neighbor in GetNeighbors(parent)) {
                if (!hm.ContainsKey(neighbor)) {
                    Vector2 coord = FindAi(neighbor);
                    if ((int)coord.x == -1) print("Error: enemy returned coord: " + coord);
                    Node temp = new Node(neighbor.board, (int)coord.x, (int)coord.y, parent, parent.exploredTiles + neighbor.newlyExploredTiles);
                    stack.Push(temp);
                    hm[neighbor] = temp;
                }
            }
            depth -= 1;
        }
        print(max + " || " + root);
        return max;
    }

    public bool CanSeePlayer(int xDir, int yDir) {
        print("We are in the generic CSP method");
        if (GameManager.instance.isHiding) return false;
        if (xDir < 0 && target.position.x > transform.position.x) {
            return false;
        }
        else if (xDir > 0 && target.position.x < transform.position.x) {
            return false;
        }
        else if (Mathf.Abs(target.position.x - transform.position.x) > (perception + float.Epsilon)) {
            return false;
        }
        else if (yDir < 0 && target.position.y > transform.position.y) {
            return false;
        }
        else if (yDir > 0 && target.position.y < transform.position.y) {
            return false;
        }
        else if (Mathf.Abs(target.position.y - transform.position.y) > (perception + float.Epsilon)) {
            return false;
        }
        if ((int)transform.position.x == 0 && xDir == -1 ||
            (int)transform.position.x == board.GetLength(0) - 1 && xDir == 1 ||
            (int)transform.position.y == 0 && yDir == -1 ||
            (int)transform.position.y == board.GetLength(0) - 1 && yDir == 1) {
            omitRight = true;
        }
        else if ((int)transform.position.x == 0 && xDir == 1 ||
                 (int)transform.position.x == board.GetLength(0) - 1 && xDir == -1 ||
                 (int)transform.position.y == 0 && yDir == 1 ||
                 (int)transform.position.y == board.GetLength(0) - 1 && yDir == -1) {
            omitLeft = true;
        }
        return CanSeePlayer(xDir, yDir, 1);
    }

    public bool CanSeePlayer(int xDir, int yDir, int len) {
        // print("We are in recursive call number " + ++num);
        // print("xPos, yPos: " + ((int)transform.position.x)) + "  " + ((int)transform.position.y);
        // print("xDir, yDir: " + xDir + " " + yDir);
        // print("Len: " + len);
        // print("xTransform, yTransform: " + ((int)transform.position.x + (xDir * len)) + " " + ((int)transform.position.y + (yDir * len)));
        if (len == perception) {
            return false;
        }
        else if ((int)transform.position.x + (xDir * len) < 0 ||
                 (int)transform.position.x + (xDir * len) > board.GetLength(0) - 1 ||
                 (int)transform.position.y + (yDir * len) < 0 ||
                 (int)transform.position.y + (yDir * len) > board.GetLength(0) - 1) {
            return false;
        }
        else if (board[(int)transform.position.x + (xDir * len), (int)transform.position.y + (yDir * len)] == 1) {
            return false;
        }
        else {
            return IsThePlayerHere(xDir, yDir, len) || CanSeePlayer(xDir, yDir, len + 1);
        }
    }
   
    public bool IsThePlayerHere(int xDir, int yDir, int len) {
        if (omitRight && omitLeft) {
            return (int)transform.position.x + (xDir * len) == (int)target.position.x && (int)transform.position.y + (yDir * len) == (int)target.position.y;
        }
        else if (omitRight) {
            if ((yDir > 0 && board[(int)transform.position.x-1, (int)transform.position.y] == 1) || 
                (yDir < 0 && board[(int)transform.position.x+1, (int)transform.position.y] == 1) || 
                (xDir < 0 && board[(int)transform.position.x, (int)transform.position.y-1] == 1) ||
                (xDir > 0 && board[(int)transform.position.x, (int)transform.position.y+1] == 1)) {
                    omitLeft = true;
            }
            return ((int)transform.position.x + (xDir * len) == (int)target.position.x || 
                        (int)transform.position.x - yDir == (int)target.position.x) && 
                   ((int)transform.position.y + (yDir * len) == (int)target.position.y || 
                        (int)transform.position.y - xDir == (int)target.position.y);
        }
        else if (omitLeft) {
            if ((yDir < 0 && board[(int)transform.position.x-1, (int)transform.position.y] == 1) ||
                (yDir > 0 && board[(int)transform.position.x+1, (int)transform.position.y] == 1) ||
                (xDir > 0 && board[(int)transform.position.x, (int)transform.position.y-1] == 1) ||
                (xDir < 0 && board[(int)transform.position.x, (int)transform.position.y+1] == 1)) {
                    omitRight = true;
            }
            return ((int)transform.position.x + (xDir * len) == (int)target.position.x || 
                        (int)transform.position.x + yDir == (int)target.position.x) && 
                   ((int)transform.position.y + (yDir * len) == (int)target.position.y || 
                        (int)transform.position.y + xDir == (int)target.position.y);
        }
        else {
            if ((yDir > 0 && board[(int)transform.position.x-1, (int)transform.position.y] == 1) || 
                (yDir < 0 && board[(int)transform.position.x+1, (int)transform.position.y] == 1) || 
                (xDir < 0 && board[(int)transform.position.x, (int)transform.position.y-1] == 1) ||
                (xDir > 0 && board[(int)transform.position.x, (int)transform.position.y+1] == 1)) {
                omitLeft = true;
            }
                return ((int)transform.position.x + (xDir * len) == (int)target.position.x ||
                            (int)transform.position.x - yDir == (int)target.position.x) &&
                       ((int)transform.position.y + (yDir * len) == (int)target.position.y ||
                            (int)transform.position.y - xDir == (int)target.position.y);
        }
    }
}