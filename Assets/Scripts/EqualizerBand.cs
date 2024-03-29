using System;
using UnityEngine;

[System.Serializable]
public class EqualizerBand {
    public int CenterFrequency;
    public float QFactor;
    public float gain;
    public float Gain {
        get { return gain; }
        set {
            if (value > MAX_GAIN) {
                throw new ArgumentOutOfRangeException("Gain cannot be more than 12!");
            }
            else if (value < MIN_GAIN) {
                throw new ArgumentOutOfRangeException("Gain cannot be less than -12!");
            }
            gain = value;
        }
    }

    public static float MAX_GAIN = 12;
    public static float MIN_GAIN = -12;

    public static int DEFAULT_EQUALIZER_BANDS_COUNT = 10;

    public EqualizerBand(int centerFrequency, float qFactor, float gain) {

        CenterFrequency = centerFrequency;
        QFactor = qFactor;
        Gain = gain;
    }

    public static EqualizerBand[] DefaultEqualizerBands() {
        var bands = new EqualizerBand[DEFAULT_EQUALIZER_BANDS_COUNT];


        for (int i = 0; i < bands.Length; i++) {
            bands[i] = new EqualizerBand((int)Equalizer.GetFrequencyByIndex(i), 0.8f, 8.0f);

        }

        return bands;
    }
}