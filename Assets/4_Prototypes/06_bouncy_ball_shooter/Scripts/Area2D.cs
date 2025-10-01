using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

public struct Area2D {

    public float2 extents;

    public Vector2 RandomVector2 =>
        new(Random.Range(-extents.x, extents.x), Random.Range(-extents.y, extents.y));

    public float2 Wrap(float2 position) => position + extents * (
        select(select(0f, 2f, position < -extents), -2f, position > extents)
    );

    public static Area2D FromView(Camera camera) {
        Area2D area;
        area.extents.y = 
            -Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 
            camera.transform.position.z;
        area.extents.x = camera.aspect * area.extents.y;
        return area;
    }

}