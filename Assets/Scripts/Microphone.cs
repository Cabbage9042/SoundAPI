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

    public bool SaveIntoFile;
    public string AbsoluteOutputPath;
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
        SampleRate = waveFormat.SampleRate;
        Bit = waveFormat.BitsPerSample;
        Channel = waveFormat.Channels;
        try {
            if (SaveIntoFile) {
                microphone.StartCapture(AbsoluteOutputPath, waveFormat);
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

   
    public void OnDestroy() {
        microphone.StopCapture();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Microphone))]
    public class MicrophoneEditor : Editor {
        public Microphone microphone;
        public SerializedProperty SaveIntoFile;
        public SerializedProperty absoluteOutputPath;

        private void OnEnable() {
            microphone = (Microphone)target;

            SaveIntoFile = serializedObject.FindProperty("SaveIntoFile");
            absoluteOutputPath = serializedObject.FindProperty("AbsoluteOutputPath");
        
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

            serializedObject.ApplyModifiedProperties();
        }

    }

#endif
}
