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

    public GameObject audioListSet;
    public GameObject audioContent;
    private List<GameObject> audioListContentSet;

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

    private void Start() {
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

    }

    public void OpenAudioCanvas() {
        audioCanvas.SetActive(true);
        Canvas canvas = audioCanvas.GetComponent<Canvas>();

        //canvas.transform.FindChild("Foreground").FindChild("").FindChild("Content")
    }
    public void CloseAudioCanvas() {

        audioCanvas.SetActive(false);
    }

    public double[] getAmplitude() {
        return audioList?.GetAmplitude(0.3f);
    }

    public void AddNewAudio() {

        audioListContentSet.Add(Instantiate(audioListSet, audioContent.transform));
        audioListContentSet[audioListContentSet.Count - 1].name = (audioListContentSet.Count - 1).ToString();

        Button audioButton = audioListContentSet[audioListContentSet.Count - 1].transform.Find("Audio Button").GetComponent<Button>();
        TMP_Dropdown equalizerDropDown = audioListContentSet[audioListContentSet.Count - 1].transform.Find("Equalizer Drop Down").GetComponent<TMP_Dropdown>();
        Button deleteButton = audioListContentSet[audioListContentSet.Count - 1].transform.Find("Delete Button").GetComponent<Button>();

        audioButton.onClick.AddListener(delegate { getAudioPath(); });
        audioButton.onClick.AddListener(delegate { addAudioBasic(); });
        audioButton.name = "Audio Button "+ (audioListContentSet.Count - 1).ToString();

        List<string> nameList = new(audioList.EqualizerListName);
        equalizerDropDown.AddOptions(nameList);

        deleteButton.onClick.AddListener(delegate { RemoveAudio(); });
        deleteButton.name = "Delete Button " + (audioListContentSet.Count - 1).ToString();


    }

    //select audio1
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
    public void addAudioBasic() {





        if (path == "") return;

        int index = Int32.Parse(EventSystem.current.currentSelectedGameObject.name.Split(' ')[2]);
        audioList.setAudioClip(this, path, index);


        /*
                audioBasic.AddOnAudioStarted(new MethodCalled(this, "AudioStarted"));
                audioBasic.AddOnAudioPaused(new MethodCalled(this, "AudioPaused"));
                audioBasic.AddOnAudioRestarted(new MethodCalled(this, "AudioRestarted"));
                audioBasic.AddOnAudioResumed(new MethodCalled(this, "AudioResumed"));
                audioBasic.AddOnAudioStopped(new MethodCalled(this, "AudioStopped"));*/

        
        pitchSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioList.SetPitch(pitchSlider.GetComponent<Slider>().value); });
        volumeSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioList.SetVolume(volumeSlider.GetComponent<Slider>().value); });
        panningSlider.GetComponent<Slider>().onValueChanged.AddListener(delegate { audioList.SetPanning(panningSlider.GetComponent<Slider>().value); });

        speakerDropDown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate { audioList.SetSpeakerNumber(speakerDropDown.GetComponent<TMP_Dropdown>().value); });



        int indexForAudioList = index;
        for (int i = 0; i < index; i++) {
            if (audioListContentSet[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text == "No Audio") {
                indexForAudioList--;
            }
        }

        audioListContentSet[index].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = audioList.audioList[indexForAudioList].Name;
    }


    public void RemoveAudio() {
        int index = Int32.Parse(EventSystem.current.currentSelectedGameObject.name.Split(' ')[2]);

        int indexForAudioList = index;
        for (int i = 0; i < index; i++) {
            if (audioListContentSet[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text == "No Audio") {
                indexForAudioList--;
            }
        }


        for (int i = index + 1; i < audioListContentSet.Count; i++) {
            audioListContentSet[i].name = (i - 1).ToString();
            audioListContentSet[i].transform.GetChild(2).GetComponent<Button>().name = (i - 1).ToString();
        }

        Destroy(audioListContentSet[index]);
        audioListContentSet.RemoveAt(index);

        if (audioList.audioList != null)
            audioList.audioList.RemoveAt(indexForAudioList);
    }

    public void ChangeLoopMode() {
        int index = loopModeDropDown.value;
        LoopMode target = (LoopMode)(Enum.GetValues(typeof(LoopMode)).GetValue(index));

        audioList.mode = target;
    }


    public void AddNewEqualizer() {
        equalizerListContentSet.Add(Instantiate(equalizerListSet,equalizerContent.transform));
        equalizerListContentSet[equalizerListContentSet.Count - 1].name = (equalizerListContentSet.Count - 1).ToString();

        Button equalizerButton = equalizerListContentSet[equalizerListContentSet.Count - 1].transform.Find("Equalizer Button").GetComponent<Button>();
        Button deleteButton = equalizerListContentSet[equalizerListContentSet.Count - 1].transform.Find("Delete Button").GetComponent<Button>();

        deleteButton.onClick.AddListener(delegate { RemoveEqualizer(); });
        deleteButton.name = "Delete Button " + (equalizerListContentSet.Count - 1).ToString();

        audioList.AddNewEqualizer();
        

    }
    public void RemoveEqualizer() {
        string indexString = EventSystem.current.currentSelectedGameObject.name.Split(' ')[2];
        int index = Int32.Parse(indexString);

        for (int i = index + 1; i < equalizerListContentSet.Count; i++) {
            equalizerListContentSet[i].name = (i - 1).ToString();
            equalizerListContentSet[i].transform.GetChild(1).GetComponent<Button>().name = "Delete Button " + (i - 1).ToString();
        }

        Destroy(equalizerListContentSet[index]);
        equalizerListContentSet.RemoveAt(index);

        audioList.RemoveEqualizer(index+1);

    }

    public string getDirectory(string path) {

        string tempDirectory = path;
        while (tempDirectory.Length >= 1 && tempDirectory[tempDirectory.Length - 1] != '/') {
            tempDirectory = tempDirectory.Remove(tempDirectory.Length - 1, 1);
        }

        return tempDirectory;
    }
}
