using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusicList : MonoBehaviour {

    public AudioList audioList;
    public void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            audioList.Play();
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            audioList.Stop();
        }
    }




}
