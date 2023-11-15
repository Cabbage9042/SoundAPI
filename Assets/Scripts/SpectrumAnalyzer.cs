
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectrumAnalyzer {

    static int BYTES_PER_POINT = 2;
    

    public static double[] GetAmplitude(byte[] buffer) {
        int frameSize = buffer.Length;
        int graphPointCount = frameSize / BYTES_PER_POINT;
        double[] fftReal = new double[graphPointCount / 2];


        Accord.Compat.Complex[] values = new Accord.Compat.Complex[graphPointCount];

        for (int i = 0; i < graphPointCount; i++) {

            int val = BitConverter.ToInt16(buffer, i * 2);

            double temp = ((float)(val) / Math.Pow(2, 16) * 200.0);

            values[i] = new Accord.Compat.Complex(temp, 0);
        }

        Accord.Math.FourierTransform.FFT(values, Accord.Math.FourierTransform.Direction.Forward);


        for (int i = 0; i < fftReal.Length; i++) {
            fftReal[i] = values[i].Magnitude;
        }

        /*
        int n = values.Length / 2;
        for (int i = 0; i < targetFrequencies.Length; i++) {
            int targerFrequency = targetFrequencies[i];
            int indexInValues = (int)(targerFrequency * n / sampleRate);

        }
        float max=-1;
        float maxIndex = 0;
        for(int i = 0; i < values.Length; i++) {
            if (values[i].X > max) { max = values[i].X; maxIndex = i; }
        }
        */

        return fftReal;
    }
 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="targetFrequencies"></param>
    /// <param name="sampleRate"></param>
    /// <param name="frameSize"></param>
    /// <returns></returns>
    public static double[] GetAmplitude(byte[] buffer, int[] targetFrequencies, int sampleRate) {

        var fft = GetAmplitude(buffer);
        double[] returnedFFT = new double[targetFrequencies.Length];
        for (int i = 0; i < returnedFFT.Length; i++) {
            //+0.5 is to make the float round up or down depends on the demical point
            int index = (int)((targetFrequencies[i] * (buffer.Length / 4) / (double)sampleRate) + 0.5);
            returnedFFT[i] = fft[index];
        }
        return returnedFFT;


    }
}
