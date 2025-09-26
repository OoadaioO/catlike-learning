using TMPro;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Paddle : MonoBehaviour {

    static readonly int timeOfLastHitId = Shader.PropertyToID("_TimeOfLastHit"),
        emissionColorId = Shader.PropertyToID("_EmissionColor"),
        faceColorId = Shader.PropertyToID("_FaceColor");

    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private MeshRenderer goalRender;
    [ColorUsage(true, true)]
    [SerializeField] private Color goalColor;

    [Min(0f)]
    [SerializeField]
    private float minExtents = 4f,
        maxExtents = 4f,
        speed = 10f,
        maxTargetingBias = 0.75f
        ;
    [SerializeField] private bool isAI;

    private int score;
    private float extents, targetingBias;
    private Material paddleMaterial, goalMaterial, scoreMaterial;

    private void Awake() {
        paddleMaterial = GetComponent<MeshRenderer>().material;
        goalMaterial = goalRender.material;
        goalMaterial.SetColor(emissionColorId, goalColor);
        scoreMaterial = scoreText.fontMaterial;

        SetScore(0);
    }

    public void StartNewGame() {
        SetScore(0);
        ChangeTargetingBias();
    }

    public bool ScorePoint(int pointToWin) {
        goalMaterial.SetFloat(timeOfLastHitId, Time.time);
        SetScore(score + 1, pointToWin);
        return score >= pointToWin;
    }
    public void SetScore(int newScore, float pointsToWin = 1000f) {
        score = newScore;
        scoreText.SetText("{0}", newScore);
        scoreMaterial.SetColor(faceColorId, goalColor * (newScore / pointsToWin));
        SetExtents(Mathf.Lerp(maxExtents, minExtents, newScore / (pointsToWin - 1f)));
    }

    public void Move(float target, float arenaExtents) {
        Vector3 p = transform.localPosition;
        p.x = isAI ? AdjustByAI(p.x, target) : AdjustByPlayer(p.x);
        float limit = arenaExtents - extents;
        p.x = Mathf.Clamp(p.x, -limit, limit);
        transform.position = p;
    }

    private float AdjustByAI(float x, float target) {
        target += targetingBias * extents;

        if (x < target) {
            return Mathf.Min(x + speed * Time.deltaTime, target);
        }
        return Mathf.Max(x - speed * Time.deltaTime, target);
    }
    private float AdjustByPlayer(float x) {
        bool goRight = Input.GetKey(KeyCode.RightArrow);
        bool goLeft = Input.GetKey(KeyCode.LeftArrow);

        if (goRight && !goLeft) {
            return x + speed * Time.deltaTime;
        } else if (!goRight && goLeft) {
            return x - speed * Time.deltaTime;
        }
        return x;
    }

    public bool HitBall(float ballX, float ballExtents, out float hitFactor) {
        ChangeTargetingBias();

        hitFactor = (ballX - transform.localPosition.x) / (ballExtents + extents);

        bool success = -1f <= hitFactor && hitFactor <= 1f;
        if (success) {
            paddleMaterial.SetFloat(timeOfLastHitId, Time.time);
        }

        return success;
    }


    private void ChangeTargetingBias() => targetingBias = Random.Range(-maxTargetingBias, maxTargetingBias);

    private void SetExtents(float newExtents) {
        extents = newExtents;
        Vector3 s = transform.localScale;
        s.x = 2f * newExtents;
        transform.localScale = s;
    }

}
