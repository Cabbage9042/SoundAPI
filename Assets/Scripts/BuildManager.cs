using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public AudioBasic audioBasic;
    public AudioList audioList;
    // Start is called before the first frame update

    void Start() {

        audioBasic.SetAudio(this, Application.streamingAssetsPath + "/testing.wav");
        audioList.SetAudio(this, Application.streamingAssetsPath + "/testing.wav",0);
        
        audioBasic.AddOnAudioStarted(new MethodCalled(this, "OnStart"));
        /*


*        audioList.SetAudio(this, "C:/tarc/fyp/project/SoundAPI/SoundAPI/Assets/Audio/lala haha.mp3",0);
        audioList.SetAudio(this, "C:/tarc/fyp/project/SoundAPI/SoundAPI/Assets/Audio/testing.mp3",1);
       audioList.AddOnAudioStarted(new MethodCalled(this, "OnStart"));
        audioList.AddOnAudioStopped(new MethodCalled(this, "OnStop"));*/




    }

 


    
    public void OnStart(MonoBehaviour mono, Audio audio) {
       // Debug.LogError(mono.name);
       // Debug.LogError(audio.Name);
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Start playing the audio

            audioBasic.Play();
            //audioListSpeaker.Play();
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            audioBasic.Restart();

        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            audioBasic.Pause();
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            audioBasic.Stop();
        }
        print(audioBasic.CurrentTime);
    }

    public void OnStop(MonoBehaviour mono, Audio audio, bool isFinished) {
        var manager = (BuildManager)mono;
        Debug.LogError(manager.name);
        Debug.LogError(audio.Name);
    }

}
