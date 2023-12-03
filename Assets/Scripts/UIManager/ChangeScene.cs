using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {

    public static string BasicSceneName = "BasicUIScene";
    public static string ListSceneName = "ListUIScene";
    public static string MicrophoneSceneName = "MicrophoneUIScene";


    public static ChangeScene Instance;
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }




    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            if (SceneManager.GetActiveScene().name != BasicSceneName)
                SceneManager.LoadScene(BasicSceneName);
        }
        else if (Input.GetKeyDown(KeyCode.L)) {
            if (SceneManager.GetActiveScene().name != ListSceneName)
                SceneManager.LoadScene(ListSceneName);

        }
        else if (Input.GetKeyDown(KeyCode.M)) {

            if (SceneManager.GetActiveScene().name != MicrophoneSceneName)
                SceneManager.LoadScene(MicrophoneSceneName);
        }
    }



}
