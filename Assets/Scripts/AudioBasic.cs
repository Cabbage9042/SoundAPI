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
    private new Audio audio = null;
    public bool loop = false;

    public MethodCalled[] onStartAudioMethod;
    public MethodCalled[] onAudioStartedMethod;
    public MethodCalled[] onAudioPausedMethod;
    public MethodCalled[] onAudioResumedMethod;
    public MethodCalled[] onAudioRestartedMethod;
    public MethodCalled[] onAudioStoppedMethod;



    [System.Serializable]
    public class MethodCalled {
        public MonoBehaviour methodOwner = null;
        public string[] allMethod;
        public int selectedMethodIndex;

        public MethodInfo methodToCall { get { return methodOwner.GetType().GetMethod(allMethod[selectedMethodIndex]); } }


    }


    public enum PlayOnStartState {
        True,
        False,
        Played
    }
    public PlayOnStartState playOnStart;

    void Start() {
        //check need to play on start or not
        if (playOnStart != PlayOnStartState.True) return;


        //if got audio attached
        if (audioClip != null) {

            //if audio is not initialize || audio name is not same as provided audio
            if (audio == null || audio.NameWoExtension != audioClip.name) {

                //assign new audio into audio
                audio = AudioClipToAudio(audioClip);
                playOnStart = PlayOnStartState.Played;
            }

            Play();
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
        audio.ClearAllEvent();

        //loop
        audio.OnAudioStopped += OnAudioStopped_CheckLoop;

        //assign each method by user into event
        foreach (var method in onStartAudioMethod) {
            audio.OnStartAudio += (Action<Audio>)Delegate.CreateDelegate(typeof(Action<Audio>), null, method.methodToCall);
        }
        foreach (var method in onAudioStartedMethod) {
            audio.OnAudioStarted += (Action<Audio>)Delegate.CreateDelegate(typeof(Action<Audio>), null, method.methodToCall);
        }
        foreach (var method in onAudioPausedMethod) {
            audio.OnAudioPaused += (Action<Audio>)Delegate.CreateDelegate(typeof(Action<Audio>), null, method.methodToCall);
        }
        foreach (var method in onAudioResumedMethod) {
            audio.OnAudioResumed += (Action<Audio>)Delegate.CreateDelegate(typeof(Action<Audio>), null, method.methodToCall);
        }
        foreach (var method in onAudioRestartedMethod) {
            audio.OnAudioRestarted += (Action<Audio, bool>)Delegate.CreateDelegate(typeof(Action<Audio, bool>), null, method.methodToCall);
        }
        foreach (var method in onAudioStoppedMethod) {
            audio.OnAudioStopped += (Action<Audio, bool>)Delegate.CreateDelegate(typeof(Action<Audio, bool>), null, method.methodToCall);
        }
        audio?.Play();
    }

    public void Stop() {
        audio.Stop();
    }

    public void Restart() {
        if (audio.State == PlaybackState.Stopped) {
            Play();
            //same as play == false in audio.restart
            audio.OnAudioRestarted?.Invoke(audio, true);
        }
        else { audio.Restart(); }
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
        private SerializedProperty loop;
        private enum EventName {
            onStartAudio,
            onAudioStarted,
            onAudioPaused,
            onAudioResumed,
            onAudioRestarted,
            onAudioStopped,
            TotalEvent
        }

        private SerializedProperty[] eventArray = new SerializedProperty[(int)EventName.TotalEvent];
        private ReorderableList[] reorderableListArray = new ReorderableList[(int)EventName.TotalEvent];
        private Dictionary<EventName, string> headerName = new Dictionary<EventName, string>();



        private bool eventIsExpanded = false;


        private GUIStyle listMargin;

        struct ListHolder {
            public ReorderableList list;
            public string headerName;
        }
        ListHolder currentList;


        private void InitializeHeaderName() {
            headerName.Add(EventName.onStartAudio, "On Start Audio");
            headerName.Add(EventName.onAudioStarted, "On Audio Started");
            headerName.Add(EventName.onAudioPaused, "On Audio Paused");
            headerName.Add(EventName.onAudioResumed, "On Audio Resumed");
            headerName.Add(EventName.onAudioRestarted, "On Audio Restarted");
            headerName.Add(EventName.onAudioStopped, "On Audio Stopped");
        }

        private void DrawAllEvent() {

            listMargin = new GUIStyle(GUI.skin.label);
            for (int i = 0; i < eventArray.Length; i++) {
                currentList.list = reorderableListArray[i];
                headerName.TryGetValue((EventName)i, out currentList.headerName);


                EditorGUILayout.BeginVertical(listMargin);
                currentList.list.DoLayoutList();
                EditorGUILayout.EndVertical();
            }
        }

        private void InitializeAllList() {
            for (int i = 0; i < eventArray.Length; i++) {

                reorderableListArray[i] = new ReorderableList(serializedObject,
                    eventArray[i], true, true, true, true);

                reorderableListArray[i].drawElementCallback = DrawListItems;
                reorderableListArray[i].drawHeaderCallback = DrawHeader;

            }

        }

        private void OnEnable() {
            InitializeHeaderName();

            audioBasic = (AudioBasic)target;
            audioClip = serializedObject.FindProperty("audioClip");
            playOnStart = serializedObject.FindProperty("playOnStart");
            loop = serializedObject.FindProperty("loop");

            eventArray[(int)EventName.onStartAudio] = serializedObject.FindProperty("onStartAudioMethod");
            eventArray[(int)EventName.onAudioStarted] = serializedObject.FindProperty("onAudioStartedMethod");
            eventArray[(int)EventName.onAudioPaused] = serializedObject.FindProperty("onAudioPausedMethod");
            eventArray[(int)EventName.onAudioResumed] = serializedObject.FindProperty("onAudioResumedMethod");
            eventArray[(int)EventName.onAudioRestarted] = serializedObject.FindProperty("onAudioRestartedMethod");
            eventArray[(int)EventName.onAudioStopped] = serializedObject.FindProperty("onAudioStoppedMethod");


            InitializeAllList();





        }


        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused) {


            //this is to get the element in index n
            SerializedProperty element = currentList.list.serializedProperty.GetArrayElementAtIndex(index);

            //object field
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, EditorGUIUtility.currentViewWidth / 3, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("methodOwner"), GUIContent.none);


            //get the object field item (class)
            MonoBehaviour methodOwner = (MonoBehaviour)element.FindPropertyRelative("methodOwner").objectReferenceValue;


            //if got class attached
            if (methodOwner != null) {

                //get the possible method that can be called
                Type type = methodOwner.GetType();
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
                SerializedProperty selectedMethodIndex = element.FindPropertyRelative("selectedMethodIndex");
                selectedMethodIndex.intValue = EditorGUI.Popup(new Rect(rectX, rect.y, popUpWidth, EditorGUIUtility.singleLineHeight),
                    selectedMethodIndex.intValue, methodNames.ToArray());
            }
        }


        void DrawHeader(Rect rect) {
            string name = currentList.headerName;
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
                DrawAllEvent();
            }



            if (Application.isPlaying) {
                //if the object field got audio
                if (audioClip.objectReferenceValue != null) {

                    //if the audio has been changed (programmer drag into field while game running)
                    if (audioClip.objectReferenceValue.name != previousString && previousString != null) {

                        //if the programmer drag the same audio into the field, the audio continue playing
                        if (((AudioClip)audioClip.objectReferenceValue).name == audioBasic.audio?.NameWoExtension && audioBasic.audio.State == PlaybackState.Playing) {
                            audioBasic.audio.Position = audioCurrentPosition;
                            audioBasic.audio.Play(checkStopped: false);
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