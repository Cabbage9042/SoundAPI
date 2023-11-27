using NAudio.Wave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneObject {

    private WaveInEvent waveIn;
    private WaveFileWriter waveWriter;
    private BufferedWaveProvider bufferedWaveProvider;
    //private ModifiedAudio modifiedAudio;
    public bool IsPlaying { get; private set; }

    private bool SaveIntoFile { get; set; }

    private double[] amplitude = null;

    private List<byte> buffer = new();
    public WaveFormat WaveFormat => waveIn.WaveFormat;
    public int GetSpeakerNumber => waveIn.DeviceNumber;

   /* public Equalizer equalizer {
        get { return modifiedAudio.equalizer; }
        set {
            modifiedAudio.equalizer = value;
            UpdateEqualizer();
        }
    }

    public float Panning {
        get { return modifiedAudio != null ? modifiedAudio.Panning : 0.0f; }
        set { if (modifiedAudio != null) modifiedAudio.Panning = value; }
    }*/

    public static WaveInCapabilities[] GetMicrophoneDevices() {

        WaveInCapabilities[] microphoneDevices = new WaveInCapabilities[WaveIn.DeviceCount];
        for (int i = 0; i < microphoneDevices.Length; i++) {
            microphoneDevices[i] = WaveIn.GetCapabilities(i);
        }
        return microphoneDevices;

    }

    public static string[] GetMicrophoneDevicesName() {
        WaveInCapabilities[] microphoneDevices = GetMicrophoneDevices();

        string[] microphoneDevicesName = new string[microphoneDevices.Length];
        for (int i = 0; i < microphoneDevicesName.Length; i++) {
            microphoneDevicesName[i] = microphoneDevices[i].ProductName;
        }
        return microphoneDevicesName;
    }

    public void Initialize() {
        waveIn = new();
    }

    private void StartCaptureCore(bool saveIntoFile, string outputPath, WaveFormat waveFormat) {

        if (IsPlaying) return;

        waveIn.WaveFormat = waveFormat;

        waveIn.DataAvailable += WaveIn_DataAvailable;


        SaveIntoFile = saveIntoFile;
        if (saveIntoFile) {
            waveWriter = new WaveFileWriter(outputPath, waveIn.WaveFormat);
        }
        bufferedWaveProvider = new(waveIn.WaveFormat);
        bufferedWaveProvider.DiscardOnBufferOverflow = true;

        //modifiedAudio = new(bufferedWaveProvider.ToSampleProvider());


        waveIn.StartRecording();
        IsPlaying = true;
        buffer = new();


    }
    public void StartCapture(WaveFormat waveFormat) {
        StartCaptureCore(false, "", waveFormat);
    }

    public void StartCapture(string outputPath, WaveFormat waveFormat) {
        StartCaptureCore(true, outputPath, waveFormat);
    }


    public void StopCapture() {
        if (!IsPlaying) return;

        waveIn.StopRecording();
        waveIn.Dispose();


        if (SaveIntoFile) {
            waveWriter.Close();
            waveWriter.Dispose();
        }
        IsPlaying = false;

        amplitude = null;

    }




  //  public void UpdateEqualizer() => modifiedAudio?.UpdateEqualizer();

    private void WaveIn_DataAvailable(object sender, WaveInEventArgs e) {


        if (SaveIntoFile) {
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }

        bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);

        buffer = new(e.Buffer);
    }

    public double[] GetAmplitude() {
        if (!IsPlaying)
            return null;

        /*
        int frameSize = 2048;
        byte[] buffer = new byte[frameSize];
        //bufferedWaveProvider.Read(buffer, 0, frameSize);


        if (buffer.Length == 0)
            return null;
        if (buffer[frameSize - 2] == 0)
            return null;

        */
        if (buffer.Count < 2048) return null;

        amplitude = SpectrumAnalyzer.GetAmplitude(buffer.GetRange(0,2048).ToArray());
        return amplitude;
    }

    public bool SetMicrophoneNumber(int id) {
        if (IsPlaying) return false;

        if (id == waveIn.DeviceNumber) {
            return true;
        }
        if (id < 0 || id >= WaveIn.DeviceCount) {
            return false;
        }

        waveIn.DeviceNumber = id;
        return true;
    }




}
