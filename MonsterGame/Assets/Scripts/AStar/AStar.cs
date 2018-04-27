//Author: Nicholas Marshman with a good amount of aid from Kevin Bechman
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {
    private int initAix;
    private int initAiy;
    private int xTarget;
    private int yTarget;

    private Queue<Vector2> path;

    private bool IsGoal(Node n) {
        if (n.aix == xTarget && n.aiy == yTarget) return true;
        else return false;
    }

    private int Heuristic(Node n) { //ManHat Distance from enemy to player 
        return Mathf.Abs(n.aix - xTarget) + Mathf.Abs(n.aiy - yTarget);
    }

    private bool ValidNeighbor(int x, int y, int[,] state) {
        if ( x < 0 || y < 0 || y >= state.GetLength(0) || x >= state.GetLength(0))
            return false;
        else if(state[x,y] == 5) {
            return true;
        }
        else if (state[x, y] != 0) {
            return false;
        }
        else return true;
    }

    private int[,] Clone(int[,] board) {
        int[,] copy = new int[board.GetLength(0), board.GetLength(0)];

        for(int i = 0; i < board.GetLength(0); i++) {
            for(int j = 0; j < board.GetLength(0); j++) {
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
        if(ValidNeighbor(n.aix - 1, n.aiy, n.state)) {
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

    private void SetPath(Node n, Queue<Vector2> path) {
        if (n.parent == null) return;
        SetPath(n.parent, path);
        Vector2 vec = new Vector2(n.aix - n.parent.aix, n.aiy - n.parent.aiy);
        path.Enqueue(vec);
    }

    private Vector2 FindAi(State state) {
        for(int i = 0; i < state.board.GetLength(0); i++) {
            for( int j = 0; j < state.board.GetLength(0); j++) {
                if (state.board[i, j] == 4) return new Vector2(i,j);
            }
        }
        return new Vector2(-1, -1);
    }

    public Queue<Vector2> DoAStar(int[,] board, int aix, int aiy, int xTarget, int yTarget) {
        path = new Queue<Vector2>();
        this.xTarget = xTarget; this.yTarget = yTarget;

        MinPQ<Node> minPQ = new MinPQ<Node>();

        board[xTarget, yTarget] = 5; //initalize the location of the last viewed player
        board[aix, aiy] = 4; //initalize enemy location

        Node root = new Node(board, aix, aiy, null) {
            g = 0, inFrontier = true
        };

        root.h = Heuristic(root);
        minPQ.Add(root);

        //This is the closest data structure to a java hashmap in C#, initialize it with the root
        Dictionary<State, Node> hashmap = new Dictionary<State, Node>();
        hashmap[new State(board)] = root; //C# dictionaries use the same syntax as an array

        while (!minPQ.IsEmpty()) {
            Node data = minPQ.Remove();
            data.inFrontier = false;

            if (IsGoal(data)) {
                SetPath(data, path);
                return path;
            }

            foreach(State neighbor in GetNeighbors(data)){
                Vector2 coord = FindAi(neighbor);
                if ((int)coord.x == -1) { //Error in finding ai on board
                    print("Error: enemy returned coord: " + coord);
                    GameManager.instance.PrintIt<int>(neighbor.board);
                    return null;
                } 

                Node n = new Node(neighbor.board, (int)coord.x, (int)coord.y, data) {
                    g = data.g + 1, inFrontier = true
                };

                n.h = Heuristic(n);
                //If the neighbor's state is a new state that hasn't been seen before
                if (!hashmap.ContainsKey(neighbor)) {
                    n.parent = data;
                    minPQ.Add(n);
                    hashmap[neighbor] = n;
                }
                //same state already exists
                else if (hashmap.ContainsKey(neighbor)) {
                    Node old = hashmap[neighbor];
                    //if old node is in the frontier and the new node has a lower f value
                    if(old.inFrontier && ((old.g+old.h) > (n.h + n.g))) {
                        //Update old's values
                        old.g = n.g;
                        old.h = n.h;
                        old.parent = data;
                        hashmap[neighbor] = old;
                        minPQ.Update(hashmap[neighbor]);
                    }
                }
                //Else the state has already been visited (in hash and no longer in frontier)
            }
        }
        return null; //Failure
    }

}
