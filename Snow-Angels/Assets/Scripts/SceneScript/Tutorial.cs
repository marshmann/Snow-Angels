using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
    private Text tutText; //the tutorial text box
    private Slider floorSlider; //the slider for the bar at the bottom

    private int tutCount = 0; //a counter for specific tutorial related events
    private bool letPlayerMove = false; //flag allowing player to move in the first move tut
    private bool letPlayerMoveP2 = false; //flag allowing player to move in the second move tut
    private bool letPlayerAttack = false; //flag allowing player to attack in the attack tut
    private bool letPlayerSneak = false; //flag allowing player to hide in the sneak tut
    private Queue<string> tutMessages; //Queue containing the appropriate tutorial messages
    private Player player; //the player object

    //Setters
    public void SetTutText(Text tutText) { this.tutText = tutText; } //set tutorial text box
    public void SetFloorSlider(Slider floorSlider) { this.floorSlider = floorSlider; } //set slider
    public void SetPM(bool flag) { letPlayerMove = flag; } //set the move (p1) flag
    public void SetPM2(bool flag) { letPlayerMoveP2 = flag; } //set the move (p2) flag
    public void SetPA(bool flag) { letPlayerAttack = flag; } //set the attack flag
    public void SetPS(bool flag) { letPlayerSneak = flag; } //set the sneak flag
    public void SetMC(int i) { tutCount = i; } //set the counter
    //Getters
    public bool GetPM() { return letPlayerMove; } //return the move (p1) flag
    public bool GetPM2() { return letPlayerMoveP2; } //return the move (p2) flag
    public bool GetPA() { return letPlayerAttack; } //return the attack flag
    public bool GetPS() { return letPlayerSneak; } //return the sneak flag
    public int Count() { return tutCount; } //return the tutorial counter

    public void IncCount() { tutCount++; } //increment the tutorial counter when called

    //When the tutorial object is created
    private void Start() {
        player = GameObject.Find("Player").GetComponent<Player>(); //get the player object
        tutMessages = new Queue<string>(); //initiate the queue
    }

    //Initiates the first half of the move tutorial
    public void StartTutorial() { StartCoroutine(MoveTutorialP1()); }

    private IEnumerator MoveTutorialP1() { //first half of the movement tutorial
        tutText.text = "Welcome to the tutorial! Here you will learn the basics. Press Enter to continue."; //print message
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return)); //wait for enter input
        tutText.text = "First things first: movement.  You can use WASD or the arrow keys to move, try now!"; //print message
        SetPM(true); //set the player move flag to allow movement
    }

    public void InitMoveTutP2() { //Initalize the second half of the move tutorial
        //Enqueue all the appropriate messages relating to this tutorial, then start the tutorial
        tutMessages.Enqueue("Nice, you now know how to move! However, you shouldn't be too brazen as you walk.");
        tutMessages.Enqueue("Traps exist in the maze. Don't worry, they happen to not be very well hidden... well, most of them anyway.");
        tutMessages.Enqueue("For instance, the tile above where your character is now located has just become a spike-trap.");
        tutMessages.Enqueue("Spike traps will reduce your health total, so be careful and make sure to avoid them!");
        tutMessages.Enqueue("Other than traps, you should also be on the look out for walls that look like the newly spawned one on your right.");
        tutMessages.Enqueue("These walls are destructable, meaning you can use them to create a new path through the maze!");
        tutMessages.Enqueue("All you need to do to destroy such a wall is, well... walk into it. It'll take a couple attempts, but you can destroy it completely!");
        tutMessages.Enqueue("Go ahead and try to destroy that wall! Oh, and I'll make sure the trap tile is gone so you don't hurt your pretty self.");

        StartCoroutine(MoveTutorialP2()); //start the second half of the move tutorial
    }

    private IEnumerator MoveTutorialP2() { //second half of the movement tutorial
        int i = 0; //counter
        while (tutMessages.Count > 1) { //while the queue has more than one message left
            i++; //increment counter
            tutText.text = tutMessages.Dequeue(); //dequeue and display a tutorial message
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return)); //wait for the player to hit enter to continue
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2); //small delay so the last enter input doesn't trigger more than one message

            if (i == 2) { //when the counter is 2, the tutorial message talking about the trap pops up
                transform.position = new Vector2(1, 1); //teleport the player to this tile
                player.AlterArrow(new Vector2(1, 0)); //alter the player's arrow dir
                GameManager.instance.tutTrapTile.GetComponent<Floor>().SetPainTrap(); //display a pain trap above the player
            }
            else if (i == 4) { //when the counter is 4, the tutorial message talking about the wall tile pops up
                Vector3 pos = GameManager.instance.tutWallTile.transform.position; //get the location of the tile
                Destroy(GameManager.instance.tutWallTile); //destroy the old floor tile

                //create a destructable wall tile at the pos
                GameManager.instance.tutWallTile = Instantiate(GameManager.instance.boardScript.wallTiles
                    [GameManager.instance.boardScript.wallTiles.Length - 1], pos, Quaternion.identity);
            }
        }
        tutText.text = tutMessages.Dequeue(); //dequeue the final message and display it
        GameManager.instance.tutTrapTile.GetComponent<Floor>().SetNotTrapped(); //ensure the tile above the player is no longer trapped
        GameManager.instance.tutTrapTile.GetComponent<Floor>().SetChanged(false); //reset it's changed attribute, allowing it to be altered again
        SetPM2(true); //set the second movement flag to true
    }
    
    public void InitAttackTut1() { //Initializes first half of the attack tutorial
        //Enqueue all the appropriate messages relating to this tutorial, then start the tutorial
        tutMessages.Enqueue("Good job! The spot the wall was located has now opened up, allowing you to walk on the tile.");
        tutMessages.Enqueue("Now let's move on to the tutorial that talks about attacking.");
        tutMessages.Enqueue("When you move on tiles you haven't walked on before, you'll see the blue bar at the bottom of your screen slowly fill up.");
        tutMessages.Enqueue("You can tell what tiles you have and haven't been on before by their texture.");
        tutMessages.Enqueue("Tiles you have been on/around before will have a blank snow texture instead of an icy looking one!");
        tutMessages.Enqueue("The gauge on the bottom of your screen is an ammo-system of sorts, you can consume it by pressing R. " +
            "This will fire a flaming arrow in the direction you are facing!");
        tutMessages.Enqueue("Here, test out the stun on this dummy monster! Hit R to fire!");

        StartCoroutine(AttackTutorialP1()); //start the first part of the attack tutorial
    }

    private IEnumerator AttackTutorialP1() { //first half of the attack tutorial
        int i = 0; //counter
        while (tutMessages.Count > 1) { //while the queue has more than one message left
            i++; //increment counter
            tutText.text = tutMessages.Dequeue(); //dequeue and display a tutorial message
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return)); //wait till player hits enter
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2); //small delay so the last enter input doesn't trigger more than one message

            if (i == 2) { //When the counter is 2, which is after the message saying we'll start the attack tutorial is displayed
                transform.position = new Vector2(1, 1); //teleport the player to this tile
                player.AlterArrow(new Vector2(1, 0)); //alter player's dir arrow
            }
        }
        tutText.text = tutMessages.Dequeue(); //dequeue the final message and display it

        GameManager.instance.CheatFloorScore(false); //give the player some floor score
        floorSlider.value = GameManager.instance.GetFloorScore(); //update the progress bar
        GameManager.instance.SpawnTutorialEnemy(); //spawn an unmoving enemy
        SetPA(true); //let player attack
    }
    
    public void InitAttackTut2() { //Initalizes second half of the attack tutorial
        //Enqueue all the appropriate messages relating to this tutorial, then start the tutorial
        tutMessages.Enqueue("Nice! Notice that the monster is merely stunned, that means he'll be able to move again soon!");
        tutMessages.Enqueue("The amount of turns the monster is stunned depends on how much gauge you had when you used the stun.");
        tutMessages.Enqueue("You'll be heavily rewarded if you hold onto your gauge as long as possible, only using it once it's full!");
        tutMessages.Enqueue("See for yourself what happens when the gauge is full!, hit R again!");

        StartCoroutine(AttackTutorialP2()); //start the second part of the attack tutorial
    }

    private IEnumerator AttackTutorialP2() { //second half of the attack tutorial
        while (tutMessages.Count > 1) { //while the queue has more than one message remaining
            tutText.text = tutMessages.Dequeue(); //dequeue and display a message
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return)); //wait for player to hit enter
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2); //small delay so the last enter input doesn't trigger more than one message
        }
        tutText.text = tutMessages.Dequeue(); //display final message
        GameManager.instance.CheatFloorScore(true); //give player a full gauge
        floorSlider.value = GameManager.instance.GetFloorScore(); //update the progress bar
        SetPA(true); //let player attack attack        
    }
    
    public void InitHideTut() { //Initalizes the hide tutorial
        //Enqueue all the appropriate messages relating to this tutorial, then start the tutorial
        tutMessages.Enqueue("See how the monster disappeared? You were able to slay him because you had a full gauge!");
        tutMessages.Enqueue("Keep this in mind as you explore the maze, as being able to temporarily " +
            "stun an enemy isn't nearly as good as permanently removing the threat.");
        tutMessages.Enqueue("There is one more topic we should discuss... hiding!");
        tutMessages.Enqueue("The enemies aren't very intelligent, so you can easily hide in your trusty crate to avoid conflict!");
        tutMessages.Enqueue("Normally when you aren't hiding, enemies will move a tile everytime you move, but when you are hiding enemies will constantly move.");
        tutMessages.Enqueue("Let's test this out, shall we? Hit F on your keyboard to enter sneak mode.");

        StartCoroutine(HideTutorial()); //start the hide tutorial
    }

    private IEnumerator HideTutorial() { //most of the hide tutorial
        while (tutMessages.Count > 1) { //while the queue has more than one message remaining
            tutText.text = tutMessages.Dequeue(); //dequeue and display a message
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return)); //wait for player to hit enter
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2); //small delay so the last enter input doesn't trigger more than one message
        }
        tutText.text = tutMessages.Dequeue(); //display final message
        SetPS(true); //allow player to hide
    }
    
    public void TutConclusion() { //Finalizes the tutorial with concluding thoughts on the hide tutorial and general information
        //Enqueue all the appropriate messages relating to this tutorial, then start the tutorial
        tutMessages.Enqueue("When you are in this box, you will be unable to move until you hit F again to leave it.");
        tutMessages.Enqueue("Remember you have to be in a safe space in order to hide, and it's not a good idea to hide when you're being chased!");
        tutMessages.Enqueue("There are many other things you have yet to learn, such as stuff about power-ups (E key to use), but those are rare!");
        tutMessages.Enqueue("It would be better to actually experience the game and learn that way than to continue this tutorial.");
        tutMessages.Enqueue("In other words... Congratulations! You beat the tutorial! Now it's time to delve into the snowy maze.  Hit enter to leave the tutorial!");

        StartCoroutine(ConcTutorial()); //finish the tutorial with some concluding messages
    }

    private IEnumerator ConcTutorial() { //finalizes hide tutorial then concludes
        while (tutMessages.Count > 0) { //while messages remain in the queue
            tutText.text = tutMessages.Dequeue(); //dequeue and display a message
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return)); //wait for player to hit enter
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2); //small delay so the last enter input doesn't trigger more than one message
        }

        player.Invoke("Restart", player.restartLevelDelay); //Call the restart function after a delay
    }
}
