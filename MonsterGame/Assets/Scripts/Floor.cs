﻿using UnityEngine;

public class Floor : MonoBehaviour {
    public Sprite alteredFloor; //container for the new floor type

    private SpriteRenderer spriteRenderer;
    private bool changed = false;
    private bool trapped = false;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        float value = Random.Range(0, 100);

        if (value >= 50) trapped = true;
    }

    public bool IsTrapped() {
        if(trapped) print("trapped tile");
        return trapped;
    }

    public void AlterFloor() {
        if (!changed) {
            //spriteRenderer.sprite = alteredFloor; //alter the floor sprite
            spriteRenderer.color = Color.blue; //temporary alteration to the floor
            changed = true; //set it so the floor can't be changed again
            GameManager.instance.SetFloorScore(); //increment the floor score counter
        }
    }
}