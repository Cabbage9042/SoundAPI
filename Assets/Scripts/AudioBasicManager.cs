using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBasicManager : MonoBehaviour {

    public AudioBasic audioBasic;

    // Start is called before the first frame update
    void Start() {
        audioBasic = GetComponent<AudioBasic>();
    }
}
