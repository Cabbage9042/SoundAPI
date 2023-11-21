using NAudio.Wave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneObject {
    private WaveInEvent waveIn;
    private WaveFileWriter waveWriter;
    private BufferedWaveProvider bufferedWaveProvider;
    private ModifiedAudio modifiedAudio;
    private Speaker speaker;
    public bool IsPlaying { get; private set; }

    private bool SaveIntoFile { get; set; }
    public bool CalculateAmplitude { get; set; } = false;

    private double[] amplitude = null;

    private void StartCaptureCore(bool saveIntoFile, string outputPath) {

        if (IsPlaying) return;

        speaker = new();
        waveIn = new WaveInEvent();

        waveIn.DataAvailable += WaveIn_DataAvailable;


        SaveIntoFile = saveIntoFile;
        if (saveIntoFile) {
            waveWriter = new WaveFileWriter(outputPath, waveIn.WaveFormat);
        }
        bufferedWaveProvider = new(waveIn.WaveFormat);
        modifiedAudio = new(bufferedWaveProvider.ToSampleProvider());

        speaker = new();
        speaker.Init(modifiedAudio);

        speaker.Play();

        waveIn.StartRecording();
        IsPlaying = true;
    }
    public void StartCapture() {
        StartCaptureCore(false, "");
    }

    public void StartCapture(string outputPath) {
        StartCaptureCore(true, outputPath);
    }


    private void WaveIn_DataAvailable(object sender, WaveInEventArgs e) {


        if (SaveIntoFile) {
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }

        bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);

        if (CalculateAmplitude) {
            amplitude = SpectrumAnalyzer.GetAmplitude(e.Buffer);
        }
    }

    public double[] GetAmplitude() {
        return amplitude;
    }


    public void StopCapture() {
        if (!IsPlaying) return;

        waveIn.StopRecording();
        waveIn.Dispose();

        speaker.Stop();

        if (SaveIntoFile) {
            waveWriter.Close();
            waveWriter.Dispose();
        }
        IsPlaying = false;

        amplitude = null;

    }


}
