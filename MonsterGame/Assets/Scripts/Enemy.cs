using UnityEngine;

public class Enemy : MovingObject {
    public int playerDamage; //Amount of food damage the player loses when hit by this enemy
    //The player damage value is set in Unity as a variable in the Enemy1 and Enemy2 prefab under the Enemy component

    private Animator animator; //the animator for the enemy
    private Transform target; //use to store player position (where the enemies will move toward)
    private bool skipMove; //enemies move every other turn

    //Below are containers for the audio effects
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;

    protected override void Start() {
        GameManager.instance.AddEnemyToList(this); //have the enemy add itself to the list in game manager
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
    }

    protected override void AttemptMove<T>(int xDir, int yDir) {
        //check to see if the enemy can move or not
        if (skipMove) {
            skipMove = false;
            return;
        }

        base.AttemptMove<T>(xDir, yDir);

        skipMove = true;
    }

    public void MoveEnemy() {
        int xDir = 0;
        int yDir = 0;

        /*  If the player's x coordinate and the enemy's x cordinate are the same 
         * (or extremely close, as represented by float.Epsilon)
         * Then we'll move in the y direction toward the player
         * Otherwise, we'll move in the x direction
         */
        if(Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon) {
            //If the player's y value is greater than the y value, he's above the enemy (so move up 1)
            //else he'll be below the enemy (so move down 1)
            yDir = target.position.y > transform.position.y ? 1 : -1;
        }
        else
            xDir = target.position.x > transform.position.x ? 1 : -1; //same as above but for x values

        AttemptMove<Player>(xDir, yDir); //Attempt to move toward the player, assuming the enemy might run into the player
    }

    protected override void OnCantMove<T>(T component) {
        Player hitPlayer = component as Player; //cast the component to be player
        animator.SetTrigger("enemyAttack"); //have the enemy visually attack the player
        SoundManager.instance.RandomizeSFX(enemyAttack1, enemyAttack2); //play a random attack sound
        hitPlayer.LoseFood(playerDamage); //hit the player, so he/she loses food related to the playerDamage number
    }
}
