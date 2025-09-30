using UnityEngine;

[System.Serializable]
public class TileSwapper {


    [Range(0.1f, 10f)]
    [SerializeField]
    private float duration = 0.25f;

    [Range(0f, 1f)]
    [SerializeField]
    private float maxDepthOffset = 0.5f;

    private Tile tileA, tileB;

    private Vector3 positionA, positionB;
    private float process = -1f;
    private bool pingPong;

    public float Swap(Tile a, Tile b, bool pingPong) {
        tileA = a;
        tileB = b;
        positionA = a.transform.localPosition;
        positionB = b.transform.localPosition;
        this.pingPong = pingPong;
        process = 0f;
        return pingPong ? 2f * duration : duration;
    }

    public void Update() {
        if (process < 0f) {
            return;
        }

        process += Time.deltaTime;
        if (process >= duration) {
            if (pingPong) {
                process -= duration;
                pingPong = false;
                (tileA, tileB) = (tileB, tileA);
            } else {
                process = -1f;
                tileA.transform.localPosition = positionB;
                tileB.transform.localPosition = positionA;
                return;
            }
        }

        float t = process / duration;
        float z = Mathf.Sin(Mathf.PI * t) * maxDepthOffset;
        Vector3 p = Vector3.Lerp(positionA, positionB, t);
        p.z = -z;
        tileA.transform.localPosition = p;
        p = Vector3.Lerp(positionA, positionB, 1f - t);
        p.z = z;
        tileB.transform.localPosition = p;

    }

}