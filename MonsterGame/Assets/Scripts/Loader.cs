//This script contains the class that'll start up the game manager object when the game starts.
//Follow these two steps for it to work
  //1: Drag this script onto the Main Camera in Unity
  //2: Drag the GameManager prefab into the GameObject spot 

using UnityEngine;
public class Loader : MonoBehaviour {
    public GameObject gameManager;

    void Awake() {
        if (GameManager.instance == null)
            Instantiate(gameManager);
    }
}
