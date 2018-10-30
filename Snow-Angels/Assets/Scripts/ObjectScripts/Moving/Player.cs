//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject {
    public int wallDamage = 1; //how much dmg the player defaultly does to a wall
    public float restartLevelDelay = 1f;
    public Text bottomText;
    public Text tutText;
    public Slider floorSlider;
    public GameObject bullet;
    public GameObject wolf;
    

    //Tutorial based globals
    public bool letPlayerMove = true;
    private bool letPlayerMoveP2 = false;
    public bool letPlayerAttack = true;
    public bool letPlayerSneak = true;
    private int moveCount = 0;
    private Queue<string> tutMessages = new Queue<string>();

    [HideInInspector] public bool spawn;

    private Transform spotlight;

    private Animator animator; //store reference to animator component
    private int lives; //stores lives
    private int gems = 0; //stores gem total
    private int stunCd;

    private string powerup = "";

    //The value that specifies the min amount of tiles that can be between the player and the enemy
    //If the enemy is within (stealthRadius) tiles of the player, he can't stealth
    public int stealthRadius = 3;

    //Below are containers for the sound effects related to the player
    public AudioClip moveSound1; public AudioClip moveSound2;
    public AudioClip gameOverSound;

    //These sound effects are still being used, even though they really shouldn't be since we aren't finding food anymore
    public AudioClip eatSound1; public AudioClip eatSound2;

    //sound effects for when the destructable walls are hit
    public AudioClip chopSound1; public AudioClip chopSound2;

    public AudioClip hitSound1; public AudioClip hitSound2;

    //Simple function that prints an updated score message
    private void PrintText() {
        string str = "";
        if (gems >= 3) str = "*"; //The * is basically there to represent the player has enough gems
        bottomText.text = "Lives: " + lives + " | Gems: " + gems + str;
    }

    //Function created to check if the player has enough gems to advance levels
    private bool CheckGems() { return gems >= 3 ? true : false; }
    
    //This'll be called whenever the player first loads onto the level
    protected override void Start() {
        animator = GetComponent<Animator>(); //init animator

        base.Start(); //Call the MovingObject's start function

        SetDefaults(GameManager.instance.playerLifeTotal);

        AlterFloor(new Vector2(0, 0)); //Change the tile the player starts on to be "shoveled"
    }

    public void SetDefaults(int lifeTotal) {
        lives = lifeTotal;
        PrintText();
        lastMove = new Vector2(1, 0);
        spawn = true;
        stunCd = 0;
        GameManager.instance.isHiding = false;
        spotlight = transform.GetChild(1).transform;
    }

    //This'll be called whenever the game moves on to the next level
    private void OnDisable() {
        GameManager.instance.playerLifeTotal = lives; //store score in game manager as we change levels
    }
   
    private void InitMoveTutP2() { //start the second half of the move tutorial
        tutMessages.Enqueue("Nice, you now know how to move! However, you shouldn't be too brazen as you walk.");
        tutMessages.Enqueue("Traps exist in the maze. Don't worry, they happen to not be very well hidden... well, most of them anyway.");
        tutMessages.Enqueue("For instance, the tile above where your character is now located has just become a spike-trap.");
        tutMessages.Enqueue("Spike traps will reduce your health total, so be careful and make sure to avoid them!");
        tutMessages.Enqueue("Other than traps, you should also be on the look out for walls that look like the newly spawned one on your right.");
        tutMessages.Enqueue("These walls are destructable, meaning you can use them to create a new path through the maze!");
        tutMessages.Enqueue("All you need to do to destroy such a wall is, well... walk into it. It'll take a couple attempts, but you can destroy it completely!");
        tutMessages.Enqueue("Go ahead and try to destroy that wall! Oh, and I'll make sure the trap tile is gone so you don't hurt your pretty self.");
        
        StartCoroutine(MoveTutorialP2()); //start the move tutorial (it's short so we don't use the queue)
    }

    //Initializes a queue containing all the tutorial messages for the Attack Tutorial Part 1
    private void InitAttackTut1() {
        tutMessages.Enqueue("Good job! The spot the wall was located has now opened up, allowing you to walk on the tile.");
        tutMessages.Enqueue("Now let's move on to the tutorial that talks about attacking.");
        tutMessages.Enqueue("When you move on tiles you haven't walked on before, you'll see the blue bar at the bottom of your screen slowly fill up.");
        tutMessages.Enqueue("You can tell what tiles you have and haven't been on before by their texture.");
        tutMessages.Enqueue("Tiles you have been on/around before will have a blank snow texture instead of an icy looking one!");
        tutMessages.Enqueue("The gauge on the bottom of your screen is an ammo-system of sorts, you can consume it by pressing R. " +
            "This will fire a flaming arrow in the direction you are facing!");
        tutMessages.Enqueue("Here, test out the stun on this dummy monster! Hit R to fire!");

        StartCoroutine(AttackTutorialP1()); //start the Attack Tutorial part 1
    }

    //Initalizes a queue containing all the tutorial messages for the Attack Tutorial Part 2
    private void InitAttackTut2() {
        tutMessages.Enqueue("Nice! Notice that the monster is merely stunned, that means he'll be able to move again soon!");
        tutMessages.Enqueue("The amount of turns the monster is stunned depends on how much gauge you had when you used the stun.");
        tutMessages.Enqueue("You'll be heavily rewarded if you hold onto your gauge as long as possible, only using it once it's full!");
        tutMessages.Enqueue("See for yourself what happens when the gauge is full!, hit R again!");

        StartCoroutine(AttackTutorialP2()); //start the attack tutorial part 2
    }
    
    //Initalizes a queue containing all the tutorial messages for the hide tutorial
    private void InitHideTut() {
        tutMessages.Enqueue("See how the monster disappeared? You were able to slay him because you had a full gauge!");
        tutMessages.Enqueue("Keep this in mind as you explore the maze, as being able to temporarily " +
            "stun an enemy isn't nearly as good as permanently removing the threat.");
        tutMessages.Enqueue("There is one more topic we should discuss... hiding!");
        tutMessages.Enqueue("The enemies aren't very intelligent, so you can easily hide in your trusty crate to avoid conflict!");
        tutMessages.Enqueue("Normally when you aren't hiding, enemies will move a tile everytime you move, but when you are hiding enemies will constantly move.");
        tutMessages.Enqueue("Let's test this out, shall we? Hit F on your keyboard to enter sneak mode.");
        StartCoroutine(HideTutorial());
    }

    private void TutConclusion() {
        tutMessages.Enqueue("When you are in this box, you will be unable to move until you hit F again to leave it.");
        tutMessages.Enqueue("Remember you have to be in a safe space in order to hide, and it's not a good idea to hide when you're being chased!");
        tutMessages.Enqueue("There are many other things you have yet to learn, such as stuff about power-ups (E key to use), but those are rare!");
        tutMessages.Enqueue("It would be better to actually experience the game and learn that way than to continue this tutorial.");
        tutMessages.Enqueue("In other words... Congratulations! You beat the tutorial! Now it's time to delve into the snowy maze.  Good Luck!");
        StartCoroutine(ConcTutorial());
    }

    public IEnumerator MoveTutorialP1() { //this is the first tutorial, it's started in the gamemanager
        tutText.text = "Welcome to the tutorial! Here you will learn the basics. Press Enter to continue.";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        tutText.text = "First things first: movement.  You can use WASD or the arrow keys to move, try now!";
        letPlayerMove = true;        
    }

    private IEnumerator MoveTutorialP2() { //this is the first tutorial, it's started in the gamemanager
        int i = 0;
        while (tutMessages.Count > 1) {
            i++;
            tutText.text = tutMessages.Dequeue();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2);

            if( i == 2 ) { //when the tutorial message talking about the trap pops up
                transform.position = new Vector2(1, 1); //teleport the player to this tile
                
                lastMove = new Vector2(1, 0); //set the direction he is facing
                SetDirArrow(lastMove, arrow); //set the direction he is facing

                GameManager.instance.tutTrapTile.GetComponent<Floor>().SetPainTrap(); //display a pain trap above the player
            }
            else if( i == 4 ) { //when the tutorial message talking about the wall tile pops up
                Vector3 pos = GameManager.instance.tutWallTile.transform.position; //get the location of the tile
                Destroy(GameManager.instance.tutWallTile); //destroy the old floor tile

                //create a destructable wall tile at the pos
                GameManager.instance.tutWallTile = Instantiate(GameManager.instance.boardScript.wallTiles
                    [GameManager.instance.boardScript.wallTiles.Length-1], pos, Quaternion.identity);                
            }
        }
        tutText.text = tutMessages.Dequeue();
        GameManager.instance.tutTrapTile.GetComponent<Floor>().SetNotTrapped();
        letPlayerMoveP2 = true;
    }

    private IEnumerator AttackTutorialP1() {
        int i = 0;
        while (tutMessages.Count > 1) {
            i++;
            tutText.text = tutMessages.Dequeue();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            yield return new WaitForSeconds(GameManager.instance.turnDelay/2);

            if(i == 2) { //after saying we'll start the attack tutorial
                transform.position = new Vector2(1, 1); //teleport the player to this tile
                lastMove = new Vector2(1, 0); //set the direction he is facing
                SetDirArrow(lastMove, arrow); //set the direction he is facing
            }
        }
        tutText.text = tutMessages.Dequeue();
        
        GameManager.instance.CheatFloorScore(false); //give him some floor score
        floorSlider.value = GameManager.instance.GetFloorScore(); //update the progress bar
        GameManager.instance.SpawnTutorialEnemy();
        letPlayerAttack = true; //let him attack        
    }

    private IEnumerator AttackTutorialP2() {
        while (tutMessages.Count > 1) {
            tutText.text = tutMessages.Dequeue();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2);
        }
        tutText.text = tutMessages.Dequeue();
        GameManager.instance.CheatFloorScore(true); //give him full gauge
        floorSlider.value = GameManager.instance.GetFloorScore(); //update the progress bar
        letPlayerAttack = true; //let him attack        
    }

    private IEnumerator HideTutorial() {
        while (tutMessages.Count > 1) {
            tutText.text = tutMessages.Dequeue();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2);
        }
        tutText.text = tutMessages.Dequeue();
        letPlayerSneak = true;
    }

    private IEnumerator ConcTutorial() {
        while (tutMessages.Count > 0) {
            tutText.text = tutMessages.Dequeue();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2);
        }

        Invoke("Restart", restartLevelDelay); //Call the restart function after a delay
    }

    //Update is called once per frame
    private void Update() {
        if (GameManager.instance == null || !GameManager.instance.playersTurn || GameManager.instance.StartMenuShowing()) return; //make sure it's the player's turn

        if (GameManager.instance.tutorial) {
            if (letPlayerMoveP2) {
                CheckMovement();

                if (GameManager.instance.tutWallTile.GetComponent<Wall>().hp == 0) {
                    letPlayerMoveP2 = false; InitAttackTut1();
                }
            }
            else if (letPlayerMove) {
                if (moveCount >= 4) { moveCount = 0; letPlayerMove = false; InitMoveTutP2(); }
                else if (CheckMovement()) { tutText.text = "Keep moving!"; moveCount++; }
            }

            if (letPlayerAttack) {
                if (Input.GetKeyDown(KeyCode.R)) {
                    moveCount++; //We have two-parts to this
                    ShootProjectile();
                    letPlayerAttack = false;

                    if (moveCount == 2) InitHideTut(); //init the hiding tutorial                     
                    else InitAttackTut2(); //init part 2 of the attack tutorial
                }
            }

            if (letPlayerSneak && Input.GetKeyDown(KeyCode.F)) {
                if (!GameManager.instance.isHiding) {
                    bottomText.text = "You are now hidden."; //notify the player
                    GameManager.instance.isHiding = true; //set the player to be hiding in the GameManager
                    animator.SetBool("playerHiding", true); //make the player hide in a box
                    TutConclusion();
                }
                else {
                    bottomText.text = "You are no longer hidden."; //Notify the player
                    GameManager.instance.isHiding = false; //set it so the player is no longer hiding
                    animator.SetBool("playerHiding", false); //make the player get out of the box
                    letPlayerSneak = false;
                }
            }

            return; //during the tutorial, we don't want the player to have access to all the other things
        }

        floorSlider.value = GameManager.instance.GetFloorScore(); //update the progress bar

        //It's a cheaky cheat to help with testing.  Don't tell anyone :]
        if (Input.GetKeyDown(KeyCode.F12)) {
            gems = 3; lives = 100; PrintText(); GameManager.instance.CheatFloorScore(true); powerup = "tp"; stunCd = 0;
        }

        if (Input.GetKeyDown(KeyCode.T)) {
            GameManager.instance.playersTurn = false;
            return;
        }

        //The player hits the attack keybind
        if (Input.GetKeyDown(KeyCode.R)) {
            if (CheckStunCoolDown()) ShootProjectile();
            else print("Stun still on cd for another " + stunCd + " turns.");
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            if (UsePowerUp()) powerup = ""; //powerups are one time use, so consume it
        }

        //If the player isn't hiding and hits the keybind to hide...
        if (GameManager.instance.isHiding == false && Input.GetKeyDown(KeyCode.F)) {
            //Check to see if any enemy is too close to the player
            //At the moment, we're ignoring walls in this check - so if an enemy is on the other side of a wall
            //but is still within the radius of the player, he won't be able to hide.

            bool playerCanStealth = true; //we'll assume the player can stealth initially

            //Multiple Enemies
            foreach (Enemy enemy in GameManager.instance.enemies) {
                //To get a good numeric distance between the player and the enemy, we'll simply do some pythag.
                //Get the absolute difference between the enemy's and the player's x and y coordinates
                int xDif = (int)System.Math.Abs(enemy.transform.position.x - transform.position.x);
                int yDif = (int)System.Math.Abs(enemy.transform.position.y - transform.position.y);

                //c = sqrt( a^2 + b^2 ) | Think of xDif as a, yDif as b, and the forming hypotenuse (which would be total) as c
                int total = (int)System.Math.Sqrt(System.Math.Pow(xDif, 2) + System.Math.Pow(yDif, 2));

                //If the total distance is less than set stealthRadius, then the player is too close to the enemy and cannot stealth
                if (total <= stealthRadius) {
                    playerCanStealth = false; //the player can't stealth
                    break; //if one enemy is within the radius, we don't need to continue checking the other enemies.
                }
            }

            //If the player can stealth, then we'll let them know and change their sprite
            if (playerCanStealth) {
                bottomText.text = "You are now hidden."; //notify the player
                GameManager.instance.isHiding = true; //set the player to be hiding in the GameManager
                animator.SetBool("playerHiding", true); //make the player hide in a box
            }
            else bottomText.text = "You can't stealth right now!"; //the player can't stealth; notify them.
        }
        //If the player is hiding and hits the keybind to hide, we'll make them stop hiding!
        else if (GameManager.instance.isHiding == true && Input.GetKeyDown(KeyCode.F)) {
            bottomText.text = "You are no longer hidden."; //Notify the player
            GameManager.instance.isHiding = false; //set it so the player is no longer hiding
            animator.SetBool("playerHiding", false); //make the player get out of the box
        }
        else CheckMovement();        
    }

    private bool CheckMovement() {
        int horizontal = 0; //the direction we want to move horizontally
        int vertical = 0; //the direction we want to move vertically

        //We want to pay attention for movement input, which can be altered in user controls.
        //Input is handled numerically
        horizontal = (int)(Input.GetAxisRaw("Horizontal")); //1 is right, -1 is left
        vertical = (int)(Input.GetAxisRaw("Vertical")); //1 is up, -1 is down

        //There was movement input from the player
        if (horizontal != 0 || vertical != 0) {

            if (horizontal != 0) //if the user had horizontal input
                vertical = 0; //set it so vertical is 0; this ensures we have no diagnal movement

            if (GameManager.instance.isHiding) {
                bottomText.text = "You cannot move while hiding!"; //don't allow the player to move if he is hiding
                return false;
            }
            else {
                lastMove = new Vector2(horizontal, vertical); //set the lastMove vector
                SetDirArrow(lastMove, arrow); //Rotate the arrow indicator

                if (horizontal == 1) transform.GetComponent<SpriteRenderer>().flipX = false;
                else if (horizontal == -1) transform.GetComponent<SpriteRenderer>().flipX = true;

                AttemptMove<Wall>(horizontal, vertical); //Attempt to move, assuming player might move into a wall

                //If the player's stun ability isn't off cd yet, reduce the timer by one turn
                if (!CheckStunCoolDown()) stunCd--;

                if (spotlight.position.x != transform.position.x || spotlight.position.y != transform.position.y) {
                    spotlight.position = new Vector3(transform.position.x, transform.position.y, spotlight.position.z);
                }

                return true;
            }            
        }
        return false;
    }
    //See if the player can successfully use a powerup (consume on use)
    private bool UsePowerUp() {
        switch (powerup) {
            case "": { //if we don't have a powerup, then we do nothing
                bottomText.text = "You don't have a powerup!";
                return false;
            }
            case "tp": { //teleport powerup
                if (TeleportPlayer()) return true;
                else {
                    bottomText.text = "Something is blocking the teleport location!";
                    return false;
                }
            }
            case "wc": { //wall changer powerup
                ChangeWalls();
                bottomText.text = "You changed the nearby walls to destroyable ones!";
                return true;
            }
            default: return false;
        }
    }

    //TODO: check tiles in the dir the player is facing, any wall tile will turn into a destructable wall.
    private void ChangeWalls() {
        Vector2 pos = transform.position;
    }

    //checks to see the target location is free of enemies, if so then it'll return true
    //if there is an enemy on the tile, it'll return false
    private bool CheckForEnemy(int x, int y) {
        foreach(Enemy e in GameManager.instance.enemies) {
            if (e.transform.position.x == x && e.transform.position.y == y) 
                return false; //an enemy is on the tile
        }
        return true; //no enemies were on the tile
    }

    //attempt to teleport player, as long as the potential spot isn't out of bounds or in a wall/enemy
    private bool TeleportPlayer() {
        Vector2 pos = transform.position;
        GameObject[,] board = GameManager.instance.GetBoardState();
        int x = (int)(pos.x + lastMove.x * 3); int y = (int)(pos.y + lastMove.y * 3);

        //Check boundaries
        if (x >= 0 && x <= board.GetUpperBound(0) && y >= 0 && y <= board.GetUpperBound(0)) {
            if (board[x,y] != null && board[x, y].GetComponent<Floor>() != null && CheckForEnemy(x,y)) {
                transform.position = new Vector3(x, y, transform.position.z);
                foreach (Enemy en in GameManager.instance.enemies) en.chasing = false;                
                return true;
            }
            else return false;
        }
        else return false;
    }

    //Create the projectile object and fire it in the direction the player is facing
    private void ShootProjectile() {
        Vector3 pos = transform.position;

        pos.x += lastMove.x; pos.y += lastMove.y; //move the projectile over/up/etc one tile

        //Create the bullet and fire it
        GameObject bullet = Instantiate(this.bullet, pos, Quaternion.identity, transform);
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();

        if (lastMove.x == 1) sr.flipX = true;
        else if (lastMove.x == -1) sr.flipX = false;
        else if (lastMove.y == 1) bullet.transform.Rotate(new Vector3(0, 0, -90));
        else if (lastMove.y == -1) bullet.transform.Rotate(new Vector3(0, 0, 90));

        stunCd = 20; //put the stun on cd for *a lot* of turns
    }

    //Check if the cooldown of the stun ability is up.
    private bool CheckStunCoolDown() { return stunCd == 0 ? true : false; }

    //A function that will attempt to move the player, given some sort of obstruction object T
    protected override void AttemptMove<T>(int xDir, int yDir) {
        PrintText(); //update the print text, just in case the player picked something up or got hit by an enemy

        base.AttemptMove<T>(xDir, yDir); //call the MovingObject's AttemptMove function
        
        CheckIfGameOver(); //self explanatory
        
        GameManager.instance.playersTurn = false; //no longer the player's turn
    }

    //When the player interacts with any item that is set to be "IsTrigger" on the board, this'll be called
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Exit") { //if we interacted with the exit tile
            if (CheckGems()) { //if we have enough gems to end the level
                Invoke("Restart", restartLevelDelay); //Call the restart function after a delay
                enabled = false; //level is over, so the player shouldn't be enabled anymore
            }
            else bottomText.text = "You need another " + (3 - gems) + " gem(s) to go through the exit.";
        }
        else if (other.tag == "Gem") { //if we interacted with a gem
            gems += 1; //add one to the counter
            PrintText(); //update the user

            //Don't ask why we're using eat sound effects
            SoundManager.instance.RandomizeSFX(eatSound1, eatSound2); //play a random eat sound effect 

            other.gameObject.SetActive(false); //disable that gem object
        }
        else if(other.tag == "PowerUp") {
            //Randomize a number between 0 and 100
            float val = Random.Range(0, 100);
            if (val <= 45f) powerup = "tp";
            else if (val <= 90f) powerup = "wc";
            else SpawnWolf();
        }
    }

    private void SpawnWolf() {
        //GameManager.instance.boardScript.LayoutObjectAtRandom(new GameObject[]{wolf}, 1, 1);
        print("Spawn Wolf");
    }

    //Called whenever the player goes to hit a wall.
    protected override void OnCantMove<T>(T component) {
        Wall hitWall = component as Wall; //store the passed component param as a wall object
        hitWall.DamageWall(wallDamage); //damage the wall  
        SoundManager.instance.RandomizeSFX(chopSound1, chopSound2); //play a sound effect
        animator.SetTrigger("playerChop"); //play the chop animation
    }

    //Function which will be called when the enemy walks into the player
    public void LoseALife(int loss) {

        //If the player is hiding, we'll get him out of the box.
        if (GameManager.instance.isHiding) {
            GameManager.instance.isHiding = false;
            animator.SetBool("playerHiding", false);
        }

        animator.SetTrigger("playerHit"); //show the player hit animation
        lives -= 1; //reduce the food total by the loss amount
        bottomText.text = "-1 life " + "Lives: " + lives + " | Gems: " + gems;
        CheckIfGameOver(); //check to see if that loss resulted in a game over
    }

    //Called when the scene gets wiped after level completion
    public void Restart() {
        SceneManager.LoadScene("Main"); //Load the main scene again
    }

    //Checks to see if the player lost, and, if they did, then call the GameManager GameOver function.
    private void CheckIfGameOver() {
        if (lives <= 0) { //if we run out of food, it's game over
            enabled = false; //player can no longer move
            SoundManager.instance.PlaySingle(gameOverSound); //play the game over sound
            SoundManager.instance.musicSource.Stop(); //stop playing music
            GameManager.instance.GameOver(); //Game over
        }
    }
}
