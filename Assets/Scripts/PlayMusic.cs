using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioBasic;

public class PlayMusic : MonoBehaviour {
    public new AudioBasic  audio;
    public void DoAfterFinish(MonoBehaviour audioBase, Audio stoppedAudio, bool hasPlayedFinished) {
        if (hasPlayedFinished) {
            Debug.LogError("yes");
        }
        else {
            Debug.LogError("no");
        }

    }



    public void DoRestart(MonoBehaviour audioBase, Audio stoppedAudio, bool sameAsPlay) {
        if (sameAsPlay) {
            Debug.LogError("yes");
        }
        else {
            Debug.LogError("no");
        }


    }

    public void OnStart(MonoBehaviour audioBase, Audio audio) {
        Debug.LogError(audio.Channels);
    }

    public void OnPause(MonoBehaviour audioBase, Audio audio) {
        Debug.LogError("Pause");
    }

    public void OnResume(MonoBehaviour audioBase, Audio audio) {
        Debug.LogError("Resume");
    }
    public void OnRestart(MonoBehaviour audioBase, Audio audio, bool sameAsPlay) {
        Debug.LogError("Restart " + (sameAsPlay ? "Same as play" : "Not same as play"));
    }



    public void OnStop(MonoBehaviour audioBase, Audio audio, bool ReachEnd) {
        Debug.LogError("Stop " + (ReachEnd ? "Reach End" : "Not reach end"));
    }

    public void DoBeforeFinish(MonoBehaviour audioBase, Audio currentAudio) {
        Debug.LogError(currentAudio.Name);
    }
    public void DoBeforeFinish2(MonoBehaviour audioBase, Audio currentAudio) {
        Debug.LogError(currentAudio.Name + "2");
    }
    public void DoBeforeFinish3(MonoBehaviour audioBase, Audio currentAudio) {
        Debug.LogError(currentAudio.Name + "3");
    }

    public void DoAfterFinish2(MonoBehaviour audioBase, Audio stoppedAudio, bool hasPlayedFinished) {
        if (hasPlayedFinished) {
            Debug.LogError("yes2");
        }
        else {
            Debug.LogError("no2");
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
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            audio.SetGain(Frequency.F1k,8);
            audio.SetGain(Frequency.F2k,8);
            audio.UpdateEqualizer();
        }else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            audio.SetGain(Frequency.F1k, 0);
            audio.SetGain(Frequency.F2k, 0);
            audio.UpdateEqualizer();
        }

        if (audio?.State == NAudio.Wave.PlaybackState.Playing) {
             //var fft = audio.GetAmplitude();
           // var targetFFT = audio.GetAmplitude(new int[] { 1000 });

        }

    }
}
