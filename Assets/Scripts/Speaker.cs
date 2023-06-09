using System;
using System.Collections;
using System.Collections.Generic;
using NAudio.Wave;
using UnityEngine;

public class Speaker : WaveOutEvent {

    

    public Speaker() : base() {

    }


    public void Init(WaveFileReader audio) {

        Init(new WaveChannel32(audio));
    }




  


}
