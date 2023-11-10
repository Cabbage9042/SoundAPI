using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AudioBasicUIManager : MonoBehaviour {

    string directory = null;
    string path;
    public AudioBasic audioBasic;
    public GameObject[] eventButton;
    public GameObject pitchSlider;
    public GameObject volumeSlider;
    public GameObject loopToggle;
 public double[] getAmplitude() {
        return audioBasic?.GetAmplitude();
    }

    //select your audio1
    public void getAudioPath() {

        if (directory == null) directory = Application.dataPath;

        path = EditorUtility.OpenFilePanel("Select your audio", directory, "wav,mp3");
        if (string.IsNullOrEmpty(path)) {
            print("No audio is selected!");
        }
        else {
            directory = getDirectory(path);
            print(path);
        }
    }

   

    //select your audio2
    public async void addAudioBasic() {

        
        audioBasic = GameObject.Find("Audio").GetComponent<AudioBasic>();// GetComponent<AudioBasic>();
        if (audioBasic == null) {
            audioBasic = GameObject.Find("Audio").AddComponent(typeof(AudioBasic)) as AudioBasic;
            eventButton[0].GetComponent<Button>().onClick.AddListener(() => audioBasic.Stop());
            eventButton[1].GetComponent<Button>().onClick.AddListener(() => audioBasic.Pause());
            eventButton[2].GetComponent<Button>().onClick.AddListener(() => audioBasic.Play());
            eventButton[3].GetComponent<Button>().onClick.AddListener(() => audioBasic.Play());
            eventButton[4].GetComponent<Button>().onClick.AddListener(() => audioBasic.Restart());
        }
        else {
            audioBasic.Stop();
        }
        if(path.EndsWith(".mp3")) {
            Audio.MP3toWAV(path);
            path = path.Replace(".mp3",".wav");
        }
        if(path != "")
            audioBasic.setAudioClip(await LoadAudioClip(), path);
        audioBasic.loop = loopToggle.GetComponent<Toggle>().isOn;

    }

    public void toggleLoop() {
        if (audioBasic == null) return;
        if (loopToggle.GetComponent<Toggle>().isOn) {
            audioBasic.loop = true;
        }
        else {
            audioBasic.loop = false;
        }
    }

    async Task<AudioClip> LoadAudioClip() {
        AudioClip clip = null;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV)) {
            uwr.SendWebRequest();

            // wrap tasks in try/catch, otherwise it'll fail silently
            try {
                while (!uwr.isDone) await Task.Delay(5);

                if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                else {
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            catch (System.Exception err) {
                Debug.Log($"{err.Message}, {err.StackTrace}");
            }
        }

        return clip;
    }


    public string getDirectory(string path) {

        string tempDirectory = path;
        while (tempDirectory.Length >= 1 && tempDirectory[tempDirectory.Length - 1] != '/') {
            tempDirectory = tempDirectory.Remove(tempDirectory.Length - 1, 1);
        }

        return tempDirectory;
    }
}
