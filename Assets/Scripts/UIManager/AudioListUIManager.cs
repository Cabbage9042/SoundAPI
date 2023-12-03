
using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AudioList;

public class AudioListUIManager : MonoBehaviour {

    string directory = null;
    string path;

    public GameObject audioNameText;

    public GameObject audioListSet;
    public GameObject audioContent;
    private List<GameObject> audioListContentSet;

    public GameObject addAudioButton;

    public GameObject equalizerListSet;
    public GameObject equalizerContent;
    private List<GameObject> equalizerListContentSet;

    public Button[] eventButton;

    public GameObject audioCanvas;
    public AudioList audioList;
    public TMP_Dropdown loopModeDropDown;

    public GameObject pitchSlider;
    public GameObject volumeSlider;
    public GameObject panningSlider;
    public GameObject speakerDropDown;

    public GameObject[] equalizerSliderArray;
    public GameObject equalizerCanvas;

    public TextMeshProUGUI pitchValueLabel;
    public TextMeshProUGUI volumeValueLabel;
    public TextMeshProUGUI panningValueLabel;

    private int indexSelected;

    private void Start() {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Audio", ".mp3", ".wav"));
        FileBrowser.SetDefaultFilter(".wav");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        audioList = GameObject.Find("Audio").AddComponent(typeof(AudioList)) as AudioList;
        audioList.usingScript = true;
        audioListContentSet = new();
        equalizerListContentSet = new();

        eventButton[0].onClick.AddListener(() => audioList.Stop());
        eventButton[1].onClick.AddListener(() => audioList.Pause());
        eventButton[2].onClick.AddListener(() => audioList.Play());
        eventButton[3].onClick.AddListener(() => audioList.Play());
        eventButton[4].onClick.AddListener(() => audioList.Restart());


        List<string> loopModeList = new();
        for (int i = 0; i < Enum.GetNames(typeof(AudioList.LoopMode)).Length; i++) {
            string enumName = ((AudioList.LoopMode)Enum.GetValues(typeof(AudioList.LoopMode)).GetValue(i)).ToString();
            loopModeList.Add(enumName);
        }
        List<string> speakerList = new(AudioList.speakerDevicesName);

        speakerDropDown.GetComponent<TMP_Dropdown>().AddOptions(speakerList);
        loopModeDropDown.AddOptions(loopModeList);


        audioList.AddOnAudioStarted(new MethodCalled(this, "AudioStarted"));
        audioList.AddOnAudioPaused(new MethodCalled(this, "AudioPaused"));
        audioList.AddOnAudioRestarted(new MethodCalled(this, "AudioRestarted"));
        audioList.AddOnAudioResumed(new MethodCalled(this, "AudioResumed"));
        audioList.AddOnAudioStopped(new MethodCalled(this, "AudioStopped"));


        pitchSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioList.SetPitch(pitchSlider.GetComponent<Slider>().value); });
        volumeSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioList.SetVolume(volumeSlider.GetComponent<Slider>().value); });
        panningSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioList.SetPanning(panningSlider.GetComponent<Slider>().value); });

        speakerDropDown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate { audioList.SetSpeakerNumber(speakerDropDown.GetComponent<TMP_Dropdown>().value); });



    }

    public void OpenAudioCanvas() {
        audioCanvas.SetActive(true);
        Canvas canvas = audioCanvas.GetComponent<Canvas>();

        //canvas.transform.FindChild("Foreground").FindChild("").FindChild("Content")
    }
    public void CloseAudioCanvas() {

        audioCanvas.SetActive(false);
    }

    public double[] GetAmplitude() {
        return audioList?.GetAmplitude(0.3f);
    }
    public double[] GetAmplitude(double[] amplitudes, int[] targetFrequencies, int sampleRate) {
        return audioList?.GetAmplitude(amplitudes, targetFrequencies, sampleRate);
    }
    public void AddNewAudio() {

        audioListContentSet.Add(Instantiate(audioListSet, audioContent.transform));

        int audioIndex = audioListContentSet.Count - 1;

        audioListContentSet[audioIndex].name = (audioIndex).ToString();

        Button audioButton = audioListContentSet[audioIndex].transform.Find("Audio Button").GetComponent<Button>();
        TMP_Dropdown equalizerDropDown = audioListContentSet[audioIndex].transform.Find("Equalizer Drop Down").GetComponent<TMP_Dropdown>();
        Button deleteButton = audioListContentSet[audioIndex].transform.Find("Delete Button").GetComponent<Button>();

        audioButton.onClick.AddListener(delegate { getAudioPath(); });
        audioButton.name = "Audio Button " + (audioIndex).ToString();

        List<string> nameList = new(audioList.EqualizerListName);
        equalizerDropDown.AddOptions(nameList);

        equalizerDropDown.onValueChanged.AddListener(delegate { AudioChangeEqualizer(audioIndex); });

        deleteButton.onClick.AddListener(delegate { GetIndexSelected(audioIndex); });
        deleteButton.onClick.AddListener(delegate { RemoveAudio(); });
        deleteButton.name = "Delete Button " + (audioIndex).ToString();

        //disable add audio
        addAudioButton.GetComponent<Button>().enabled = false;
        addAudioButton.GetComponent<Image>().color = Color.grey;

    }

    public void AudioChangeEqualizer(int audioIndex) {
        int value = audioListContentSet[audioIndex].transform.Find("Equalizer Drop Down").GetComponent<TMP_Dropdown>().value;
        audioList.SetEqualizerToAudio(value, audioIndex);
    }



    //select audio1
    public void GetIndexSelected(int index) {
        int i = index;
        indexSelected = i;
    }

    public void getAudioPath() {

        if (directory == null) directory = Application.streamingAssetsPath;

        indexSelected = Int32.Parse(EventSystem.current.currentSelectedGameObject.name.Split(' ')[2]);
        FileBrowser.ShowLoadDialog((paths) => { GetPathOnSuccess(paths[0]); }, () => { GetPathOnCancel(); },
         FileBrowser.PickMode.Files, initialPath: directory);
        // path = EditorUtility.OpenFilePanel("Select your audio", directory, "wav,mp3");
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
        CheckAudioIndexAndTryActiveAddAudioButton();
    }
    private void GetPathOnCancel() {
        print("No audio is selected!");
    }

    //select your audio2
    public void addAudioBasic() {





        if (path == "") return;

        //int index = Int32.Parse(EventSystem.current.currentSelectedGameObject.name.Split(' ')[2]);
        int index = indexSelected;
        audioList.SetAudio(this, path, index);



        int indexForAudioList = index;
        for (int i = 0; i < index; i++) {
            if (audioListContentSet[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text == "No Audio") {
                indexForAudioList--;
            }
        }

        audioListContentSet[index].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = audioList.audioList[indexForAudioList].Name;


        AudioChangeEqualizer(index);
    }

    //select audio 3
    public void CheckAudioIndexAndTryActiveAddAudioButton() {

        if (path == "") return;
        int index = indexSelected;
        // int index = Int32.Parse(EventSystem.current.currentSelectedGameObject.name.Split(' ')[2]);
        if (index == audioListContentSet.Count - 1) {
            addAudioButton.GetComponent<Button>().enabled = true;
            addAudioButton.GetComponent<Image>().color = Color.white;
        }
    }

    public void RemoveAudio() {

        int index = Int32.Parse(EventSystem.current.currentSelectedGameObject.name.Split(' ')[2]);
        //int index = indexSelected;
        int indexForAudioList = index;
        for (int i = 0; i < index; i++) {
            if (audioListContentSet[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text == "No Audio") {
                indexForAudioList--;
            }
        }


        for (int i = index + 1; i < audioListContentSet.Count; i++) {
            audioListContentSet[i].name = (i - 1).ToString();
            audioListContentSet[i].transform.GetChild(2).GetComponent<Button>().name = "Delete Button " + (i - 1).ToString();
            audioListContentSet[i].transform.GetChild(0).GetComponent<Button>().name = "Audio Button " + (i - 1).ToString();
        }

        Destroy(audioListContentSet[index]);
        audioListContentSet.RemoveAt(index);

        if (audioList.audioList != null)
            if(indexForAudioList < audioList.AudioCount)
                audioList.RemoveAudio(indexForAudioList);
    }

    public void ChangeLoopMode() {
        int index = loopModeDropDown.value;
        LoopMode target = (LoopMode)(Enum.GetValues(typeof(LoopMode)).GetValue(index));

        audioList.Mode = target;
    }


    public void AddNewEqualizer() {
        equalizerListContentSet.Add(Instantiate(equalizerListSet, equalizerContent.transform));
        equalizerListContentSet[equalizerListContentSet.Count - 1].name = (equalizerListContentSet.Count - 1).ToString();

        Button equalizerButton = equalizerListContentSet[equalizerListContentSet.Count - 1].transform.Find("Equalizer Button").GetComponent<Button>();
        Button deleteButton = equalizerListContentSet[equalizerListContentSet.Count - 1].transform.Find("Delete Button").GetComponent<Button>();

        audioList.AddNewEqualizer();

        int index = equalizerListContentSet.Count;
        equalizerButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text += (" " + index.ToString());
        equalizerButton.onClick.AddListener(delegate { OpenEqualizer(index); });


        deleteButton.onClick.AddListener(delegate { RemoveEqualizer(); });
        deleteButton.name = "Delete Button " + (equalizerListContentSet.Count - 1).ToString();

        //update selectable eq in audio

        List<string> eqNameList = new(audioList.EqualizerListName);

        for (int i = 0; i < audioListContentSet.Count; i++) {
            audioListContentSet[i].transform.Find("Equalizer Drop Down").GetComponent<TMP_Dropdown>().ClearOptions();
            audioListContentSet[i].transform.Find("Equalizer Drop Down").GetComponent<TMP_Dropdown>().AddOptions(eqNameList);
        }

    }
    public void OpenEqualizer(int index) {
        equalizerCanvas.SetActive(true);

        for (int i = 0; i < equalizerSliderArray.Length; i++) {
            int localIndex = i;
            equalizerSliderArray[i].GetComponent<Slider>().value = audioList.EqualizerList[index].equalizerBands[i].Gain;
            equalizerSliderArray[i].GetComponent<Slider>().onValueChanged.AddListener(delegate { ChangeGain(index, localIndex); });
        }


    }

    public void ChangeGain(int equalizerIndex, int equalizerBandIndex) {
        float gain = equalizerSliderArray[equalizerBandIndex].GetComponent<Slider>().value;
        audioList.SetGain(equalizerIndex, Equalizer.GetFrequencyByIndex(equalizerBandIndex), gain);
        audioList.UpdateEqualizer();
    }

    public void RemoveEqualizer() {
        //string indexString = EventSystem.current.currentSelectedGameObject.name.Split(' ')[2];
        //int index = Int32.Parse(indexString);
        int index = indexSelected;

        for (int i = index + 1; i < equalizerListContentSet.Count; i++) {
            equalizerListContentSet[i].name = (i - 1).ToString();
            equalizerListContentSet[i].transform.GetChild(1).GetComponent<Button>().name = "Delete Button " + (i - 1).ToString();
        }

        Destroy(equalizerListContentSet[index]);
        equalizerListContentSet.RemoveAt(index);

        audioList.RemoveEqualizer(index + 1);

        List<string> eqNameList = new(audioList.EqualizerListName);

        for (int i = 0; i < audioListContentSet.Count; i++) {
            var equalizerDropDown = audioListContentSet[i].transform.Find("Equalizer Drop Down").GetComponent<TMP_Dropdown>();
            if (equalizerDropDown.value == index) {
                equalizerDropDown.value = 0;
            }
            equalizerDropDown.ClearOptions();
            equalizerDropDown.AddOptions(eqNameList);
        }

    }

    public void ResetPitch() {
        audioList.SetPitch(1);
        pitchSlider.GetComponent<Slider>().value = 1;
    }
    public void ResetVolume() {
        audioList.SetVolume(1);
        volumeSlider.GetComponent<Slider>().value = 1;
    }
    public void ResetPanning() {
        audioList.SetPanning(0);
        panningSlider.GetComponent<Slider>().value = 0;
    }

    public void ChangePitchLabel() {
        pitchValueLabel.text = Math.Round(pitchSlider.GetComponent<Slider>().value, 2).ToString();
    }

    public void ChangeVolumeLabel() {
        volumeValueLabel.text = Math.Round(volumeSlider.GetComponent<Slider>().value, 2).ToString();
    }

    public void ChangePanningLabel() {
        panningValueLabel.text = Math.Round(panningSlider.GetComponent<Slider>().value, 2).ToString();
    }

    public string getDirectory(string path) {

        string tempDirectory = path;
        while (tempDirectory.Length >= 1 && tempDirectory[tempDirectory.Length - 1] != '/') {
            tempDirectory = tempDirectory.Remove(tempDirectory.Length - 1, 1);
        }

        return tempDirectory;
    }

    public void ChangeAudioName(AudioListUIManager audioManager, string status) {

        if (audioManager == null) return;
        var name = audioManager.audioNameText.GetComponent<TextMeshProUGUI>();
        if (audioManager.audioList == null) {
            name.text = "No audio is selected!";
        }
        else {
            name.text = status;
        }
    }
    public void Exit() {
        Application.Quit();
    }

    public void SkipToPrevious() {
        audioList.Stop();
        audioList.currentPosition--;
        audioList.Play();

    }
    public void SkipToNext() {
        audioList.Stop();
        audioList.currentPosition++;
        audioList.Play();

    }

    #region SetAudioStatus

    public void AudioStarted(MonoBehaviour audioManager, Audio audio) {
        ChangeAudioName((AudioListUIManager)audioManager, "Now playing: " + audio.Name);

    }
    public void AudioPaused(MonoBehaviour audioManager, Audio audio) {
        ChangeAudioName((AudioListUIManager)audioManager, "Paused: " + audio.Name);
    }

    public void AudioRestarted(MonoBehaviour audioManager, Audio audio, bool sameAsPlay) {
        ChangeAudioName((AudioListUIManager)audioManager, "Restarted: " + audio.Name);

    }

    public void AudioResumed(MonoBehaviour audioManager, Audio audio) {
        ChangeAudioName((AudioListUIManager)audioManager, "Resumed: " + audio.Name);
    }
    public void AudioStopped(MonoBehaviour audioManager, Audio audio, bool hasFinishedPlaying) {
        ChangeAudioName((AudioListUIManager)audioManager, (hasFinishedPlaying ? "Finished playing: " : "Stopped: ") + audio.Name);

    }

    #endregion
}
