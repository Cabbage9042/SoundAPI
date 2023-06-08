using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Wave;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioBasic : MonoBehaviour {
    public AudioClip audioClip = null;
    public new Audio audio = null;

    public enum ThreeState {
        True,
        False,
        Played
    }
    public ThreeState playOnStart;

    void Start() {
        if (playOnStart == ThreeState.True) {
            if (audioClip != null) {
                if (audio == null || audio.NameWoExtension != audioClip.name) {
                    audio = AudioClipToAudio(audioClip);
                    playOnStart = ThreeState.Played;
                }

                audio?.Play();
            }
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Start playing the audio

            audio?.Play();
            //output.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            audio?.Restart();

        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            audio?.Pause();
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            audio?.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.Plus)) {
            audio.OnAudioStopped += Audio_OnAudioStopped;
        }else if (Input.GetKeyDown(KeyCode.Minus)) {
            audio.OnAudioStopped += Audio_OnAudioStopped;
        }
    }

    private void Audio_OnAudioStopped(Audio arg1, bool arg2) {
        
    }

    private void OnDestroy() {
        audio?.Stop();
        audio?.Dispose();
    }
    public static Audio AudioClipToAudio(AudioClip audioClip) {
        string[] assetPathArray = AssetDatabase.GetAssetPath(audioClip.GetInstanceID()).Split("/");
        string path = Application.dataPath + "/";
        for (int i = 1; i < assetPathArray.Length; i++) {
            path += (assetPathArray[i] + "/");
        }

        path = path.Remove(path.Length - 1);
        return new Audio(path);


    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AudioBasic))]
    public class AudioBasicEditor : Editor {

        SerializedProperty audioClip;
        public AudioBasic audioBasic;
        private string previousString;

        private long audioCurrentPosition;

        private SerializedProperty playOnStart;


        private void OnEnable() {
            audioBasic = (AudioBasic)target;
            audioClip = serializedObject.FindProperty("audioClip");
            playOnStart = serializedObject.FindProperty("playOnStart");

            /*if (Application.isPlaying) {
                if (playOnStart.intValue == (int)ThreeState.True) {
                    if (audio == null) {
                        Debug.LogWarning("No audio attached");
                        playOnStart.intValue = (int)ThreeState.Played;
                    }
                    else {
                        //audioBasic.audio.Play();
                        playOnStart.intValue = (int)ThreeState.Played;
                    }

                }
            }*/

        }
        private void OnDisable() {
            if (Application.isPlaying) {
                audioCurrentPosition = audioBasic.audio.Position;
            }
        }


        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(audioClip, true);

            EditorGUI.BeginDisabledGroup(Application.isPlaying); // Disable the EnumPopup
            playOnStart.enumValueIndex = (int)(ThreeState)EditorGUILayout.EnumPopup("Play On Start", (ThreeState)playOnStart.enumValueIndex);
            EditorGUI.EndDisabledGroup();
            //serializedAudioClip.objectReferenceValue = EditorGUILayout.ObjectField("Audio", serializedAudioClip.objectReferenceValue, typeof(AudioClip), true) as AudioClip;  


            //if the object field got audio
            if (audioClip.objectReferenceValue != null) {

                if (Application.isPlaying) {


                    //if the audio has been changed (programmer drag into field while game running)
                    if (audioClip.objectReferenceValue.name != previousString) {

                        if (((AudioClip)audioClip.objectReferenceValue).name == audioBasic.audio?.NameWoExtension && audioBasic.audio.State == PlaybackState.Playing) {
                            audioBasic.audio.Position = audioCurrentPosition;
                            audioBasic.audio.Play(false);
                        }
                        else {
                            audioBasic.audio?.Stop();
                            audioBasic.audio?.Dispose();

                            audioBasic.audio = AudioClipToAudio((AudioClip)audioClip.objectReferenceValue);
                        }
                    }

                }
                //disable the audio.name != string checking, the audio will immediately change when a new audio drag into the field
                previousString = ((AudioClip)audioClip.objectReferenceValue).name;
            }
            serializedObject.ApplyModifiedProperties();



        }

    }

#endif
}