using NAudio.Wave;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class Audio {
    private WaveFileReader wave;
    private Speaker speaker;

    /// <summary>
    /// The file path of the audio
    /// </summary>
    public string FilePath { get; }
    /// <summary>
    /// The file name of the audio with extension
    /// </summary>
    public string Name {
        get {
            string[] list = FilePath.Split("/");
            return list[list.Length - 1];
        }
    }


    public string NameWoExtension { get { return Name.Substring(0, Name.Length - 4); } }
    /// <summary>
    /// The total length of audio in time
    /// </summary>
    public TimeSpan TotalTime { get { return wave.TotalTime; } }
    /// <summary>
    /// The current position of the audio being played in bytes
    /// </summary>
    public long Position { get; set; }
    /// <summary>
    /// The total length of the audio being played in bytes
    /// </summary>
    public long Length { get; }

    public PlaybackState State { get { return speaker.PlaybackState; } }

    private bool isPlaying = false;


    public float Volume { get { return speaker.Volume; } set { speaker.Volume = value; } }






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

    public PlaybackState currentState {
        get {
            return speaker.PlaybackState;
        }
    }


    public Audio(string filePath) {
        speaker = new Speaker();
        wave = GetFileInWAV(filePath);
        speaker.Init(wave);
        FilePath = filePath;

    }

    /// <summary>
    /// Play the audio. If the audio is playing, do nothing
    /// </summary>
    /// <param name="checkStopped">Check is the audio stopped or not</param>
    public void Play(bool checkStopped = true) {
        //add onAudioStopped if start to play at the beginning
        if (speaker.PlaybackState == PlaybackState.Stopped) {
            speaker.PlaybackStopped += Speaker_PlaybackStopped;
        }
        //resume
        else if (speaker.PlaybackState == PlaybackState.Paused) {
            OnAudioResumed?.Invoke(this);
        }

        speaker.Play();

        OnAudioStarted?.Invoke(this);
        isPlaying = true;
        if (checkStopped) Task.Run(() => CheckAudioFinished());

    }




    /// <summary>
    /// Restart the playing audio. If the audio is stopped, play from beginning
    /// </summary>
    public void Restart() {
        //just set the offset to the beginning
        wave.Position = 0;

        //if audio is not playing, 
        bool sameAsPlay = State == PlaybackState.Stopped;

        speaker.Play();
        isPlaying = true;

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
        while (wave.Position < wave.Length) {
            System.Threading.Thread.Sleep(100);
        }

        //stop if finish, and reset the offset
        isPlaying = false;
        Stop();
        wave.Position = 0;
    }

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


    private void Speaker_PlaybackStopped(object sender, StoppedEventArgs e) {
        OnAudioStopped?.Invoke(this, !isPlaying);
        //remove the onAudioStopped
        speaker.PlaybackStopped -= Speaker_PlaybackStopped;

    }

    /// <summary>
    /// Stop playing the audio
    /// </summary>
    public void Stop() {
        speaker?.Stop();
        wave.Position = 0;
    }

    /// <summary>
    /// Dispose (Close) the audio 
    /// </summary>
    public void Dispose() {
        speaker?.Dispose();
        wave?.Dispose();
    }

    public static WaveFileReader GetFileInWAV(string filePath) {
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



}
