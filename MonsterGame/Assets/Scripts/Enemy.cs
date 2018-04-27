//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
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
    private bool onPath = false;
    private int lastSeenX = 0;
    private int lastSeenY = 0;

    //Randomize these in start
    private int lastMoveX; 
    private int lastMoveY;

    protected override void Start() {
        GameManager.instance.AddEnemyToList(this); //have the enemy add itself to the list in game manager
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        base.Start();

        path = new Queue<Vector2>();
        perception = board.GetLength(0)/2;

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

        print(lastMoveX + ", " +lastMoveY);
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
        for(int i = 0; i < v.Count; i++) {
            cp.Enqueue(v.Dequeue());
        }
        return cp;
    }

    public int[,] GetBoard() {
        return board;
    }

    public void MoveEnemy() {
        //If we can see the player, We'll do Astar
        //Even if we already on the path to the last known location, if we still see him then it'll need to be updated
        //Also, don't need to worry about newInfo here as it's accounted for
        bool update = false;
        Node start, goal;
        Vector2 move = new Vector2(0,0); //Initalize the move vector
        if (CanSeePlayer(lastMoveX, lastMoveY)) { 
            int x = (int)target.position.x;
            int y = (int)target.position.y;
            lastSeenX = x; lastSeenY = y; //Store the last seen location

            AStar aStar = gameObject.AddComponent<AStar>();
            path = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                (int)transform.position.y, x, y));

            print("Hello1");

            DestroyImmediate(aStar);
            print(path.Peek());
            update = true;
        }
        else{
            if (path.Count == 0) {
                //Call Dave's Code to Explore, should return a Vector2
                // we need to figure out how to do the IDDFS now here
                print("We gotta call Dave's code");
            }
            else { //We are on the path to the last place the player was seen
                if (newInfo) { //We got new information in the maze as we moved, so we rerun AStar
                    AStar aStar = gameObject.AddComponent<AStar>();
                    //We don't know the player's current position, so we go to the last place he was seen
                    path = DeepCopyQueue(aStar.DoAStar(knownBoard, (int)transform.position.x,
                        (int)transform.position.y, lastSeenX, lastSeenY));

                    print("Hello2");
                    DestroyImmediate(aStar);
                }
                update = true;
            }
        }
        print(move);
        newInfo = false; //If the newInfo tag changed to true on the last move, change it back to false
        if (update) {
            move = path.Dequeue();
            lastMoveX = (int)move.x; lastMoveY = (int)move.y;
        }
        AttemptMove<Player>((int)move.x, (int)move.y);
    }

    protected override void OnCantMove<T>(T component) {
        Player hitPlayer = component as Player; //cast the component to be player
        animator.SetTrigger("enemyAttack"); //have the enemy visually attack the player
        SoundManager.instance.RandomizeSFX(enemyAttack1, enemyAttack2); //play a random attack sound
        hitPlayer.LoseALife(playerDamage); //hit the player
    }

    // ref: codeproject.com/Articles/203828/AI-Simple-Implementation-of-Uninformed-Search-Stra
    public class Node {
        public int depth;
        public int state;
        public int cost;
        public Node parent;

        // Parent node which has depth: 0 and parent: null
        public Node(int state) {
            this.state = state;
            parent = null;
            cost = 0;
        }

        public Node(int state, Node parent) {
            this.state = state;
            this.parent = parent;
            depth = (parent == null) ? 0 : parent.depth + 1;
        }

        public Node(int state, Node parent, int cost) {
            this.state = state;
            this.parent = parent;
            this.cost = cost;
            depth = (parent == null) ? 0 : parent.depth + 1;
        }

        //Before making edits, the node class DID NOT END HERE.
        //This is a big deal, so make sure it's correct.
    }

    public class GetSucc {
        public ArrayList GetSuccessor(int state) {
            ArrayList result = new ArrayList {
                2 * state + 1,
                2 * state + 2
            };
            return result;
        }

        public ArrayList GetSuccessor(int state, Node parent) {
            ArrayList result = new ArrayList();
            Test t = new Test();
            // Currently, the cost function for the nodes is random
            result.Add(new Node(2 * state + 1, parent, Random.Range(1, 100) + parent.cost));
            result.Add(new Node(2 * state + 2, parent, Random.Range(1, 100) + parent.cost));
            result.Sort(t);
            return result;
        }
    }

    public class Test : IComparer {
        public int Compare(object x, object y) {
            int val1 = ((Node)x).cost;
            int val2 = ((Node)y).cost;
            return val1 <= val2 ? 1 : 0;
        }
    }

    public static void Iterative_Deepening_Search(Node Start, Node Goal) {
        bool Cutt_off = false;
        int depth = 0;
        while(Cutt_off == false) {
            print("Search Goal at Depth {0} " + depth);
            Depth_Limited_Search(Start, Goal, depth,ref Cutt_off);
            print("-----------------------------");
            depth++;
        }     
    }

    public static void Depth_Limited_Search(Node Start, Node Goal, int depth_Limite,ref bool Cut_off) {
        GetSucc x = new GetSucc();
        ArrayList children = new ArrayList();
        Stack Fringe = new Stack();
        Fringe.Push(Start);
        while (Fringe.Count != 0) {
            Node Parent = (Node)Fringe.Pop();
            print("Node {0} Visited " + Parent.state);
            // Console.ReadKey();
            if (Parent.state == Goal.state) {
                print("Find Goal   " + Parent.state);
                Cut_off = true;
                break;
            } 
            if (Parent.depth == depth_Limite) {
                continue;
            }
            else {
                children = x.GetSuccessor(Parent.state);
                for (int i = 0; i < children.Count; i++) {
                    int State = (int)children[i];
                    Node Tem = new Node(State, Parent);
                    Fringe.Push(Tem);

                } 
            } 
        } 
    } 

    public bool CanSeePlayer(int xDir, int yDir) {
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
        //return CanSeePlayer(xDir, yDir, 1);

        return true;
    }

    public bool CanSeePlayer(int xDir, int yDir, int len) {
        if (len == perception) {
            return false;
        }
        else if (board[(int)transform.position.x + (xDir * len), (int)transform.position.y + (yDir * len)] == 1) {
            return false;
        }
        else {
            return IsThePlayerHere(xDir, yDir, len) || CanSeePlayer(xDir, yDir, len++);
        }
    }

    public bool IsThePlayerHere(int xDir, int yDir, int len) {
        return xDir != 0 ?
           (Mathf.Abs(target.position.x - transform.position.x) == (len + float.Epsilon)):
           (Mathf.Abs(target.position.y - transform.position.y) == (len + float.Epsilon));
    }
}