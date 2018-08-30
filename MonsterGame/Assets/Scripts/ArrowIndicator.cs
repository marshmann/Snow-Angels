using UnityEngine;

public class ArrowIndicator : MonoBehaviour {
    //Below code will always have the arrow indicator around the enemy target the player
    //however it's overriden by the SetDirArrow() function in Enemy.cs, thus this code is depricated.
    /*
    private Transform target;

    private void Start() {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update () {
        var dir = target.position - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        transform.GetChild(0).rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    */
}
