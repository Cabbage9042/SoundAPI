using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusicList : MonoBehaviour {

    public AudioList audioList;
    public void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Start playing the audio
            Debug.LogError("playing");
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
        Debug.LogError(audio.NameWoExtension);
    }

    public void AudioStop(MonoBehaviour audioBase, Audio audio, bool hasFinishedPlaying) {
        // print(audio.NameWoExtension + " " + hasFinishedPlaying);
        if (hasFinishedPlaying) {
            // ((AudioList)audioBase).StopNextAudio();
            Debug.LogError("finish");

        }
        else {
            Debug.LogError("not finish");
        }
    }

}
