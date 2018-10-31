using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
    public Text tutText;
    public Slider floorSlider;

    private int moveCount = 0;
    private bool letPlayerMove = false;
    private bool letPlayerMoveP2 = false;
    private bool letPlayerAttack = false;
    private bool letPlayerSneak = false;
    private Queue<string> tutMessages;
    private Player player;

    //Setters
    public void SetPM(bool flag) { letPlayerMove = flag; }
    public void SetPM2(bool flag) { letPlayerMoveP2 = flag; }
    public void SetPA(bool flag) { letPlayerAttack = flag; }
    public void SetPS(bool flag) { letPlayerSneak = flag; }
    public void SetMC(int i) { moveCount = i; }
    //Getters
    public bool GetPM() { return letPlayerMove; }
    public bool GetPM2() { return letPlayerMoveP2; }
    public bool GetPA() { return letPlayerAttack; }
    public bool GetPS() { return letPlayerSneak; }
    public int Count() { return moveCount; }

    public void IncCount() { moveCount++; }

    private void Start() { player = GameObject.Find("Player").GetComponent<Player>(); tutMessages = new Queue<string>(); }

    public void InitMoveTutP2() { //start the second half of the move tutorial
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
    public void InitAttackTut1() {
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
    public void InitAttackTut2() {
        tutMessages.Enqueue("Nice! Notice that the monster is merely stunned, that means he'll be able to move again soon!");
        tutMessages.Enqueue("The amount of turns the monster is stunned depends on how much gauge you had when you used the stun.");
        tutMessages.Enqueue("You'll be heavily rewarded if you hold onto your gauge as long as possible, only using it once it's full!");
        tutMessages.Enqueue("See for yourself what happens when the gauge is full!, hit R again!");

        StartCoroutine(AttackTutorialP2()); //start the attack tutorial part 2
    }

    //Initalizes a queue containing all the tutorial messages for the hide tutorial
    public void InitHideTut() {
        tutMessages.Enqueue("See how the monster disappeared? You were able to slay him because you had a full gauge!");
        tutMessages.Enqueue("Keep this in mind as you explore the maze, as being able to temporarily " +
            "stun an enemy isn't nearly as good as permanently removing the threat.");
        tutMessages.Enqueue("There is one more topic we should discuss... hiding!");
        tutMessages.Enqueue("The enemies aren't very intelligent, so you can easily hide in your trusty crate to avoid conflict!");
        tutMessages.Enqueue("Normally when you aren't hiding, enemies will move a tile everytime you move, but when you are hiding enemies will constantly move.");
        tutMessages.Enqueue("Let's test this out, shall we? Hit F on your keyboard to enter sneak mode.");
        StartCoroutine(HideTutorial());
    }

    public void TutConclusion() {
        tutMessages.Enqueue("When you are in this box, you will be unable to move until you hit F again to leave it.");
        tutMessages.Enqueue("Remember you have to be in a safe space in order to hide, and it's not a good idea to hide when you're being chased!");
        tutMessages.Enqueue("There are many other things you have yet to learn, such as stuff about power-ups (E key to use), but those are rare!");
        tutMessages.Enqueue("It would be better to actually experience the game and learn that way than to continue this tutorial.");
        tutMessages.Enqueue("In other words... Congratulations! You beat the tutorial! Now it's time to delve into the snowy maze.  Hit enter to leave the tutorial!");
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

            if (i == 2) { //when the tutorial message talking about the trap pops up
                transform.position = new Vector2(1, 1); //teleport the player to this tile

                player.AlterArrow(new Vector2(1, 0));

                GameManager.instance.tutTrapTile.GetComponent<Floor>().SetPainTrap(); //display a pain trap above the player
            }
            else if (i == 4) { //when the tutorial message talking about the wall tile pops up
                Vector3 pos = GameManager.instance.tutWallTile.transform.position; //get the location of the tile
                Destroy(GameManager.instance.tutWallTile); //destroy the old floor tile

                //create a destructable wall tile at the pos
                GameManager.instance.tutWallTile = Instantiate(GameManager.instance.boardScript.wallTiles
                    [GameManager.instance.boardScript.wallTiles.Length - 1], pos, Quaternion.identity);
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
            yield return new WaitForSeconds(GameManager.instance.turnDelay / 2);

            if (i == 2) { //after saying we'll start the attack tutorial
                transform.position = new Vector2(1, 1); //teleport the player to this tile
                player.AlterArrow(new Vector2(1, 0));
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

        player.Invoke("Restart", player.restartLevelDelay); //Call the restart function after a delay
    }
}
