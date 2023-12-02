using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using NAudio.Wave;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif


public class AudioList : AudioBase {


    public AudioClip[] audioClipArray;
    public List<Audio> audioList;

    public int currentPosition = 0;
    public LoopMode Mode = LoopMode.Sequence;
    private bool PlayNextAudio = true;
    private bool audioIsPlaying = false;

    public Equalizer[] equalizerList = new Equalizer[] { new Equalizer() };
    public Equalizer[] EqualizerList { get { return equalizerList; } }
    public int[] selectedEqualizer = new int[1];

    public bool usingScript = false;

    public int AudioCount => audioList.Count;

    public Equalizer CurrentEqualizer {
        get {
            return base.EqualizerProperty;
        }
        set {
            base.EqualizerProperty = value;
        }
    }

    public string[] EqualizerListName {
        get {
            string[] names = new string[equalizerList.Length];
            names[0] = "No equalizer";
            for (int i = 1; i < names.Length; i++) {
                names[i] = "Equalizer " + (i).ToString();
            }

            return names;
        }
    }


    public enum LoopMode {
        Sequence,
        Random,
        Single,
        Once
    }




    private void CheckAudioListAndRefresh() {
        if (audioList == null) {
            CreateAudios();
            return;
        }


        string index = ArrayAndListIsTheSame();

        if (index != "-1") {
            UpdateAudios(index);
        }

    }


    public void StopNextAudio() {
        PlayNextAudio = false;
    }

    private void UpdateAudios(string index) {

        int audioIndex = int.Parse(index.Split(' ')[0]);
        int audioClipIndex = int.Parse(index.Split(' ')[1]);

        audioList.RemoveRange(audioIndex, audioList.Count - audioIndex);
        for (int i = audioClipIndex; i < audioClipArray.Length; i++) {

            while (audioClipArray[i] == null) {
                i++;
                if (i >= audioClipArray.Length) {
                    return;
                }
            }
#if UNITY_EDITOR
            audioList.Add(Audio.AudioClipToAudio(audioClipArray[i], this));
#endif
        }
    }

    public void CreateAudios() {
        audioList = new List<Audio>();

        for (int i = 0; i < audioClipArray.Length; i++) {

            //the element is empty
            while (audioClipArray[i] == null) {
                i++;
            }
#if UNITY_EDITOR
            audioList.Add(Audio.AudioClipToAudio(audioClipArray[i], this));
#endif
        }
    }




    /// <summary>
    /// if audioList.count and audioClipArray.Length same, check each element same or not
    /// </summary>
    /// <returns>
    /// return audio index + " " + audio Clip index<br></br>
    /// return -1 if same</returns>
    private string ArrayAndListIsTheSame() {

        int loopCount = audioClipArray.Length > audioList.Count ? audioClipArray.Length : audioList.Count;

        for (int audioI = 0, audioClipI = 0; audioClipI < loopCount; audioI++, audioClipI++) {

            //skip null
            while (audioClipArray[audioClipI] == null) {
                audioClipI++;
            }

            if (audioList.Count >= audioI) {
                return audioI.ToString() + " " + audioClipI.ToString();
            }

#if UNITY_EDITOR
            if (audioList[audioI].FilePath.EndsWith(
                AssetDatabase.GetAssetPath(audioClipArray[audioClipI].GetInstanceID())
                ) == false) {

                return audioI.ToString() + " " + audioClipI.ToString();
            }
#endif
        }

        return "-1";

    }

    private void PlaySameList() {
        PlaySameList(currentPosition);
    }

    private void PlaySameList(int position) {

        if (!usingScript) {
            CheckAudioListAndRefresh();
        }
        else if (audioList == null) {
            audioList = new();
        }

        if (audioList.Count == 0) return;

        //if current position out of range
        if (position >= audioList.Count) {
            currentPosition = 0;
            position = 0;
        }

        audio = audioList[position];

        CurrentEqualizer = equalizerList[selectedEqualizer[position]];

        base.Play(DefaultOnAudioStopped);

        /*
        audioList[position].ClearAllEvent();

        try {
            foreach (var method in onAudioStartedMethod) {
                audioList[position].OnAudioStarted += (Action<MonoBehaviour, Audio>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioPausedMethod) {
                audioList[position].OnAudioPaused += (Action<MonoBehaviour, Audio>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioResumedMethod) {
                audioList[position].OnAudioResumed += (Action<MonoBehaviour, Audio>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioRestartedMethod) {
                audioList[position].OnAudioRestarted += (Action<MonoBehaviour, Audio, bool>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio, bool>), null, method.methodToCall);
            }
            foreach (var method in onAudioStoppedMethod) {
                audioList[position].OnAudioStopped += (Action<MonoBehaviour, Audio, bool>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio, bool>), null, method.methodToCall);
            }
        }
        catch (TargetParameterCountException) {
            Debug.LogError("Parameter mismatch. Please change your parameter variable, or change your method");
            return;
        }

        audioList[position].OnAudioStopped += DefaultOnAudioStopped;



        //only do action if not playing

        if (audioList[position].State != PlaybackState.Playing) {
            audioList[position].PitchFactor = this.pitchFactor;
            audioList[position].Volume = volume;
            if (audioList[position].State == PlaybackState.Stopped)
                audioList[position].SetSpeakerNumber(SpeakerDeviceNumber);
            audioList[position].Play();
        }
        audioIsPlaying = true;
        */
    }

    public void Play(int position) {
        if (audioIsPlaying) return;
        PlaySameList(position);

        audioIsPlaying = true;
    }

    public void Play() {
        if (audioIsPlaying) return;
        PlaySameList(currentPosition);

        audioIsPlaying = true;
    }
    public new void Pause() {
        audioIsPlaying = false;
        audio?.Pause();
    }

    public void Restart() {
        base.Restart(this, DefaultOnAudioStopped);
    }


    private void DefaultOnAudioStopped(MonoBehaviour audioBase, Audio stoppedAudio, bool hasPlayedFinished) {
        if (Mode == LoopMode.Once) {
            audioIsPlaying = false;
            return;
        }
        if (hasPlayedFinished && PlayNextAudio == true) {
            ChangeNextSong();
            PlaySameList();
        }
        else if (PlayNextAudio == false) {
            PlayNextAudio = true;
            audioIsPlaying = false;
        }
    }
    public new void Stop() {
        //audioList[currentPosition].Stop();
        base.Stop();
        audioIsPlaying = false;
    }



    public void SetAudio(MonoBehaviour methodOwner, string path, int index) {
        if (audioList == null) {
            audioList = new();
        }

        if (index >= audioList.Count) {
            audioList.Add(new Audio(path, methodOwner));
            Array.Resize(ref selectedEqualizer, index + 1);
        }
        else {
            audioList[index] = new Audio(path, methodOwner);

        }
        selectedEqualizer[index] = 0;


        onAudioStartedMethod = new MethodCalled[0];
        onAudioPausedMethod = new MethodCalled[0];
        onAudioResumedMethod = new MethodCalled[0];
        onAudioRestartedMethod = new MethodCalled[0];
        onAudioStoppedMethod = new MethodCalled[0];

        usingScript = true;
    }

    public void RemoveAudio(int index) {

        audioList.RemoveAt(index);
    }

    public Audio GetAudio(int index) {
        return audioList[index];
    }




    #region LoopMode

    private void ChangeNextSong() {
        switch (Mode) {
            case LoopMode.Sequence:
                SequenceNextSong();
                break;
            case LoopMode.Random:
                RandomNextSong();
                break;
            case LoopMode.Single:
                SingleNextSong();
                break;
            default:
                break;
        }

    }

    private void SingleNextSong() {
        return;
    }

    private void RandomNextSong() {

        if (audioList.Count == 1) {
            currentPosition = 0;
            return;
        }
        else if (audioList.Count == 2) {
            SequenceNextSong();
            return;
        };

        int offset = UnityEngine.Random.Range(1, audioList.Count);
        currentPosition += offset;
        if (currentPosition >= audioList.Count) {
            currentPosition -= audioList.Count;
        }
    }

    private void SequenceNextSong() {
        if (audioList.Count - 1 == currentPosition) {
            currentPosition = 0;
        }
        else {
            currentPosition++;
        }
    }

    #endregion


    private new void OnDestroy() {
        if (audioList == null) return;
        if (audioList?.Count != 0) {
            foreach (var audio in audioList) {
                audio?.Stop();
                audio?.Dispose();
            }
        }
        audioIsPlaying = false;
    }

    private void Start() {

        if (audioClipArray == null || audioClipArray.Length == 0) return;

        if (PlayOnStart == false) return;


        audioList = new();
        Play();



    }

    public void AddNewEqualizer() {

        Array.Resize(ref equalizerList, equalizerList.Length + 1);
        equalizerList[equalizerList.Length - 1] = new Equalizer();
    }

    public void RemoveEqualizer(int index) {
        for (int k = 0; k < selectedEqualizer.Length; k++) {
            if (selectedEqualizer[k] == index) {
                selectedEqualizer[k] = 0;

                if (currentPosition == k) {
                    CurrentEqualizer = equalizerList[0];
                    audio?.UpdateEqualizer();
                }

            }

        }
        for (int i = index; i < equalizerList.Length - 1; i++) {

            equalizerList[i] = equalizerList[i + 1];
        }
        Array.Resize(ref equalizerList, equalizerList.Length - 1);
    }

    public void SetGain(int equalizerIndex, Frequency frequency, float gain) {
        equalizerList[equalizerIndex].equalizerBands[Equalizer.GetIndexByFrequency(frequency)].Gain = gain;
    }

    public float GetGain(int equalizerIndex, Frequency frequency) {
        return equalizerList[equalizerIndex].equalizerBands[Equalizer.GetIndexByFrequency(frequency)].Gain;
    }
    public void SetEqualizerToAudio(int equalizerIndex, int audioIndex) {
        selectedEqualizer[audioIndex] = equalizerIndex;
        if (audioIndex == currentPosition) {
            CurrentEqualizer = equalizerList[equalizerIndex];
            UpdateEqualizer();
        }
    }

    public void SetEqualizerByIndex(Equalizer equalizer, int equalizerIndex) {
        if (equalizerList[equalizerIndex] == CurrentEqualizer) {
            equalizerList[equalizerIndex] = equalizer;
            CurrentEqualizer = equalizerList[equalizerIndex];
            UpdateEqualizer();
            return;
        }
        equalizerList[equalizerIndex] = equalizer;
    }

    public Equalizer GetCurrentEqualizer() {
        return EqualizerProperty;
    }

    public Equalizer GetEqualizerByIndex(int index) {
        return equalizerList[index];
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AudioList))]
    public class AudioListEditor : Editor {

        private bool eventIsExpanded = false;
        private GUIStyle listMargin;
        private SerializedProperty[] eventArray = new SerializedProperty[(int)EventName.TotalEvent];
        private ReorderableList[] reorderableListArray = new ReorderableList[(int)EventName.TotalEvent];
        private Dictionary<EventName, string> headerName = new Dictionary<EventName, string>();
        struct ListHolder {
            public ReorderableList list;
            public string headerName;
        }
        ListHolder currentList;

        private GUIStyle labelStyle = new GUIStyle();

        public AudioList audioList;

        SerializedProperty audioClipArray;
        SerializedProperty mode;
        SerializedProperty playOnStart;
        private SerializedProperty SpeakerDeviceNumber;

        private SerializedProperty pitchFactor;
        private SerializedProperty volume;
        private SerializedProperty Panning;
        private SerializedProperty equalizerList;
        private SerializedProperty selectedEqualizer;

        private float lastFramePanning;
        private float lastFrameVolume;
        private float lastFramePitch;
        private bool equalizerIsExpanded;
        private bool[] eachEqualizerIsExpanded;

        private string[] frequencyList = {
            "31Hz","63Hz","125Hz","250Hz","500Hz","1kHz","2kHz","4kHz","8kHz","16kHz"
        };

        private void OnEnable() {

            InitializeHeaderName();

            audioList = (AudioList)target;
            audioClipArray = serializedObject.FindProperty("audioClipArray");
            mode = serializedObject.FindProperty("Mode");
            playOnStart = serializedObject.FindProperty("PlayOnStart");

            SpeakerDeviceNumber = serializedObject.FindProperty("SpeakerDeviceNumber");
            labelStyle.normal.textColor = Color.yellow;

            pitchFactor = serializedObject.FindProperty("PitchFactor");
            volume = serializedObject.FindProperty("volume");
            Panning = serializedObject.FindProperty("Panning");
            equalizerList = serializedObject.FindProperty("equalizerList");
            selectedEqualizer = serializedObject.FindProperty("selectedEqualizer");


            eventArray[(int)EventName.onAudioStarted] = serializedObject.FindProperty("onAudioStartedMethod");
            eventArray[(int)EventName.onAudioPaused] = serializedObject.FindProperty("onAudioPausedMethod");
            eventArray[(int)EventName.onAudioResumed] = serializedObject.FindProperty("onAudioResumedMethod");
            eventArray[(int)EventName.onAudioRestarted] = serializedObject.FindProperty("onAudioRestartedMethod");
            eventArray[(int)EventName.onAudioStopped] = serializedObject.FindProperty("onAudioStoppedMethod");


            if (audioList.equalizerList == null || audioList.equalizerList.Length == 0) {
                audioList.equalizerList = new Equalizer[] { new Equalizer() };
            }


            if (audioList.selectedEqualizer == null || audioList.selectedEqualizer.Length == 0) {
                audioList.selectedEqualizer = new int[] { 0 };
            }
            eachEqualizerIsExpanded = new bool[equalizerList.arraySize];

            InitializeAllList();

        }


        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.LabelField("Audio", labelStyle);

            EditorGUILayout.PropertyField(audioClipArray, true);

            /*
            if (audioList.equalizerList.Length != audioList.audioClipArray.Length) {
                Array.Resize(ref audioList.equalizerList, audioList.audioClipArray.Length);
            }
            int removedIndex = AudioClipArrayHasChanged();
            if (removedIndex != -1) {

                //not latest > latest
                if (audioList.audioClipArray.Length > audioClipArray.arraySize) {
                    while (removedIndex < audioList.audioClipArray.Length - 1) {
                        audioList.equalizerList[removedIndex] = audioList.equalizerList[removedIndex + 1];
                        removedIndex++;
                    }
                    Array.Resize(ref audioList.equalizerList, audioList.equalizerList.Length - 1);
                }
                else {
                    Array.Resize(ref audioList.equalizerList, audioList.equalizerList.Length + 1);
                    if (audioList.equalizerList.Length == 1) {
                        audioList.equalizerList[0] = 0;
                    }
                    else {
                        audioList.equalizerList[audioList.equalizerList.Length - 1] = audioList.equalizerList[audioList.equalizerList.Length - 2];
                    }
                }


            }*/





            mode.enumValueIndex = (int)(AudioList.LoopMode)EditorGUILayout.EnumPopup("Mode", (AudioList.LoopMode)mode.enumValueIndex);


            EditorGUILayout.LabelField("Setting", labelStyle);

            //play on start
            EditorGUI.BeginDisabledGroup(Application.isPlaying); // Disable the EnumPopup
            EditorGUILayout.PropertyField(playOnStart, true);
            EditorGUI.EndDisabledGroup();



            //event
            eventIsExpanded = EditorGUILayout.Foldout(eventIsExpanded, "Event");
            if (eventIsExpanded) {
                DrawAllEvent();
            }

            //select speaker device
            SpeakerDeviceNumber.intValue = EditorGUILayout.Popup("Speaker Device", SpeakerDeviceNumber.intValue, speakerDevicesName);
            lastFramePanning = Panning.floatValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Panning", GUILayout.Width(EditorGUIUtility.labelWidth));
            Panning.floatValue = EditorGUILayout.Slider(Panning.floatValue, -1.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            if (lastFramePanning != Panning.floatValue) {
                audioList.SetPanning(Panning.floatValue);

            }



            //select pitch factor
            lastFramePitch = pitchFactor.floatValue;
            EditorGUILayout.PropertyField(pitchFactor);
            if (lastFramePitch != pitchFactor.floatValue) {

                audioList.SetPitch(pitchFactor.floatValue);


            }

            //volume

            lastFrameVolume = volume.floatValue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Volume", GUILayout.Width(EditorGUIUtility.labelWidth));
            volume.floatValue = EditorGUILayout.Slider(volume.floatValue, 0, 1);
            EditorGUILayout.EndHorizontal();

            if (volume.floatValue != lastFrameVolume) {

                audioList.SetVolume(audioList.volume);

            }
            PrintEqualizer();


            serializedObject.ApplyModifiedProperties();


        }

        private void PrintEqualizer() {
            equalizerIsExpanded = EditorGUILayout.Foldout(equalizerIsExpanded, "Equalizer");
            if (!equalizerIsExpanded) return;

            //audio + equalizer
            for (int i = 0; i < audioClipArray.arraySize; i++) {
                EditorGUILayout.BeginHorizontal();
                string audioName;
                bool disablePopUp = false;
                if ((AudioClip)audioClipArray.GetArrayElementAtIndex(i).objectReferenceValue == null) {
                    audioName = "No Audio Clip";
                    disablePopUp = true;
                }
                else {
                    audioName = ((AudioClip)audioClipArray.GetArrayElementAtIndex(i).objectReferenceValue).name;
                }
                EditorGUILayout.LabelField(audioName, GUILayout.Width(EditorGUIUtility.labelWidth));

                //select equalizer
                EditorGUI.BeginDisabledGroup(disablePopUp);
                int oriEqualizerIndex;
                if (i >= selectedEqualizer.arraySize)
                    selectedEqualizer.arraySize += 1;
                oriEqualizerIndex = selectedEqualizer.GetArrayElementAtIndex(i).intValue;

                selectedEqualizer.GetArrayElementAtIndex(i).intValue = EditorGUILayout.Popup(selectedEqualizer.GetArrayElementAtIndex(i).intValue, audioList.EqualizerListName);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                if (oriEqualizerIndex != selectedEqualizer.GetArrayElementAtIndex(i).intValue) {
                    audioList.CurrentEqualizer = audioList.equalizerList[selectedEqualizer.GetArrayElementAtIndex(i).intValue];
                    audioList.UpdateEqualizer();
                }


            }

            //equalizer only

            for (int iEqualizer = 1; iEqualizer < equalizerList.arraySize; iEqualizer++) {
                eachEqualizerIsExpanded[iEqualizer] = EditorGUILayout.Foldout(eachEqualizerIsExpanded[iEqualizer], "Equalizer " + iEqualizer);
                if (!eachEqualizerIsExpanded[iEqualizer]) continue;

                for (int iFrequency = 0; iFrequency < frequencyList.Length; iFrequency++) {
                    EditorGUILayout.BeginHorizontal();
                    var gain = equalizerList.GetArrayElementAtIndex(iEqualizer).FindPropertyRelative("equalizerBands").GetArrayElementAtIndex(iFrequency).FindPropertyRelative("gain");
                    var oriGain = gain.floatValue;
                    EditorGUILayout.LabelField(frequencyList[iFrequency], GUILayout.Width(50));
                    gain.floatValue = EditorGUILayout.Slider(gain.floatValue, Equalizer.MIN_GAIN, Equalizer.MAX_GAIN);
                    if (oriGain != gain.floatValue) {

                        audioList.equalizerList[iEqualizer].equalizerBands[iFrequency].Gain = gain.floatValue;
                        //((Equalizer)equalizerList.GetArrayElementAtIndex(i).objectReferenceValue).equalizerBands[j].Gain = gain.floatValue;
                        //audioList.EqualizerProperty.equalizerBands[j].Gain = gain.floatValue;

                        audioList.audio?.UpdateEqualizer();

                    }
                    EditorGUILayout.EndHorizontal();
                }

                //reset button
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                bool resetIsPressed = GUILayout.Button("Reset To Default", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2));

                EditorGUILayout.EndHorizontal();
                if (resetIsPressed) {
                    for (int j = 0; j < frequencyList.Length; j++) {
                        audioList.equalizerList[iEqualizer].equalizerBands[j].Gain = 0.0f;
                    }
                    audioList.audio?.UpdateEqualizer();

                }
                if (GUILayout.Button("Delete Equalizer " + iEqualizer)) {
                    audioList.RemoveEqualizer(iEqualizer);


                }


            }


            if (GUILayout.Button("Add Equalizer")) {

                audioList.AddNewEqualizer();
                Array.Resize(ref eachEqualizerIsExpanded, eachEqualizerIsExpanded.Length + 1);

            }



        }

        private int AudioClipArrayHasChanged() {


            if (audioList.audioClipArray.Length == audioClipArray.arraySize) {
                return -1;
            }

            //not latest > latest
            if (audioList.audioClipArray.Length < audioClipArray.arraySize) {
                return audioList.audioClipArray.Length;
            }
            int i;
            for (i = 0; i < audioClipArray.arraySize; i++) {
                if (((AudioClip)audioClipArray.GetArrayElementAtIndex(i).objectReferenceValue) == null) {
                    return audioClipArray.arraySize;
                }
                if (audioList.audioClipArray[i].name != ((AudioClip)audioClipArray.GetArrayElementAtIndex(i).objectReferenceValue).name) {


                    break;
                }
            }

            return i;




        }



        #region Event
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
        void DrawHeader(Rect rect) {
            string name = currentList.headerName;
            EditorGUI.LabelField(rect, name);

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

        #endregion


    }


#endif

}



