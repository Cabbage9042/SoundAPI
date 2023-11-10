using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equalizer : IEnumerable<EqualizerBand>{
    public EqualizerBand[] equalizerBands;
    public int Length { get { return equalizerBands.Length; } }

    public Equalizer(EqualizerBand[] equalizerBands) {
        this.equalizerBands = equalizerBands;
    }
    public Equalizer(int numberOfBands) {
        equalizerBands = new EqualizerBand[numberOfBands];
    }

    public EqualizerBand this[int index] {
        get {
            return equalizerBands[index];
        }
        set {
            equalizerBands[index] = value;
        }
    }

  

    public IEnumerator<EqualizerBand> GetEnumerator() {
        for(int i = 0; i < Length; ++i) {
            yield return equalizerBands[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}