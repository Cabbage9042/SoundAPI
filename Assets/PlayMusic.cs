using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    public AudioBasic audio;
   public void DoAfterFinish(Audio stoppedAudio, bool hasPlayedFinished) {
        if (hasPlayedFinished) {
            print("yes");
        }
        else {
            print("no");
        }
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
            //output.Stop();
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
       /* else if (Input.GetKeyDown(KeyCode.Plus)) {
            audio.OnAudioStopped += Audio_OnAudioStopped;
        }
        else if (Input.GetKeyDown(KeyCode.Minus)) {
            audio.OnAudioStopped += Audio_OnAudioStopped;
        }*/

    }
}
