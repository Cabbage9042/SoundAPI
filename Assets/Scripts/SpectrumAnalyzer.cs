
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectrumAnalyzer {

    static int BYTES_PER_POINT = 2;


    public static double[] GetAmplitude(byte[] buffer, int channelCount) {
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



        if (channelCount == 2) {

            int beforeIndex;
            for (int i = 0; i < fftReal.Length; i += 2) {
                beforeIndex = i / 2 + 1;
                fftReal[i] = values[beforeIndex].Magnitude;
                fftReal[i + 1] = (values[beforeIndex].Magnitude + values[beforeIndex + 1].Magnitude) / 2;
            }
        }
        else {
            for (int i = fftReal.Length / 2 - 1; i >= 0; i--) {
                fftReal[i] = values[i].Magnitude;
                fftReal[i + 1] = values[i].Magnitude;
            }
        }


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
    public static double[] GetAmplitude(byte[] buffer, int[] targetFrequencies, int sampleRate, int channelCount) {

        var fft = GetAmplitude(buffer, channelCount);
        double[] returnedFFT = new double[targetFrequencies.Length];
        for (int i = 0; i < returnedFFT.Length; i++) {
            //+0.5 is to make the float round up or down depends on the demical point
            int index = GetIndex(fft.Length, targetFrequencies[i], sampleRate, channelCount);
            returnedFFT[i] = fft[index];
        }
        return returnedFFT;


    }

    public static double[] GetAmplitude(double[] amplitudes, int[] targetFrequencies, int sampleRate, int channelCount) {



        double[] returnedFFT = new double[targetFrequencies.Length];
        for (int i = 0; i < returnedFFT.Length; i++) {
            //+0.5 is to make the float round up or down depends on the demical point
            int index = GetIndex(amplitudes.Length, targetFrequencies[i],sampleRate, channelCount);
            returnedFFT[i] = amplitudes[index];
        }
        return returnedFFT;
    }

    public static int GetIndex(int amplitudeArrayLength, int targetFrequency, int sampleRate, int channelCount) {
        int index = ((int)((targetFrequency * (amplitudeArrayLength) / (double)sampleRate) + 0.5)) * 2 * channelCount;
        return index;
    }
}
