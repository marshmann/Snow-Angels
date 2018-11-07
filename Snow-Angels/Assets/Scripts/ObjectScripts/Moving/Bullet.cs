using UnityEngine;

public class Bullet : MovingObject {
    private float score; //the current score the player has
    private Vector2 end; //the max distance the projectile will go
    public AudioClip enemyHit; //sound effect for when player hits enemy
    public Vector2 dir; //the direction the player was facing when the projectile was shot

    protected override void Start() {
        score = GameManager.instance.GetFloorScore(); //store the score at the time the object was created
        GameManager.instance.ReduceFloorScore(); //make the score 0 as payment for the bullet
        transform.SetParent(null); //remove the parent attachment (so the projectile's movements aren't relative)
        boxCollider = GetComponent<BoxCollider2D>(); //get boxcollider component
        rb2d = GetComponent<Rigidbody2D>(); //get the rigidbody component
        inverseMoveTime = 3f / moveTime; //store the inverse move time
        Vector2 start = transform.position; //the current position of the bullet
        end = start + (dir*5); //the end position
    }

    private void Update() {
        //If the bullet is at the end position, destroy it, as it didn't come in contact with anything
        if (transform.position.x == end.x && transform.position.y == end.y) DestroyImmediate(gameObject);         
        else {
            RaycastHit2D hit; //reference to object bullet hit during "Move"
            if (!Move((int)dir.x, (int)dir.y, out hit)) { //if it can't move then it hit something
                Enemy enemy = hit.transform.gameObject.GetComponent<Enemy>();
                if(hit.transform.tag == "Enemy") { //if it hit an enemy
                    if (score >= 1) { //Kill the enemy if the score bar is full
                        GameManager.instance.enemies.Remove(enemy); //remove it from enemy list
                        DestroyImmediate(hit.transform.gameObject); //destroy the enemy
                    }
                    else { //Stun the Enemy
                        enemy.ps.Play(); //display stun effect
                        enemy.stunned = true; //set the enemy to be stunned
                        enemy.stunLength = (int)(score * 5) + 3; //calculate stun length
                        print("Stunned Enemy for " + ((int)(score * 5) + 3) + " turns."); //temp print
                    }

                    SoundManager.instance.PlaySingle(enemyHit); //play the enemy hit sound effect
                }
                DestroyImmediate(gameObject); //destroy the bullet
            }
        }
    }

    protected override void OnCantMove<T>(T component) { return; }
}
