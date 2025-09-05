using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour {
    [SerializeField] private Transform secondPivot;
    [SerializeField] private Transform minutePivot;
    [SerializeField] private Transform hourPivot;


    float secondsDegree = 6f;
    float minuteDegree = 6f;
    float hourDegree = 30f;

    private void Update() {

        TimeSpan time = DateTime.Now.TimeOfDay;
        secondPivot.localRotation = Quaternion.Euler(0f, secondsDegree * (float)time.TotalSeconds,0f);
        minutePivot.localRotation = Quaternion.Euler(0f, minuteDegree * (float)time.TotalMinutes,0f);
        hourPivot.localRotation = Quaternion.Euler(0f, hourDegree * (float)time.TotalHours,0f);
    }

    
}
