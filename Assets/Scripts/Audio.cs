using NAudio.Wave;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NAudio.Wave.SampleProviders;
using UnityEditor;

#if UNITY_EDITOR

using UnityEngine;
#endif

public class Audio {


    private WaveStream Wave;
    private Speaker speaker;
    public float PitchFactor = 1.0f;
    public float volume = 1.0f;
    private bool AudioHasFinished = true;

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
    public TimeSpan TotalTime { get { return Wave.TotalTime; } }
    /// <summary>
    /// The current position of the audio being played in bytes
    /// </summary>
    public long Position { get; set; }
    /// <summary>
    /// The total length of the audio being played in bytes
    /// </summary>
    public long Length { get; }

    public PlaybackState State { get { return speaker.PlaybackState; } }


    public float Volume { get { return speaker.Volume; } set { speaker.Volume = value; } }

    private bool errorOccuredAndStopped = false;


    #region Device
    public static WaveOutCapabilities[] speakerDevices { get { return Speaker.GetSpeakerDevices(); } }
    public static string[] speakerDevicesName { get { return Speaker.GetSpeakerDevicesName(); } }

    public bool SetSpeakerNumber(int id) {
        if (id < 0 || id >= WaveOut.DeviceCount) {
            return false;
        }
        speaker.DeviceNumber = id;
        speaker.Init(Wave);
        return true;
    }

    #endregion

    #region Event


    /// <summary>
    /// It is called after the audio has started.<br></br>
    /// Note that OnAudioStarted will also be called when OnAudioRestarted is called and sameAsPlay == true (Audio is stopped before)
    /// <para>
    /// <param name="currentAudio"><b>Audio</b>: The audio that was played.</param>
    /// <param name="sameAsRestart"><b>bool</b>: The  </param>
    /// </para>
    /// <para>
    /// void Your_Method_Name(Audio currentAudio){<br></br>
    ///       //Your code here...<br></br>
    /// } 
    /// </para>
    /// </summary>
    public Action<Audio> OnAudioStarted;

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
    public Action<Audio> OnAudioPaused;

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
    public Action<Audio> OnAudioResumed;

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
    public Action<Audio, bool> OnAudioRestarted;

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
    public Action<Audio, bool> OnAudioStopped;

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


    public Audio(string filePath) {
        speaker = new Speaker();
        Wave = GetFileInWAV(filePath);
        speaker.Init(Wave);
        FilePath = filePath;

        //new NAudio.Wave.Wave32To16Stream();
        //var h = new WaveFileReader(new SmbPitchShiftingSampleProvider(wave.ToSampleProvider()));
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
        }
        //resume
        else if (speaker.PlaybackState == PlaybackState.Paused) {
            OnAudioResumed?.Invoke(this);
        }

        //get pitch factor and change
        if (PitchFactor != 1.0f) {
            var pitch = new SmbPitchShiftingSampleProvider(Wave.ToSampleProvider());
            pitch.PitchFactor = PitchFactor;
            speaker.Init(pitch);
        }

        //change volume
        try {
            speaker.Volume = volume;
        }
        catch (ArgumentOutOfRangeException e) {
            errorOccuredAndStopped = true;
            speaker.Stop();

            throw e;
        }

        speaker.Play();
        AudioHasFinished = false;

        OnAudioStarted?.Invoke(this);
        if (checkStopped) Task.Run(() => CheckAudioFinished());

    }




    /// <summary>
    /// Restart the playing audio. If the audio is stopped, play from beginning
    /// </summary>
    public void Restart() {
        //just set the offset to the beginning
        Wave.Position = 0;

        //if audio is not playing, 
        bool sameAsPlay = State == PlaybackState.Stopped;

        speaker.Play();
        AudioHasFinished = false;

        //same as play == true in basic.play
        OnAudioRestarted?.Invoke(this, sameAsPlay);
    }

    /// <summary>
    /// Pause the playing audio. If the audio is stopped, do nothing
    /// </summary>
    public void Pause() {
        if (speaker.PlaybackState == PlaybackState.Playing) {
            speaker.Pause();
            OnAudioPaused?.Invoke(this);
        }
    }

    /// <summary>
    /// Check is the audio finished playing or not.
    /// </summary>
    private void CheckAudioFinished() {
        //if havent yet, sleep for a while
        while (Wave.Position < Wave.Length) {
            System.Threading.Thread.Sleep(100);
        }

        AudioHasFinished = true;

        //stop if finish, and reset the offset
        Stop();

        Wave.Position = 0;
    }

    private void Speaker_PlaybackStopped(object sender, StoppedEventArgs e) {
        if (errorOccuredAndStopped == false) {
            OnAudioStopped?.Invoke(this, AudioHasFinished);

        }
        else {
            errorOccuredAndStopped = false;
        }
        //remove the onAudioStopped
        speaker.PlaybackStopped -= Speaker_PlaybackStopped;

    }

    /// <summary>
    /// Stop playing the audio
    /// </summary>
    public void Stop() {
        speaker?.Stop();
        Wave.Position = 0;
    }

    /// <summary>
    /// Dispose (Close) the audio 
    /// </summary>
    public void Dispose() {
        speaker?.Dispose();
        Wave?.Dispose();
    }


    #endregion

    #region ClearEvent

    public void ClearOnAudioStopped() {
        if (OnAudioStopped == null) return;
        Delegate[] allMethod = OnAudioStopped.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioStopped -= (Action<Audio, bool>)allMethod[i];
        }

    }
    public void ClearOnAudioStarted() {
        if (OnAudioStarted == null) return;
        Delegate[] allMethod = OnAudioStarted.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioStarted -= (Action<Audio>)allMethod[i];
        }

    }

    public void ClearOnAudioPaused() {
        if (OnAudioPaused == null) return;
        Delegate[] allMethod = OnAudioPaused.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioPaused -= (Action<Audio>)allMethod[i];
        }

    }

    public void ClearOnAudioResumed() {
        if (OnAudioResumed == null) return;
        Delegate[] allMethod = OnAudioResumed.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioResumed -= (Action<Audio>)allMethod[i];
        }

    }

    public void ClearOnAudioRestarted() {
        if (OnAudioRestarted == null) return;
        Delegate[] allMethod = OnAudioRestarted.GetInvocationList();
        for (int i = allMethod.Length - 1; i >= 0; i--) {

            OnAudioRestarted -= (Action<Audio, bool>)allMethod[i];
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


#if UNITY_EDITOR
    public static Audio AudioClipToAudio(AudioClip audioClip) {
        string[] assetPathArray = AssetDatabase.GetAssetPath(audioClip.GetInstanceID()).Split("/");
        string path = Application.dataPath + "/";
        for (int i = 1; i < assetPathArray.Length; i++) {
            path += (assetPathArray[i] + "/");
        }

        path = path.Remove(path.Length - 1);
        return new Audio(path);

    }

#endif



}
