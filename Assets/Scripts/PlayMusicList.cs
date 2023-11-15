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

    public void AudioStart(AudioBase audioBase, Audio audio) {
        print(audio.NameWoExtension);
    }

    public void AudioStop(AudioBase audioBase, Audio audio, bool hasFinishedPlaying) {
        // print(audio.NameWoExtension + " " + hasFinishedPlaying);
        if (hasFinishedPlaying) {
            ((AudioList)audioBase).StopNextAudio();
            
        
        }
    }

}
