using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditorInternal;
using System.Reflection;
using NAudio.Wave;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class AudioList : AudioBase {


    public AudioClip[] audioClipArray;
    public List<Audio> audioList;

    public int currentPosition = 0;
    public LoopMode mode = LoopMode.Sequence;

    private bool audioIsPlaying = false;





    public enum LoopMode {
        Sequence,
        Random,
        Single
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

            audioList.Add(Audio.AudioClipToAudio(audioClipArray[i]));

        }
    }

    public void CreateAudios() {
        audioList = new List<Audio>();

        for (int i = 0; i < audioClipArray.Length; i++) {

            //the element is empty
            while (audioClipArray[i] == null) {
                i++;
            }
            audioList.Add(Audio.AudioClipToAudio(audioClipArray[i]));

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

            if (audioList[audioI].FilePath.EndsWith(
                AssetDatabase.GetAssetPath(audioClipArray[audioClipI].GetInstanceID())
                ) == false) {

                return audioI.ToString() + " " + audioClipI.ToString();
            }

        }

        return "-1";

    }

    private void PlaySameList() {
        PlaySameList(currentPosition);
    }

    private void PlaySameList(int position) {

        CheckAudioListAndRefresh();

        if (audioList.Count == 0) return;

        //if current position out of range
        if (position >= audioList.Count) {
            currentPosition = 0;
            position = 0;
        }


        audioList[position].ClearAllEvent();

        try {
            foreach (var method in onAudioStartedMethod) {
                audioList[position].OnAudioStarted += (Action<Audio>)Delegate.CreateDelegate(typeof(Action<Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioPausedMethod) {
                audioList[position].OnAudioPaused += (Action<Audio>)Delegate.CreateDelegate(typeof(Action<Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioResumedMethod) {
                audioList[position].OnAudioResumed += (Action<Audio>)Delegate.CreateDelegate(typeof(Action<Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioRestartedMethod) {
                audioList[position].OnAudioRestarted += (Action<Audio, bool>)Delegate.CreateDelegate(typeof(Action<Audio, bool>), null, method.methodToCall);
            }
            foreach (var method in onAudioStoppedMethod) {
                audioList[position].OnAudioStopped += (Action<Audio, bool>)Delegate.CreateDelegate(typeof(Action<Audio, bool>), null, method.methodToCall);
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
            audioList[position].volume = volume;
            if (audioList[position].State == PlaybackState.Stopped)
                audioList[position].SetSpeakerNumber(SpeakerDeviceMonoNumber);
            audioList[position].Play();
        }
        audioIsPlaying = true;

    }

    public void Play(int position) {
        if (audioIsPlaying) return;
        PlaySameList(currentPosition);

    }

    public void Play() {
        if (audioIsPlaying) return;
        PlaySameList(currentPosition);
    }



    private void DefaultOnAudioStopped(Audio stoppedAudio, bool hasPlayedFinished) {
        if (hasPlayedFinished) {
            ChangeNextSong();
            PlaySameList();
        }
    }
    public void Stop() {
        audioList[currentPosition].Stop();
        audioIsPlaying = false;
    }


    #region LoopMode

    private void ChangeNextSong() {
        switch (mode) {
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


    private void OnDestroy() {
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

        if (playOnStart == false) return;

        Play();

    }

    /*
    public MethodCalled[] onAudioStartedMethod;
    public MethodCalled[] onAudioPausedMethod;
    public MethodCalled[] onAudioResumedMethod;
    public MethodCalled[] onAudioRestartedMethod;
    public MethodCalled[] onAudioStoppedMethod;*/



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
        private SerializedProperty SpeakerDeviceMonoNumber;

        private SerializedProperty pitchFactor;
        private SerializedProperty volume;

        private void OnEnable() {

            InitializeHeaderName();

            audioList = (AudioList)target;
            audioClipArray = serializedObject.FindProperty("audioClipArray");
            mode = serializedObject.FindProperty("mode");
            playOnStart = serializedObject.FindProperty("playOnStart");

            SpeakerDeviceMonoNumber = serializedObject.FindProperty("SpeakerDeviceMonoNumber");
            labelStyle.normal.textColor = Color.yellow;
            pitchFactor = serializedObject.FindProperty("pitchFactor");
            volume = serializedObject.FindProperty("volume");

            eventArray[(int)EventName.onAudioStarted] = serializedObject.FindProperty("onAudioStartedMethod");
            eventArray[(int)EventName.onAudioPaused] = serializedObject.FindProperty("onAudioPausedMethod");
            eventArray[(int)EventName.onAudioResumed] = serializedObject.FindProperty("onAudioResumedMethod");
            eventArray[(int)EventName.onAudioRestarted] = serializedObject.FindProperty("onAudioRestartedMethod");
            eventArray[(int)EventName.onAudioStopped] = serializedObject.FindProperty("onAudioStoppedMethod");

            InitializeAllList();
        }


        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.LabelField("Audio", labelStyle);

            EditorGUILayout.PropertyField(audioClipArray, true);

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
            SpeakerDeviceMonoNumber.intValue = EditorGUILayout.Popup("Speaker Device", SpeakerDeviceMonoNumber.intValue, speakerDevicesName);

            //select pitch factor
            EditorGUILayout.PropertyField(pitchFactor);

            EditorGUILayout.PropertyField(volume);

            serializedObject.ApplyModifiedProperties();
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


}





#endif