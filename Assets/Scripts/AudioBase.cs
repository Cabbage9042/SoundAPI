using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


public enum EventName {
    onAudioStarted,
    onAudioPaused,
    onAudioResumed,
    onAudioRestarted,
    onAudioStopped,
    TotalEvent
}

public enum PlayOnStartState {
    True,
    False,
    Played
}


[System.Serializable]
public class MethodCalled {
    public MonoBehaviour methodOwner = null;
    public string[] allMethod;
    public int selectedMethodIndex;

    public MethodInfo methodToCall { get { return methodOwner.GetType().GetMethod(allMethod[selectedMethodIndex]); } }

    public MethodCalled(MonoBehaviour methodOwner, string methodName) {
        this.methodOwner = methodOwner;
        allMethod = GetPossibleMethods(methodOwner);

        selectedMethodIndex = Array.IndexOf(allMethod, methodName);
        if (selectedMethodIndex == -1) {
            Debug.LogError($"Method {methodName} not found in class {methodOwner.GetType().Name}!");
        }

    }

    public override bool Equals(object obj) {

        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        if (((MethodCalled)obj).methodToCall == methodToCall) {
            return true;
        }
        return false;
    }



    public static string[] GetPossibleMethods(MonoBehaviour methodOwner) {
        MethodInfo[] allMethodInfo = methodOwner.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
        List<string> methodNames = new List<string>();
        for (int i = 0; i < allMethodInfo.Length; i++) {
            if (!allMethodInfo[i].Name.StartsWith("get_") && !allMethodInfo[i].Name.StartsWith("set_")) {
                methodNames.Add(allMethodInfo[i].Name);
            }
        }
        return methodNames.ToArray();
    }

}

public class AudioBase : MonoBehaviour {
    protected new Audio audio = null;
    protected Audio[] audioStereo = new Audio[2];


    public bool playOnStart;

    public float pitchFactor = 1.0f;

    public float volume = 1.0f;
    public MethodCalled[] onAudioStartedMethod;
    public MethodCalled[] onAudioPausedMethod;
    public MethodCalled[] onAudioResumedMethod;
    public MethodCalled[] onAudioRestartedMethod;
    public MethodCalled[] onAudioStoppedMethod;

   public int SpeakerDeviceMonoNumber = 0;
   public int SpeakerDeviceLeftNumber;
   public int SpeakerDeviceRightNumber;
   
   public bool Stereo = false;
   public float Panning = 0.0f;



    public static string[] speakerDevicesName { get { return Audio.speakerDevicesName; } }




}
