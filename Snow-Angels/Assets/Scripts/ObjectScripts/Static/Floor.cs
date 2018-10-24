using UnityEngine;

public class Floor : MonoBehaviour {
    public Sprite alteredFloor; //container for the new floor type

    private SpriteRenderer spriteRenderer;
    private bool changed = false;
    private string trapped = "";

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        float value = Random.Range(0, 100);

        //Assigning Trap Type
        if (value >= 97.5) { //Pain Trap
            trapped = "Pain";
            spriteRenderer.color = Color.red; //test purposes, should change sprite instead
        }
        else if(value >= 92.5) trapped = "Wall"; //spawn a wall tile        
    }

    public string IsTrapped() {  return trapped; }
    public void SetNotTrapped() { trapped = ""; spriteRenderer.color = Color.white; }

    public void AlterFloor() {
        if (trapped == "" && !changed) {
            spriteRenderer.sprite = alteredFloor; //alter the floor sprite
            changed = true; //set it so the floor can't be changed again
            GameManager.instance.SetFloorScore(); //increment the floor score counter
        }
    }
}