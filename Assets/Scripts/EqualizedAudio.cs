using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EqualizedAudio : WaveStream {

    public static int NUMBER_OF_BANDS = 10;


    public EqualizerBand[] equalizerBands ;
    private BiQuadFilter[] filter;
    private WaveStream wave;
    public override WaveFormat WaveFormat { get { return wave.WaveFormat; } }


    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public EqualizedAudio(ISampleProvider sampleProvider = null) {
        /*equalizerBands = new EqualizerBand[NUMBER_OF_BANDS];

        if (sampleProvider != null) {
            this.sampleProvider = sampleProvider;
        }

        //foreach(FrequencyBand bandFrequency in System.Enum.GetValues(typeof(FrequencyBand))) {
        for (int i = 0; i < equalizerBands.Length; i++) {
            equalizerBands[i] = new EqualizerBand(GetFrequencyByIndex(i), 0.8f,  0.0f);

        }

        filter = new BiQuadFilter[equalizerBands.Length];
        CreateFilter();
        */

    }

    public EqualizedAudio(EqualizerBand[] equalizerBands, ISampleProvider sampleProvider = null) {
/*
        if (sampleProvider != null) {
            this.sampleProvider = sampleProvider;
        }
        this.equalizerBands = equalizerBands;

        filter = new BiQuadFilter[equalizerBands.Length];
        CreateFilter();*/
    }

    private void CreateFilter() {
        for (int i = 0; i < equalizerBands.Length; i++) {
            if (filter[i] == null) {
                filter[i] = BiQuadFilter.PeakingEQ(wave.WaveFormat.SampleRate,
                    equalizerBands[i].CenterFrequency, equalizerBands[i].QFactor,
                    equalizerBands[i].Gain);
            }
            else {
                filter[i].SetPeakingEq(wave.WaveFormat.SampleRate,
                    equalizerBands[i].CenterFrequency, equalizerBands[i].QFactor,
                    equalizerBands[i].Gain);
            }

        }
    }


    public void ChangeGain(Frequency frequency, float Gain) {
        int index = GetIndexByFrequency(frequency);
        equalizerBands[index].Gain = Gain;

    }


    public static int GetIndexByFrequency(Frequency frequency) {
        Frequency[] values = (Frequency[])Enum.GetValues(typeof(Frequency));
        int x =Array.IndexOf(values, frequency);
        return x;
    }

    public ISampleProvider ApplyEqualization(ISampleProvider input) {
        var output = new SampleToWaveProvider16(input);

        foreach (var band in equalizerBands) {
            var filter = BiQuadFilter.PeakingEQ(
                input.WaveFormat.SampleRate,
                band.CenterFrequency,
                band.QFactor,
                band.Gain);


        }

        return output.ToSampleProvider();

    }

    public static int GetFrequencyByIndex(int index) {
        return (int)System.Enum.GetValues(typeof(Frequency)).GetValue(index);
    }


 

    
    public override int Read(byte[] buffer, int offset, int count) {
        int sampleRead = wave.Read(buffer, offset, count);
        //float[] floatBuffer = BitConverter.ToSingle(buffer, 0);

        /*
        for (int i = 0; i < sampleRead; i++) {
            for (int band = 0; band < equalizerBands.Length; band++) {
                buffer[offset + i] = filter[band].Transform(buffer[offset + i]);
            }
        }*/
        return sampleRead;
    }
}
public class EqualizerBand {
    public int CenterFrequency { get; set; }
    public float QFactor { get; set; }
    public float Gain { get; set; }

    public static float MAX_GAIN = 12;
    public static float MIN_GAIN = 12;

    public EqualizerBand(int centerFrequency, float qFactor, float gain) {
        CenterFrequency = centerFrequency;
        QFactor = qFactor;
        Gain = gain;
    }
}
public enum Frequency {
    F31 = 31,
    F63 = 63,
    F125 = 125,
    F250 = 250,
    F500 = 500,
    F1k = 1000,
    F2k = 2000,
    F4k = 4000,
    F8k = 8000,
    F16k = 16000
}

