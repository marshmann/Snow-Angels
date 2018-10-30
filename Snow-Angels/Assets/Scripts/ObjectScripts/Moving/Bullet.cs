﻿using UnityEngine;

public class Bullet : MovingObject {
    private float score;
    private Player parent;
    private Vector2 end;
    private Vector2 dir;

    protected override void Start() {
        score = GameManager.instance.GetFloorScore(); //store the score at the time the object was created
        GameManager.instance.ReduceFloorScore(); //make the score 0 as payment for the bullet
        parent = transform.parent.GetComponent<Player>(); //store the parent component
        transform.parent = null; //remove the parent attachment (so the projectile's movements aren't relative)

        //Set the required fields for the super class
        boxCollider = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        inverseMoveTime = 3f / moveTime;
        
        dir = parent.lastMove; //Set the direction the bullet was fired

        Vector2 start = transform.position; //the current position of the bullet
        end = start + (dir*5); //the end position
    }
    private void Update() {
        //If the bullet is at the end position
        if (transform.position.x == end.x && transform.position.y == end.y) {
            DestroyImmediate(gameObject); //destroy it, it didn't come in contact with anything
        }
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
                        enemy.stunned = true;
                        enemy.stunLength = (int)(score * 5) + 3;
                        print("Stunned Enemy for " + ((int)(score * 5) + 3) + " turns.");
                    }
                }
                DestroyImmediate(gameObject);
            }  
            //else {
                //It hit a wall.  Potentially could display an animation for this.
            //}
        }
    }

    //This'll never be called, but is required due to MovingObject's implementation
    protected override void OnCantMove<T>(T component) { return; }
}
