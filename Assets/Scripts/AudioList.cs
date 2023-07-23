using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioList : MonoBehaviour {
    public AudioClip[] audioClipArray;
    public List<Audio> audioList;
    int currentPosition = 0;
    public LoopMode mode = LoopMode.Sequence;

    public enum LoopMode {
        Sequence,
        Random,
        Single
    }

    public PlayOnStartState playOnStart;

    public void Play(int position) {
        if (audioList == null || audioList.Count != audioClipArray.Length) {
            AudioClipsToAudios();

        }

        if (audioList.Count == 0) return;

        if (position >= audioList.Count) {
            currentPosition = 0;
            position = 0;
        }

        audioList[position].ClearAllEvent();

        audioList[position].OnAudioStopped += DefaultOnAudioStopped;

        audioList[position].Play();

    }

    public void Play() {
        Play(currentPosition);
    }



    private void DefaultOnAudioStopped(Audio stoppedAudio, bool hasPlayedFinished) {
        if (hasPlayedFinished) {
            ChangeNextSong();
            Play();
        }
    }
    public void Stop() {
        audioList[currentPosition].Stop();
    }

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

    public void AudioClipsToAudios() {
        audioList = new List<Audio>();

        for (int audioCLipI = 0, audioI = 0; audioCLipI < audioClipArray.Length; audioCLipI++, audioI++) {

            //the element is empty
            if (audioClipArray[audioCLipI] == null) {
                audioCLipI++;
            }
            audioList.Add(Audio.AudioClipToAudio(audioClipArray[audioCLipI]));

        }
    }

    private void OnDestroy() {
        if (audioList == null) return;
        if (audioList?.Count != 0) {
            foreach (var audio in audioList) {
                audio?.Stop();
                audio?.Dispose();
            }
        }
    }

    private void Start() {

        if (audioClipArray == null || audioClipArray.Length == 0) return;

        if (playOnStart != PlayOnStartState.True) return;

        Play();
        playOnStart = PlayOnStartState.Played;

    }

    /*
    public MethodCalled[] onAudioStartedMethod;
    public MethodCalled[] onAudioPausedMethod;
    public MethodCalled[] onAudioResumedMethod;
    public MethodCalled[] onAudioRestartedMethod;
    public MethodCalled[] onAudioStoppedMethod;*/



}


#if UNITY_EDITOR
[CustomEditor(typeof(AudioList))]
public class AudioListEditor : Editor {

    private GUIStyle labelStyle = new GUIStyle();

    public AudioList audioList;

    SerializedProperty audioClipArray;
    SerializedProperty mode;
    SerializedProperty playOnStart;

    private void OnEnable() {
        audioList = (AudioList)target;
        audioClipArray = serializedObject.FindProperty("audioClipArray");
        mode = serializedObject.FindProperty("mode");
        playOnStart = serializedObject.FindProperty("playOnStart");

        labelStyle.normal.textColor = Color.yellow;
    }
    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.LabelField("Audio", labelStyle);

        EditorGUILayout.PropertyField(audioClipArray, true);
        if (GUILayout.Button("Refresh List")) {
            audioList.AudioClipsToAudios();
        }
        mode.enumValueIndex = (int)(AudioList.LoopMode)EditorGUILayout.EnumPopup("Mode", (AudioList.LoopMode)mode.enumValueIndex);


        EditorGUILayout.LabelField("Setting", labelStyle);

        //play on start
        EditorGUI.BeginDisabledGroup(Application.isPlaying); // Disable the EnumPopup
        playOnStart.enumValueIndex = (int)(PlayOnStartState)EditorGUILayout.EnumPopup("Play On Start", (PlayOnStartState)playOnStart.enumValueIndex);
        EditorGUI.EndDisabledGroup();

        if (Application.isPlaying) {

        }
        else {
            //dont allow user to select played
            if (playOnStart.enumValueIndex == (int)PlayOnStartState.Played) {
                audioList.playOnStart = PlayOnStartState.False;
            }
        }



        serializedObject.ApplyModifiedProperties();
    }



}





#endif