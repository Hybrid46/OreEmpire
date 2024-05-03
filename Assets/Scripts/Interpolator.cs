using System;
using UnityEngine;

[Serializable]
public struct Interpolator
{
    [Serializable]
    public struct InterpolationValues
    {
        public float time;
        public Color color;

        public InterpolationValues(float time, Color color)
        {
            this.time = time;
            this.color = color;
        }
    }

    public float time;
    public float targetTime;
    public float interpolationSpeed;
    public InterpolationValues interpolateFrom;
    public InterpolationValues interpolateTo;
    public InterpolationValues interpolationCurrent;
    public int interpolationTimeMode; //0 single, 1 highlight, 2 pingpong
    public int interpolationTimeScaling; //0 linear, 1 exponential, 2 smoothstep
    public bool interpolateColor;
    public bool isActive;
    public bool applyDeltaTime;

    private const float TIME_ERROR = 0.001f;

    public Interpolator(InterpolationValues interpolateFrom, InterpolationValues interpolateTo, int interpolationTimeMode = 0, int interpolationTimeScaling = 0, float interpolationSpeed = 0.025f, bool interpolateColor = false, bool applyDeltaTime = true)
    {
        time = 0.0f;
        targetTime = 1.0f;
        this.interpolationSpeed = interpolationSpeed;
        this.interpolateFrom = interpolateFrom;
        this.interpolateTo = interpolateTo;
        interpolationCurrent = new InterpolationValues(0f, Color.white);
        this.interpolationTimeMode = interpolationTimeMode;
        this.interpolationTimeScaling = interpolationTimeScaling;
        this.interpolateColor = interpolateColor;
        isActive = true;
        this.applyDeltaTime = applyDeltaTime;
    }

    public void Update()
    {
        if (!isActive) return;
        UpdateTime();
        UpdateInterpolation();
    }

    private void UpdateTime()
    {
        if (Mathf.Abs(time - targetTime) < TIME_ERROR) time = targetTime;

        switch (interpolationTimeMode)
        {
            case 0: //single
                time = Mathf.Lerp(time, targetTime, ConvertInterpolationSpeed());
                break;
            case 1: //highlight
                if (time == 1.0f && targetTime == 1.0f) targetTime = 0.0f;

                time = Mathf.Lerp(time, targetTime, ConvertInterpolationSpeed());
                break;
            case 2: //pingpong
                if (time == 0.0f && targetTime == 0.0f) targetTime = 1.0f;
                if (time == 1.0f && targetTime == 1.0f) targetTime = 0.0f;

                time = Mathf.Lerp(time, targetTime, ConvertInterpolationSpeed());
                break;
        }
    }

    private void UpdateInterpolation()
    {
        interpolationCurrent.time = Mathf.Lerp(interpolateFrom.time, interpolateTo.time, time);

        if (interpolateColor)
        {
            interpolationCurrent.color = Color.Lerp(interpolateFrom.color, interpolateTo.color, time);
        }
    }

    private float ConvertInterpolationSpeed()
    {
        switch (interpolationTimeScaling)
        {
            case 0: //linear
                float distance = Mathf.Abs(time - targetTime);
                float linearSpeed = interpolationSpeed * (1f / distance);
                return linearSpeed;
            case 1: //exponential
                return interpolationSpeed;
            case 2: //smoothstep
                float t = interpolationSpeed;
                t = t * t * (3f - 2f * t) * 10f;
                return t;
        }

        return applyDeltaTime ? interpolationSpeed * Time.deltaTime : interpolationSpeed;
    }
}