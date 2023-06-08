using UnityEngine;
using NAudio.Wave;
using System.Windows;

public class AudioPlayer : MonoBehaviour {
    private Audio audio = null;

    private void Start() {  

        audio = new Audio($"{Application.dataPath}/Audio/testing3.mp3");
        audio.OnAudioStopped += Audio_OnAudioStopped;
    }

    private void Audio_OnAudioStopped(Audio stoppedAudio, bool hasFinishedPlaying) {
        if (hasFinishedPlaying) {
            print("the audio done");
        }
        else {
            print("not done");
        }
        
    }


    

    private void Update() {
        // Play audio when the user presses the spacebar

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
        //print(wave.currentState.ToString());
        
    }

    private void OnDestroy() {
        // Stop and dispose the WaveOut instance when the script is destroyed
        audio?.Stop();
        audio?.Dispose();
    }
}