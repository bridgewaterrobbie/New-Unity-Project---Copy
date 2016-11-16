using UnityEngine;
using System.Collections;
using UnityHue;
using System;

public class BodyLightManager : MonoBehaviour {

    public GestureSourceManager GSM;
    public HueBridge HB;
    public bool lightsOn = true;

    // Use this for initialization
    void Start () {
        GSM.OnGesture += changeLights;
	}

    private void changeLights(GestureSourceManager.EventArgs e)
    {


        string gestureName = e.name;
        float confidence = e.confidence;


        if(confidence>0)
            Debug.Log("Detected Gesture " + gestureName + " with Confidence " + confidence);
        if (confidence>.1)
        {
            if(gestureName.Equals("HandsOpen") && lightsOn == false)
            {
                foreach (HueLamp HL in HB.Lights)
                    {
                    HueLampState newState = HL.lampState;
                    newState.on = true;
                    HL.SetState(newState);
                }
                lightsOn = true;
            }

            else if (gestureName.Equals("HandDownArc") && lightsOn == true)
            {
                foreach (HueLamp HL in HB.Lights)
                {
                    HueLampState newState = HL.lampState;
                    newState.on = false;
                    HL.SetState(newState);
                }
                lightsOn = false;
            }
        }

        throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update () {
	
	}
}
