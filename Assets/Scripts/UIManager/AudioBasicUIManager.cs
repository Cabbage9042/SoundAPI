using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
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
    public GameObject audioNameText;
    public GameObject monoDropDown;
    public GameObject leftDropDown;
    public GameObject rightDropDown;
    public GameObject stereo;

    public GameObject equalizerUI;


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

    private void Start() {
        List<string> speakerList = new(AudioBasic.speakerDevicesName);

        monoDropDown.GetComponent<TMP_Dropdown>().AddOptions(speakerList);
        leftDropDown.GetComponent<TMP_Dropdown>().AddOptions(speakerList);
        rightDropDown.GetComponent<TMP_Dropdown>().AddOptions(speakerList);
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
        if (path.EndsWith(".mp3")) {
            Audio.MP3toWAV(path);
            path = path.Replace(".mp3", ".wav");
        }
        if (path != "")
            audioBasic.setAudioClip(this, await LoadAudioClip(), path);

        audioBasic.AddOnAudioStarted(new MethodCalled(this, "AudioStarted"));
        audioBasic.AddOnAudioPaused(new MethodCalled(this, "AudioPaused"));
        audioBasic.AddOnAudioRestarted(new MethodCalled(this, "AudioRestarted"));
        audioBasic.AddOnAudioResumed(new MethodCalled(this, "AudioResumed"));
        audioBasic.AddOnAudioStopped(new MethodCalled(this, "AudioStopped"));

        pitchSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioBasic.ChangePitch(pitchSlider.GetComponent<Slider>().value); });
        volumeSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioBasic.ChangeVolume(volumeSlider.GetComponent<Slider>().value); });

        monoDropDown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate { audioBasic.SetMonoSpeakerNumber(monoDropDown.GetComponent<TMP_Dropdown>().value); });


        audioBasic.loop = loopToggle.GetComponent<Toggle>().isOn;

    }

    public void ChangePitch() {
        audioBasic.ChangePitch(pitchSlider.GetComponent<Slider>().value);
    }
    public void ToggleStereo() {
        var toggle = stereo.GetComponent<Toggle>();
        if (toggle.isOn) {//stereo
            leftDropDown.SetActive(true);
            rightDropDown.SetActive(true);
            monoDropDown.SetActive(false);
        }
        else { // mono

            leftDropDown.SetActive(false);
            rightDropDown.SetActive(false);
            monoDropDown.SetActive(true);
        }

        audioBasic.Stereo = toggle.isOn;

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

    public void OpenEqualizerUI() {
        equalizerUI.SetActive(true);
        
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

    public void ChangeAudioName(AudioBasicUIManager audioManager, string status) {

        var name = audioManager.audioNameText.GetComponent<TextMeshProUGUI>();
        if (audioManager.audioBasic == null) {
            name.text = "No audio is selected!";
        }
        else {
            name.text = status;
        }
    }

    public void AudioStarted(MonoBehaviour audioManager, Audio audio) {
        ChangeAudioName((AudioBasicUIManager)audioManager, "Now playing: " + audio.Name);

    }
    public void AudioPaused(MonoBehaviour audioManager, Audio audio) {
        ChangeAudioName((AudioBasicUIManager)audioManager, "Paused: " + audio.Name);
    }

    public void AudioRestarted(MonoBehaviour audioManager, Audio audio, bool sameAsPlay) {
        ChangeAudioName((AudioBasicUIManager)audioManager, "Restarted: " + audio.Name);

    }

    public void AudioResumed(MonoBehaviour audioManager, Audio audio) {
        ChangeAudioName((AudioBasicUIManager)audioManager, "Resumed: " + audio.Name);
    }
    public void AudioStopped(MonoBehaviour audioManager, Audio audio, bool hasFinishedPlaying) {
        ChangeAudioName((AudioBasicUIManager)audioManager, (hasFinishedPlaying ? "Finished playing: " : "Stopped: ") + audio.Name);

    }

    public string getDirectory(string path) {

        string tempDirectory = path;
        while (tempDirectory.Length >= 1 && tempDirectory[tempDirectory.Length - 1] != '/') {
            tempDirectory = tempDirectory.Remove(tempDirectory.Length - 1, 1);
        }

        return tempDirectory;
    }
}
