using UnityEngine;

public class Floor : MonoBehaviour {
    public Sprite alteredFloor; //container for the altered floor tile
    private Sprite og = null; //the original sprite of the floor tile
    private SpriteRenderer spriteRenderer; //the spriteRenderer component of the floor prefab
    private bool changed = false; //a boolean representing if the tile has been altered or not
    private string trapped = ""; //a string containing the name of the trap type

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>(); //get the spriterenderer component
        if (GameManager.instance.tutorial) return; //if the tutorial is going on, return as we don't want traps
        
        float value = Random.Range(0, 100); //generate a random number
        if (value >= 97.5) SetPainTrap(); //set the tile to be spike/pain trapped
        else if(value >= 92.5) trapped = "Wall"; //spawn a wall tile on this tile      
    }

    //changes the tile to be a pain trap
    public void SetPainTrap() {
        if(changed) spriteRenderer.sprite = og; //revert the sprite if it was altered
        trapped = "Pain"; //set the trap type
        spriteRenderer.color = Color.red; //change the color of the sprite (TODO: create alternate sprites)
    }

    public string IsTrapped() {  return trapped; } //return the trapped string
    public void SetChanged(bool changed) { this.changed = changed; } //setter for the changed flag
    public void SetNotTrapped() { trapped = ""; spriteRenderer.color = Color.white; } //remove the trap from a tile

    //alter the floor tile to have a different sprite if the player walked on it/near it
    public void AlterFloor() {
        if (trapped == "" && !changed) { //if it's not trapped and hasn't been changed yet
            og = spriteRenderer.sprite; //set the og sprite to be the current sprite
            spriteRenderer.sprite = alteredFloor; //alter the floor sprite
            changed = true; //set it so the floor can't be changed again
            GameManager.instance.IncFloorScore(); //increment the floor score counter
        }
    }
}