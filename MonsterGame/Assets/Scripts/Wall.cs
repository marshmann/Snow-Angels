//Depricated Code, as we removed the damage wall feature
using UnityEngine;

public class Wall : MonoBehaviour {

    public Sprite dmgSprite; //container for the damaged wall sprite
    public int hp = 2; //health of the wall

    private SpriteRenderer spriteRenderer;

    void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); }

    public void DamageWall(int loss) {
        spriteRenderer.sprite = dmgSprite; //Change the img to show it was damaged
        hp -= loss; //reduce the hp by the amount the player hit the wall for
        if (hp <= 0) { //check to see if the wall is destroyed

            //We need to put a floor tile where the wall was, so the spot isn't just a black-hole.
            BoardManager bm = GameManager.instance.boardScript; //Get the BoardManager from the GameManager
            GameObject chosenTile = bm.floorTiles[Random.Range(0, bm.floorTiles.Length)]; //Choose a floor tile randomly
            GameObject instance = Instantiate(chosenTile, new Vector3(transform.position.x, transform.position.y, 0f), Quaternion.identity) as GameObject; //Instantiate it

            instance.GetComponent<Floor>().SetNotTrapped(); //make sure the new tile isn't trapped
            instance.transform.SetParent(bm.boardHolder); //for organization sake, make it a child of the boardHolder object

            int x = (int)transform.position.x; int y = (int)transform.position.y;

            GameManager.instance.board[x, y] = 0; //change the board status to reflect a floor tile

            Enemy enemy = GameManager.instance.enemy; //get the enemy object       
            if (enemy != null) { //make sure the enemy AI hasn't been destroyed
                if (enemy.knownBoard[x, y] != 0) {
                    enemy.knownBoard[x, y] = 0; //change the AI's knowledge of the wall to be a floor tile (if he already saw it as a wall)
                    enemy.newInfo = true;
                }
            }

            gameObject.SetActive(false); //disable the wall object
        }
    }
}
