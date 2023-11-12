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
            if (filter != null) { CreateFilter(); }
        }
    }
    private BiQuadFilter[] filter;
    private ISampleProvider sampleProvider;
    private bool updated = false;

    public WaveFormat WaveFormat => sampleProvider.WaveFormat;

    public EqualizedAudio(ISampleProvider sampleProvider = null) {
        equalizer = new();

        if (sampleProvider != null) {
            this.sampleProvider = sampleProvider;
        }

       

        filter = new BiQuadFilter[equalizer.Length];
        CreateFilter();


    }


    private void CreateFilter() {
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

        }
    }


    public void Update() {
        updated = true;
        CreateFilter();
    }


    public void ChangeGain(Frequency frequency, float Gain) {
        PrivateChangeGain(frequency, Gain);
        Update();

    }
    public void ChangeGains(Frequency[] frequencies, float[] Gains) {
        for (int i = 0; i < frequencies.Length; i++) {
            PrivateChangeGain(frequencies[i], Gains[i]);
        }
        Update();
    }
    private void PrivateChangeGain(Frequency frequency, float Gain) {
        int index = GetIndexByFrequency(frequency);
        if (equalizer.equalizerBands[index].Gain == Gain) return;
        equalizer.equalizerBands[index].Gain = Gain;
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

    public static Frequency GetFrequencyByIndex(int index) {
        return (Frequency)System.Enum.GetValues(typeof(Frequency)).GetValue(index);
    }





    public int Read(float[] buffer, int offset, int count) {
        int sampleRead = sampleProvider.Read(buffer, offset, count);

        if (updated) {
            CreateFilter();
            updated = false;
        }
        if (sampleRead == 0) {
            int i = 0;

        }
        for (int i = 0; i < sampleRead; i++) {
            for (int band = 0; band < equalizer.Length; band++) {
                buffer[offset + i] = filter[band].Transform(buffer[offset + i]);
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

