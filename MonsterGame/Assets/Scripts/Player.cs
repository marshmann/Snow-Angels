//Author: Nicholas Marshman - using Unity 2D roguelike tutorial as a base
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject {
    public int wallDamage = 1; //how much dmg the player defaultly does to a wall
    public float restartLevelDelay = 1f;
    public Text bottomText;
    
    private Animator animator; //store reference to animator component
    private int lives; //stores lives
    private int gems = 0; //stores gem total

    //The value that specifies the min amount of tiles that can be between the player and the enemy
    //If the enemy is within (stealthRadius) tiles of the player, he can't stealth
    public int stealthRadius = 3;

    //Below are containers for the sound effects related to the player
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    //Simple function that prints
    private void PrintText() {
        string str = "";
        if(gems >= 3) {
            str = "*";
        }
        bottomText.text = "Lives: " + lives + " | Gems: " + gems + str;
    }

    private bool CheckGems() {
        return gems >= 3 ? true : false;
    }

    protected override void Start() {
        animator = GetComponent<Animator>();
        lives = GameManager.instance.playerLifeTotal;
        PrintText();
        base.Start(); //Call the MovingObject's start function
    }

    private void OnDisable() {
        GameManager.instance.playerLifeTotal = lives; //store score in game manager as we change levels
    }

    //Update is called once per frame
    private void Update() {
        if (!GameManager.instance.playersTurn) return; //make sure it's the player's turn

        bool isHiding = GameManager.instance.isHiding;
        int horizontal = 0; //the direction we want to move horizontally
        int vertical = 0; //the direction we want to move vertically

        //It's a cheaky cheat to help with testing.  Don't tell anyone :]
        if (Input.GetKeyDown(KeyCode.F12)) {
            gems = 3;
            PrintText();
        }

        if (isHiding == false && Input.GetKeyDown(KeyCode.F)) {
            bool playerCanStealth = true;

            //Check to see if any enemy is too close to the player
            //I'm just ignoring wall's because that's annoying and a waste of time
            foreach (Enemy enemy in GameManager.instance.enemies) { 
                if ((int)System.Math.Abs(enemy.transform.position.x - transform.position.x) <= stealthRadius ||
                    (int)System.Math.Abs(enemy.transform.position.y - transform.position.y) <= stealthRadius) {
                    playerCanStealth = false;
                    break;
                }
            }
            if (playerCanStealth) {
                bottomText.text = "You are now hidden.";
                GameManager.instance.isHiding = true;
                animator.SetBool("playerHiding", true); //make the player hide in a box
            }
            else bottomText.text = "You can't stealth right now!";
        }
        else if (isHiding == true && Input.GetKeyDown(KeyCode.F)) {
            GameManager.instance.isHiding = false; //Player is no longer hiding
            animator.SetBool("playerHiding", false); //Player gets out of the box
        }
        else {
            horizontal = (int)(Input.GetAxisRaw("Horizontal"));
            vertical = (int)(Input.GetAxisRaw("Vertical"));

            if (horizontal != 0) //if we're moving horizontally
                vertical = 0; //set it so vertical is 0; this ensures we have no diagnal movement

            //Player tried to move
            if (horizontal != 0 || vertical != 0) {
                if (isHiding) {
                    GameManager.instance.isHiding = false;
                    animator.SetBool("playerHiding", false); //player gets out of the box
                    //Change Sprite back
                }
                AttemptMove<Wall>(horizontal, vertical); //Attempt to move, assuming player might move into a wall
            }
        }
    }
    
    protected override void AttemptMove<T>(int xDir, int yDir) {
        PrintText();

        base.AttemptMove<T>(xDir, yDir); //call the MovingObject's AttemptMove function

        RaycastHit2D hit; //Here we can do audio-related calls if we want
        if (Move(xDir, yDir, out hit)) { //if we can move
            SoundManager.instance.RandomizeSFX(moveSound1, moveSound2); //play a random move sound
        }

        CheckIfGameOver();

        GameManager.instance.playersTurn = false; //no longer the player's turn
    }

    //In Unity we set the exit, soda, and food items to be "Is Trigger"
    //In other words, when the player interacts with them, it'll auto call this function!
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Exit") { //if we interacted with the exit tile
            if (CheckGems()) { //if we have enough gems to end the level
                Invoke("Restart", restartLevelDelay); //Call the restart function after a delay
                enabled = false; //level is over, so the player shouldn't be enabled anymore
            }
            else {
                bottomText.text = "You need another " + (3 - gems) + " gem(s) to go through the exit.";
            }
        }
        else if (other.tag == "Gem"){
            gems += 1; 
            PrintText();
            SoundManager.instance.RandomizeSFX(eatSound1, eatSound2); //play a random eat sound effect
            other.gameObject.SetActive(false); //disable that food object
        }
    }

    //Depricated code since we removed the destroyable wall feature
    protected override void OnCantMove<T>(T component) {
        Wall hitWall = component as Wall; //store the passed component param as a wall object
        hitWall.DamageWall(wallDamage); //damage the wall
        
         /* This causes the player's "player chop" boolean to be set to true
         * In other words, it'll cause the player chop animation to happen,
         * and due to how we set up the animator code, it'll happen once then return to the idle animation
         */
        animator.SetTrigger("playerChop");
    }

    public void LoseALife(int loss) {
        animator.SetTrigger("playerHit"); //show hit animation
        lives -= 1; //reduce the food total by the loss amount
        bottomText.text = "-1 life " + "Lives: " + lives + " | Gems: " + gems;
        CheckIfGameOver(); //check to see if that loss resulted in a game over
    }

    private void Restart() {
        SceneManager.LoadScene("Main"); //Load the main scene again
    }

    private void CheckIfGameOver() {
        if (lives <= 0) { //if we run out of food, it's game over
            SoundManager.instance.PlaySingle(gameOverSound); //play the game over sound
            SoundManager.instance.musicSource.Stop(); //stop playing music
            GameManager.instance.GameOver(); //Game over
        }
    }
}
