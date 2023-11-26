using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

public class ModifiedAudio : ISampleProvider {

    public static int NUMBER_OF_BANDS = 10;

    private Equalizer privateEqualizer;


    private BiQuadFilter[] filter;
    private ISampleProvider sampleProvider;
    private bool updated = true;
    private bool equalizerIsNotSet = true;
    private float panning;

    public float Panning {
        get { return panning; }
        set { panning = Math.Max(-1.0f, Math.Min(1.0f, value)); }
    }

    public WaveFormat WaveFormat => sampleProvider.WaveFormat;

    /// <summary>
    /// Stores the information about the boost or lost. Remember to call Update() after updating.
    /// </summary>
    public Equalizer equalizer {
        get { return privateEqualizer; }
        set {
            privateEqualizer = value;
            if (filter != null) { UpdateEqualizer(); }
        }
    }

    public ModifiedAudio(ISampleProvider sampleProvider = null) {
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
            equalizerIsNotSet = false;

        }
        else {
            equalizerIsNotSet = true;
        }
    }


    public void UpdateEqualizer() {
        updated = true;
        CreateFilter();
    }


    public void ChangeGain(Frequency frequency, float Gain) {
        int index = Equalizer. GetIndexByFrequency(frequency);
        if (equalizer.equalizerBands[index].Gain == Gain) return;
        equalizer.equalizerBands[index].Gain = Gain;
        UpdateEqualizer();

    }


    public int Read(float[] buffer, int offset, int count) {
        int sampleRead = sampleProvider.Read(buffer, offset, count);

        if (updated) {
            CreateFilter();
            updated = false;
        }

        if (equalizerIsNotSet == false || Panning != 0.0f) {
            for (int i = 0; i < sampleRead; i += 2) {
                if (equalizerIsNotSet == false) {
                    for (int band = 0; band < equalizer.Length; band++) {
                        buffer[offset + i] = filter[band].Transform(buffer[offset + i]);
                        buffer[offset + i + 1] = filter[band].Transform(buffer[offset + i + 1]);
                    }
                }
                if (Panning != 0.0f) {
                    float normPan = (-panning + 1) / 2;
                    float leftVolume = normPan;
                    float rightVolume = 1 - normPan;

                    buffer[offset + i] *= leftVolume;
                    buffer[offset + i + 1] *= rightVolume;
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

