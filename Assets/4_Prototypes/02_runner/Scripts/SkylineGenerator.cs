using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using runner;

public class SkylineGenerator : MonoBehaviour {
    const float boarder = 10f;

    [SerializeField] private SkylineObject[] prefabs;
    [SerializeField] private float distance;
    [SerializeField] private FloatRange altitude;
    [SerializeField] private FloatRange gapLength, sequenceLength;
    [SerializeField] private SkylineObject gapPrefab;

    [Min(0f)]
    [SerializeField] private float maxYDifference;

    [SerializeField] private bool singleSequenceStart;

    private Vector3 endPosition;
    private SkylineObject leftmost, rightmost;

    private float sequenceEndX;


    public SkylineObject StartNewGame(TrackingCamera view) {
        // 回收所有skyline object
        while (leftmost != null) {
            leftmost = leftmost.Recycle();
        }

        // 获取视野范围
        FloatRange visibleX = view.VisibleX(distance).GrowExtents(boarder);
        // 默认位置从0开始
        endPosition = new Vector3(visibleX.min, altitude.RandomValue, distance);
        sequenceEndX = singleSequenceStart ? visibleX.max : endPosition.x + sequenceLength.RandomValue;

        // 第一个实例
        leftmost = rightmost = GetInstance();
        // 放置
        endPosition = rightmost.PlaceAfter(endPosition);
        // 填充
        FillView(view);
        return leftmost;
    }

    private void StartNewSequence(float gap, float sequence) {

        if (gapPrefab != null) {
            rightmost = rightmost.Next = gapPrefab.GetInstance();
            rightmost.transform.SetParent(transform, false);
            rightmost.FillGap(endPosition, gap);
        }

        endPosition.x += gap;
        sequenceEndX = endPosition.x + sequence;

        if (maxYDifference > 0f) {
            endPosition.y = new FloatRange(
                Mathf.Max(endPosition.y - maxYDifference, altitude.min),
                Mathf.Min(endPosition.y + maxYDifference, altitude.max)
            ).RandomValue;
        } else {
            endPosition.y = altitude.RandomValue;
        }
    }

    public void FillView(TrackingCamera view, float extraGapLength = 0f, float extraSequenceLength = 0f) {
        FloatRange visibleX = view.VisibleX(distance).GrowExtents(boarder);
        // 回收
        while (leftmost != rightmost && leftmost.MaxX < visibleX.min) {
            leftmost = leftmost.Recycle();
        }

        // 放置
        while (endPosition.x < visibleX.max) {

            if (endPosition.x > sequenceEndX) {
                StartNewSequence(
                    gapLength.RandomValue + extraGapLength,
                    sequenceLength.RandomValue + extraSequenceLength
                );
            }

            rightmost = rightmost.Next = GetInstance();
            endPosition = rightmost.PlaceAfter(endPosition);
        }
    }

    SkylineObject GetInstance() {
        SkylineObject instance = prefabs[Random.Range(0, prefabs.Length)].GetInstance();
        instance.transform.SetParent(transform, false);
        return instance;
    }
}
