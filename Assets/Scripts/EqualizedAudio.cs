using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

public class EqualizedAudio : ISampleProvider {

    public static int NUMBER_OF_BANDS = 10;

    private Equalizer privateEqualizer;

    /// <summary>
    /// Stores the information about the boost or lost. Remember to call Update() after updating.
    /// </summary>
    public Equalizer equalizer {
        get { return privateEqualizer; }
        set {
            privateEqualizer = value;
            if (filter != null) { Update(); }
        }
    }
    private BiQuadFilter[] filter;
    private ISampleProvider sampleProvider;
    private bool updated = true;
    private bool allZero = true;
    public WaveFormat WaveFormat => sampleProvider.WaveFormat;

    public EqualizedAudio(ISampleProvider sampleProvider = null) {
        equalizer = new();

        if (sampleProvider != null) {
            this.sampleProvider = sampleProvider;
        }



        filter = new BiQuadFilter[equalizer.Length];
        CreateFilter();


    }


    public void CreateFilter() {
        bool thisTimeGotNotZero = false;
        for (int i = 0; i < equalizer.Length; i++) {
            if (filter[i] == null) {
                filter[i] = BiQuadFilter.PeakingEQ(sampleProvider.WaveFormat.SampleRate,
                    equalizer.equalizerBands[i].CenterFrequency, equalizer.equalizerBands[i].QFactor,
                    equalizer.equalizerBands[i].Gain);
            }
            else {
                filter[i].SetPeakingEq(sampleProvider.WaveFormat.SampleRate,
                    equalizer.equalizerBands[i].CenterFrequency, equalizer.equalizerBands[i].QFactor,
                    equalizer.equalizerBands[i].Gain);
            }
            if (equalizer.equalizerBands[i].Gain != 0.0f && thisTimeGotNotZero == false) {
                thisTimeGotNotZero = true;
            }


        }

        if (thisTimeGotNotZero) {
            allZero = false;

        }
        else {
            allZero = true;
        }
    }


    public void Update() {
        updated = true;
        CreateFilter();
    }


    public void ChangeGain(Frequency frequency, float Gain) {
        int index = GetIndexByFrequency(frequency);
        if (equalizer.equalizerBands[index].Gain == Gain) return;
        equalizer.equalizerBands[index].Gain = Gain;
        Update();

    }




    public static int GetIndexByFrequency(Frequency frequency) {
        Frequency[] values = (Frequency[])Enum.GetValues(typeof(Frequency));
        int x = Array.IndexOf(values, frequency);
        return x;
    }



    public static Frequency GetFrequencyByIndex(int index) {
        return (Frequency)System.Enum.GetValues(typeof(Frequency)).GetValue(index);
    }


    public int Read(float[] buffer, int offset, int count) {
        int sampleRead = sampleProvider.Read(buffer, offset, count);

        if (updated) {
            CreateFilter();
            updated = false;
        }

        if (allZero == false) {
            for (int i = 0; i < sampleRead; i++) {
                for (int band = 0; band < equalizer.Length; band++) {
                    buffer[offset + i] = filter[band].Transform(buffer[offset + i]);
                }
            }
        }
        return sampleRead;
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

