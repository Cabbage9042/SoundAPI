using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferedWave : IWaveProvider {

    private readonly IWaveProvider sourceWaveProvider;
    public double[] amplitude;
    public bool CalculateAmplitude { get; set; } = false;

    public BufferedWave(IWaveProvider wave) {
        this.sourceWaveProvider = wave;
    }

    public WaveFormat WaveFormat => sourceWaveProvider.WaveFormat;


    public int Read(byte[] buffer, int offset, int count) {
        int sampleRead = sourceWaveProvider.Read(buffer, offset, count);

        if (CalculateAmplitude) {
            amplitude = SpectrumAnalyzer.GetAmplitude(buffer);
        }



        return sampleRead;
    }


}
