//Code is the same as the unity 2d roguelike tutorial, since audio is just a bonus.
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public AudioSource efxSource; //sound effects
    public AudioSource musicSource; //music
    public static SoundManager instance = null;

    //The below pitch ranges allow the audio to vary VERY minorly
    public float lowPitchRange = 0.95f;
    public float highPitchRange = 1f;

    private void Awake() {
        //Same code here as in GameManager - only allow one instance of this manager at a time
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    //Used to play single audioclips
    public void PlaySingle(AudioClip clip) {
        efxSource.clip = clip; //set the clip
        efxSource.Play(); //play it
    }

    //params allows us to pass in a comma seperated list of objects of the same time
    //In other words, we can send multiple audioclips to this function
    public void RandomizeSFX(params AudioClip [] clips) {
        int randomIndex = Random.Range(0, clips.Length); //choose a random clip to play
        float randomPitch = Random.Range(lowPitchRange, highPitchRange); //choose a random pitch

        efxSource.pitch = randomPitch; //set the pitch
        efxSource.clip = clips[randomIndex]; //set the clip
        efxSource.Play(); //play it
    }
}
