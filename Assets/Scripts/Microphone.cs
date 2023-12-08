using NAudio.Wave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Microphone : MonoBehaviour {
    MicrophoneObject microphone = new();

    public bool saveIntoFile;
    public bool calculateAmplitude;
    public string absoluteOutputPath;
    private int SampleRate;
    private int Bit;
    private int Channel;
    public WaveFormat WaveFormat => microphone.WaveFormat;

    public float Panning = 0.0f;

    public static string[] MicrophoneDevicesName => MicrophoneObject.GetMicrophoneDevicesName();

    public static WaveInCapabilities[] MicrophoneDevices => MicrophoneObject.GetMicrophoneDevices();


    public void Start() {
        microphone.Initialize();
    }

    public void StartCapture() {
        var waveFormat = new WaveFormat(SampleRate, Bit, Channel);
        StartCapture(waveFormat);
    }

    public void StartCapture(WaveFormat waveFormat) {
        try {
            if (saveIntoFile) {
                microphone.StartCapture(absoluteOutputPath, waveFormat);
            }
            else {
                microphone.StartCapture(waveFormat);
            }
        }
        catch (MmException) {
            throw new System.ArgumentException("Your microphone does not support " + Channel + " channel(s)!");

            
        }
    }
    

    public void StopCapture() {
        microphone.StopCapture();

    }


    public double[] GetAmplitude() {
        return microphone.GetAmplitude();
    }

    public int GetSampleRate() {
        return SampleRate;
    }
    public int GetBit() {
        return Bit;
    }
    public int GetChannel() {
        return Channel;
    }

    public void SetSampleRate(int sampleRate) {
        this.SampleRate = sampleRate;
    }
    public void SetBit(int bit) {
        this.Bit = bit;
    }
    public void SetChannel(int channel) {
        this.Channel = channel;
    }

    public void SetMicrophoneNumber(int id) {
        microphone?.SetMicrophoneNumber(id);
    }

    public int GetMicrophoneNumber() {
        return microphone.GetSpeakerNumber;
    }

    //  public void UpdateEqualizer() => microphone?.UpdateEqualizer();
    /*   public void SetEqualizer(Equalizer equalizer) {
           EqualizerProperty = equalizer;
       }


       public void SetGain(Frequency frequency, float Gain) {
           int index = Equalizer.GetIndexByFrequency(frequency);
           this.EqualizerProperty.equalizerBands[index].Gain = Gain;
       }*/
    /*    public void SetPanning(float panning) {
            this.Panning = panning;
     //       if(microphone != null ) microphone.Panning = panning;
        }*/

    public void OnDestroy() {
        microphone.StopCapture();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Microphone))]
    public class MicrophoneEditor : Editor {
        public Microphone microphone;
        public SerializedProperty SaveIntoFile;
        public SerializedProperty CalculateAmplitude;
        public SerializedProperty absoluteOutputPath;
        /*private SerializedProperty equalizer;
        private SerializedProperty Panning;
        private bool equalizerIsExpanded;

        private string[] frequencyList = {
            "31Hz","63Hz","125Hz","250Hz","500Hz","1kHz","2kHz","4kHz","8kHz","16kHz"
        };
        */
        private void OnEnable() {
            microphone = (Microphone)target;

            SaveIntoFile = serializedObject.FindProperty("saveIntoFile");
            CalculateAmplitude = serializedObject.FindProperty("calculateAmplitude");
            absoluteOutputPath = serializedObject.FindProperty("absoluteOutputPath");
        
           // equalizer = serializedObject.FindProperty("privateEqualizer");
            //Panning = serializedObject.FindProperty("Panning");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sample Rate (Hz)",  GUILayout.Width(EditorGUIUtility.labelWidth));
            microphone.SampleRate = int.Parse( EditorGUILayout.TextField(microphone.SampleRate.ToString()));
            EditorGUILayout.EndHorizontal();  
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bit",  GUILayout.Width(EditorGUIUtility.labelWidth));
            microphone.Bit= int.Parse( EditorGUILayout.TextField(microphone.Bit.ToString()));
            EditorGUILayout.EndHorizontal();   
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Channel",  GUILayout.Width(EditorGUIUtility.labelWidth));
            microphone.Channel = int.Parse( EditorGUILayout.TextField(microphone.Channel.ToString()));
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.PropertyField(SaveIntoFile);

            if (SaveIntoFile.boolValue) {
                EditorGUILayout.PropertyField(absoluteOutputPath);
            }
           

            bool pressed = GUILayout.Button("Start", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2));
            if (pressed) {
                if (Application.isPlaying)
                    microphone.StartCapture();
            }
            if (GUILayout.Button("Stop", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2))) {
                if (Application.isPlaying) microphone.StopCapture();
            }

        /*     float lastFramePanning = Panning.floatValue;

           EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Panning",  GUILayout.Width(EditorGUIUtility.labelWidth));
            Panning.floatValue = EditorGUILayout.Slider(Panning.floatValue, -1.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            if (lastFramePanning != Panning.floatValue) {
                microphone.SetPanning(Panning.floatValue);

            }
      */
            //PrintEqualizer();

            serializedObject.ApplyModifiedProperties();
        }

   /*     private void PrintEqualizer() {
            equalizerIsExpanded = EditorGUILayout.Foldout(equalizerIsExpanded, "Equalizer");
            if (!equalizerIsExpanded) return;


            for (int i = 0; i < frequencyList.Length; i++) {
                EditorGUILayout.BeginHorizontal();
                var gain = equalizer.FindPropertyRelative("equalizerBands").GetArrayElementAtIndex(i).FindPropertyRelative("Gain");
                var oriGain = gain.floatValue;
                EditorGUILayout.LabelField(frequencyList[i], GUILayout.Width(50));
                gain.floatValue = EditorGUILayout.Slider(gain.floatValue, Equalizer.MIN_GAIN, Equalizer.MAX_GAIN);
                if (oriGain != gain.floatValue) {
                    microphone.EqualizerProperty.equalizerBands[i].Gain = gain.floatValue;

                    microphone.UpdateEqualizer();

                    //audioBasic.GetMonoOrStereoAudio()?.ChangeGain(frequency, gain.floatValue);
                }
                EditorGUILayout.EndHorizontal();
            }

            //reset button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool resetIsPressed = GUILayout.Button("Reset To Default", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2));

            EditorGUILayout.EndHorizontal();
            if (resetIsPressed) {
                for (int i = 0; i < frequencyList.Length; i++) {
                    microphone.EqualizerProperty.equalizerBands[i].Gain = 0.0f;
                }
                microphone.UpdateEqualizer();

            }

        }*/
    }

#endif
}
