using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Random Notes:
 * For the subclasses...
 *      What I'd like to be able to do is have, in Floor: a Trap object, and a method to set it to contain a trap (SetTrap).
 *      Then, when given a Tile object, call the SetTrap method on it...  is this possible, without defining a method SetTrap in the Tile object?
 *      
 * I would also like to have a general method in Trap called GetProb(), which returns the "chance" variable for a sub-class
 * The issue is that this variable should also be static, as it is different depending on which type of trap is being examined.
 * */

namespace MonsterGame1
{
    class MonsterGame
    {
        static void Main(string[] args)
        {
            Board gameBoard = new Board();
            Console.WriteLine(gameBoard.GetType());
            Console.WriteLine(gameBoard.GetBoard().GetLength(0));
            Console.WriteLine(gameBoard.GetBoard().GetType());

            Tile[,] b = new Tile[,] { {new Wall(), new Wall(), new Wall(), new Wall(), new Wall(), new Wall()},
                                      {new Wall(), new Floor(), new Wall(), new CrackedWall(), new Wall(), new CrackedWall() },
                                      {new Wall(), new Floor(), new Floor(), new Floor(), new Floor(), new Wall() },
                                      {new Wall(), new SmokeFloor(), new Floor(), new Wall(), new Floor(), new Wall() },
                                      {new Wall(), new Wall(), new CrackedWall(), new Wall(), new Wall(), new Wall() } };
            Board gb = new Board(b, 2);
            Console.WriteLine(gb.ToString());
            gb.Init();
            Console.WriteLine(gb.ToString());

            //Console.WriteLine(BadTrap.GetProb());
            //Console.WriteLine(GoodTrap.GetProb());
            //Console.WriteLine(Trap.GetProb());
        }
    }


    static class Constants
    {
        /*
        // Chance for traps is out of 100, i.e. if the chance is 1, then it has 1/100 chance of being added to the tile
        public const int BadTrapChance = 2;
        public const int GoodTrapChance = 1;

        // Indexes in trapTiles list:
        public const int BadTrapIndex = 0;
        public const int GoodTrapIndex = 1;

        // Tile Type indexes:
        public const int Wall = 0;
        public const int CrackedWall = 1;
        public const int Floor = 2;
        public const int Smoke = 3;
        */
    }


    // Board
    class Board
    {   
        // Class Fields:
        private static int defaultNumKeys = 3;
        private static int defaultHeight = 20;
        private static int defaultWidth = 20;
        private static Random rand = new Random();

        // Properties:
        private Tile[,] board;
        private int height, width;
        private int numKeys;

        // Constructors:
        public Board (int height, int width, int numKeys)
        {
            this.height = height;
            this.width = width;
            this.numKeys = numKeys;
            board = new Tile[height, width];
        }
// *Note*: Should really make Tile abstract and not even bother having these constructors; afaik we will only be using the last one (the one that takes a 2D array of Tile objects (which will be Floor, Wall, etc.) and assigns them to the Board's 2D array.        
        // With default values
        public Board(int height, int width) 
            : this(height, width, defaultNumKeys) { }
        public Board() 
            : this(defaultHeight, defaultWidth, defaultNumKeys) { }

        // Passing a 2D array of tiles to create a Board with appropriate height and width:
        public Board (Tile[,] tiles, int numKeys)
        {
            height = tiles.GetLength(0);
            width = tiles.GetLength(1);
            board = new Tile[height, width];
            this.numKeys = numKeys;
            // Re-create tile 2D array
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    board[r, c] = tiles[r, c];  // Assigns reference atm... but won't re-use the fed Tile[,] afaik so w/e
                }
            }
        }

        // Methods:

        /* Performs various tasks to set the maze up and prepare it for play
         * 1. Assigns keys to tiles
         * 2. Assigns traps to tiles */
        public void Init()
        {
            // Iterate through each tile and store "corners" to a list; in addition, add traps to tiles:
            List<Tile> cornerTiles = new List<Tile>();
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    // Test if tile is a corner:
                    if (IsCorner(row, col))
                    {
                        //Console.WriteLine(board[row, col].ToString() + " is corner?");
                        cornerTiles.Add(board[row, col]);
                    }

                    // Assign trap tiles:
                    if (board[row, col].GetName() == "floor")
                    {
                        //Console.WriteLine("attempt:[" + row + "," + col + "]");
                        AttemptTrap(row, col);
                    }
                    
                }
            }
            //Console.WriteLine("ct: " + cornerTiles.Count);

            // Randomly assign keys to [numKeys] corner tiles (if there are fewer corner tiles than keys, change # of keys)
            // For each key, look randomly through the corner tiles until one without a key is found.
            // If none without keys exist, the loop will exit and none will be added (though this shouldn't happen,
            // because the number of keys is limited by the number of corner tiles, if the former is >= the latter
            Random rand = new Random();
            if (cornerTiles.Count < numKeys)
                numKeys = cornerTiles.Count;
            
            for (int i=0; i<numKeys; i++)
            {
                for (int j=0; j < cornerTiles.Count; j++) {
                    int index = rand.Next(cornerTiles.Count);
                    if (cornerTiles[index].HasKey() == false)
                    {
                        cornerTiles[index].GiveKey();
                        break;
                    }
                }
            }
        }
        // Attempt to add a random trap to a tile.
        // Takes random # b/w 1 and 100 (incl) and sets a trap if the # falls within range of the trap's chance to occur:
        public void AttemptTrap(int row, int col)
        {
            int chanceToTrap = rand.Next(100);
            if (chanceToTrap < 50)
            {
                int num = rand.Next(100);
                if (num >= 0 && num < 50)
                    board[row, col].SetTrap(new BadTrap());
                else if (num >= 50 && num < 75)
                    board[row, col].SetTrap(new GoodTrap());
                else board[row, col].SetTrap(new NeutralTrap());
            }
        }

        // Test if a tile has two bordering wall tiles (the adjacent wall tiles must be diagonal from each other;
        // (a hallway is not a corner)
        public Boolean IsCorner(int row, int col)
        {
            //Console.WriteLine("r: " + row + " col: " + col);
            if (IsAtEdge(row, col) || !(board[row, col].IsPassable()))
                return false;
            // Console.WriteLine("(Not Edge) r: " + row + " col: " + col);
            // For each neighboring tile, check two tiles that would constitute corner:
            // i.e.: N + E, E + S, S + W, W + N; both must be wall to consider tile a corner tile
            if (!board[row - 1, col].IsPassable() &&   // N + E
                 !board[row, col + 1].IsPassable())
                return true;
            if (!board[row, col + 1].IsPassable() &&   // E + S
                 !board[row + 1, col].IsPassable())
                return true;
            if (!board[row + 1, col].IsPassable() &&   // S + W
                 !board[row, col - 1].IsPassable())
                return true;
            if (!board[row, col - 1].IsPassable() &&   // W + N
                 !board[row - 1, col].IsPassable())
                return true;
            // If this point is reached, no corners were found.
            return false;
        }

        public Boolean IsAtEdge(int row, int col)
        {
            if (row == 0 || col == 0 || row == height - 1 || col == width - 1)
                return true;
            return false;
        }

        public Tile[,] GetBoard() { return board; }

        override
        public String ToString()
        {
            String str = "";
            for (int r = 0; r < board.GetLength(0); r++)
            {
                for (int c = 0; c < board.GetLength(1); c++)
                {
                    str += board[r, c].ToString();
                }
                str += "\n";
            }
            return str;
        }
    }

    // Tile - includes: Floor, SmokeFloor, Wall, CrackedWall
    public class Tile
    {
        private Trap trap;
        private String name;
        private String img;
        private Boolean hasKey;
        private Boolean isPassable;
        private Effect effect;

        // Constructors:
        public Tile(String name, String img, Effect effect, Boolean isPassable, Boolean hasKey, Trap trap)
        {
            this.name = name;
            this.img = img;
            this.effect = effect;
            this.isPassable = isPassable;
            this.hasKey = hasKey;
            this.trap = trap;
        }

        public Tile(String name, String img, Effect effect, Boolean isPassable) 
            : this(name, img, effect, isPassable, false, null) { }

        // Methods:
        public Boolean IsPassable() { return isPassable; }
        public Boolean HasKey() { return hasKey; }
        public String GetName() { return name; }

        public void GiveKey() { hasKey = true; img = "k"; }
        public void SetTrap(Trap trap) { this.trap = trap; img = trap.GetSymbol(); }

        /*
        public Tile CopyTile()
        {
            return 
        }
        */

        override
        public String ToString()
            { return img; }

    }
    // Tile Types:
    public class Floor : Tile
    {
        public Floor() : base("floor", " ", new Effect(), true) { }
    }

    public class SmokeFloor : Tile
    {
        public SmokeFloor() : base("smoke-floor", "~", new Effect(), true) { }
    }

    public class Wall : Tile
    {
        public Wall() : base("wall", "X", new Effect(), false) { }
    }

    public class CrackedWall : Tile
    {
        public CrackedWall() : base("cracked-wall", "C", new Effect(), false) { }
    }


    // Trap super class
    public class Trap
    {
        private String name;
        private String symbol;
        private Effect effect;

        public Trap(String name, String symbol, Effect effect)
        {
            this.name = name;
            this.symbol = symbol;
            this.effect = effect;
        }

        public String GetName() { return name; }
        public String GetSymbol() { return symbol; }
    }

    // Trap types:
    public class BadTrap : Trap
    {
        //private static int chance = 50;
        public BadTrap () : base("bad", "b", new Effect()) { }
        //public static int GetProb() { return chance; }
    }

    public class GoodTrap : Trap
    {
        //private static int chance = 50;
        public GoodTrap () : base("good", "g", new Effect()) { }
        //public static int GetProb() { return chance; }
    }

    public class NeutralTrap : Trap
    {
        public NeutralTrap () : base("neutral", "n", new Effect()) { }
    }

    // Effect
    public class Effect
    {
        // Cause player to move slower, enemy to move faster, smaller visibility, buff, w/e
    }
}
