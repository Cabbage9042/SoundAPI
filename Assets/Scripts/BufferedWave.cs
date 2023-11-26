using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferedWave : IWaveProvider {

    public BufferedWaveProvider bufferedWaveProvider;
    public double[] amplitude;
    public bool CalculateAmplitude { get; set; } = true;

    public BufferedWave(WaveFormat waveFormat) {
        bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
    }

    public WaveFormat WaveFormat => bufferedWaveProvider.WaveFormat;


    public int Read(byte[] buffer, int offset, int count) {
        int sampleRead = bufferedWaveProvider.Read(buffer, offset, count);

        if (CalculateAmplitude) {
            amplitude = SpectrumAnalyzer.GetAmplitude(buffer);
        }



        return sampleRead;
    }


}
