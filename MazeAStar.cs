using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Description:
 * ANode class represents node in the implicit graph
 * Tiles class is a holder class for the 2d array, and provides a GetHashCode() and Equals() method
 * MinPQ class is for the frontier
 * Dictionary is C#'s version of a Hashmap, and I use a Stack to store and return the path of Nodes.
 * 
 * A few notes:
 * The AstarPickDirection method returns a Stack of ANode objects, representing the path (excluding start state).  Each ANode object has a "direction" property, which could be modified to represent the proper way of identifying how the enemy will move (atm it has no meaning and is never assigned).
 * 
 * ATM, most properties are private with Get methods - I may change this in future just for ease, but will keep as it is for now.
 * 
 * */

namespace MazeAStar
{
    class MazeAStar
    {
        // Testing of MinPQ
        public class Thing : IComparer, IDenumerable
        {
            public int i;
            int num;
            public Thing(int i) { this.i = i; }
            public override Boolean Equals(Object obj)
            {
                Thing t = (Thing)obj;
                if (i == t.i)
                    return true;
                return false;
            }
            public Thing GetCopy() { return new Thing(i); }
            public void SetNumber(int n) { num = n; }      // minpq stuff
            public int GetNumber() { return num; }         // ^
            int IComparer.Compare(Object o1, Object o2)
            {
                Thing t1 = (Thing)o1;
                Thing t2 = (Thing)o2;
                if (t1.i > t2.i)
                    return 1;
                if (t1.i < t2.i)
                    return -1;
                return 0;
            }
            public override String ToString() { return Convert.ToString(i) + "("+num+")"; }
        }

        static void Main(string[] args)
        {
            /*
            MinPQ<Thing> pq = new MinPQ<Thing>();
            Thing a = new Thing(10);
            Thing b = new Thing(20);
            Thing c = new Thing(30);
            Thing d = new Thing(70);
            Thing e = new Thing(60);
            Thing f = new Thing(-20);
            Thing g = new Thing(190);

            pq.Add(a);
            pq.Add(b);
            pq.Add(c);

            Thing z = c.GetCopy();      // Deep copy of 'c' object
            z.i = 15;                   // Change inner field
            z.SetNumber(c.GetNumber()); // Set number of new object to be that of old object
            pq.Update(z);               // Call update (swim, then sink)

            pq.Add(d);
            pq.Add(e);
            pq.Add(f);

            Thing y = e.GetCopy();
            y.i = 25;
            y.SetNumber(e.GetNumber());
            pq.Update(y);

            pq.Add(g);

            Thing x = g.GetCopy();
            x.i = -15;
            x.SetNumber(g.GetNumber());
            pq.Update(x);

            pq.Remove();

            while (!pq.IsEmpty())
                Console.WriteLine(pq.Remove());
            */

            int[,] t1 = new int[,]
            { 
                { 1, 1, 1, 1, 1 },
                { 1, 0, 0, 0, 1 },
                { 1, 0, 1, -1, 1 },
                { 1, 1, 1, 0, 1 },
                { 1, 1, 1, 1, 1 },
            };
            Tiles tiles1 = new Tiles(t1, 3, 2, 1, 2);

            int[,] t2 = new int[,]
            {
                { 1, 1, 1, 1, 1 },
                { 1, 0, 0, -1, 1 },
                { 1, 0, 1, 0, 1 },
                { 1, 1, 1, 0, 1 },
                { 1, 1, 1, 1, 1 },
            };
            Tiles tiles2 = new Tiles(t2, 3, 2, 1, 2);
            Console.WriteLine(tiles1.Equals(tiles2));
        }

        

        // Returns stack of actions taken to reach goal state (with first action to take on top)
        // ***NOTE: The other four parameters are just here for testing purposes.
        static Stack AstarPickDirection(Tiles tiles, int xSelf, int ySelf, int xTarget, int yTarget)
        {
            // Initializing empty frontier and explored hashmap
            MinPQ<ANode> frontier = new MinPQ<ANode>();                           // frontier
            Dictionary<Tiles, ANode> hashmap = new Dictionary<Tiles, ANode>();    // hashmap (explored nodes)

            // Starting state (state of maze reflected through int[,] tiles)
            ANode start = new ANode(tiles, null, xSelf, ySelf, xTarget, yTarget);
                /* Again, just for testing purposes:
                transform.position.x,
                transform.position.y,
                target.position.x,
                target.position.y); 
                */
            hashmap[tiles] = start;      // Add starting node to explored set
            frontier.Add(start);

            // Cycle through nodes in the frontier
            while (true)
            {
                // Check if the frontier is empty, failure if so
                if (frontier.IsEmpty())
                    return null;  // Failure

                // Remove node with lowest GetCost() value from frontier
                ANode node = frontier.Remove();
                tiles = node.GetTiles();    // Set tiles 2d array to be that of the current node

                // Check if node's tilemap represents the agent as being next to the player.
                if (node.IsGoal())
                {
                    Stack path = new Stack();
                    // Traverse back up the path, and stop at the 2nd node on the path (null -> startnode -> 2nd node)
                    while (node.GetParent().GetParent() != null)
                    {
                        path.Push(node);
                        node = node.GetParent();
                    }
                    return path;     // Return the direction of the step taken to get to this node.
                }
                node.SetExplored();     // Set node to be explored
                                        //hashmap[tiles] = node;  // Update it in hashmap (may be unnecessary, if it is already set by reference?)
                // Expand the node:
                // Cycle through the possible actions the agent can take:
                int selfTile = tiles.GetTiles()[node.XSelf(), node.YSelf()];
                int oppDir = -1;
                ANode moveNode;     // Ref to node with move performed
                for (int i = 0; i < 4; i++) // Can move four different directions
                {
                    // Assign opposite direction for reversing the move; returns node back to its original state
                    switch (i)
                    {
                        case 0: { oppDir = 1; break; }
                        case 1: { oppDir = 0; break; }
                        case 2: { oppDir = 3; break; }
                        case 3: { oppDir = 2; break; }
                    }
                    // If the tile to be attempted is not a wall or cracked wall, it is valid, so take action
                    // If true, move is legal (moving to another floor tile) and move is performed on this node
                    // Otherwise, don't process it and move on to next direction
                    if (node.MoveTo(i))
                    {
                        // Create copy of node with move having been performed, and store it to new reference.
                        //----Change the xt and yt to be a global variable, there's no point storing it in every node---
                        int xs = node.XSelf(), ys = node.YSelf(), xt = node.XTarget(), yt = node.YTarget();
                        moveNode = new ANode(new Tiles(node.GetTiles().CopyTiles(), xs, ys, xt, yt), node, xs, ys, xt, yt); // Passing these variables twice is a bit derpy
                        node.MoveTo(oppDir);    // Retract move, set node back to original state

                        // If new state already exists in the hashmap:
                        if (hashmap.ContainsKey(moveNode.GetTiles()))
                        {
                            // If cost already represented in hashmap is greater than current cost:
                            int newCost = moveNode.GetCost();                       // current state (may have diff value than one in hashmap)
                            int oldCost = hashmap[moveNode.GetTiles()].GetCost();   // last state stored
                            if (oldCost > newCost)
                            {
                                // Set Number of moveNode to the one stored in the hashmap before overwriting it?
                                //moveNode.SetNumber(hashmap[moveNode.GetTiles()].GetNumber());
                                hashmap[moveNode.GetTiles()] = moveNode;    // Update moveNode in hashmap with new cost
                                frontier.Update(hashmap[moveNode.GetTiles()]);
                            }
                            // Else newCost is lower, so do nothing and move on.
                        }
                        else // State does not exist in hashmap, so add it to hashmap and frontier.
                        {
                            hashmap[moveNode.GetTiles()] = moveNode;
                            frontier.Add(moveNode);
                        }
                    }
                    // Else, move isn't legal, do nothing and move on.
                }
            }
            //---Hey, oh sugar honey iced tea, don't forget to add random walk if the astar fails... which it shouldn't
        }            

        class Tiles
        {
            public int xSelf, ySelf, xTarget, yTarget;
            public int[,] tiles;
            public Tiles(int[,] tiles, int xs, int ys, int xt, int yt)
            {
                this.tiles = tiles;
                xSelf = xs;
                ySelf = ys;
                xTarget = xt;
                yTarget = yt;
            }

            public int[,] GetTiles() { return tiles; }

            // Deep copy
            public int[,] CopyTiles()
            {
                int[,] ret = new int[tiles.GetLength(0), tiles.GetLength(1)];
                for (int r = 0; r < tiles.GetLength(0); r++)
                    for (int c = 0; c < tiles.GetLength(1); c++)
                        ret[r, c] = tiles[r, c];
                return ret;
            }

            public override int GetHashCode()
            {
                return (xTarget * 4) + (yTarget * 3) + (xSelf * 2) + (ySelf);
            }

            public override Boolean Equals(Object o)
            {
                Tiles t = (Tiles)o;
                for (int r = 0; r < tiles.GetLength(0); r++)
                    for (int c = 0; c < tiles.GetLength(1); c++)
                        if (tiles[r, c] != t.tiles[r, c])
                            return false;
                return true;
            }
        }

        class ANode : IDenumerable, IComparer
        {
            private int direction;  // step taken in direction to get to this node's state
            private int num;        // for MinPQ reference
            private ANode parent;    // ref to previous node
            private int cost, pathCost, manHatDist; // cost variables
            private Tiles tiles;       // 2D Array of tiles
            private int xTarget, yTarget;   // coords in tiles of target (player for enemy, exit for player?)
            private int xSelf, ySelf;       // coords of agent
            private Boolean inFrontier;

            public ANode(Tiles tiles, ANode parent, int xSelf, int ySelf, int xTarget, int yTarget)
            {
                this.tiles = tiles;
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

            public Boolean MoveTo(int i)
            {
                int moveTile;
                // First, test move to see if it's legal
                switch (i)
                {
                    case 0: { moveTile = tiles.GetTiles()[XSelf(), YSelf() + 1]; break; }   // Up
                    case 1: { moveTile = tiles.GetTiles()[XSelf(), YSelf() - 1]; break; }   // Down
                    case 2: { moveTile = tiles.GetTiles()[XSelf() - 1, YSelf()]; break; }   // Left
                    case 3: { moveTile = tiles.GetTiles()[XSelf() + 1, YSelf()]; break; }   // Right
                    default: { moveTile = -5; throw new Exception("Error while moving."); }
                }
                if (moveTile == 1 || moveTile == 2) // If tile being tested for move is wall or cracked wall
                    return false;   // Don't move and return false

                // Otherwise, go ahead and make move for this node, and return true
                else
                {
                    tiles.GetTiles()[xSelf, ySelf] = 0;    // Assign tile the agent moved FROM to be a floor
                    switch (i)
                    {
                        case 0: { ySelf += 1; break; }  // Up
                        case 1: { ySelf -= 1; break; }  // Down
                        case 2: { xSelf += 1; break; }  // Left
                        case 3: { xSelf -= 1; break; }  // Right
                        default: { throw new Exception("Error(2) while moving."); }
                    }
                    tiles.GetTiles()[xSelf, ySelf] = -1;   // Assign tile the agent has moved TO to be -1
                    return true;
                }
            }

            // This may be unnecessary, as the int[,] is now stored inside Tiles object
            public int[,] CopyTiles()
            {
                int[,] ret = new int[tiles.GetTiles().GetLength(0), tiles.GetTiles().GetLength(1)];
                for (int r = 0; r < tiles.GetTiles().GetLength(0); r++)
                    for (int c = 0; c < tiles.GetTiles().GetLength(1); c++)
                        ret[r, c] = tiles.GetTiles()[r, c];
                return ret;
            }

            // Finds Manhattan Distance between enemy's position in the maze to player's position
            int CalcManhatDist()
            {
                return Math.Abs(xTarget - xSelf) + Math.Abs(yTarget - ySelf);
            }

            int CalcPathCost()
            {
                return (parent == null) ? 0 : pathCost + 1;
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
            public Tiles GetTiles() { return tiles; }
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
                if (tiles.Equals(n.GetTiles()) && cost == n.GetCost())
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

        // ***NOTE: The Compare(obj, obj) method is a method of the ANode class right now, so it has to be perfomed on an ANode object... that doesn't seem right.

        //Code given by Dr. Simon
        // adapted from Sedgewick.


        // GENERIC MINPQ: 
        public interface IDenumerable
        {
            int GetNumber();
            void SetNumber(int x);
        }
        public class MinPQ<Key> where Key : IComparer, IDenumerable
        {
            public ArrayList pq;
            private int N = 0;

            public MinPQ(int cap)
            {
                pq = new ArrayList(cap + 1);
                pq.Add(null);
            }

            public MinPQ()
                : this(1) { }

            public Boolean IsEmpty() { return N == 0; }

            public int Size() { return N; }

            public Key Min()
            {
                if (IsEmpty()) throw new Exception("Priority queue underflow");
                return (Key)pq[1];
            }

            public void Add(Key x)
            {
                N++;
                pq.Add(x);
                Swim(N, x);
            }

            public Key Remove()
            {
                if (IsEmpty()) throw new Exception("Priority queue underflow");
                Key min = (Key)pq[1];
                Swap(pq, 1, N--);
                Sink(1);
                pq.RemoveAt(N + 1);
                min.SetNumber(0);
                return min;
            }

            private void Swim(int k, Key q)
            {
                pq[k] = q;
                Key x = (Key)pq[k]; // assigned outside of loop
                while (k > 1 && ((Key)pq[k / 2]).Compare((Key)pq[k / 2], x) > 0)
                {
                    pq[k] = pq[k / 2];
                    ((Key)pq[k / 2]).SetNumber(k);
                    k = k / 2;
                }
                pq[k] = x;
                x.SetNumber(k);
            }
            private void Sink(int k)
            {
                Key x = (Key)pq[k];
                Key y;
                while (2 * k <= N)
                {
                    int j = 2 * k;
                    y = (Key)pq[j];
                    if (j < N && y.Compare(y, (Key)pq[j + 1]) > 0) j++;
                    if (x.Compare(x, (Key)pq[j]) <= 0) break;
                    pq[k] = pq[j];
                    Key key = (Key)pq[j];
                    key.SetNumber(k);
                    k = j;
                }
                pq[k] = x;
                x.SetNumber(k);
            }

            private static void Swap(ArrayList pq, int i, int j)
            {
                Key t = (Key)pq[i];
                pq[i] = pq[j];
                pq[j] = t;
                ((Key)pq[i]).SetNumber(i);
                ((Key)pq[j]).SetNumber(j);
            }

            public void Update(Key x)
            {
                Swim(x.GetNumber(), x);
                Sink(x.GetNumber());
            }
        }
    }
}
