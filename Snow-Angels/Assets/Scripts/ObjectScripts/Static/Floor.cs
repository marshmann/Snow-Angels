using UnityEngine;

public class Floor : MonoBehaviour {
    public Sprite alteredFloor; //container for the new floor type

    private Sprite og = null;
    private SpriteRenderer spriteRenderer;
    private bool changed = false;
    private string trapped = "";

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (GameManager.instance.tutorial) return;

        //Assigning Trap Type
        float value = Random.Range(0, 100);
        if (value >= 97.5) SetPainTrap(); //Pain Trap
        else if(value >= 92.5) trapped = "Wall"; //spawn a wall tile        
    }

    public void SetPainTrap() {
        if(changed) spriteRenderer.sprite = og;
        trapped = "Pain";
        spriteRenderer.color = Color.red;
    }

    public string IsTrapped() {  return trapped; }
    public void SetNotTrapped() { trapped = ""; spriteRenderer.color = Color.white; }

    public void AlterFloor() {
        if (trapped == "" && !changed) {
            og = spriteRenderer.sprite;
            spriteRenderer.sprite = alteredFloor; //alter the floor sprite
            changed = true; //set it so the floor can't be changed again
            GameManager.instance.SetFloorScore(); //increment the floor score counter
        }
    }
}