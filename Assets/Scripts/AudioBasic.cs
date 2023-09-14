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

public class AudioBasic : AudioBase {
    public AudioClip audioClip = null;

    public bool loop = false;


    #region Add Remove OnEvent

    #region Started
    public void AddOnAudioStarted(MethodCalled methodCalled) {
        AddOnEvent(ref onAudioStartedMethod, methodCalled);
    }

    public bool RemoveOnAudioStarted(MethodCalled methodCalled) {
        return RemoveOnEvent(ref onAudioStartedMethod, methodCalled);
    }

    public void RemoveAllOnAudioStarted() {
        onAudioStartedMethod = null;
    }
    #endregion

    #region Pause
    public void AddOnAudioPaused(MethodCalled methodCalled) {
        AddOnEvent(ref onAudioPausedMethod, methodCalled);
    }

    public bool RemoveOnAudioPaused(MethodCalled methodCalled) {
        return RemoveOnEvent(ref onAudioPausedMethod, methodCalled);
    }

    public void RemoveAllOnAudioPaused() {
        onAudioPausedMethod = null;
    }
    #endregion

    #region Resumed
    public void AddOnAudioResumed(MethodCalled methodCalled) {
        AddOnEvent(ref onAudioResumedMethod, methodCalled);
    }

    public bool RemoveOnAudioResumed(MethodCalled methodCalled) {
        return RemoveOnEvent(ref onAudioResumedMethod, methodCalled);
    }

    public void RemoveAllOnAudioResumed() {
        onAudioResumedMethod = null;
    }

    #endregion

    #region Restarted
    public void AddOnAudioRestarted(MethodCalled methodCalled) {
        AddOnEvent(ref onAudioRestartedMethod, methodCalled);
    }

    public bool RemoveOnAudioRestarted(MethodCalled methodCalled) {
        return RemoveOnEvent(ref onAudioRestartedMethod, methodCalled);
    }

    public void RemoveAllOnAudioRestarted() {
        onAudioRestartedMethod = null;
    }

    #endregion

    #region Stopped
    public void AddOnAudioStopped(MethodCalled methodCalled) {
        AddOnEvent(ref onAudioStoppedMethod, methodCalled);
    }

    public bool RemoveOnAudioStopped(MethodCalled methodCalled) {
        return RemoveOnEvent(ref onAudioStoppedMethod, methodCalled);
    }

    public void RemoveAllOnAudioStopped() {
        onAudioStoppedMethod = null;
    }

    #endregion

    #region Base
    //return true if found and removed, false if not found
    private bool RemoveOnEvent(ref MethodCalled[] onEvent, MethodCalled methodCalled) {

        for (int i = 0; i < onEvent.Length; i++) {
            if (onEvent[i].Equals(methodCalled)) {
                for (int j = i; j < onEvent.Length - 1; j++) {
                    onEvent[j] = onEvent[j + 1];
                }

                Array.Resize(ref onEvent, onEvent.Length - 1);
                return true;
            }
        }
        return false;
    }

    private void AddOnEvent(ref MethodCalled[] onEvent, MethodCalled methodCalled) {
        if (onEvent == null || onEvent.Length == 0) {
            onEvent = new MethodCalled[] { methodCalled };
        }
        else {
            Array.Resize(ref onEvent, onEvent.Length + 1);
            onEvent[onEvent.Length - 1] = methodCalled;
        }
    }
    #endregion

    #endregion





    void Start() {


        //if got audio attached
        if (audioClip != null) {

            //assign new audio into audio
            audio = Audio.AudioClipToAudio(audioClip);
            //check need to play on start or not
            if (playOnStart == false) return;


            Play();
        }

    }

    private void OnDestroy() {
        audio?.Stop();
        audio?.Dispose();
    }



    public void Play(bool sameAsRestart = false) {


        audio.ClearAllEvent();

        //loop
        audio.OnAudioStopped += OnAudioStopped_CheckLoop;

        try {
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
        }
        catch (TargetParameterCountException) {
            Debug.LogError("Parameter mismatch. Please change your parameter variable, or change your method");
            return;
        }

        //only do action if not playing
        if (audio.State != PlaybackState.Playing) {
            audio.PitchFactor = this.pitchFactor;
            audio.volume = volume;
            if (audio.State == PlaybackState.Stopped)
                audio.SetSpeakerNumber(SpeakerDeviceMonoNumber);
            audio?.Play();
        }

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

    public void ChangePitch(float pitchFactor) {
        audio.PitchFactor = pitchFactor;
    }

    public void ChangeVolume(float volume) {
        audio.volume = volume;
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
        private SerializedProperty SpeakerDeviceMonoNumber;
        private SerializedProperty pitchFactor;
        private SerializedProperty volume;

        private SerializedProperty stereo;
        private SerializedProperty SpeakerDeviceLeftNumber;
        private SerializedProperty SpeakerDeviceRightNumber;


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
            SpeakerDeviceMonoNumber = serializedObject.FindProperty("SpeakerDeviceMonoNumber");
            pitchFactor = serializedObject.FindProperty("pitchFactor");
            volume = serializedObject.FindProperty("volume");

            stereo = serializedObject.FindProperty("Stereo");
            SpeakerDeviceLeftNumber = serializedObject.FindProperty("SpeakerDeviceLeftNumber");
            SpeakerDeviceRightNumber = serializedObject.FindProperty("SpeakerDeviceRightNumber");


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
                string[] methodNames = MethodCalled.GetPossibleMethods(methodOwner);

                //get the reference to the all method in class
                SerializedProperty allMethod = element.FindPropertyRelative("allMethod");
                allMethod.arraySize = methodNames.Length;

                //set each name into the all method
                for (int i = 0; i < methodNames.Length; i++) {
                    allMethod.GetArrayElementAtIndex(i).stringValue = methodNames[i];
                }

                float popUpWidth = EditorGUIUtility.currentViewWidth / 2;
                float rectX = rect.x + rect.width - popUpWidth;

                //display popup
                SerializedProperty selectedMethodIndex = element.FindPropertyRelative("selectedMethodIndex");
                selectedMethodIndex.intValue = EditorGUI.Popup(new Rect(rectX, rect.y, popUpWidth, EditorGUIUtility.singleLineHeight),
                    selectedMethodIndex.intValue, methodNames);
            }
        }


        void DrawHeader(Rect rect) {
            string name = currentList.headerName;
            EditorGUI.LabelField(rect, name);

        }
        private void OnDisable() {
            if (Application.isPlaying && audioBasic.audio != null) {
                audioCurrentPosition = audioBasic.audio.Position;
            }
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            //audioclip field
            EditorGUILayout.PropertyField(audioClip, true);

            //play on start 
            EditorGUI.BeginDisabledGroup(Application.isPlaying); // Disable the EnumPopup
            EditorGUILayout.PropertyField(playOnStart);
            EditorGUI.EndDisabledGroup();

            //loop
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

                            audioBasic.audio = Audio.AudioClipToAudio((AudioClip)audioClip.objectReferenceValue);
                        }
                    }

                }
            }

            //disable the audio.name != string checking, the audio will immediately change when a new audio drag into the field
            previousString = ((AudioClip)audioClip.objectReferenceValue)?.name;

            //select stereo
            EditorGUILayout.PropertyField(stereo);

            if (stereo.boolValue == true) {

                SpeakerDeviceLeftNumber.intValue = EditorGUILayout.Popup("Left Device", SpeakerDeviceLeftNumber.intValue, speakerDevicesName);
                SpeakerDeviceRightNumber.intValue = EditorGUILayout.Popup("Right Device", SpeakerDeviceRightNumber.intValue, speakerDevicesName);
            }
            else {
                //select speaker device
                SpeakerDeviceMonoNumber.intValue = EditorGUILayout.Popup("Speaker Device", SpeakerDeviceMonoNumber.intValue, speakerDevicesName);

            }

            //select pitch factor
            EditorGUILayout.PropertyField(pitchFactor);

            //volume
            EditorGUILayout.PropertyField(volume);

            serializedObject.ApplyModifiedProperties();



        }

    }

#endif
}