using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioBasic;

public class PlayMusic : MonoBehaviour {
    public AudioBasic audio;
    public void DoAfterFinish(AudioBase audioBase, Audio stoppedAudio, bool hasPlayedFinished) {
        if (hasPlayedFinished) {
            print("yes");
        }
        else {
            print("no");
        }

    }



    public void DoRestart(AudioBase audioBase, Audio stoppedAudio, bool sameAsPlay) {
        if (sameAsPlay) {
            print("yes");
        }
        else {
            print("no");
        }


    }

    public void OnStart(AudioBase audioBase, Audio audio) {
        print(audio.Channels);
    }

    public void OnPause(AudioBase audioBase, Audio audio) {
        print("Pause");
    }

    public void OnResume(AudioBase audioBase, Audio audio) {
        print("Resume");
    }
    public void OnRestart(AudioBase audioBase, Audio audio, bool sameAsPlay) {
        print("Restart " + (sameAsPlay ? "Same as play" : "Not same as play"));
    }



    public void OnStop(AudioBase audioBase, Audio audio, bool ReachEnd) {
        print("Stop " + (ReachEnd ? "Reach End" : "Not reach end"));
    }

    public void DoBeforeFinish(AudioBase audioBase, Audio currentAudio) {
        print(currentAudio.Name);
    }
    public void DoBeforeFinish2(AudioBase audioBase, Audio currentAudio) {
        print(currentAudio.Name + "2");
    }
    public void DoBeforeFinish3(AudioBase audioBase, Audio currentAudio) {
        print(currentAudio.Name + "3");
    }

    public void DoAfterFinish2(AudioBase audioBase, Audio stoppedAudio, bool hasPlayedFinished) {
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
            //audioSpeaker.Play();
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

            audio.AddOnAudioStarted(new MethodCalled(this, "DoBeforeFinish"));
            audio.AddOnAudioStarted(new MethodCalled(this, "DoBeforeFinish2"));
        }
        else if (Input.GetKeyDown(KeyCode.M)) {

            print(audio.RemoveOnAudioStarted(new MethodCalled(this, "DoBeforeFinish")));
        }
        else if (Input.GetKeyDown(KeyCode.L)) {

            audio.RemoveAllOnAudioStarted();
        }
        else if (Input.GetKeyDown(KeyCode.C)) {
        }

        if (audio?.State == NAudio.Wave.PlaybackState.Playing) {
             //var fft = audio.GetAmplitude();
            var targetFFT = audio.GetAmplitude(new int[] { 1000 });

        }

    }
}
