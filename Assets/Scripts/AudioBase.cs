using NAudio.Wave;
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

    public override int GetHashCode() {
        return HashCode.Combine(methodOwner, allMethod, selectedMethodIndex, methodToCall);
    }
}

public class AudioBase : MonoBehaviour {
    protected new Audio audio = null;

    public Equalizer privateEqualizer = new();
    protected Equalizer EqualizerProperty {
        get {
            if (privateEqualizer == null) privateEqualizer = new();
            return privateEqualizer;
        }
        set {
            privateEqualizer = value;
            if (audio != null) audio.equalizer = privateEqualizer;

        }
    }

    public bool playOnStart;

    public float pitchFactor = 1.0f;

    public float volume = 1.0f;
    public MethodCalled[] onAudioStartedMethod;
    public MethodCalled[] onAudioPausedMethod;
    public MethodCalled[] onAudioResumedMethod;
    public MethodCalled[] onAudioRestartedMethod;
    public MethodCalled[] onAudioStoppedMethod;

    public int SpeakerDeviceNumber = 0;

    public float Panning = 0.0f;


    protected List<double[]> delayedAmplitude;
    protected float playStartedTime;
    protected bool isDelaying = false;

    public static string[] speakerDevicesName { get { return Audio.speakerDevicesName; } }
    public double[] GetAmplitude(float offset = 0) {


        if (offset < 0) {
            offset = 0;
        }
        if (offset == 0) {
            return audio?.GetAmplitude();
        }
        if (delayedAmplitude == null) {
            delayedAmplitude = new();
        }
        double[] temp = audio?.GetAmplitude();
        if (temp != null) delayedAmplitude.Add(temp);
        else {
            if (delayedAmplitude.Count == 0) {
                isDelaying = false;
                return null;
            }

        }

        if (isDelaying == false) {
            if (Time.time - playStartedTime < offset) {
                return null;
            }
            else {
                isDelaying = true;
            }
        }

        double[] fft = delayedAmplitude[0];
        delayedAmplitude.RemoveAt(0);
        return fft;

    }
    public double[] GetAmplitude(double[] amplitudes, int[] targetFrequencies, int sampleRate) {
        return SpectrumAnalyzer.GetAmplitude(amplitudes, targetFrequencies, sampleRate);
    }

    public double[] GetAmplitude(int[] targetAmplitudes) {
        return audio?.GetAmplitude(targetAmplitudes);
    }


    protected void Play(Action<MonoBehaviour, Audio, bool> defaultStop) {
        if (audio.State == PlaybackState.Playing) {
            return;
        }

        audio.ClearAllEvent();

        //loop
        audio.OnAudioStopped += defaultStop;

        try {
            foreach (var method in onAudioStartedMethod) {
                audio.OnAudioStarted += (Action<MonoBehaviour, Audio>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioPausedMethod) {
                audio.OnAudioPaused += (Action<MonoBehaviour, Audio>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioResumedMethod) {
                audio.OnAudioResumed += (Action<MonoBehaviour, Audio>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio>), null, method.methodToCall);
            }
            foreach (var method in onAudioRestartedMethod) {
                audio.OnAudioRestarted += (Action<MonoBehaviour, Audio, bool>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio, bool>), null, method.methodToCall);
            }
            foreach (var method in onAudioStoppedMethod) {
                audio.OnAudioStopped += (Action<MonoBehaviour, Audio, bool>)Delegate.CreateDelegate(typeof(Action<MonoBehaviour, Audio, bool>), null, method.methodToCall);
            }
        }
        catch (TargetParameterCountException) {
            Debug.LogError("Parameter mismatch. Please change your parameter variable, or change your method");
            return;
        }


        audio.SetSpeakerNumber(SpeakerDeviceNumber);
        audio.ChangePitch(pitchFactor);
        audio.Volume = volume;
        audio.Panning = Panning;
        audio?.Play();
        audio.equalizer = EqualizerProperty;



        playStartedTime = Time.time;
        isDelaying = false;
        delayedAmplitude?.Clear();
    }

    public void Stop() {
        audio?.Stop();
    }
    protected void Restart(MonoBehaviour childClass, Action<MonoBehaviour, Audio, bool> defaultStop) {
        //this is == play
        if (audio.State == PlaybackState.Stopped) {
            Play(defaultStop);
            //same as play == false in audio.restart
            audio.OnAudioRestarted?.Invoke(childClass, audio, true);
        }
        //this is the true restart(audio is playing)
        else {

            audio.Restart();

        }
    }
    public void Pause() {
        audio.Pause();
    }

    #region Equalizer



    public void SetPanning(float Panning) {
        if (audio != null) audio.Panning = Panning;
        this.Panning = Panning;
    }

    public void SetPitch(float pitchFactor) {
        this.pitchFactor = pitchFactor;
        audio?.ChangePitch(pitchFactor);
    }

    public void SetVolume(float volume) {
        this.volume = volume;
        if (audio != null) audio.Volume = volume;
    }

    public void SetSpeakerNumber(int id) {
        audio.SetSpeakerNumber(id);
        SpeakerDeviceNumber = id;
    }
    public void UpdateEqualizer() {
        audio?.UpdateEqualizer();
    }
    public void SetEqualizer(Frequency frequency, float Gain) {
        int index = ModifiedAudio.GetIndexByFrequency(frequency);
        this.EqualizerProperty.equalizerBands[index].Gain = Gain;

    }
    #endregion
    private void OnDestroy() {
        audio?.Stop();
        audio?.Dispose();
    }

}
