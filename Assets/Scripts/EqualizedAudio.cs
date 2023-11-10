using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EqualizedAudio : ISampleProvider {

    public static int NUMBER_OF_BANDS = 10;


    public Equalizer equalizer;
    private BiQuadFilter[] filter;
    private ISampleProvider sampleProvider;
    private bool updated = false;

    public WaveFormat WaveFormat => sampleProvider.WaveFormat;

    public EqualizedAudio(ISampleProvider sampleProvider = null) {
       // equalizer = new Equalizer(NUMBER_OF_BANDS);
        
        if (sampleProvider != null) {
            this.sampleProvider = sampleProvider;
        }/*

        //foreach(FrequencyBand bandFrequency in System.Enum.GetValues(typeof(FrequencyBand))) {
        for (int i = 0; i < equalizer.Length; i++) {
            equalizer[i] = new EqualizerBand(GetFrequencyByIndex(i), 0.8f, 0.0f);

        }

        filter = new BiQuadFilter[equalizer.Length];
        CreateFilter();
        */

    }


    private void CreateFilter() {
        for (int i = 0; i < equalizer.Length; i++) {
            if (filter[i] == null) {
                filter[i] = BiQuadFilter.PeakingEQ(sampleProvider.WaveFormat.SampleRate,
                    equalizer[i].CenterFrequency, equalizer[i].QFactor,
                    equalizer[i].Gain);
            }
            else {
                filter[i].SetPeakingEq(sampleProvider.WaveFormat.SampleRate,
                    equalizer[i].CenterFrequency, equalizer[i].QFactor,
                    equalizer[i].Gain);
            }

        }
    }
    public void Update() {
        updated = true;
        CreateFilter();
    }


    public void ChangeGain(Frequency frequency, float Gain) {
        int index = GetIndexByFrequency(frequency);
        if (equalizer[index].Gain == Gain) return;
        equalizer[index].Gain = Gain;
        Update();

    }


    public static int GetIndexByFrequency(Frequency frequency) {
        Frequency[] values = (Frequency[])Enum.GetValues(typeof(Frequency));
        int x = Array.IndexOf(values, frequency);
        return x;
    }

    public ISampleProvider ApplyEqualization(ISampleProvider input) {
        var output = new SampleToWaveProvider16(input);

        foreach (var band in equalizer) {
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





    public int Read(float[] buffer, int offset, int count) {
        int sampleRead = sampleProvider.Read(buffer, offset, count);
        /*
        if (updated) {
            CreateFilter();
            updated = false;
        }
        
        for (int i = 0; i < sampleRead; i++) {
            for (int band = 0; band < equalizer.Length; band++) {
                buffer[offset + i] = filter[band].Transform(buffer[offset + i]);
            }
        }**/
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

