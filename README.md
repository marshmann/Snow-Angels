# MonsterGame
AI final project

Append your ideas (with your name, perferably) to this read me file, don't edit anyone else's ideas!  Also, seperate your thoughts like I did with my name below, to allow for easy readability.

Format: name | Comment

| Name | Comments |
| --- | --- |
Nick | The main premise of this game is that the player needs to find keys (thinking 3-5) in a maze to unlock the door at the end.  There exists a monster (or multiple monsters) that will chase and kill the player on sight.  The player will want to hide from them whenever he feels threatened.
Nick | When the player moves, he'll move one tile and the monsters will move two tiles (four tiles if a trap was recently triggered, more on that below).  If the player is on a "hide tile" then time will automatically advance.  This means that the monsters will automatically move on a set timer as long as the player is "hidden."  Once he steps off that tile, the monster movement should revert to how it was prior. |
Nick | The monster AI should work as follows:
Nick | It will decide on how to move as it explores the board.  It wants to find the player, so it tries to go to places it hasn't been before or to places it hasn't been recently (so the monsters will eventually go back to older tiles).
Nick | When a trap is triggered, the monster will run (advancing two times the normal speed, so four tiles whenever time advances) toward the location the trap was triggered.
Nick | If a monster sees the player, that is if the player is on a tile in the direction the monster is currently facing (and as long as there isn't a piece of terrain blocking view), it'll lock on and go toward the player.  After losing sight of the player, it should go to the last known tile of the player.  Perhaps it stays on the tile for a single turn (where a turn = one time advancement ie player movement), then slowly advances around as if sniffing out the player (so it'll move 1 tile per time increment for a short period of time).
Nick | Finally, there should be a "darkness feature" where the player can only see a few tiles ahead of him.  The monster wouldn't be affected.
Nick | Optional Ideas:
Nick | 1) Add an AI for the player who will play through the board and show the path it would take to win.  In order to do this, we will likely need to run iterative deepening A* and find the best path through a preset maze BEFORE our in-class presentation.  It wouldn't be a very entertaining watch to see it try, and fail, to beat the game.
Nick | 2) Add treasure in the maze that'll contribute to a score! The reason against adding such an obvious feature would be that it makes the above idea harder to execute.  Do we prioritize earnings, time, or survival?
Nick | 3) Add hidden passages throughout the map, so the player might be able to go through a piece of seemingly unpassable terrain.
Nick | 4) Add multiple terrain options.  Maybe something that causes the player to slide, like ice for example.  Also, make it so some of the walls are see-through.  This would mean the monster could detect the player, regardless of the fact terrain is in the way.  However, this would still mean that the monster would have to run around the terrain to get to the player, unlike the normal instance where there wouldn't be any terrain in the way if the monster detects the player.
Nick | 5) Items such as traps the player can set down to stun/slow the monster and torches that allow the player to temporarily see further might be a good idea to add if we have the time.  My friend suggested adding an item that "pings where the monster is."