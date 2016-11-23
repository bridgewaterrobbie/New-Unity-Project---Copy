using UnityEngine;
using System.Collections;
using UnityHue;
using System;
using System.Collections.Generic;
using Kinect = Windows.Kinect;


public class BodyLightManager : MonoBehaviour
{

    public GestureSourceManager GSM;
    public HueBridge HB;

    public GameObject BodySourceManager;
    public BodySourceManager _BodyManager;
    public bool lightsOn = true;
    public BodySourceView KinectDictSource;

    public LimitedCircularQueue<circularGestureComponent> pastFrames = new LimitedCircularQueue<circularGestureComponent>(100);
    public bool isCircling = false;
    public bool debugCircles=false;
    public bool debugCenters = true;

    // Use this for initialization
    void Start()
    {
        GSM.OnGesture += callGesture;
    }

    public void callGesture(GestureSourceManager.EventArgs e)
    {
        string gestureName = e.name;
        float value = e.confidence;



        if (gestureName.Equals("HandsOpen"))
        {
            handOpenGesture(value);
            return;
        }
        if (gestureName.Equals("HandDownArc"))
        {
            handCloseGesture(value);
            return;
        }

        if (gestureName.Equals("ArmCircles_Right"))
        {
            handleContinuousCircles(value);
        }
        if (gestureName.Equals("armCircleDiscrete_Right"))
            handleDiscreteCircle(value);

    }

    private void handleDiscreteCircle(float confidence)
    {

        // Debug.Log("HandDiscrete triggered, confidence: " + confidence);
        if (confidence > .05 && pastFrames.Count>50)
        {
            isCircling = true;
            
        }
        if (confidence == 0 && isCircling)
        {
            isCircling = false;
            Debug.Log("Done Circling!");

            circularGestureComponent[] previousFramesArr = pastFrames.ToArray();
            
            pastFrames.Clear();
            

            float[] diffArray = new float[previousFramesArr.Length - 3];
            
            //Get the slope at different points. We effectively chop off the end, it wont matter
            for (int i = 0; i < diffArray.Length; i++)
            {
                diffArray[i] = previousFramesArr[i].confidence - previousFramesArr[i + 3].confidence;
            }
            
            float max = float.MinValue;
            int maxIndex = -1;

            for (int i = 0; i < diffArray.Length; i++)
            {
                if (diffArray[i] > max)
                {
                    max = diffArray[i];
                    maxIndex = i;
                }
            }
            
            if (maxIndex == -1)
                Debug.Log("We didnt find a max value??");
            else
            {
                Debug.Log("Max value is: " + max + " at index: " + maxIndex);
            }
            for (int i = maxIndex; i < previousFramesArr.Length; i++)
            {
                //   Debug.Log("At index " + i + " we have confidence " + previousFramesArr[i].confidence);
            }

            Vector3[] validWristFrames = new Vector3[previousFramesArr.Length - maxIndex];
            Vector3[] validHandFrames = new Vector3[previousFramesArr.Length - maxIndex];
            int j = 0;
            for (int i = maxIndex; i < previousFramesArr.Length; i++)
            {
                validWristFrames[j] = previousFramesArr[i].wristPosition;
                validHandFrames[j] = previousFramesArr[i].handPosition;
                j++;
                if (debugCircles)
                { 
                    GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pointObj.transform.position = previousFramesArr[i].wristPosition;
                    pointObj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                    pointObj.name = "PointCloud" + i;
                }

            }

            Vector3 centerH = new Vector3(0, 0, 0);
            foreach (Vector3 v in validHandFrames)
            {
                centerH += v;

            }
            centerH = centerH / (validHandFrames.Length);
            
            Vector3 centerW = new Vector3(0, 0, 0);
            foreach (Vector3 v in validWristFrames)
            {
                centerW += v;
            }
            centerW = centerW / (validHandFrames.Length);


            if (debugCenters)
            {
                GameObject centerHobj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                centerHobj.transform.position = centerH;
                centerHobj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                centerHobj.name = "centerH";

                GameObject centerWobj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                centerWobj.transform.position = centerW;
                centerWobj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                centerWobj.name = "centerW";
            }


            float radius = 0;
            foreach (Vector3 v in validHandFrames)
            {
                radius += Vector3.Distance(centerH, v);
            }
            radius = 2*radius / (validHandFrames.Length);
            
            RaycastHit[] hits=Physics.SphereCastAll(centerW, radius, centerH - centerW);
            foreach (RaycastHit h in hits)
            {
                string tagName = h.transform.tag;
                Debug.Log("Hit on " + tagName);
                foreach (HueLamp HL in HB.Lights)
                {
                    if (HL.name.Equals(tagName))
                    {
                        Debug.Log("Trying to change lamp " + HL.name);
                        HueLampState newState = HL.lampState;
                        newState.on = !newState.on;
                        HL.SetState(newState);
                        Debug.Log("I think I changed lamp " + HL.name);
                    }
                   // else
                        //Debug.Log("Unhit lamp name is: " + HL.name);
                }
            }


        }
    }


    private void handleContinuousCircles(float progress)
    {
        
        Vector3 wristPos = KinectDictSource.bonePositions[Kinect.JointType.WristRight];
        
        Vector3 handPos = KinectDictSource.bonePositions[Kinect.JointType.HandRight];
        //Debug.Log("Hand pos: " + handPos + " Wrist pos: " + wristPos + " Progress: " + progress);

        pastFrames.Enqueue(new circularGestureComponent(wristPos, handPos, progress));
        
        //Debug.Log("PastFrameArr: " + pastFrames.ToArray());
        //Debug.Log("PastFrameArr: "+pastFrames.Peek().ToString());
    }

    private void handOpenGesture(float confidence)
    {
        if (confidence > .1 && lightsOn == false)
        {
            foreach (HueLamp HL in HB.Lights)
            {
                
                HueLampState newState = HL.lampState;
                newState.on = true;
                HL.SetState(newState);
            }
            lightsOn = true;
        }
    }

    private void handCloseGesture(float confidence)
    {
        if (confidence > .1 && lightsOn == true)
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



    // Update is called once per frame
    void Update()
    {

    }

    public Kinect.Body getBody()
    {
        Kinect.Body[] data = _BodyManager.GetData();


        foreach (var body in data)
        {

        }

        if (data == null)
        {
            return null;
        }
        if (data[0] != null && data[0].IsTracked)
            return data[0];

        foreach (var body in data)
        {
            if (body == null && body.IsTracked)
            {
                return body;
            }
        }
        return null;

    }
}
