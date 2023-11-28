using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifySphere : MonoBehaviour {

    public Material material;
    public GraphManager graphManager;
    public int[] targetFrequency = { 1000};
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (graphManager?.amplitudes == null || graphManager?.amplitudes.Length == 0) return;
        float amplitude = (float)SpectrumAnalyzer.GetAmplitude(graphManager.amplitudes, targetFrequency, graphManager.sampleRate)[0];
        material.SetFloat("_Amplitude", amplitude);

    }
}
