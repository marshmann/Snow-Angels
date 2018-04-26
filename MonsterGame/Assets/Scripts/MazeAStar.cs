using System;
using System.Collections;
using System.Collections.Generic;

/* Description:
* ANode class represents node in the implicit graph
* State class is a holder class for the 2d array, and provides a GetHashCode() and Equals() method
* MinPQ class is for the frontier
* Dictionary is C#'s version of a Hashmap, and I use a Stack to store and return the path of Nodes.
* 
* A few notes:
* The AstarPickDirection method returns a Stack of ANode objects, representing the path (excluding start state).  Each ANode object has a "direction" property, which could be modified to represent the proper way of identifying how the enemy will move (atm it has no meaning and is never assigned).
* 
* ATM, most properties are private with Get methods - I may change this in future just for ease, but will keep as it is for now.
* 
* */
public class MazeAStar : UnityEngine.MonoBehaviour {

    public int[,] CloneArr(int[,] arr) {
        int length = arr.GetLength(0);
        int[,] clone = new int[length, length];

        for (int i = 0; i < length; i++) {
            for (int j = 0; j < length; j++) {
                clone[i, j] = arr[i, j];
            }
        }
        return clone;
    }

    public int[,] Swap(ANode n, int x, int y) {
        int xSelf = n.XSelf(); int ySelf = n.YSelf();
        int[,] cp = CloneArr(n.GetState().GetState());

        int t = cp[xSelf, ySelf];
        cp[xSelf, ySelf] = cp[x, y];
        cp[x, y] = t;

        return cp;
    }

    public List<State> GetNeighbors(ANode n) {
        int x = n.XSelf(), y = n.YSelf();
        List<State> list = new List<State>(4);
        int[,] state = n.GetState().GetState();
        if (ValidNeighbor(x - 1, y, state)) {
            State t = new State(Swap(n, x - 1, y), x - 1, y, n.xTarget, n.yTarget);
            list.Add(t);
        }
        if (ValidNeighbor(x + 1, y, state)) {
            State t = new State(Swap(n, x + 1, y), x - 1, y, n.xTarget, n.yTarget);
            list.Add(t);
        }
        if (ValidNeighbor(x, y - 1, state)) {
            State t = new State(Swap(n, x, y - 1), x, y - 1, n.xTarget, n.yTarget);
            list.Add(t);
        }
        if (ValidNeighbor(x, y + 1, state)) {
            State t = new State(Swap(n, x, y + 1), x, y + 1, n.xTarget, n.yTarget);
            list.Add(t);
        }
        return list;
    }

    public bool ValidNeighbor(int y, int x, int[,] board) {
        if (y >= board.GetLength(0) || x >= board.GetLength(0) || x <= 0 || y <= 0)
            return false;

        if (board[x, y] != 0) 
            return false;
     
        return true;
    }

    // Returns stack of actions taken to reach goal state (with first action to take on top)
    // ***NOTE: The other four parameters are just here for testing purposes.
    public Stack AstarPickDirection(int[,] board, int xSelf, int ySelf, int xTarget, int yTarget) {
        board[xTarget, yTarget] = 5;
        board[xSelf, ySelf] = 4;

        State state = new State(board, xSelf, ySelf, xTarget, yTarget);

        // Initializing empty frontier and explored hashmap
        MinPQ<ANode> frontier = new MinPQ<ANode>();                           // frontier
        Dictionary<State, ANode> hashmap = new Dictionary<State, ANode>();    // hashmap (explored nodes)

        // Starting state (state of maze reflected through int[,] state)
        ANode start = new ANode(state, null, xSelf, ySelf, xTarget, yTarget);
        hashmap[state] = start;      // Add starting node to explored set

        frontier.Add(start);
        int count = 0;
        // Cycle through nodes in the frontier
        while (true) {
            // Check if the frontier is empty, failure if so
            if (frontier.IsEmpty()) {
                print(count);
                return null;  // Failure
            }
           
            // Remove node with lowest GetCost() value from frontier
            ANode node = frontier.Remove();
            state = node.GetState();    // Set state 2d array to be that of the current node
           
            // Check if node's tilemap represents the agent as being next to the player.
            if (node.IsGoal()) {
                Stack path = new Stack();
                // Traverse back up the path, and stop at the 2nd node on the path (null -> startnode -> 2nd node)
                while (node.GetParent().GetParent() != null) {
                    path.Push(node);
                    node = node.GetParent();
                }

                print(path);
                return path;  // Return the direction of the step taken to get to this node.
            }
            node.SetExplored();  // Set node to be explored

            foreach(State neighbor in GetNeighbors(node)) {  // Check possible moves
                ANode n = new ANode(neighbor, node, neighbor.xSelf, neighbor.ySelf, xTarget, yTarget);
                if (hashmap.ContainsKey(neighbor)) {
                    ANode old = hashmap[neighbor];
                    if (old.InFrontier() && (old.GetCost() > n.GetCost())) {
                        hashmap[neighbor] = n;    
                        frontier.Update(hashmap[neighbor]);
                    }

                    print("duplicate");
                    // Else the new cost is lower, so do nothing and move on.
                }
                else if(!hashmap.ContainsKey(neighbor)) { // State does not exist in hashmap, so add it to hashmap and frontier.

                    if (count++ < 10) {
                        GameManager.instance.PrintIt<int>(state.GetState());
                        print("------------");
                    }

                    hashmap[neighbor] = n;
                    frontier.Add(n);
                }
                // Else state has already been visited (in hash and no longer in the frontier)
            }
        }
    }
}

public class State {
    public int xSelf, ySelf, xTarget, yTarget;
    public int[,] state;
    public State(int[,] state, int xs, int ys, int xt, int yt) {
        this.state = state;
        xSelf = xs;
        ySelf = ys;
        xTarget = xt;
        yTarget = yt;
    }

    public int[,] GetState() { return state; }

    // Deep copy
    public int[,] CopyState() {
        int[,] ret = new int[state.GetLength(0), state.GetLength(1)];
        for (int r = 0; r < state.GetLength(0); r++)
            for (int c = 0; c < state.GetLength(1); c++)
                ret[r, c] = state[r, c];
        return ret;
    }

    public override int GetHashCode() {
        return (xTarget * 4) + (yTarget * 3) + (xSelf * 2) + (ySelf);
    }

    public override Boolean Equals(Object o) {
        State t = (State)o;
        for (int r = 0; r < state.GetLength(0); r++)
            for (int c = 0; c < state.GetLength(1); c++)
                if (state[r, c] != t.state[r, c])
                    return false;
        return true;
    }
}

public class ANode : IDenumerable, IComparer  {
    private int direction;  // step taken in direction to get to this node's state
    private int num;        // for MinPQ reference
    private ANode parent;    // ref to previous node
    private int cost, pathCost, manHatDist; // cost variables
    private State state;       // 2D Array of state
    public int xTarget, yTarget;   // coords in state of target (player for enemy, exit for player?)
    public int xSelf, ySelf;       // coords of agent
    private Boolean inFrontier;

    public ANode(State state, ANode parent, int xSelf, int ySelf, int xTarget, int yTarget)
    {
        this.state = state;
        this.parent = parent;
        this.xSelf = xSelf;
        this.ySelf = ySelf;
        this.xTarget = xTarget;
        this.yTarget = yTarget;

        inFrontier = true;
        pathCost = CalcPathCost();
        manHatDist = CalcManhatDist();
        cost = manHatDist + pathCost;
    }
    
    // This may be unnecessary, as the int[,] is now stored inside State object
    public int[,] CopyState(int[,] state)
    {
        int[,] ret = new int[state.GetLength(0), state.GetLength(1)];
        for (int r = 0; r < state.GetLength(0); r++)
            for (int c = 0; c < state.GetLength(1); c++)
                ret[r, c] = state[r, c];
        return ret;
    }

    // Finds Manhattan Distance between enemy's position in the maze to player's position
    int CalcManhatDist()
    {
        return Math.Abs(xTarget - xSelf) + Math.Abs(yTarget - ySelf);
    }

    int CalcPathCost()
    {
        return (parent == null) ? 0 : parent.pathCost + 1;
    }

    public Boolean IsGoal()
    {
        if (xSelf == xTarget && ySelf == yTarget)
            return true;
        return false;
    }
           
    public int GetDirection() { return direction; }
    public int GetCost() { return cost; }
    public ANode GetParent() { return parent; }
    public State GetState() { return state; }
    public Boolean InFrontier() { return inFrontier; }
    public void SetExplored() { inFrontier = false; }
    public void SetNumber(int n) { num = n; }      // minpq stuff
    public int GetNumber() { return num; }         // ^
    public int XSelf() { return xSelf; }
    public int YSelf() { return ySelf; }
    public int XTarget() { return xTarget; }
    public int YTarget() { return yTarget; }

    public override Boolean Equals(Object o)
    {
        ANode n = (ANode)o;
        if (state.Equals(n.GetState()) && cost == n.GetCost())
            return true;
        return false;
        }

    int IComparer.Compare(Object o1, Object o2)
    {
        ANode a = (ANode)o1;
        ANode b = (ANode)o2;
        if (a.GetCost() > b.GetCost())
            return 1;
        else if (a.GetCost() < b.GetCost())
            return -1;
        else return 0;
    }
}