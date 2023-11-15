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
    public Equalizer privateEqualizer = new();
    public Equalizer EqualizerProperty {
        get {
            if (privateEqualizer == null) privateEqualizer = new();
            return privateEqualizer;
        }
        set {
            privateEqualizer = value;
            if (MonoIsPlaying) {
                audio.equalizer = privateEqualizer;
            }
            else if (StereoIsPlaying) {
                audioStereo[0].equalizer = privateEqualizer;
                audioStereo[1].equalizer = privateEqualizer;
            }
        }
    }


    public int SampleRate { get { return audio.WaveFormat.SampleRate; } }

    public bool loop = false;
    public double[] GetAmplitude() {
        return audio?.GetAmplitude();
    }
    public double[] GetAmplitude(int[] targetAmplitudes) {
        return audio?.GetAmplitude(targetAmplitudes);
    }
    public bool MonoIsPlaying { get { return audio.State == PlaybackState.Playing; } }
    public bool StereoIsPlaying { get { return audioStereo[0].State == PlaybackState.Playing; } }

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

    private Audio GetMonoOrStereoAudio() {
        if (audio.State == PlaybackState.Playing) {
            return audio;
        }
        else if (audioStereo[0] != null && audioStereo[0].State == PlaybackState.Playing) {
            return audioStereo[0];
        }

        if (Stereo) {
            return audioStereo[0];
        }
        else {
            return audio;
        }
    }

    public void setAudioClip(AudioClip audioClip, string path) {
        this.audioClip = audioClip;
        audio = new Audio(path, this);
        audioStereo[0] = new Audio(path, this);
        audioStereo[1] = new Audio(path, this);

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

    private void Update() {
        //audio?.GetAmplitude();
        //print(Panning);
    }



    private void OnDestroy() {
        audio?.Stop();
        audio?.Dispose();
        audioStereo[0]?.Stop();
        audioStereo[0]?.Dispose();
        audioStereo[1]?.Stop();
        audioStereo[1]?.Dispose();
    }



    public void Play(bool sameAsRestart = false) {



        if (audioStereo[0].State == PlaybackState.Playing || audio.State == PlaybackState.Playing) {
            return;
        }

        GetMonoOrStereoAudio().ClearAllEvent();

        //loop
        GetMonoOrStereoAudio().OnAudioStopped += OnAudioStopped_CheckLoop;

        try {
            foreach (var method in onAudioStartedMethod) {
                GetMonoOrStereoAudio().OnAudioStarted += (Action<AudioBase, Audio>)Delegate.CreateDelegate(typeof(Action<AudioBase, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioPausedMethod) {
                GetMonoOrStereoAudio().OnAudioPaused += (Action<AudioBase, Audio>)Delegate.CreateDelegate(typeof(Action<AudioBase, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioResumedMethod) {
                GetMonoOrStereoAudio().OnAudioResumed += (Action<AudioBase, Audio>)Delegate.CreateDelegate(typeof(Action<AudioBase, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioRestartedMethod) {
                GetMonoOrStereoAudio().OnAudioRestarted += (Action<AudioBase, Audio, bool>)Delegate.CreateDelegate(typeof(Action<AudioBase, Audio, bool>), null, method.methodToCall);
            }
            foreach (var method in onAudioStoppedMethod) {
                GetMonoOrStereoAudio().OnAudioStopped += (Action<AudioBase, Audio, bool>)Delegate.CreateDelegate(typeof(Action<AudioBase, Audio, bool>), null, method.methodToCall);
            }
        }
        catch (TargetParameterCountException) {
            Debug.LogError("Parameter mismatch. Please change your parameter variable, or change your method");
            return;
        }

        //only do action if not playing
        if (GetMonoOrStereoAudio().State != PlaybackState.Playing) {
            if (Stereo) {
                audioStereo[0].PitchFactor = this.pitchFactor;
                audioStereo[1].PitchFactor = this.pitchFactor;
                float left, right;
                CalculatePanningVolume(out left, out right);
                audioStereo[0].Volume = left;
                audioStereo[1].Volume = right;
                audioStereo[0].SetSpeakerNumber(SpeakerDeviceLeftNumber);
                audioStereo[1].SetSpeakerNumber(SpeakerDeviceRightNumber);
                audioStereo[0].Play();
                audioStereo[1].Play();
                audioStereo[0].equalizer = EqualizerProperty;
                audioStereo[1].equalizer = EqualizerProperty;
            }
            else {
                audio.PitchFactor = this.pitchFactor;
                audio.Volume = volume;
                audio.SetSpeakerNumber(SpeakerDeviceMonoNumber);
                audio?.Play();
                audio.equalizer = EqualizerProperty;
            }

            //GetMonoOrStereoAudio().ChangeGain(Frequency.F31, 0.01f);
            // GetMonoOrStereoAudio().ChangeGain(Frequency.F31,0.0f);

        }

    }

    private void CalculatePanningVolume(out float left, out float right) {


        /*left = (Panning <= 0) ? left : (left * (1 - Panning) / 2.0f);
        right = (Panning >= 0) ? right : (right * (Panning + 1) / 2.0f);*/
        left = (Panning <= 0) ? 1 : 1 - Panning;
        right = (Panning <= 0) ? 1 + Panning : 1;
        left *= volume;
        right *= volume;
    }
    public void UpdatePanning() {
        float left, right;
        CalculatePanningVolume(out left, out right);
        if (audioStereo[0] != null) {
            audioStereo[0].Volume = left;
            audioStereo[1].Volume = right;
        }
    }

    public void Stop() {
        if (StereoIsPlaying) {
            audioStereo[0].Stop();
            audioStereo[1].Stop();
        }
        else {
            audio.Stop();

        }
    }

    public void Restart() {
        //this is == play
        if (GetMonoOrStereoAudio().State == PlaybackState.Stopped) {
            Play();
            //same as play == false in audio.restart
            GetMonoOrStereoAudio().OnAudioRestarted?.Invoke(this, audio, true);
        }
        //this is the true restart(audio is playing)
        else {
            if (StereoIsPlaying) {
                audioStereo[0].Restart();
                audioStereo[1].Restart();
            }
            else {
                audio.Restart();
            }
        }
    }

    public void Pause() {
        if (StereoIsPlaying) {
            audioStereo[0].Pause();
            audioStereo[1].Pause();
        }
        else {
            audio.Pause();
        }
    }

    private void OnAudioStopped_CheckLoop(AudioBase audioBase, Audio stoppedAudio, bool hasFinishedPlaying) {
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
        audio.Volume = volume;
    }

    private void UpdateLatestAudio() {
        audio = Audio.AudioClipToAudio(audioClip, this);

        audioStereo[0] = Audio.AudioClipToAudio(audioClip, this);
        audioStereo[1] = Audio.AudioClipToAudio(audioClip, this);
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
        private SerializedProperty Panning;
        private SerializedProperty equalizer;


        private SerializedProperty[] eventArray = new SerializedProperty[(int)EventName.TotalEvent];
        private ReorderableList[] reorderableListArray = new ReorderableList[(int)EventName.TotalEvent];
        private Dictionary<EventName, string> headerName = new Dictionary<EventName, string>();

        private bool updateAudio = false;

        private float lastFramePanning = 0.0f;
        private float lastFrameVolume = 1.0f;

        private bool eventIsExpanded = false;
        private bool equalizerIsExpanded = false;

        private string[] frequencyList = {
            "31Hz","63Hz","125Hz","250Hz","500Hz","1kHz","2kHz","4kHz","8kHz","16kHz"
        };


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
            Panning = serializedObject.FindProperty("Panning");
            equalizer = serializedObject.FindProperty("privateEqualizer");



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

            //select stereo
            EditorGUILayout.PropertyField(stereo);

            if (stereo.boolValue == true) {

                SpeakerDeviceLeftNumber.intValue = EditorGUILayout.Popup("Left Device", SpeakerDeviceLeftNumber.intValue, speakerDevicesName);
                SpeakerDeviceRightNumber.intValue = EditorGUILayout.Popup("Right Device", SpeakerDeviceRightNumber.intValue, speakerDevicesName);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Panning", GUILayout.Width(EditorGUIUtility.labelWidth));
                Panning.floatValue = EditorGUILayout.Slider(Panning.floatValue, -1.0f, 1.0f);
                EditorGUILayout.EndHorizontal();

                if (lastFramePanning != Panning.floatValue) {
                    audioBasic.UpdatePanning();

                }

                lastFramePanning = Panning.floatValue;

            }
            else {
                //select speaker device
                SpeakerDeviceMonoNumber.intValue = EditorGUILayout.Popup("Speaker Device", SpeakerDeviceMonoNumber.intValue, speakerDevicesName);

            }

            //select pitch factor
            EditorGUILayout.PropertyField(pitchFactor);

            //volume

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Volume", GUILayout.Width(EditorGUIUtility.labelWidth));
            volume.floatValue = EditorGUILayout.Slider(volume.floatValue, 0, 1);
            EditorGUILayout.EndHorizontal();

            if (volume.floatValue != lastFrameVolume) {
                audioBasic.UpdatePanning();
            }

            lastFrameVolume = volume.floatValue;

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
                    if (audioBasic.MonoIsPlaying) {
                        audioBasic.audio.UpdateEquilizer();
                    }else if (audioBasic.StereoIsPlaying) {
                        audioBasic.audioStereo[0].UpdateEquilizer();
                        audioBasic.audioStereo[1].UpdateEquilizer();
                    }
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
                if (audioBasic.MonoIsPlaying) {
                    audioBasic.audio.UpdateEquilizer();
                }
                else if (audioBasic.StereoIsPlaying) {
                    audioBasic.audioStereo[0].UpdateEquilizer();
                    audioBasic.audioStereo[1].UpdateEquilizer();
                }
            }


        }


    }

#endif
}