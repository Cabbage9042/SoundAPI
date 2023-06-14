using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour {
    public AudioBasic audio;
    public void DoAfterFinish(Audio stoppedAudio, bool hasPlayedFinished) {
        if (hasPlayedFinished) {
            print("yes");
        }
        else {
            print("no");
        }


    }
    public void DoRestart(Audio stoppedAudio, bool sameAsPlay) {
        if (sameAsPlay) {
            print("yes");
        }
        else {
            print("no");
        }


    }
    public void DoBeforeFinish(Audio currentAudio) {
        print(currentAudio.Name);
    }
    public void DoBeforeFinish2(Audio currentAudio) {
        print(currentAudio.Name + "2");
    }
    public void DoBeforeFinish3(Audio currentAudio) {
        print(currentAudio.Name + "3");
    }

    public void DoAfterFinish2(Audio stoppedAudio, bool hasPlayedFinished) {
        if (hasPlayedFinished) {
            print("yes2");
        }
        else {
            print("no2");
        }
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Start playing the audio

            audio.Play();
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            audio.Restart();

        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            audio.Pause();
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            audio.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.A)) {

            audio.AddOnAudioStarted(new AudioBasic.MethodCalled(this, "DoBeforeFinish"));
            audio.AddOnAudioStarted(new AudioBasic.MethodCalled(this, "DoBeforeFinish2"));
        }
        else if (Input.GetKeyDown(KeyCode.M)) {

            print(audio.RemoveOnAudioStarted(new AudioBasic.MethodCalled(this, "DoBeforeFinish")));
        }
        else if (Input.GetKeyDown(KeyCode.L)) {

            audio.RemoveAllOnAudioStarted();
        }

    }
}
