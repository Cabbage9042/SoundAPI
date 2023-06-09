using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Wave;
using System.Reflection;
using System;
using UnityEditorInternal;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioBasic : MonoBehaviour {
    public AudioClip audioClip = null;
    public new Audio audio = null;
    public FunctionCalled[] functionCalled;
    public bool loop = false;


    //public MethodInfo functionCalled = null;
    //public int functionCalledIndex = 0;

    [System.Serializable]
    public class FunctionCalled {
        public MonoBehaviour functionOwner = null;
        public string[] allMethod;
        public int selectedFunctionIndex;

        public MethodInfo methodToCall { get { return functionOwner.GetType().GetMethod(allMethod[selectedFunctionIndex]); } }


    }


    public enum PlayOnStartState {
        True,
        False,
        Played
    }
    public PlayOnStartState playOnStart;

    void Start() {
        if (playOnStart == PlayOnStartState.True) {
            if (audioClip != null) {
                if (audio == null || audio.NameWoExtension != audioClip.name) {
                    audio = AudioClipToAudio(audioClip);
                    playOnStart = PlayOnStartState.Played;
                }

                Play();
            }
        }
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

    public void Play() {
        audio.RemoveOnAudioStopped();


        //loop
        audio.OnAudioStopped += OnAudioStopped_CheckLoop;

        //assign each function by user into event
        foreach (var function in functionCalled) {
            audio.OnAudioStopped += (Action<Audio, bool>)Delegate.CreateDelegate(typeof(Action<Audio, bool>), null, function.methodToCall);
        }
        audio?.Play();
    }

    public void Stop() {
        audio.Stop();
    }

    public void Restart() {
        audio.Restart();
    }

    public void Pause() {
        audio.Pause();
    }

    private void OnAudioStopped_CheckLoop(Audio stoppedAudio, bool hasFinishedPlaying) {
        if (hasFinishedPlaying) {
            if (loop) {
                Play();
            }
        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AudioBasic))]
    public class AudioBasicEditor : Editor {

        public AudioBasic audioBasic;
        private string previousString;
        private long audioCurrentPosition;

        SerializedProperty audioClip;
        private SerializedProperty playOnStart;
        private SerializedProperty functionCalled;
        private SerializedProperty loop;
        ReorderableList listOnAudioStopped;
        private bool eventIsExpanded = false;

        private void OnEnable() {
            audioBasic = (AudioBasic)target;
            audioClip = serializedObject.FindProperty("audioClip");
            playOnStart = serializedObject.FindProperty("playOnStart");
            functionCalled = serializedObject.FindProperty("functionCalled");
            loop = serializedObject.FindProperty("loop");


            listOnAudioStopped = new ReorderableList(serializedObject,
                functionCalled, true, true, true, true);

            listOnAudioStopped.drawElementCallback = DrawListItems;
            listOnAudioStopped.drawHeaderCallback = DrawHeader;


        }
        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused) {


            //this is to get the element in index n
            SerializedProperty element = listOnAudioStopped.serializedProperty.GetArrayElementAtIndex(index);

            //object field
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, EditorGUIUtility.currentViewWidth / 3, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("functionOwner"), GUIContent.none);


            //get the object field item (class)
            MonoBehaviour functionOwner = (MonoBehaviour)element.FindPropertyRelative("functionOwner").objectReferenceValue;


            //if got class attached
            if (functionOwner != null) {

                //get the possible method that can be called
                Type type = functionOwner.GetType();
                MethodInfo[] allMethodInfo = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                List<string> methodNames = new List<string>();
                for (int i = 0; i < allMethodInfo.Length; i++) {
                    if (!allMethodInfo[i].Name.StartsWith("get_") && !allMethodInfo[i].Name.StartsWith("set_")) {
                        methodNames.Add(allMethodInfo[i].Name);
                    }
                }


                //get the reference to the all method in class
                SerializedProperty allMethod = element.FindPropertyRelative("allMethod");
                allMethod.arraySize = methodNames.Count;

                //set each name into the all method
                for (int i = 0; i < methodNames.Count; i++) {
                    allMethod.GetArrayElementAtIndex(i).stringValue = methodNames[i];
                }

                float popUpWidth = EditorGUIUtility.currentViewWidth / 2;
                float rectX = rect.x + rect.width - popUpWidth;

                //display popup
                SerializedProperty selectedFunctionIndex = element.FindPropertyRelative("selectedFunctionIndex");
                selectedFunctionIndex.intValue = EditorGUI.Popup(new Rect(rectX, rect.y, popUpWidth, EditorGUIUtility.singleLineHeight),
                    selectedFunctionIndex.intValue, methodNames.ToArray());
            }
        }


        void DrawHeader(Rect rect) {
            string name = "On Audio Stopped";
            EditorGUI.LabelField(rect, name);

        }
        private void OnDisable() {
            if (Application.isPlaying) {
                audioCurrentPosition = audioBasic.audio.Position;
            }
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            //audioclip field
            EditorGUILayout.PropertyField(audioClip, true);

            //play on start 
            EditorGUI.BeginDisabledGroup(Application.isPlaying); // Disable the EnumPopup
            playOnStart.enumValueIndex = (int)(PlayOnStartState)EditorGUILayout.EnumPopup("Play On Start", (PlayOnStartState)playOnStart.enumValueIndex);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(loop);

            //event
            eventIsExpanded = EditorGUILayout.Foldout(eventIsExpanded, "Event");
            if (eventIsExpanded) {
                listOnAudioStopped.DoLayoutList();
            }



            if (Application.isPlaying) {
                //if the object field got audio
                if (audioClip.objectReferenceValue != null) {

                    //if the audio has been changed (programmer drag into field while game running)
                    if (audioClip.objectReferenceValue.name != previousString) {

                        //if the programmer drag the same audio into the field, the audio continue playing
                        if (((AudioClip)audioClip.objectReferenceValue).name == audioBasic.audio?.NameWoExtension && audioBasic.audio.State == PlaybackState.Playing) {
                            audioBasic.audio.Position = audioCurrentPosition;
                            audioBasic.audio.Play(false);
                        }
                        //if the programmer drag the different audio into the field, the audio stop playing and the audio is changed and ready to be played
                        else {
                            audioBasic.audio?.Stop();
                            audioBasic.audio?.Dispose();

                            audioBasic.audio = AudioClipToAudio((AudioClip)audioClip.objectReferenceValue);
                        }
                    }

                }
            }
            else {
                //dont allow user to select played
                if (playOnStart.enumValueIndex == (int)PlayOnStartState.Played) {
                    audioBasic.playOnStart = PlayOnStartState.False;
                }
            }

            //disable the audio.name != string checking, the audio will immediately change when a new audio drag into the field
            previousString = ((AudioClip)audioClip.objectReferenceValue)?.name;



            serializedObject.ApplyModifiedProperties();



        }

    }

#endif
}