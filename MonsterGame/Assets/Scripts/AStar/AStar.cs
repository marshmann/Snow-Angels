//Author: Nicholas Marshman
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {
    private int xTarget; //the x coordinate of the target location
    private int yTarget; //the y coordinate of the target location

    private Queue<Vector2> path; //the generated path from the AI to the Target

    //Function that checks to see if the AI is on it's target or not
    private bool IsGoal(Node n) {
        if (n.aix == xTarget && n.aiy == yTarget) return true;
        else return false;
    }

    //Calculate ManHatthen distance from enemy to player, and use that as the heuristic
    private int Heuristic(Node n) {
        return Mathf.Abs(n.aix - xTarget) + Mathf.Abs(n.aiy - yTarget);
    }

    //Check to see if the neighbors are "valid"
    //Valid spaces are floor tiles and the tile the player is on, not walls or tiles that are out of bounds.
    private bool ValidNeighbor(int x, int y, int[,] state) {
        if (x < 0 || y < 0 || y >= state.GetLength(0) || x >= state.GetLength(0))
            return false;
        else if (state[x, y] == 5) { //if the tile is where the player is at
            return true; //return true
        }
        else if (state[x, y] != 0 && state[x,y] != 3) { //if the state isn't a floor/exit tile
            return false; //we can't walk on it
        }
        else return true;
    }

    //Deep copy a board state
    private int[,] Clone(int[,] board) {
        int[,] copy = new int[board.GetLength(0), board.GetLength(0)];

        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(0); j++) {
                copy[i, j] = board[i, j];
            }
        }
        return copy;
    }

    //A method to swap the current location of the AI with the location he is moving
    private int[,] Swap(Node n, int x, int y) {
        int[,] cp = Clone(n.state);

        int t = cp[x, y];
        cp[x, y] = cp[n.aix, n.aiy];
        cp[n.aix, n.aiy] = t;

        return cp;
    }

    //Get all the possible neighboring spaces of the AI, first checking if they are valid.
    private List<State> GetNeighbors(Node n) {
        List<State> neighbors = new List<State>();
        if (ValidNeighbor(n.aix - 1, n.aiy, n.state)) {
            State child = new State(Swap(n, n.aix - 1, n.aiy));
            neighbors.Add(child);
        }
        if (ValidNeighbor(n.aix + 1, n.aiy, n.state)) {
            State child = new State(Swap(n, n.aix + 1, n.aiy));
            neighbors.Add(child);
        }
        if (ValidNeighbor(n.aix, n.aiy - 1, n.state)) {
            State child = new State(Swap(n, n.aix, n.aiy - 1));
            neighbors.Add(child);
        }
        if (ValidNeighbor(n.aix, n.aiy + 1, n.state)) {
            State child = new State(Swap(n, n.aix, n.aiy + 1));
            neighbors.Add(child);
        }
        return neighbors;
    }

    //Recursively set the path to the goal
    private void SetPath(Node n, Queue<Vector2> path) {
        if (n.parent == null) return;
        SetPath(n.parent, path);
        Vector2 vec = new Vector2(n.aix - n.parent.aix, n.aiy - n.parent.aiy);
        path.Enqueue(vec);
    }

    //Find the location of the AI on the board, since it's not stored in memory
    //Note: AStar is memory intensive, not time intensive. This is why we are locating it
    //rather than storing it.
    private Vector2 FindAi(State state) {
        for (int i = 0; i < state.board.GetLength(0); i++) {
            for (int j = 0; j < state.board.GetLength(0); j++) {
                if (state.board[i, j] == 4) return new Vector2(i, j);
            }
        }
        return new Vector2(-1, -1);
    }

    //The function that will run the AStar Algorithm
    public Queue<Vector2> DoAStar(int[,] board, int aix, int aiy, int xTarget, int yTarget) {
        path = new Queue<Vector2>(); //init path to the goal
        this.xTarget = xTarget; this.yTarget = yTarget; //set the target location

        MinPQ<Node> minPQ = new MinPQ<Node>(); //create a MinPQ

        //Note: 4 on the board is the AI location, 5 is target location
        board[xTarget, yTarget] = 5; //initalize the location of the last viewed player
        board[aix, aiy] = 4; //initalize enemy location

        //Initialize the root node/state of the board
        Node root = new Node(board, aix, aiy, null) {
            g = 0,
            inFrontier = true
        };

        //calculate the heursistic and store it
        root.h = Heuristic(root);
        minPQ.Add(root); //add the root to the PQ

        //In Java, we use a HashMap to do AStar.  C# doesn't have a HashMap explicitly
        //A dictionary is functionally the same though!
        Dictionary<State, Node> hashmap = new Dictionary<State, Node>();
        hashmap[new State(board)] = root; //C# dictionaries use the same syntax as an array

        //While the min PQ isn't empty
        while (!minPQ.IsEmpty()) {
            Node data = minPQ.Remove(); //get the node with the lowest priority
            data.inFrontier = false; //remove it from the frontier

            //check if it's a goal state
            if (IsGoal(data)) {
                SetPath(data, path); //if it is a goal, then set the path and return
                return path;
            }

            //if it's not a goal
            foreach (State neighbor in GetNeighbors(data)) { //Get the neighbors of the node
                Vector2 coord = FindAi(neighbor); //get the location of the AI
                //Create a new node with the AI's location
                Node n = new Node(neighbor.board, (int)coord.x, (int)coord.y, data) {
                    g = data.g + 1, //initalize the g value to be the parent's + 1
                    inFrontier = true //set it to be in the frontier
                };

                n.h = Heuristic(n); //Calculate the heuristic and store it

                //If the neighbor's state is a new state that hasn't been seen before
                if (!hashmap.ContainsKey(neighbor)) {
                    n.parent = data; //set it's parent
                    minPQ.Add(n); //add it to the pq
                    hashmap[neighbor] = n; //add it to the hashmap
                }
                //same state already exists
                else if (hashmap.ContainsKey(neighbor)) {
                    Node old = hashmap[neighbor]; //get the old node from the hashmap
                    //if old node is in the frontier and the new node has a lower f value
                    if (old.inFrontier && ((old.g + old.h) > (n.h + n.g))) {
                        //Update the old node's values
                        old.g = n.g; //set the g to be the new node's
                        old.h = n.h; //set the h to be the new hode's
                        old.parent = data; //set the parent to be the current node
                        hashmap[neighbor] = old; //place the node back into the hashmap
                        minPQ.Update(hashmap[neighbor]); //update the priority in the MinPQ
                    }
                }
                //Else the state has already been visited (in hash and no longer in frontier)
            }
        }
        return null; //Failure
    }
}
