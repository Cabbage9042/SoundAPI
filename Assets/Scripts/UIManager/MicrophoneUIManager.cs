
using SimpleFileBrowser;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MicrophoneUIManager : MonoBehaviour {

    private static string NO_FOLDER_SELECTED = "No folder selected";
    public Microphone microphone;
    public GameObject startRecording;
    public GameObject stopRecording;

    public GameObject sampleRateInput;
    public GameObject bitsInput;
    public GameObject channelInput;

    public TextMeshProUGUI mainStatus;
    public Toggle saveIntoFile;
    public GameObject choosePath;
    public GameObject enterFileName;
    public GameObject saveFileStatus;
    public GraphManager graphManager;

    public TMP_Dropdown inputDropDown;

    private string folder = null;

    public int SampleRate => microphone.GetSampleRate();

    // Start is called before the first frame update
    void Start() {
        microphone = GameObject.Find("Audio").AddComponent(typeof(Microphone)) as Microphone;
     /*   startRecording.GetComponent<Button>().onClick.AddListener(delegate { StartCapture(); });
        stopRecording.GetComponent<Button>().onClick.AddListener(delegate { StopCapture(); });
*/
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Audio", ".mp3", ".wav"));
        FileBrowser.SetDefaultFilter(".wav");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
        stopRecording.GetComponent<Image>().color = Color.gray;

        var inputList = new List<string>(Microphone.MicrophoneDevicesName);
        inputDropDown.AddOptions(inputList);

        stopRecording.GetComponent<Button>().enabled = false;
    }

    public void StartCapture() {

        int sampleRate, bits, channel;
        bool isSuccessed;

        string sampleRateString = sampleRateInput.GetComponent<TMP_InputField>().text;
        isSuccessed = int.TryParse(sampleRateString, out sampleRate);
        if (!isSuccessed) {
            mainStatus.text = "Sample Rate is not a integer!";
            return;
        }

        string bitsString = bitsInput.GetComponent<TMP_InputField>().text;
        isSuccessed = int.TryParse(bitsString, out bits);
        if (!isSuccessed) {
            mainStatus.text = "Bits is not a integer!";
            return;
        }

        string channelString = channelInput.GetComponent<TMP_InputField>().text;
        isSuccessed = int.TryParse(channelString, out channel);
        if (!isSuccessed) {
            mainStatus.text = "Channel is not a integer!";
            return;
        }



        microphone.SetSampleRate(sampleRate);
        microphone.SetBit(bits);
        microphone.SetChannel(channel);

        int microphoneID = inputDropDown.value;
        microphone.SetMicrophoneNumber(microphoneID);

        if (saveIntoFile.isOn) {

            if (folder == null) {
                mainStatus.text = "Folder is not selected!";
                return;
            }
            string filename = enterFileName.GetComponent<TMP_InputField>().text;
            if (filename == null || filename.Length == 0) {
                mainStatus.text = "Please enter your audio name!";
                return;
            }

            if (!filename.EndsWith(".wav")) {
                filename += ".wav";
            }
            string folderPlusName = folder + "\\" + filename;

            microphone.absoluteOutputPath = folderPlusName;
            microphone.saveIntoFile = true;
            microphone.StartCapture();
        }
        else {

            microphone.saveIntoFile = false;
            microphone.StartCapture();
        }

        startRecording.GetComponent<Button>().enabled = false;
        startRecording.GetComponent<Image>().color = Color.gray;

        stopRecording.GetComponent<Button>().enabled = true;
        stopRecording.GetComponent<Image>().color = Color.white;

        sampleRateInput.GetComponent<TMP_InputField>().enabled = false;
        bitsInput.GetComponent<TMP_InputField>().enabled = false;
        channelInput.GetComponent<TMP_InputField>().enabled = false;

        mainStatus.text = "Recording!";

        saveIntoFile.enabled = false;
        saveIntoFile.gameObject.transform.Find("Background").GetComponent<Image>().color = Color.gray;

        choosePath.GetComponent<Image>().color = Color.gray;
        choosePath.GetComponent<Button>().enabled = false;
        enterFileName.GetComponent<Image>().color = Color.gray;
        enterFileName.GetComponent<TMP_InputField>().enabled = false;
    }

    public void StopCapture() {
        microphone.StopCapture();

        startRecording.GetComponent<Button>().enabled = true;
        startRecording.GetComponent<Image>().color = Color.white;

        stopRecording.GetComponent<Button>().enabled = false;
        stopRecording.GetComponent<Image>().color = Color.gray;

        sampleRateInput.GetComponent<TMP_InputField>().enabled = true;
        bitsInput.GetComponent<TMP_InputField>().enabled = true;
        channelInput.GetComponent<TMP_InputField>().enabled = true;

        mainStatus.text = "Stop Recorded!";

        saveIntoFile.enabled = true;
        saveIntoFile.gameObject.transform.Find("Background").GetComponent<Image>().color = Color.white;

        choosePath.GetComponent<Image>().color = Color.white;
        choosePath.GetComponent<Button>().enabled = true;
        enterFileName.GetComponent<Image>().color = Color.white;
        enterFileName.GetComponent<TMP_InputField>().enabled = true;

    }

    public double[] GetAmplitude() {
        return microphone?.GetAmplitude();
    }

    public void EnableChosoePathAndEnterFileName() {
        if (saveIntoFile.isOn) {
            choosePath.SetActive(true);
            enterFileName.SetActive(true);
            saveFileStatus.SetActive(true);
        }
        else {
            choosePath.SetActive(false);
            enterFileName.SetActive(false);
            saveFileStatus.SetActive(false);

        }
    }

    public void ChoosePath() {

        if (folder == null) folder = Application.streamingAssetsPath;

        FileBrowser.ShowLoadDialog((paths) => { GetPathOnSuccess(paths[0]); }, () => { GetPathOnCancel(); },
          FileBrowser.PickMode.Folders, initialPath: folder);


    }

    private void GetPathOnSuccess(string folder) {
        this.folder = folder;
        saveFileStatus.GetComponent<TextMeshProUGUI>().text = "Folder selected: " + folder;

    }

    private void GetPathOnCancel() {
        saveFileStatus.GetComponent<TextMeshProUGUI>().text = NO_FOLDER_SELECTED;
    }
    public void Exit() {
        Application.Quit();
    }



}
