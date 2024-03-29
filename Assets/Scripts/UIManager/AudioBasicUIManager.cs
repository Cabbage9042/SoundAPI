
using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
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
    public GameObject panningSlider;
    public GameObject loopToggle;
    public GameObject audioNameText;
    public GameObject monoDropDown;

    public TextMeshProUGUI pitchValueLabel;
    public TextMeshProUGUI volumeValueLabel;
    public TextMeshProUGUI panningValueLabel;
    public Button pitchResetButton;
    public Button volumeResetButton;
    public Button panningResetButton;

    public GraphManager graphManager;

    public Slider slider31;
    public Slider slider63;
    public Slider slider125;
    public Slider slider250;
    public Slider slider500;
    public Slider slider1k;
    public Slider slider2k;
    public Slider slider4k;
    public Slider slider8k;
    public Slider slider16k;

    public double[] GetAmplitude() {
        return audioBasic?.GetAmplitude(0.3f);
    }
    public double[] GetAmplitude(double[] amplitudes, int[] targetFrequencies, int sampleRate) {
        return audioBasic?.GetAmplitude(amplitudes, targetFrequencies, sampleRate);
    }
    private void Start() {
        List<string> speakerList = new(AudioBasic.SpeakerDevicesName);

        monoDropDown.GetComponent<TMP_Dropdown>().AddOptions(speakerList);

        FileBrowser.SetFilters(true, new FileBrowser.Filter("Audio", ".mp3", ".wav"));
        FileBrowser.SetDefaultFilter(".wav");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        audioBasic = GameObject.Find("Audio").GetComponent<AudioBasic>();// GetComponent<AudioBasic>();
        audioBasic = GameObject.Find("Audio").AddComponent(typeof(AudioBasic)) as AudioBasic;
        eventButton[0].GetComponent<Button>().onClick.AddListener(() => audioBasic.Stop());
        eventButton[1].GetComponent<Button>().onClick.AddListener(() => audioBasic.Pause());
        eventButton[2].GetComponent<Button>().onClick.AddListener(() => audioBasic.Play());
        eventButton[3].GetComponent<Button>().onClick.AddListener(() => audioBasic.Play());
        eventButton[4].GetComponent<Button>().onClick.AddListener(() => audioBasic.Restart());



        monoDropDown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate { audioBasic.SetSpeakerNumber(monoDropDown.GetComponent<TMP_Dropdown>().value); });
        pitchSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioBasic.SetPitch(pitchSlider.GetComponent<Slider>().value); });
        volumeSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioBasic.SetVolume(volumeSlider.GetComponent<Slider>().value); });
        panningSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioBasic.SetPanning(panningSlider.GetComponent<Slider>().value); });





        audioBasic.Loop = loopToggle.GetComponent<Toggle>().isOn;
    }

    //select your audio1
    public void getAudioPath() {
        if (directory == null) directory = Application.streamingAssetsPath;
        FileBrowser.ShowLoadDialog((paths) => { GetPathOnSuccess(paths[0]); }, () => { GetPathOnCancel(); },
            FileBrowser.PickMode.Files, initialPath: directory);




        //path = EditorUtility.OpenFilePanel("Select your audio", directory, "wav,mp3");
        /* if (string.IsNullOrEmpty(path)) {
             print("No audio is selected!");
         }
         else {
             directory = getDirectory(path);
             print(path);
         }*/
    }
    private void GetPathOnSuccess(string path) {
        directory = getDirectory(path);
        print(path);
        this.path = path;

        addAudioBasic();
        ChangeAudioNameSelected();
    }
    private void GetPathOnCancel() {
        print("No audio is selected!");
    }
    //select your audio2
    public void addAudioBasic() {



        audioBasic.Stop();



        audioBasic.SetAudio(this, path);


        audioBasic.AddOnAudioStarted(new MethodCalled(this, "AudioStarted"));
        audioBasic.AddOnAudioPaused(new MethodCalled(this, "AudioPaused"));
        audioBasic.AddOnAudioRestarted(new MethodCalled(this, "AudioRestarted"));
        audioBasic.AddOnAudioResumed(new MethodCalled(this, "AudioResumed"));
        audioBasic.AddOnAudioStopped(new MethodCalled(this, "AudioStopped"));




    }

    //select ur audio 3
    public void ChangeAudioNameSelected() {
        ChangeAudioName(this, audioBasic.Name + " is selected!");
    }


    public void ChangePitchValueLabel() {
        pitchValueLabel.text = Math.Round(pitchSlider.GetComponent<Slider>().value, 2).ToString();
    }
    public void ChangeVolumeValueLabel() {
        volumeValueLabel.text = Math.Round(volumeSlider.GetComponent<Slider>().value, 2).ToString();
    }
    public void ChangePanningValueLabel() {
        panningValueLabel.text = Math.Round(panningSlider.GetComponent<Slider>().value, 2).ToString();
    }

    public void ResetPitch() {
        audioBasic.SetPitch(1);
        pitchSlider.GetComponent<Slider>().value = 1;
    }
    public void ResetVolume() {
        audioBasic.SetVolume(1);
        volumeSlider.GetComponent<Slider>().value = 1;
    }
    public void ResetPanning() {
        audioBasic.SetPanning(0);
        panningSlider.GetComponent<Slider>().value = 0;
    }

    public void toggleLoop() {
        if (audioBasic == null) return;
        if (loopToggle.GetComponent<Toggle>().isOn) {
            audioBasic.Loop = true;
        }
        else {
            audioBasic.Loop = false;
        }
    }
    /*
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
    }*/

    public void ChangeAudioName(AudioBasicUIManager audioManager, string status) {
        if(audioManager == null) return;
        var name = audioManager.audioNameText.GetComponent<TextMeshProUGUI>();
        if (audioManager.audioBasic == null) {
            name.text = "No audio is selected!";
        }
        else {
            name.text = status;
        }
    }
    public void Exit() {
        Application.Quit();
    }

    #region SetAudioStatus

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


    #endregion

    #region Equalizer

    public void SetEqualizer31() {
        audioBasic?.SetGain(Frequency.F31, slider31.value);
        audioBasic?.UpdateEqualizer();
    }
    public void SetEqualizer63() {

        audioBasic?.UpdateEqualizer();
        audioBasic?.SetGain(Frequency.F63, slider63.value);
    }
    public void SetEqualizer125() {

        audioBasic?.SetGain(Frequency.F125, slider125.value);
        audioBasic?.UpdateEqualizer();
    }
    public void SetEqualizer250() {

        audioBasic?.SetGain(Frequency.F250, slider250.value);
        audioBasic?.UpdateEqualizer();
    }
    public void SetEqualizer500() {

        audioBasic?.SetGain(Frequency.F500, slider500.value);
        audioBasic?.UpdateEqualizer();
    }
    public void SetEqualizer1k() {

        audioBasic?.SetGain(Frequency.F1k, slider1k.value);
        audioBasic?.UpdateEqualizer();
    }
    public void SetEqualizer2k() {

        audioBasic?.SetGain(Frequency.F2k, slider2k.value);
        audioBasic?.UpdateEqualizer();
    }
    public void SetEqualizer4k() {

        audioBasic?.SetGain(Frequency.F4k, slider4k.value);
        audioBasic?.UpdateEqualizer();
    }
    public void SetEqualizer8k() {
        audioBasic?.SetGain(Frequency.F8k, slider8k.value);
        audioBasic?.UpdateEqualizer();

    }
    public void SetEqualizer16k() {
        audioBasic.SetGain(Frequency.F16k, slider16k.value);
        audioBasic.UpdateEqualizer();

    }
    #endregion

    public string getDirectory(string path) {

        string tempDirectory = path;
        while (tempDirectory.Length >= 1 && tempDirectory[tempDirectory.Length - 1] != '/') {
            tempDirectory = tempDirectory.Remove(tempDirectory.Length - 1, 1);
        }

        return tempDirectory;
    }
}