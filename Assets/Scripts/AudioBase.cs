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

    public bool PlayOnStart;

    public float PitchFactor = 1.0f;

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


    public string FilePath { get { return audio.FilePath; } }
    public string Name { get { return audio.Name; } }
    public string NameWoExtension { get { return audio.NameWoExtension; } }
    public TimeSpan TotalTime { get { return audio.TotalTime; } }
    public long Position { get { return audio.Position; } }
    public long Length { get { return audio.Length; } }


    public int SampleRate { get { return audio.WaveFormat.SampleRate; } }

    public double[] GetAmplitude(float offset) {


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

    public double[] GetAmplitude() {
        return GetAmplitude(0.0f);
    }

    public double[] GetAmplitude(double[] amplitudes, int[] targetFrequencies, int sampleRate) {
        return SpectrumAnalyzer.GetAmplitude(amplitudes, targetFrequencies, sampleRate);
    }

    public double[] GetAmplitude(int[] targetAmplitudes) {
        return audio?.GetAmplitude(targetAmplitudes);
    }


    protected void Play(Action<MonoBehaviour, Audio, bool> defaultStop) {
        if (audio?.State == PlaybackState.Playing) {
            return;
        }

        audio.ClearAllEvent();


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
        catch (Exception) {
            Debug.LogError("Parameter mismatch. Please change your parameter variable, or change your method");
            return;
        }

        //loop
        audio.OnAudioStopped += defaultStop;

        audio.SetSpeakerNumber(SpeakerDeviceNumber);
        audio.ChangePitch(PitchFactor);
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
        if (audio?.State == PlaybackState.Stopped) {
            Play(defaultStop);
            //same as play == false in audio.restart
            audio?.OnAudioRestarted?.Invoke(childClass, audio, true);
        }
        //this is the true restart(audio is playing)
        else {

            audio?.Restart();

        }
    }
    public void Pause() {
        audio?.Pause();
    }


    public void SetPanning(float panning) {
        if (audio != null) audio.Panning = panning;
        this.Panning = panning;
    }
    public float GetPanning() { return Panning; }

    public void SetPitch(float pitchFactor) {
        this.PitchFactor = pitchFactor;
        audio?.ChangePitch(pitchFactor);
    }
    public float GetPitch() { return PitchFactor; }

    public void SetVolume(float volume) {
        this.volume = volume;
        if (audio != null) audio.Volume = volume;
    }

    public float GetVolume() {
        return volume;
    }

    public void SetSpeakerNumber(int id) {
        audio.SetSpeakerNumber(id);
        SpeakerDeviceNumber = id;
    }

    public int GetSpeakerNumber() {

        return SpeakerDeviceNumber;
    }
    #region Equalizer


    public void UpdateEqualizer() {
        audio?.UpdateEqualizer();
    }
    protected void SetGain(Frequency frequency, float gain) {
        int index = Equalizer.GetIndexByFrequency(frequency);
        this.EqualizerProperty.equalizerBands[index].Gain = gain;

    }
    protected float GetGain(Frequency frequency) {
        int index = Equalizer.GetIndexByFrequency(frequency);
        return this.EqualizerProperty.equalizerBands[index].Gain;
    }

    protected void SetEqualizer(Equalizer equalizer) {
        EqualizerProperty = equalizer;
        UpdateEqualizer();
    }

    public Equalizer GetEqualizer() {
        return EqualizerProperty;
    }

    #endregion
    public void OnDestroy() {
        audio?.Stop();
        audio?.Dispose();
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
}
