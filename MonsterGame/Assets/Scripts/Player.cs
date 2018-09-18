//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject {
    public int wallDamage = 1; //how much dmg the player defaultly does to a wall
    public float restartLevelDelay = 1f;
    public Text bottomText;
    public Slider floorSlider;
    public GameObject bullet;
    [HideInInspector] public bool spawn;

    private Animator animator; //store reference to animator component
    private int lives; //stores lives
    private int gems = 0; //stores gem total
    private int stunCd;

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
        lastMoveX = 1; lastMoveY = 0;
        spawn = true;
        stunCd = 0;
    }

    //This'll be called whenever the game moves on to the next level
    private void OnDisable() {
        GameManager.instance.playerLifeTotal = lives; //store score in game manager as we change levels
    }

    //Update is called once per frame
    private void Update() {
        if (GameManager.instance == null || !GameManager.instance.playersTurn || GameManager.instance.startMenu) return; //make sure it's the player's turn

        floorSlider.value = GameManager.instance.GetFloorScore(); //update the progress bar

        bool isHiding = GameManager.instance.isHiding; //check to see if the player is hiding or not
        int horizontal = 0; //the direction we want to move horizontally
        int vertical = 0; //the direction we want to move vertically

        //It's a cheaky cheat to help with testing.  Don't tell anyone :]
        if (Input.GetKeyDown(KeyCode.F12)) {
            gems = 3; lives = 3; PrintText(); GameManager.instance.CheatFloorScore();
        }

        //The player hits the attack keybind
        if (Input.GetKeyDown(KeyCode.R)) {
            if (CheckStunCoolDown()) ShootProjectile();
            else print("Stun still on cd for another " + stunCd + " turns.");
        }

        //If the player isn't hiding and hits the keybind to hide...
        if (isHiding == false && Input.GetKeyDown(KeyCode.F)) {
            //Check to see if any enemy is too close to the player
            //At the moment, we're ignoring walls in this check - so if an enemy is on the other side of a wall
            //but is still within the radius of the player, he won't be able to hide.

            bool playerCanStealth = true; //we'll assume the player can stealth initially

            /* Depricated: We only have ONE enemy now, so we don't need to check the list of enemies
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
            */

            Enemy enemy = GameManager.instance.enemy; //get reference to the enemy object

            //To get a good numeric distance between the player and the enemy, we'll simply do some pythag.
            //Get the absolute difference between the enemy's and the player's x and y coordinates
            int xDif = (int)System.Math.Abs(enemy.transform.position.x - transform.position.x);
            int yDif = (int)System.Math.Abs(enemy.transform.position.y - transform.position.y);

            //c = sqrt( a^2 + b^2 ) | Think of xDif as a, yDif as b, and the forming hypotenuse (which would be total) as c
            int total = (int)System.Math.Sqrt(System.Math.Pow(xDif, 2) + System.Math.Pow(yDif, 2));

            //If the total distance is less than set stealthRadius
            if (total <= stealthRadius) { playerCanStealth = false; } //the player can't stealth, player's too close to the enemy

            //If the player can stealth, then we'll let them know and change their sprite
            if (playerCanStealth) {
                bottomText.text = "You are now hidden."; //notify the player
                GameManager.instance.isHiding = true; //set the player to be hiding in the GameManager
                animator.SetBool("playerHiding", true); //make the player hide in a box
            }
            else bottomText.text = "You can't stealth right now!"; //the player can't stealth; notify them.
        }
        //If the player is hiding and hits the keybind to hide, we'll make them stop hiding!
        else if (isHiding == true && Input.GetKeyDown(KeyCode.F)) {
            bottomText.text = "You are no longer hidden."; //Notify the player
            GameManager.instance.isHiding = false; //set it so the player is no longer hiding
            animator.SetBool("playerHiding", false); //make the player get out of the box
        }
        else { //Any keypresses other than F12 and F will end up here

            //We want to pay attention for movement input, which can be altered in user controls.
            //Input is handled numerically
            horizontal = (int)(Input.GetAxisRaw("Horizontal")); //1 is right, -1 is left
            vertical = (int)(Input.GetAxisRaw("Vertical")); //1 is up, -1 is down
           
            //There was movement input from the player
            if (horizontal != 0 || vertical != 0) {

                if (horizontal != 0) //if the user had horizontal input
                    vertical = 0; //set it so vertical is 0; this ensures we have no diagnal movement

                if (isHiding) bottomText.text = "You cannot move while hiding!"; //don't allow the player to move if he is hiding
                else {
                    SetDirArrow(horizontal, vertical, arrow); //Rotate the arrow indicator
                    lastMoveX = horizontal; lastMoveY = vertical;
                    AttemptMove<Wall>(horizontal, vertical); //Attempt to move, assuming player might move into a wall

                    //If the player's stun ability isn't off cd yet, reduce the timer by one turn
                    if (!CheckStunCoolDown()) stunCd--;
                }
            }
        }
    }

    //Create the projectile object and fire it in the direction the player is facing
    private void ShootProjectile() {
        Vector3 pos = transform.position;
        pos.x += lastMoveX; pos.y += lastMoveY; //move the projectile over one tile

        //Create the bullet and fire it
        Instantiate(bullet, pos, Quaternion.identity, transform);

        stunCd = 20; //put the stun on cd for *a lot* of turns
    }

    //Check if the cooldown of the stun ability is up.
    private bool CheckStunCoolDown() { return stunCd == 0 ? true : false; }

    //A function that will attempt to move the player, given some sort of obstruction object T
    protected override void AttemptMove<T>(int xDir, int yDir) {
        PrintText(); //update the print text, just in case the player picked something up or got hit by an enemy

        base.AttemptMove<T>(xDir, yDir); //call the MovingObject's AttemptMove function
        
        CheckIfGameOver(); //self explanatory

        //if(!GameManager.instance.sliding)
        GameManager.instance.playersTurn = false; //no longer the player's turn
    }

    //In Unity we set the exit, soda, and food items to be "Is Trigger"
    //In other words, when the player interacts with them, it'll auto call this function!
    //We don't use the food or soda items anymore, but we added one called Gem for the gems
    //the player needs to pick up.
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
    }

    //Called whenever the player goes to hit a wall.
    protected override void OnCantMove<T>(T component) {
        Wall hitWall = component as Wall; //store the passed component param as a wall object
        hitWall.DamageWall(wallDamage); //damage the wall
        
        SoundManager.instance.RandomizeSFX(chopSound1, chopSound2); //play a random chop sound

        /* This causes the player's "player chop" boolean to be set to true
        * In other words, it'll cause the player chop animation to happen,
        * and due to how we set up the animator code, it'll happen once then return to the idle animation */
        animator.SetTrigger("playerChop");
    }

    //Function which will be called when the enemy walks into the player
    public void LoseALife(int loss) {
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
            enabled = false; //level is over, so the player shouldn't be enabled anymore
            SoundManager.instance.PlaySingle(gameOverSound); //play the game over sound
            SoundManager.instance.musicSource.Stop(); //stop playing music
            GameManager.instance.GameOver(); //Game over
        }
    }
}
