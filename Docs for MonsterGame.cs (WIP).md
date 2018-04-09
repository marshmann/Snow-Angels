#### List of classes:
MonsterGame (Main)
Board
Trap (just generic examples atm)
    BadTrap
    GoodTrap
Tile
    Floor
    SmokeFloor
    Wall
    CrackedWall
Effect
Constants (optional atm, could make organization easier by keeping all the values (like chance of traps, tile properties, etc.) in one place)

#### Brief Description
- Just as a note: I have not tested this stuff beyond what's in Main; there is still possibility of bugs in logic.
### ***Still need to code:***
    - setting the player and enemy spawns

The general idea is that a 2D array of Tiles is passed to the board, and the board prepares it for play.


### **Board**
## Constructor (the one we will use): Board(Tile[,] tiles, int numKeys)
- tiles : a 2D array of Tile subclass objects (Floor, Wall, etc.)
- numKeys : number of keys the player has to acquire to exit the maze

## Methods
- void **Init()**
    - Assigns keys and traps to the given Board
    - To assign keys, searches for corner tiles and picks random ones to place keys at
    - To add traps, calls AttemptTrap() method
- void **AttemptTrap(int row, int col)**
    - given row and col of tile:
    - cycle once through the trap types, setting a trap (and moving on to next if trap is not set)
        - trap is set if the random number falls within range of current trap type's probability (out of 100)
        - ex.: Random # b/w 1 and 100 (incl) is generated; chance for trap is 5; # must be <= 5 for trap to be set
- Boolean **IsCorner(int row, int col)**
    - returns true if "diagonally adjacent" tiles are found (e.g. N + E)
- Boolean **IsAtEdge(int row, int col)**
    - returns true if tile at row and col is on the edge of the board, false otherwise
- Tile **GetBoard()**
    - returns reference to the 2D array of Tiles in this Board object
- String **ToString()**
    - returns string representation of each Tile (just for debugging, etc.) arranged properly to show the maze


### **Tile**
## **Subclasses:**
# - Floor, SmokeFloor, Wall, CrackedWall
## Constructor : Tile(String name, String img, Effect effect, Boolean isPassable)
- name : name of the tile ("wall", "floor)
- img : string repr. of tile ("D" for wall, "B" for cracked wall)
- effect : some kind of effect the tile may have on the player, board, etc.
- isPassable : if the player can walk on the tile or not

## Methods
- Basically just getters and setters

The Tile object is the superclass of all the tile type classes (Floor, Wall, etc.).
When a Tile[,] is passed to the Board through its constructor, it must contain only the subclasses.
(Might just make Tile abstract to force this implicitly).

### **Trap**
## **Subclasses:**
# - GoodTrap, BadTrap
## Constructor: Trap(String name, Effect effect)
- name : name of trap ("good", "bad", etc.)
- effect : effect trap will have
    - *Note: Maybe could base trap chance off of the size of the maze.  Would have to set the chances in Init(), before they are set.*

## Methods
- static int **GetProb()**
    - returns the probability (out of 100) that the trap will be set
- other getters

### **Effect**
Some kind of modifier to other values?
