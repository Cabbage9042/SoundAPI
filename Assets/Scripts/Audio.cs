using NAudio.Wave;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NAudio.Wave.SampleProviders;
using UnityEditor;
using NAudio.Dsp;

#if UNITY_EDITOR

using UnityEngine;
#endif

public class Audio {


    private WaveStream OriginalWave;
    private EqualizedAudio EqualizedWave;
    private MonoBehaviour audioBase;


    public WaveFormat WaveFormat => OriginalWave.WaveFormat;
    private Speaker speaker;
    public float PitchFactor = 1.0f;
    public Equalizer equalizer {
        get { return EqualizedWave.equalizer; }
        set {
            EqualizedWave.equalizer = value;
            EqualizedWave.Update();
        }
    }
    private bool AudioHasFinished = true;
    public int Channels { get { return OriginalWave.WaveFormat.Channels; } }
    /// <summary>
    /// The file path of the audio
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// The file name of the audio with extension
    /// </summary>
    public string Name {
        get {
            string[] list = FilePath.Split("/");
            return list[list.Length - 1];
        }
        private set { Name = value; }
    }

    public string NameWoExtension {
        get { return Name.Substring(0, Name.Length - 4); }
        private set { NameWoExtension = value; }
    }
    /// <summary>
    /// The total length of audio in time
    /// </summary>
    public TimeSpan TotalTime { get { return OriginalWave.TotalTime; } }
    /// <summary>
    /// The current position of the audio being played in bytes
    /// </summary>
    public long Position { get { return OriginalWave.Position; } set { OriginalWave.Position = value; } }
    /// <summary>
    /// The total length of the audio being played in bytes
    /// </summary>
    public long Length { get; }

    public PlaybackState State { get { return speaker.PlaybackState; } }


    public float Volume { get { return speaker.Volume; } set { speaker.Volume = value; } }

    private bool IgnoreAudioOnStopped = false;



    #region Device
    public static WaveOutCapabilities[] speakerDevices { get { return Speaker.GetSpeakerDevices(); } }
    public static string[] speakerDevicesName { get { return Speaker.GetSpeakerDevicesName(); } }


    public bool SetSpeakerNumber(int id) {
        if (id == speaker.DeviceNumber) {
            return true;
        }
        if (id < 0 || id >= WaveOut.DeviceCount) {
            return false;
        }
        speaker.DeviceNumber = id;

        if (State == PlaybackState.Playing) {
            float Volume = speaker.Volume;

            long currentPosition = OriginalWave.Position;
            IgnoreAudioOnStopped = true;
            Stop();
            speaker.Init(EqualizedWave);
            OriginalWave.Position = currentPosition;
            speaker.Volume = Volume;
            speaker.Play();
        }
        return true;
    }

    #endregion

    #region Event


    /// <summary>
    /// It is called after the audio has started.<br></br>
    /// Note that OnAudioStarted will also be called when OnAudioRestarted is called and sameAsPlay == true (Audio is stopped before)
    /// <para>
    /// <param name="currentAudio"><b>Audio</b>: The audio that was played.</param>
    /// </para>
    /// <para>
    /// void Your_Method_Name(Audio currentAudio){<br></br>
    ///       //Your code here...<br></br>
    /// } 
    /// </para>
    /// </summary>
    public Action<MonoBehaviour, Audio> OnAudioStarted;

    /// <summary>
    /// It is called after the audio has been paused.
    /// <para>
    /// <param name="currentAudio"><b>Audio</b>: The audio that is been paused.</param>
    /// </para>
    /// <para>
    /// void Your_Method_Name(Audio currentAudio){<br></br>
    ///       //Your code here...<br></br>
    /// } 
    /// </para>
    /// </summary>
    public Action<MonoBehaviour, Audio> OnAudioPaused;

    /// <summary>
    /// It is called after the audio has been resumed after pausing.
    /// <para>
    /// <param name="currentAudio"><b>Audio</b>: The audio that is been resumed.</param>
    /// </para>
    /// <para>
    /// void Your_Method_Name(Audio currentAudio){<br></br>
    ///       //Your code here...<br></br>
    /// } 
    /// </para>
    /// </summary>
    public Action<MonoBehaviour, Audio> OnAudioResumed;

    /// <summary>
    /// It is called after the audio has been resumed after pausing.<br></br>
    /// Note that OnAudioStarted will also be called when OnAudioRestarted is called and sameAsPlay == true (Audio is stopped before)
    /// <para>
    /// <param name="currentAudio"><b>Audio</b>: The audio that is been replayed.</param><br></br>
    /// <param name="sameAsPlay">
    ///     <b>bool</b>: True if the audio is stopped before.<br></br>
    ///     False if the audio is playing or pausing before.
    /// </param>
    /// </para>
    /// <para>
    /// void Your_Method_Name(Audio currentAudio, bool sameAsPlay){<br></br>
    ///       //Your code here...<br></br>
    /// } 
    /// </para>
    /// </summary>
    public Action<MonoBehaviour, Audio, bool> OnAudioRestarted;

    /// <summary>
    /// It is called when the audio is stopped (Including manually stopped and played completely).<br></br>
    /// If the audio is looping, the event is also called before continue looping.
    /// <para>
    /// <param name="stoppedAudio"><b>Audio</b>: The audio that was stopped.</param><br></br>
    /// <param name="hasFinishedPlaying"><b>bool</b>: True if the audio reach the end (not stopped by interrupt).</param>
    /// </para>
    /// <para>
    /// void Your_Method_Name(Audio stoppedAudio, bool hasFinishedPlaying){<br></br>
    ///       //Your code here...<br></br>
    /// } 
    /// </para>
    /// </summary>
    public Action<MonoBehaviour, Audio, bool> OnAudioStopped;

    #endregion

    /*What to do when adding new event:
    clearEvent()
    clearAllEvent()
    Invoke()
    
     Basic Play() foreach
    add new MethodCall[] 
    
     Editor
    Enum
    InitializeHeaderName
    OnEnable serializedObject.FindProperty();*/


    public Audio(string filePath, MonoBehaviour audioBase) {
        speaker = new Speaker();
        OriginalWave = GetFileInWAV(filePath);
        //EqualizedWave = OriginalWave.ToSampleProvider();
        EqualizedWave = new(OriginalWave.ToSampleProvider());
        FilePath = filePath;
        this.audioBase = audioBase;
    }



    #region Action

    /// <summary>
    /// Play the audio. If the audio is playing, do nothing
    /// </summary>
    /// <param name="checkStopped">Check is the audio stopped or not</param>
    public void Play(bool checkStopped = true, bool sameAsRestart = false) {



        //add onAudioStopped if start to play at the beginning
        if (speaker.PlaybackState == PlaybackState.Stopped) {
            speaker.PlaybackStopped += Speaker_PlaybackStopped;
            OnAudioStarted?.Invoke(audioBase, this);
        }
        //resume
        else if (speaker.PlaybackState == PlaybackState.Paused) {
            OnAudioResumed?.Invoke(audioBase, this);
        }

        //equalizer


        if (speaker.PlaybackState != PlaybackState.Paused) {
            //get pitch factor and change
            if (PitchFactor != 1.0f) {
                var pitch = new SmbPitchShiftingSampleProvider(EqualizedWave);
                pitch.PitchFactor = PitchFactor;
                speaker.Init(pitch);
            }
            else {
                speaker.Init(EqualizedWave);
            }
        }

        //change volume
        /*try {
            speaker.Volume = volume;
        }
        catch (ArgumentOutOfRangeException e) {
            errorOccuredAndStopped = true;
            speaker.Stop();

            throw e;
        }*/
        speaker.Play();
        AudioHasFinished = false;

        if (checkStopped) Task.Run(() => CheckAudioFinished());


    }


    /// <summary>
    /// Restart the playing audio. If the audio is stopped, play from beginning
    /// </summary>
    public void Restart() {
        //just set the offset to the beginning
        OriginalWave.Position = 0;

        //if audio is not playing, 
        bool sameAsPlay = State == PlaybackState.Stopped;

        speaker.Play();
        AudioHasFinished = false;

        //same as play == true in basic.play
        OnAudioRestarted?.Invoke(audioBase, this, sameAsPlay);
    }

    /// <summary>
    /// Pause the playing audio. If the audio is stopped, do nothing
    /// </summary>
    public void Pause() {
        if (speaker.PlaybackState == PlaybackState.Playing) {
            speaker.Pause();
            OnAudioPaused?.Invoke(audioBase, this);
        }
    }

    /// <summary>
    /// Check is the audio finished playing or not.
    /// </summary>
    private void CheckAudioFinished() {
        //if havent yet, sleep for a while
        while (OriginalWave.Position < OriginalWave.Length) {
            System.Threading.Thread.Sleep(100);
        }

        AudioHasFinished = true;

        //stop if finish, and reset the offset
        Stop();

        OriginalWave.Position = 0;
    }

    private void Speaker_PlaybackStopped(object sender, StoppedEventArgs e) {
        if (IgnoreAudioOnStopped == false) {
            OnAudioStopped?.Invoke(audioBase, this, AudioHasFinished);

            //remove the onAudioStopped
            speaker.PlaybackStopped -= Speaker_PlaybackStopped;
        }
        else {
            IgnoreAudioOnStopped = false;
        }

    }

    /// <summary>
    /// Stop playing the audio
    /// </summary>
    public void Stop(bool IgnoreAudioOnStopped = false) {
        if (IgnoreAudioOnStopped) { this.IgnoreAudioOnStopped = true; }
        speaker?.Stop();
        OriginalWave.Position = 0;
    }

    /// <summary>
    /// Dispose (Close) the audio 
    /// </summary>
    public void Dispose() {
        speaker?.Dispose();
        OriginalWave?.Dispose();
    }


    #endregion

    #region ClearEvent

    public void ClearOnAudioStopped() {
        if (OnAudioStopped == null) return;
        Delegate[] allMethod = OnAudioStopped.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioStopped -= (Action<MonoBehaviour, Audio, bool>)allMethod[i];
        }

    }
    public void ClearOnAudioStarted() {
        if (OnAudioStarted == null) return;
        Delegate[] allMethod = OnAudioStarted.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioStarted -= (Action<MonoBehaviour, Audio>)allMethod[i];
        }

    }

    public void ClearOnAudioPaused() {
        if (OnAudioPaused == null) return;
        Delegate[] allMethod = OnAudioPaused.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioPaused -= (Action<MonoBehaviour, Audio>)allMethod[i];
        }

    }

    public void ClearOnAudioResumed() {
        if (OnAudioResumed == null) return;
        Delegate[] allMethod = OnAudioResumed.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioResumed -= (Action<MonoBehaviour, Audio>)allMethod[i];
        }

    }

    public void ClearOnAudioRestarted() {
        if (OnAudioRestarted == null) return;
        Delegate[] allMethod = OnAudioRestarted.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioRestarted -= (Action<MonoBehaviour, Audio, bool>)allMethod[i];
        }

    }

    public void ClearAllEvent() {
        ClearOnAudioStarted();
        ClearOnAudioPaused();
        ClearOnAudioStopped();
        ClearOnAudioResumed();
        ClearOnAudioRestarted();

    }

    #endregion

    #region Pitch

    /// <summary>
    /// Change the pitch by the given factor.
    /// 0.5f means an octave down.
    /// 1.0f means no pitch change.
    /// 2.0f means an octiv
    /// </summary>
    /// <param name="pitchFactor"></param>
    public void ChangePitch(float pitchFactor) {
        if (pitchFactor == this.PitchFactor) return;

        this.PitchFactor = pitchFactor;
        bool isPlaying = speaker.PlaybackState == PlaybackState.Playing;

        long OriginalPosition = OriginalWave.Position;
        IgnoreAudioOnStopped = true;
        Stop();

        if (pitchFactor != 1.0f) {
            var pitch = new SmbPitchShiftingSampleProvider(EqualizedWave);
            pitch.PitchFactor = PitchFactor;
            speaker.Init(pitch);
        }
        else {
            speaker.Init(EqualizedWave);
        }
        OriginalWave.Position = OriginalPosition;
        speaker.Volume = Volume;
        if (isPlaying) {
            speaker.Play();
        }

    }

    #endregion

    public static WaveStream GetFileInWAV(string filePath) {
        if (filePath.EndsWith(".mp3")) {
            return MP3toWAV(filePath);
        }
        else if (filePath.EndsWith(".wav")) {

            return new WaveFileReader(filePath);
        }
        throw new Exception("File format not supported");
    }

    public static WaveFileReader MP3toWAV(string mp3FilePath) {

        string wavFilePath = mp3FilePath.Substring(0, mp3FilePath.Length - 4) + ".wav";

        WaveFileReader wavFile;

        try {
            wavFile = new WaveFileReader(wavFilePath);
        }
        catch (FileNotFoundException) {
            using (var reader = new Mp3FileReader(mp3FilePath)) {
                WaveFileWriter.CreateWaveFile(wavFilePath, reader);
            }
            return new WaveFileReader(wavFilePath);
        }
        return wavFile;


    }

    #region Amplitude


    public double[] GetAmplitude(int frameSize = 1024) {
        if (!IsPowerOfTwo(frameSize)) {
            throw new Exception("Frame Size should be power of two!");
        }
        if (speaker.PlaybackState == PlaybackState.Playing) {
            return GetFFT(null, frameSize);
        }
        return null;
    }

    public double[] GetAmplitude(int[] targetFrequencies, int frameSize = 1024) {
        if (!IsPowerOfTwo(frameSize)) {
            throw new Exception("Frame Size should be power of two!");
        }
        if (speaker.PlaybackState == PlaybackState.Playing) {
            return GetFFT(targetFrequencies, frameSize);
        }
        return null;

    }
    private double[] GetFFT(int[] targetFrequencies, int frameSize = 2048) {
        byte[] byteBuffer = new byte[frameSize];
        long oriPosition = OriginalWave.Position;

        OriginalWave.Read(byteBuffer, 0, frameSize);
        OriginalWave.Position = oriPosition;
        if (targetFrequencies != null) {
            return SpectrumAnalyzer.GetAmplitude(byteBuffer, targetFrequencies, OriginalWave.WaveFormat.SampleRate);
        }
        else {
            return SpectrumAnalyzer.GetAmplitude(byteBuffer);
        }
    }

    public static bool IsPowerOfTwo(int x) {
        return (x != 0) && ((x & (x - 1)) == 0);
    }

    #endregion

    #region Equalizer

    public void ChangeGain(Frequency frequency, float Gain) {
        EqualizedWave.ChangeGain(frequency, Gain);
    }

    public void UpdateEquilizer() {
        EqualizedWave.Update();
    }


    #endregion


#if UNITY_EDITOR
    public static Audio AudioClipToAudio(AudioClip audioClip, AudioBase audioBase) {
        string[] assetPathArray = AssetDatabase.GetAssetPath(audioClip.GetInstanceID()).Split("/");
        string path = Application.dataPath + "/";
        for (int i = 1; i < assetPathArray.Length; i++) {
            path += (assetPathArray[i] + "/");
        }

        path = path.Remove(path.Length - 1);
        return new Audio(path, audioBase);

    }


#endif



}
