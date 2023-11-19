using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphManager : MonoBehaviour {

    public GameObject graph;
    public GameObject dot;
    public GameObject[] dotList;
    public AudioBasicUIManager audioManager;
    public static int DOT_COUNT = 512;
    public static float SPECTRUM_VISUALIZER_SCALE = 25;
    private float dotOriginY;

    private int currentAudioRate = 0;
    private List<GameObject> labels;
    public GameObject labelPrefab;
    public int labelCount;


    // Start is called before the first frame update
    void Start() {
        //bar
        dotList = new GameObject[DOT_COUNT];

        float left = graph.GetComponent<RectTransform>().position.x;
        dotOriginY = graph.GetComponent<RectTransform>().position.y;
        float offset = graph.GetComponent<RectTransform>().rect.width / DOT_COUNT;




        for (int i = 0; i < DOT_COUNT; i++) {
            left += offset;
            var newDotPosition = new Vector3(left, dotOriginY);
            dotList[i] = Instantiate(dot, newDotPosition, Quaternion.identity, graph.transform);
            dotList[i].name = "Dot " + i.ToString();
        }



    }

    // Update is called once per frame
    void Update() {

        //print(graph.GetComponent<RectTransform>().rect.width);
        //print(graph.transform.localPosition.x + " " + graph.transform.localPosition.y +' '+graph.transform.localPosition.z);
        var amplitude = audioManager.getAmplitude();
        if (amplitude == null) return;
        

        //double max=0;
        for (int i = 0; i < amplitude.Length; i++) {
            // dotList[i].transform.localPosition = new Vector3(dotList[i].transform.localPosition.x, (float)amplitude[i] * 50);
            dotList[i].transform.localScale = new Vector3(1, (float)amplitude[i] * SPECTRUM_VISUALIZER_SCALE, 1);
            float newY = ((float)(amplitude[i] * (SPECTRUM_VISUALIZER_SCALE / 2)) * dot.GetComponent<RectTransform>().rect.height);
            dotList[i].transform.localPosition = new Vector3(dotList[i].transform.localPosition.x, newY, dotList[i].transform.localPosition.z);
            //if(max < amplitude[i])                 max = amplitude[i];
            //lineRenderer.GetComponent<LineRenderer>().SetPosition(i, dotList[i].transform.position);
        }

        ChangeLabelHz(audioManager.audioBasic.SampleRate);
        // print(max);


    }
    
    public void ResetGraph() {
            for (int i = 0; i > dotList.Length; i++) {
                dotList[i].transform.localScale = new Vector3(1, 1, 1);
                dotList[i].transform.localPosition = new Vector3(dotList[i].transform.localPosition.x, 0, dotList[i].transform.localPosition.z);
            }
        
    }

    private void ChangeLabelHz(int currentAudioRate) {
        currentAudioRate /= 2;
        if (currentAudioRate == this.currentAudioRate) return;
        this.currentAudioRate = currentAudioRate;

        int labelCount = currentAudioRate / 1000;
        if (labels == null) {
            labels = new List<GameObject>();

            for (int i = 0; i < labelCount; i++) {


                labels.Add(Instantiate(labelPrefab, graph.transform));
                labels[i].transform.localPosition = new Vector3(0, -30);


            }
        }
        else if (labels.Count < labelCount) {
            int oriCount = labels.Count;
            for (int i = 0; i < labelCount - oriCount; i++) {

                labels.Add(Instantiate(labelPrefab, graph.transform));
                labels[i].transform.localPosition = new Vector3(0, -30);
            }

        }
        else if (labels.Count > labelCount) {
            for (int i = labels.Count - 1; i >= labelCount; i--) {
                Destroy(labels[i]);
                labels.RemoveAt(i);
            }

        }

        LabelHz();

    }

    void LabelHz() {



        int fftMaxHz = audioManager.audioBasic.SampleRate / 2;

        float spaceBetween2Label = graph.GetComponent<RectTransform>().rect.width / labels.Count;
        float left = 0;
        for (int i = 0; i < labels.Count; i++) {
            labels[i].transform.localPosition = new Vector3(left, labels[i].transform.localPosition.y, labels[i].transform.localPosition.z);
            labels[i].GetComponent<TextMeshProUGUI>().text = i.ToString();
            left += spaceBetween2Label;
        }
    }
}
