
#if UNITY_EDITOR
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MicrophoneUIManager : MonoBehaviour {

    public Microphone microphone;
    public GameObject startRecording;
    public GameObject stopRecording;

    public GameObject sampleRateInput;
    public GameObject bitsInput;
    public GameObject channelInput;

    public TextMeshProUGUI status;

    public int SampleRate => microphone.GetSampleRate();

    // Start is called before the first frame update
    void Start() {
        microphone = GameObject.Find("Audio").AddComponent(typeof(Microphone)) as Microphone;
        startRecording.GetComponent<Button>().onClick.AddListener(delegate { StartCapture(); });
        stopRecording.GetComponent<Button>().onClick.AddListener(delegate { StopCapture(); });

        stopRecording.SetActive(false);

    }

    private void StartCapture() {

        int sampleRate, bits, channel;
        bool isSuccessed;

        string sampleRateString = sampleRateInput.GetComponent<TMP_InputField>().text;
        isSuccessed = int.TryParse(sampleRateString, out sampleRate);
        if (!isSuccessed) {
            status.text = "Sample Rate is not a integer!";
            return;
        }

        string bitsString = bitsInput.GetComponent<TMP_InputField>().text;
        isSuccessed = int.TryParse(bitsString, out bits);
        if (!isSuccessed) {
            status.text = "Bits is not a integer!";
            return;
        }

        string channelString = channelInput.GetComponent<TMP_InputField>().text;
        isSuccessed = int.TryParse(channelString, out channel);
        if (!isSuccessed) {
            status.text = "Channel is not a integer!";
            return;
        }



        microphone.SetSampleRate(sampleRate);
        microphone.SetBit(bits);
        microphone.SetChannel(channel);
        microphone.StartCapture();

        startRecording.GetComponent<Button>().enabled = false;
        stopRecording.GetComponent<Button>().enabled = true;

        sampleRateInput.GetComponent<TMP_InputField>().enabled = false;
        bitsInput.GetComponent<TMP_InputField>().enabled = false;
        channelInput.GetComponent<TMP_InputField>().enabled = false;

        status.text = "Recording!";

    }

    private void StopCapture() {
        microphone.StopCapture();

        startRecording.GetComponent<Button>().enabled = true;
        stopRecording.GetComponent<Button>().enabled = false;

        sampleRateInput.GetComponent<TMP_InputField>().enabled = true;
        bitsInput.GetComponent<TMP_InputField>().enabled = true;
        channelInput.GetComponent<TMP_InputField>().enabled = true;

        status.text = "Stop Recorded!";

    }

    public double[] GetAmplitude() {
        return microphone?.GetAmplitude();
    }




}
#endif