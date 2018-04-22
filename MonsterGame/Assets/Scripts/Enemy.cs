//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
//In addition: Kevin Bechman and Dave Kelly, due to this class being where the AI mostly is
using UnityEngine;

public class Enemy : MovingObject {
    public int playerDamage; //Amount of food damage the player loses when hit by this enemy
    //The player damage value is set in Unity as a variable in the Enemy1 and Enemy2 prefab under the Enemy component

    private Animator animator; //the animator for the enemy
    private Transform target; //use to store player position (where the enemies will move toward)
    private bool skipMove; //enemies move every other turn

    private int perception = GameManager.instance.columns; // Hack-y version right now where I'm just hard coding. Need to update to reference game board dimensions.

    //Below are containers for the audio effects
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;

    protected override void Start() {
        GameManager.instance.AddEnemyToList(this); //have the enemy add itself to the list in game manager
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
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

    public void MoveEnemy() {
        int xDir = 0;
        int yDir = 0;

        /*  If the player's x coordinate and the enemy's x cordinate are the same 
         * (or extremely close, as represented by float.Epsilon)
         * Then we'll move in the y direction toward the player
         * Otherwise, we'll move in the x direction
         */
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon) {
            //If the player's y value is greater than the y value, he's above the enemy (so move up 1)
            //else he'll be below the enemy (so move down 1)
            yDir = target.position.y > transform.position.y ? 1 : -1;
        }
        else
            xDir = target.position.x > transform.position.x ? 1 : -1; //same as above but for x values

        AttemptMove<Player>(xDir, yDir); //Attempt to move toward the player, assuming the enemy might run into the player
    }

    protected override void OnCantMove<T>(T component) {
        Player hitPlayer = component as Player; //cast the component to be player
        animator.SetTrigger("enemyAttack"); //have the enemy visually attack the player
        SoundManager.instance.RandomizeSFX(enemyAttack1, enemyAttack2); //play a random attack sound
        hitPlayer.LoseALife(playerDamage); //hit the player
    }



    // ref: codeproject.com/Articles/203828/AI-Simple-Implementation-of-Uninformed-Search-Stra
    private class Node {
        public int depth;
        public int state;
        public int cost;
        public Node parent;

        // Parent node which has depth: 0 and parent: null
        public Node(int state) {
            this.state = state;
            this.parent = null;
            this.cost = 0;
        }

        pubilc Node(int state) {
            this.state = state;
        }

        public Node(int state, Node parent) {
            this.state = state;
            this.parent = parent;
            this.depth = (parent == null) ? 0 : parent.depth + 1;
        }

        public Node(int state, Node parent, int cost) {
            this.state = state;
            this.parent = parent;
            this.depth = (parent == null) ? 0 : parent.depth + 1;
            this.cost = cost;
        }
    }

    public class getSucc {

        public ArrayList getSuccessor(int state) {
            ArrayList result = new ArrayList();
            result.add(2 * state + 1);
            result.add(2 * state + 2);
            return result;
        }

        public ArrayList getSuccessor(int state, Node parent) {
            ArrayList result = new ArrayList();
            Random n = new Random();
            Test t = new Test();
            // Currently, the cost function for the nodes is random
            result.add(new Node(2 * state + 1, parent, n.next(1, 100) + parent.cost));
            result.add(new Node(2 * state + 2, parent, n.next(1, 100) + parent.cost));
            result.sort(t);
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

    public void depthLimitedSearch(Node start, Node goal, int depthLimit) {
        getSucc x = new getSucc();
        ArrayList children = new ArrayList();
        Stack fringe = new Stack();
        fringe.push(start);
        while (fringe.count != 0) {
            Node parent = (Node)fringe.pop();
            if (parent.state == goal.state) {
                break;
            }
            if (parent.depth = depthLimit) {
                continue;
            } else {
                children = x.getSuccessor(parent.state);
                for (int i = 0; i < children.count; i++) {
                    int state = (int)children[i];
                    Node temp = new Node(state, parent);
                    fringe.push(temp);
                }
            }
        }
    }

    // Input: Vector of where the AI is looking
    public void updateMap(int xDir, int yDir) {
        if (canSeePlayer(xDir, yDir)) {
            // Make note of the current location of the ai
            // Swap to the astar AI
        } else {
            // We do the exploration AI, which is depth limited search in this implementation
            // We need to add the nodes for start and goal states
            Node start = base.board[transform.position.x, transform.position.y];
            Node goal = base.board[target.position.x, target.position.y];
            int depth = perception;
            depthLimitedSearch(start, goal, depth);
        }
    }

    public bool canSeePlayer(int xDir, int yDir) {
        if (xDir < 0 && target.position.x > transform.position.x) {
            return false;
        } else if (xDir > 0 && target.position.x < transform.position.x) {
            return false;
        } else if (Mathf.Abs(target.position.x - transform.position.x) > (this.perception + float.Epsilon)) {
            return false;
        } else if (yDir < 0 && target.position.y > transform.position.y) {
            return false;
        } else if (yDir > 0 && target.position.y < transform.position.y) {
            return false;
        } else if (Mathf.Abs(target.position.y - transform.position.y) > (this.perception + float.Epsilon)) {
            return false;
        }
        return canSeePlayer(xDir, yDir, 1);
    }

    public bool canSeePlayer(int xDir, int yDir, int len) {
        if (len == this.perception) {
            return false;
        } else if (xDir != 0 && isThisTileAWall("x", xDir, yDir, len)) {
            return false;
        } else if (yDir != 0 && isThisTileAWall("y", xDir, yDir, len)) {
            return false;
        } else {
            return isThePlayerHere(xDir, yDir, len) || canSeePlayer(xDir, yDir, len++);
        }
    }

    // There is a possibility that this is wrong if xDir/yDir is negative. I will have to test it.
    public bool isThisTileAWall(char c, int x, int y, int len) {
        return c == "x" ?
            base.board[transform.position.x + len, transform.position.y] == 1 ? true : false:
            base.board[transform.position.x, transform.position.y + len] == 1 ? true : false;
    }

    public bool isThePlayerHere(int xDir, int yDir, int len) {
        return xDir != 0 ? 
            (Mathf.Abs(target.position.x - transform.position.x) == (len + float.Epsilon)):
            (Mathf.Abs(target.position.y - transform.position.y) == (len + float.Epsilon));
    }
}
