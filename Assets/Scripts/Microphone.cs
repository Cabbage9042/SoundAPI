using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Microphone : MonoBehaviour {
    MicrophoneObject microphone = new();

    public bool saveIntoFile;
    public bool calculateAmplitude;
    public string absoluteOutputPath;


    public void StartCapture() {
        if (saveIntoFile) {
            microphone.StartCapture(absoluteOutputPath);
        }
        else {
            microphone.StartCapture();
        }
    }

    public void StopCapture() {
        microphone.StopCapture();

    }
    //
    //public bool 

    public double[] GetAmplitude() {
        return microphone.GetAmplitude();
    }


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

        private void OnEnable() {
            microphone = (Microphone)target;

            SaveIntoFile = serializedObject.FindProperty("saveIntoFile");
            CalculateAmplitude = serializedObject.FindProperty("calculateAmplitude");
            absoluteOutputPath = serializedObject.FindProperty("absoluteOutputPath");
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(SaveIntoFile);

            if (SaveIntoFile.boolValue) {
                EditorGUILayout.PropertyField(absoluteOutputPath);
            }
            EditorGUILayout.PropertyField(CalculateAmplitude);

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
