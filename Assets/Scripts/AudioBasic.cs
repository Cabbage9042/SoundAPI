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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset">In seconds</param>
    /// <returns></returns>



    public PlaybackState State {
        get {
            return audio == null ? PlaybackState.Stopped : audio.State;
        }
    }
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


    public void setAudioClip(MonoBehaviour methodOwner, string path) {
        audio = new Audio(path, methodOwner);

        onAudioStartedMethod = new MethodCalled[0];
        onAudioPausedMethod = new MethodCalled[0];
        onAudioResumedMethod = new MethodCalled[0];
        onAudioRestartedMethod = new MethodCalled[0];
        onAudioStoppedMethod = new MethodCalled[0];
    }

    void Start() {


        //if got audio attached
        if (audioClip != null) {

            //assign new audio into audio
            UpdateLatestAudio();

            //check need to play on start or not
            if (playOnStart == false) return;


            Play();
        }

    }

    




    public void Play() {

        base.Play(OnAudioStopped_CheckLoop);

    }


    public void Restart() {
        base.Restart(this, OnAudioStopped_CheckLoop);
    }





    private void OnAudioStopped_CheckLoop(MonoBehaviour audioBase, Audio stoppedAudio, bool hasFinishedPlaying) {
        if (hasFinishedPlaying) {
            if (loop) {
                Play();
            }
        }
        //stopDelaying = false;
        //delayedAmplitude.Clear();
    }




    private void UpdateLatestAudio() {
        audio = Audio.AudioClipToAudio(audioClip, this);

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
        private SerializedProperty SpeakerDeviceNumber;
        private SerializedProperty pitchFactor;
        private SerializedProperty volume;

        private SerializedProperty Panning;
        private SerializedProperty equalizer;


        private SerializedProperty[] eventArray = new SerializedProperty[(int)EventName.TotalEvent];
        private ReorderableList[] reorderableListArray = new ReorderableList[(int)EventName.TotalEvent];
        private Dictionary<EventName, string> headerName = new Dictionary<EventName, string>();

        private bool updateAudio = false;

        private float lastFramePanning;
        private float lastFrameVolume;
        private float lastFramePitch;

        private bool eventIsExpanded = false;
        private bool equalizerIsExpanded = false;

        private string[] frequencyList = {
            "31Hz","63Hz","125Hz","250Hz","500Hz","1kHz","2kHz","4kHz","8kHz","16kHz"
        };


        private GUIStyle listMargin;

        private void OnEnable() {
            InitializeHeaderName();

            audioBasic = (AudioBasic)target;
            audioClip = serializedObject.FindProperty("audioClip");
            playOnStart = serializedObject.FindProperty("playOnStart");
            loop = serializedObject.FindProperty("loop");
            SpeakerDeviceNumber = serializedObject.FindProperty("SpeakerDeviceNumber");
            pitchFactor = serializedObject.FindProperty("pitchFactor");
            volume = serializedObject.FindProperty("volume");

            Panning = serializedObject.FindProperty("Panning");
            equalizer = serializedObject.FindProperty("privateEqualizer");



            eventArray[(int)EventName.onAudioStarted] = serializedObject.FindProperty("onAudioStartedMethod");
            eventArray[(int)EventName.onAudioPaused] = serializedObject.FindProperty("onAudioPausedMethod");
            eventArray[(int)EventName.onAudioResumed] = serializedObject.FindProperty("onAudioResumedMethod");
            eventArray[(int)EventName.onAudioRestarted] = serializedObject.FindProperty("onAudioRestartedMethod");
            eventArray[(int)EventName.onAudioStopped] = serializedObject.FindProperty("onAudioStoppedMethod");


            InitializeAllList();

        }

        private void OnDisable() {
            if (Application.isPlaying && audioBasic.audio != null) {
                audioCurrentPosition = audioBasic.audio.Position;
            }
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            //if last frame got change audioclip, change the audio this frame
            if (updateAudio) {
                audioBasic.UpdateLatestAudio();
                updateAudio = false;
            }

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
                            //set to true, next frame only update
                            updateAudio = true;


                        }
                    }

                }
            }

            //disable the audio.name != string checking, the audio will immediately change when a new audio drag into the field
            previousString = ((AudioClip)audioClip.objectReferenceValue)?.name;




            //select speaker device
            int oriSpeakerMono = SpeakerDeviceNumber.intValue;
            SpeakerDeviceNumber.intValue = EditorGUILayout.Popup("Speaker Device", SpeakerDeviceNumber.intValue, speakerDevicesName);
            if (oriSpeakerMono != SpeakerDeviceNumber.intValue) {
                audioBasic.audio?.SetSpeakerNumber(SpeakerDeviceNumber.intValue);
            }



            lastFramePanning = Panning.floatValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Panning", GUILayout.Width(EditorGUIUtility.labelWidth));
            Panning.floatValue = EditorGUILayout.Slider(Panning.floatValue, -1.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            if (lastFramePanning != Panning.floatValue) {
                audioBasic.SetPanning(Panning.floatValue);

            }



            //select pitch factor
            lastFramePitch = pitchFactor.floatValue;
            EditorGUILayout.PropertyField(pitchFactor);
            if (lastFramePitch != pitchFactor.floatValue) {

                audioBasic.SetPitch(pitchFactor.floatValue);


            }

            //volume

            lastFrameVolume = volume.floatValue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Volume", GUILayout.Width(EditorGUIUtility.labelWidth));
            volume.floatValue = EditorGUILayout.Slider(volume.floatValue, 0, 1);
            EditorGUILayout.EndHorizontal();

            if (volume.floatValue != lastFrameVolume) {

                audioBasic.SetVolume(audioBasic.volume);

            }


            //equalizer

            /*if(equalizerbands.arraySize == 0) {
                audioBasic.equalizerbands = EqualizerBand.DefaultEqualizerBands();
            }*/




            //   Debug.Log($"Bands property path: {bands.arraySize}");
            //   Debug.Log($"Band property path: {band.propertyPath}");
            //  Debug.Log($"QFactor property path: {la.propertyPath}");
            //q = !q;
            //equalizer.FindPropertyRelative("equalizerBands").GetArrayElementAtIndex(0).FindPropertyRelative("Gain").floatValue =  

            PrintEqualizer();


            serializedObject.ApplyModifiedProperties();
        }

        private void PrintEqualizer() {
            equalizerIsExpanded = EditorGUILayout.Foldout(equalizerIsExpanded, "Equalizer");
            if (!equalizerIsExpanded) return;



            for (int i = 0; i < frequencyList.Length; i++) {
                EditorGUILayout.BeginHorizontal();
                var gain = equalizer.FindPropertyRelative("equalizerBands").GetArrayElementAtIndex(i).FindPropertyRelative("Gain");
                var oriGain = gain.floatValue;
                EditorGUILayout.LabelField(frequencyList[i], GUILayout.Width(50));
                gain.floatValue = EditorGUILayout.Slider(gain.floatValue, Equalizer.MIN_GAIN, Equalizer.MAX_GAIN);
                if (oriGain != gain.floatValue) {
                    audioBasic.EqualizerProperty.equalizerBands[i].Gain = gain.floatValue;

                    audioBasic.audio?.UpdateEqualizer();

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
                    audioBasic.EqualizerProperty.equalizerBands[i].Gain = 0.0f;
                }
                audioBasic.audio?.UpdateEqualizer();

            }


        }
 #region Event
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

        #endregion


    }
       
#endif
}