using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buffer<T>
{
    //buffer
    //
    T buffer;
    float bufferTime;
    float t;
    public delegate void OnWriteToBuffer();
    public event OnWriteToBuffer onWriteToBuffer;
    public Buffer(T initial, float bufferTime)
    {
        buffer = initial;
        this.bufferTime = bufferTime;
        t = bufferTime;
    }

    public void UpdateBuffer(float timeDelta, T runningValue)
    {
        t += timeDelta;
        if(t > bufferTime)
        {
            buffer = runningValue;
            onWriteToBuffer?.Invoke();
            t = 0;
        }
    }
    public T GetBuffer()
    {
        return buffer;
    }
}