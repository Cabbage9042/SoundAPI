using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusicList : MonoBehaviour {

    public AudioList audioList;
    public void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Start playing the audio

            audioList.Play();
            //audioSpeaker.Play();
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            audioList.Restart();

        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            audioList.Pause();
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            audioList.Stop();
        }


    }

    public void AudioStart(MonoBehaviour audioBase, Audio audio) {
        print(audio.NameWoExtension);
    }

    public void AudioStop(MonoBehaviour audioBase, Audio audio, bool hasFinishedPlaying) {
        // print(audio.NameWoExtension + " " + hasFinishedPlaying);
        if (hasFinishedPlaying) {
            // ((AudioList)audioBase).StopNextAudio();
            print("finish");

        }
        else {
            print("not finish");
        }
    }

}
