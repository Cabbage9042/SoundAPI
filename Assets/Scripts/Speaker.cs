using System;
using System.Collections;
using System.Collections.Generic;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using UnityEngine;

public class Speaker : WaveOutEvent {

    public static WaveOutCapabilities[] GetSpeakerDevices() {

        WaveOutCapabilities[] speakerDevices = new WaveOutCapabilities[WaveOut.DeviceCount];
        for (int i = 0; i < speakerDevices.Length; i++) {
            speakerDevices[i] = WaveOut.GetCapabilities(i);
        }
        return speakerDevices;

    }

    public static string[] GetSpeakerDevicesName() {
        WaveOutCapabilities[] speakerDevices = GetSpeakerDevices();

        string[] speakerDevicesName = new string[speakerDevices.Length];
        for (int i = 0; i < speakerDevicesName.Length; i++) {
            speakerDevicesName[i] = speakerDevices[i].ProductName;
        }
        return speakerDevicesName;
    }

    public Speaker() : base() { }

    public void Init(WaveFileReader audio) {
        Init(new WaveChannel32(audio));
    }

}
