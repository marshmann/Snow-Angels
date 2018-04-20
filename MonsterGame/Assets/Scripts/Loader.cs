using UnityEngine;

//Class to start up the gameManager
//Drag this onto the Main Camera in Unity and then drag the GameManager prefab into it
public class Loader : MonoBehaviour {
    public GameObject gameManager;

    void Awake() {
        if (GameManager.instance == null)
            Instantiate(gameManager);
    }
}
