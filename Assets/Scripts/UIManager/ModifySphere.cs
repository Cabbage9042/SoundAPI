using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifySphere : MonoBehaviour {

    public Material material;
    public GraphManager graphManager;
    public int targetFrequency;
    private int[] targetFrequencies;

    private enum AmplitudeIndex {
        Index500
    }

    private void Start() {
        targetFrequencies = new int[1];
        targetFrequencies[0] = targetFrequency;
    }

    // Update is called once per frame
    void Update() {
        if (graphManager?.amplitudes == null || graphManager?.amplitudes.Length == 0) {
            material.SetFloat("_Amplitude", 0.0f);
            return;
        }

        if(targetFrequency != targetFrequencies[0]) {
            targetFrequencies[0] = targetFrequency;
        }

        var amplitudes = SpectrumAnalyzer.GetAmplitude(graphManager.amplitudes, targetFrequencies, graphManager.sampleRate, graphManager.ChannelCount);

        material.SetFloat("_Amplitude", (float)amplitudes[(int)AmplitudeIndex.Index500]);


    }
}
