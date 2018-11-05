//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject {
    public int wallDamage = 1; //how much dmg the player defaultly does to a wall
    public float restartLevelDelay = 1f; //little time delay on level restart
    public int stealthRadius = 3; //If the enemy is within stealthRadius tiles of the player, he can't stealth
    [HideInInspector] public bool spawn; //spawn flag, used to make it so the movement sound effect isn't played on spawn

    public Text bottomText; //text on the bottom of UI
    public Text tutText; //the textbox in the upper-middle portion of the screen for tutorial text
    public Slider floorSlider; //slider representing the amount of tiles explored by player

    public GameObject bullet; //the bullet prefab
    public GameObject wolf; //the wolf prefab
    private Tutorial tutorial; //tutorial object

    private Animator animator; //store reference to animator component
    private Transform spotlight; //reference to the player's spotlight component

    private int lives; //stores lives
    private int gems = 0; //stores gem total
    private int stunCd; //stores current cd on the player's projectile ability
    private string powerup = ""; //stores the name of the powerup the player has (default: none)

    //Below are containers for the sound effects related to the player
    public AudioClip moveSound1; public AudioClip moveSound2;
    public AudioClip gameOverSound;
    //These sound effects are still being used, even though they really shouldn't be since we aren't finding food anymore
    public AudioClip eatSound1; public AudioClip eatSound2;
    //sound effects for when the destructable walls are hit
    public AudioClip chopSound1; public AudioClip chopSound2;
    public AudioClip hitSound1; public AudioClip hitSound2;
    
    //This'll be called whenever the player first loads onto the level
    protected override void Start() {
        animator = GetComponent<Animator>(); //init animator
        base.Start(); //Call the MovingObject's start function
        SetDefaults(GameManager.instance.playerLifeTotal); //set all the appropriate defaults
        AlterFloor(new Vector2(0, 0)); //Change the tile the player starts on to be "shoveled"
    }

    //Simple function that prints an updated score message on the player's UI
    private void PrintText() {
        string str = "";
        if (gems >= 3) str = "*"; //The * is there to represent the player has enough gems
        bottomText.text = "Lives: " + lives + " | Gems: " + gems + str; //print text on UI
    }

    //set the player's inital values
    public void SetDefaults(int lifeTotal) {
        lives = lifeTotal; //set the life total
        PrintText(); //update the UI text
        lastMove = new Vector2(1, 0); //initalize the player's dir
        spawn = true; //init spawn flag to true
        stunCd = 0; //reset stun cd
        GameManager.instance.isHiding = false; //ensure hiding flag is false
        spotlight = transform.GetChild(1).transform; //obtain spotlight transform
    }

    //This'll be called whenever the game moves on to the next level, store score in game manager as we change levels
    private void OnDisable() { GameManager.instance.playerLifeTotal = lives; }

    //Function created to check if the player has enough gems to advance levels
    private bool CheckGems() { return gems >= 3 ? true : false; }

    //Start the tutorial if the player decided they wanted to play through the tutorial
    public void StartTutorial() {
        AlterFloor(new Vector2(0, 0)); //Change the tile the player starts on to be "shoveled"
        spawn = false; //allow the player to have movement sound effects on spawn

        tutorial = gameObject.AddComponent(typeof(Tutorial)) as Tutorial; //create tut object
        tutorial.SetTutText(tutText); //set the UI text component
        tutorial.SetFloorSlider(floorSlider); //set the floor slider
        tutorial.StartTutorial(); //start the tutorial
    }

    //set the direction the player is facing
    public void AlterArrow(Vector2 move) { lastMove = move; SetDirArrow(lastMove, arrow); }

    //Tutorial function that checks for user input depending on what controls they have access to.
    private void Tutorial() {
        //Controls are turned on and off depending on where the player is in the tutorial

        if (tutorial.GetPM()) { //if the first movement flag is active, the player will be able to move
            //the player will need to move 4 times before moving on in the tutorial
            if (tutorial.Count() >= 4) { //if they have moved 4 (or somehow more) times
                tutorial.SetMC(0); //reset the counter
                tutorial.SetPM(false); //set the first move flag to false, disabling player movement
                tutorial.InitMoveTutP2(); //start the second half of the movement tutorial
            }
            //if they haven't moved 4 times yet, check for movement input
            else if (CheckMovement()) { //if CheckMovement returned true, then the player inputed a move command
                tutText.text = "Keep moving!"; //change the tutorial text
                tutorial.IncCount(); //increment the tutorial counter
            }
        }
        else if (tutorial.GetPM2()) { //if the second movement flag is active, we'll allow the player to move again
            //In this portion of the tutorial, the player needs to destroy a wall by moving into it.

            CheckMovement(); //check for player movement (we don't need the return value here)

            //If the wall that was spawned was destroyed (health would be 0)
            if (GameManager.instance.tutWallTile.GetComponent<Wall>().hp == 0) {
                tutorial.SetPM2(false); //set the second move flag to false, disabling player movement
                tutorial.InitAttackTut1(); //initiate the attack tutorial
            }
        }

        if (tutorial.GetPA()) { //if the player attack tutorial flag is active
            //We have two-parts to this portion of the tutorial,
            //first we have the player stun an enemy then we have the player kill one.

            if (Input.GetKeyDown(KeyCode.R)) { //check for the attack keybind
                tutorial.IncCount(); //increment the counter on attack use
                ShootProjectile(); //spawn and shoot the projectile

                floorSlider.value = 0; //reset the progress bar
                tutorial.SetPA(false); //disable the attack flag

                //If the tutorial count isn't 2, then we'll start the kill half of the attack tutorial
                //if the tutorial count is 2 then we'll start the hiding tutorial
                if (tutorial.Count() == 2) tutorial.InitHideTut(); //init the hiding tutorial                     
                else tutorial.InitAttackTut2(); //init part 2 of the attack tutorial
            }
        }

        //If the hiding tutorial flag is active, and the player hits the hide key
        if (tutorial.GetPS() && Input.GetKeyDown(KeyCode.F)) {
            if (!GameManager.instance.isHiding) { //if the player wasn't already hiding
                bottomText.text = "You are now hidden."; //notify the player
                GameManager.instance.isHiding = true; //set the player to be hiding in the GameManager
                animator.SetBool("playerHiding", true); //make the player hide in a box
                tutorial.TutConclusion(); //start the tutorial conclusion portion
            }
            else { //this isn't a required part of the tutorial; the player can hit the hide button again to stop hiding
                bottomText.text = "You are no longer hidden."; //Notify the player
                GameManager.instance.isHiding = false; //set it so the player is no longer hiding
                animator.SetBool("playerHiding", false); //make the player get out of the box
                tutorial.SetPS(false); //make it so the player can no longer hide as the tutorial ends
            }
        }
    }

    //Update is called once per frame
    private void Update() {
        if (GameManager.instance == null || !GameManager.instance.playersTurn || GameManager.instance.StartMenuShowing()) return; //make sure it's the player's turn

        //during the tutorial, we don't want the player to have access to specific controls
        if (GameManager.instance.tutorial) { Tutorial(); return; }

        floorSlider.value = GameManager.instance.GetFloorScore(); //update the progress bar

        //It's a cheaky cheat to help with testing.  Don't tell anyone :]
        if (Input.GetKeyDown(KeyCode.F12)) {
            gems = 3; lives = 100; PrintText();
            GameManager.instance.CheatFloorScore(true);
            powerup = "tp"; stunCd = 0;
        }

        //Allow the player to "wait" in place, in other words allowing him to skip his turn
        if (Input.GetKeyDown(KeyCode.T)) { GameManager.instance.playersTurn = false; return; }

        //The player hits the attack keybind
        if (Input.GetKeyDown(KeyCode.R)) {
            if (CheckStunCoolDown()) ShootProjectile();
            else print("Stun still on cd for another " + stunCd + " turns.");
        }

        //consume the player's powerup, if he has one
        if (Input.GetKeyDown(KeyCode.E)) { if (UsePowerUp()) powerup = ""; }

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

                if (spotlight.position.x != transform.position.x || spotlight.position.y != transform.position.y) 
                    spotlight.position = new Vector3(transform.position.x, transform.position.y, spotlight.position.z);                

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
            float val = Random.Range(0, 100); //Randomize a number between 0 and 100
            if (val <= 45f) powerup = "tp"; //the player has access to a teleport powerup
            else if (val <= 90f) powerup = "wc"; //the player has access to a wall-changer powerup
            else SpawnWolf(); //the player unfortunately gets a bad roll and summons an enemy
        }
    }

    //TODO: implement spawn wolf mechanic
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
    public void Restart() { SceneManager.LoadScene("Main"); /* Load the main scene again */ }

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
