//Depricated Code, as we removed the damage wall feature
using UnityEngine;

public class Wall : MonoBehaviour {

    public Sprite dmgSprite; //container for the damaged wall sprite
    public int hp = 2; //health of the wall

    private SpriteRenderer spriteRenderer;

    //Below are the audio containers
    public AudioClip chopSound1;
    public AudioClip chopSound2;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall(int loss) {
        SoundManager.instance.RandomizeSFX(chopSound1, chopSound2);
        spriteRenderer.sprite = dmgSprite; //Change the img to show it was damaged
        hp -= loss; //reduce the hp by the amount the player hit the wall for
        if (hp <= 0) //check to see if the wall is destroyed
            gameObject.SetActive(false); //if so, remove it from the board (set it to not active)
    }
}
