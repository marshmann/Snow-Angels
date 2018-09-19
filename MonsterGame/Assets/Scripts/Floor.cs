using UnityEngine;

public class Floor : MonoBehaviour {
    public Sprite alteredFloor; //container for the new floor type

    private SpriteRenderer spriteRenderer;
    private bool changed = false;
    private string trapped = "";
    //private int turnLimit = 0;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        float value = Random.Range(0, 100);

        //Assigning Trap Type
        if (value >= 97.5) { //Pain Trap
            trapped = "Pain";
            spriteRenderer.color = Color.red; //test purposes
        }
        else if(value >= 92.5) { //Ice Trap
            trapped = "Ice";
            spriteRenderer.color = Color.gray; //test purposes
        }
        /*else {
            if (GameManager.instance.CanTurnBack()) {
                //int snowRate = (int)GameManager.instance.GetSnowRate();
                //turnLimit = Random.Range(snowRate - 5, snowRate + 5);
            }
        }*/
    }

    private void Update() {
        if (changed && GameManager.instance.CanTurnBack()) {
            //Check if player moved, reduce turnLimit by 1 until it's 0
            //ALTERNATIVELY: make it so there's a small chance it'll turn back.
        }
    }

    public string IsTrapped() {  return trapped; }
    public void SetNotTrapped() { trapped = ""; spriteRenderer.color = Color.white; }

    public void AlterFloor() {
        if (!changed && trapped == "") {
            //spriteRenderer.sprite = alteredFloor; //alter the floor sprite
            spriteRenderer.color = Color.blue; //temporary alteration to the floor
            changed = true; //set it so the floor can't be changed again
            GameManager.instance.SetFloorScore(); //increment the floor score counter
        }
    }
}