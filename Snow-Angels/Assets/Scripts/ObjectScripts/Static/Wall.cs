using UnityEngine;

public class Wall : MonoBehaviour {
    public Sprite dmgSprite; //container for the damaged wall sprite
    public int hp = 2; //health of the destructable wall
    private SpriteRenderer spriteRenderer; //spriterenderer component of the wall prefab

    private void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); /* init spriterenderer component */}

    //Damage the wall by a specific amount
    public void DamageWall(int loss) {
        spriteRenderer.sprite = dmgSprite; //Change the img to show it was damaged
        hp -= loss; //reduce the hp by the amount the player hit the wall for
        if (hp <= 0) { //check to see if the wall is destroyed
            //get the transform's x and y values
            int x = (int)transform.position.x; int y = (int)transform.position.y;

            //We need to put a floor tile where the wall was, so the spot isn't just a black-hole.
            BoardManager bm = GameManager.instance.boardScript; //Get the BoardManager from the GameManager
            GameObject chosenTile = bm.floorTiles[Random.Range(0, bm.floorTiles.Length)]; //Choose a floor tile randomly
            GameObject instance = Instantiate(chosenTile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject; //Instantiate it at the transform's pos

            instance.GetComponent<Floor>().SetNotTrapped(); //make sure the new tile isn't trapped
            instance.transform.SetParent(bm.boardHolder); //for organization sake, make it a child of the boardHolder object            

            //change the board status to reflect a floor tile is now there
            int[,] board = GameManager.instance.GetBoard(); //get the board
            if(board != null) board[x, y] = 0; //set the coord to be a floor tile

            GameManager.instance.SetBoard(board); //store the altered board

            foreach (Enemy enemy in GameManager.instance.enemies) { //loop over every enemy in the list
                if (enemy.knownBoard[x, y] != 0) { //if the enemy knew this tile was a wall before it got destroyed
                    enemy.knownBoard[x, y] = 0; //change the AI's knowledge of the wall to be a floor tile (if he already saw it as a wall)
                    enemy.newInfo = true; //inform the enemy that they their knowledge pool was altered
                }
            }
            gameObject.SetActive(false); //disable the wall object
        }
    }
}
