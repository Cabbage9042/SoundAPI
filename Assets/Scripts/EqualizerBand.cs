using System;
using UnityEngine;

[System.Serializable]
public class EqualizerBand {
    public int CenterFrequency;
    public float QFactor;
    public float Gain;

    public static float MAX_GAIN = 12;
    public static float MIN_GAIN = -12;

    public static int DEFAULY_EQUALIZER_BANDS_COUNT = 10;

    public EqualizerBand(int centerFrequency, float qFactor, float gain) {
        if (gain > MAX_GAIN) {
            throw new Exception("Gain cannot be more than 12!");
        }
        else if (gain < MIN_GAIN) {
            throw new Exception("Gain cannot be less than -12!");
        }
        CenterFrequency = centerFrequency;
        QFactor = qFactor;
        Gain = gain;
    }

    public static EqualizerBand[] DefaultEqualizerBands() {
        var bands = new EqualizerBand[DEFAULY_EQUALIZER_BANDS_COUNT];


        for (int i = 0; i < bands.Length; i++) {
            bands[i] = new EqualizerBand((int)EqualizedAudio.GetFrequencyByIndex(i), 0.8f, 0.0f);

        }

        return bands;
    }
}