using UnityEngine;
using System.Collections.Generic;
using System;

public class LimitedCircularQueue<T> : Queue<T> {
    public int maxCapacity;

    public LimitedCircularQueue(int capacity) : base()
    {
        maxCapacity = capacity;

    }

    public new void Enqueue(T t)
    {
        if(Count==maxCapacity)
        {
            Dequeue();
            
        }
        base.Enqueue(t);
    }
	
}


public class circularGestureComponent : IComparable<circularGestureComponent>
{
    public Vector3 wristPosition;
    public Vector3 handPosition;
    public float confidence;
    public circularGestureComponent(Vector3 wristPos, Vector3 handPos, float c)
    {
        confidence = c;
        handPosition = handPos;
        wristPosition = wristPos;
    }
    public string ToString()
    {
        return "Wrist Position: " + wristPosition + " Hand Position: " + handPosition + " Confidence: " + confidence;
    }

    public int CompareTo(circularGestureComponent obj)
    {
        return confidence.CompareTo(obj.confidence);
    }
}