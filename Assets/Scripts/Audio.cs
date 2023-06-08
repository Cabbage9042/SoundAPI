using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

public class Audio {
    private WaveFileReader wave;

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

    public PlaybackState State {get { return speaker.PlaybackState; }}

    private bool isPlaying = false;

    private Speaker speaker;
    /// <summary>
    /// It is called when the audio is stopped (Including manually stopped and played completely).
    /// <para>
    /// <param name="stoppedAudio">The audio that was stopped.</param>
    /// <param name="hasFinishedPlaying">True if the audio reach the end.</param>
    /// </para>
    /// </summary>
    public event Action<Audio, bool> OnAudioStopped;

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
        //add onAudioStopped if start to play add the beginning
        if (speaker.PlaybackState == PlaybackState.Stopped)
            speaker.PlaybackStopped += Speaker_PlaybackStopped;

        speaker.Play();
        isPlaying = true;
        if (checkStopped) Task.Run(() => CheckAudioFinished());
    }


    /// <summary>
    /// Restart the playing audio. If the audio is stopped, do nothing
    /// </summary>
    public void Restart() {
        if (speaker.PlaybackState != PlaybackState.Stopped) {
            //just set the offset to the beginning
            wave.Position = 0;
            if (speaker.PlaybackState == PlaybackState.Paused) {
                speaker.Play();
            }
        }
    }

    /// <summary>
    /// Pause the playing audio. If the audio is stopped, do nothing
    /// </summary>
    public void Pause() {
        if (speaker.PlaybackState == PlaybackState.Playing)
            speaker.Pause();
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
