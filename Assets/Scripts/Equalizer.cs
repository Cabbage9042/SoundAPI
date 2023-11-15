using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Equalizer : IEnumerable<EqualizerBand> {

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
            equalizerBands[i] = new EqualizerBand((int)EqualizedAudio.GetFrequencyByIndex(i), 0.8f, 0.0f);

        }
    }






    public IEnumerator<EqualizerBand> GetEnumerator() {
        for (int i = 0; i < Length; ++i) {
            yield return equalizerBands[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

