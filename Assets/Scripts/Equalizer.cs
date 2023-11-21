using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Equalizer {

    public static float MIN_GAIN { get { return EqualizerBand.MIN_GAIN; } }

    public static float MAX_GAIN { get { return EqualizerBand.MAX_GAIN; } }

    public static int DEFAULY_EQUALIZER_BANDS_COUNT = 10;

    [SerializeField]
    public EqualizerBand[] equalizerBands;
    public int Length { get { return equalizerBands.Length; } }

    public Equalizer(EqualizerBand[] equalizerBands) {
        this.equalizerBands = equalizerBands;
    }
    public Equalizer(int numberOfBands) {
        equalizerBands = new EqualizerBand[numberOfBands];
    }
    public Equalizer() {
        equalizerBands = new EqualizerBand[DEFAULY_EQUALIZER_BANDS_COUNT];
        for (int i = 0; i < equalizerBands.Length; i++) {
            equalizerBands[i] = new EqualizerBand((int)GetFrequencyByIndex(i), 0.8f, 0.0f);

        }
    }






    public static int GetIndexByFrequency(Frequency frequency) {
        Frequency[] values = (Frequency[])Enum.GetValues(typeof(Frequency));
        int x = Array.IndexOf(values, frequency);
        return x;
    }



    public static Frequency GetFrequencyByIndex(int index) {
        return (Frequency)System.Enum.GetValues(typeof(Frequency)).GetValue(index);
    }


}

