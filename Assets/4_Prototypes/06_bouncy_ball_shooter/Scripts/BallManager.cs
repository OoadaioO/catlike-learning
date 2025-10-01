using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BallManager : MonoBehaviour {

    [SerializeField] private BallVisualization[] ballPrefabs;
    [Min(0f)]
    [SerializeField] private float startingCooldown = 4f, avoidSpawnRadius = 2f;

    [Range(0.1f, 1f)]
    [SerializeField] private float cooldownPersistent = 0.96f;

    [Min(0f)]
    [SerializeField] private float maxSpeed = 12.5f, maxStartStpeed = 4f;

    [Min(0f)]
    [SerializeField] private float bounceStrength = 100f,explosionStrength = 2f;

    [SerializeField, Range(0.01f, 1f)]
	private float fragmentSeparation = 0.6f;

    private float cooldown, cooldownDuration;

    private NativeList<BallState> states;
    private List<BallVisualization> visualizations;

    private UpdateBallJob updateBallJob;
    private BounceBallsJob bounceBallsJob;
    private VerifySpawnPositionJob verifySpawnPositionJob;

    public void Initialize(Area2D worldArea, ref HitJob hitJob) {
        states = new NativeList<BallState>(100, Allocator.Persistent);
        visualizations = new List<BallVisualization>(states.Capacity);
        cooldown = cooldownDuration = startingCooldown;

        updateBallJob = new UpdateBallJob {
            balls = states,
            worldArea = worldArea,
            maxSpeed = maxSpeed
        };

        bounceBallsJob = new BounceBallsJob {
            balls = states,
            worldArea = worldArea,
            bounceStrength = bounceStrength
        };

        verifySpawnPositionJob = new VerifySpawnPositionJob {
            balls = states,
            success = new NativeReference<bool>(Allocator.Persistent),
            worldArea = worldArea,
            avoidRadius = avoidSpawnRadius,
            radius = BallState.radii[BallState.initialStage],
        };
        hitJob.balls = states;
        hitJob.fragmentSeparation = fragmentSeparation;
        hitJob.explosionStrength = explosionStrength;
    }

    public void Dispose() {
        states.Dispose();
        verifySpawnPositionJob.success.Dispose();
    }
    public void StartNewGame() {
        for (int i = 0; i < visualizations.Count; i++) {
            visualizations[i].Despawn();
        }
        visualizations.Clear();
        states.Clear();

        cooldown = cooldownDuration = startingCooldown;
    }

    public JobHandle UpdateBalls(float dt) {
        cooldown -= dt;
        bounceBallsJob.dt = updateBallJob.dt = dt;
        return updateBallJob.Schedule(states.Length, default);
    }

    public void ResolveBalls(Vector2 avoidSpawnPosition, JobHandle dependency) {

        dependency = bounceBallsJob.Schedule(dependency);

        if (cooldown <= 0f) {
            verifySpawnPositionJob.avoidPosition = avoidSpawnPosition;
            verifySpawnPositionJob.position = updateBallJob.worldArea.RandomVector2;
            dependency = verifySpawnPositionJob.Schedule(dependency);
        }
        dependency.Complete();

        if (cooldown <= 0f && verifySpawnPositionJob.success.Value) {
            cooldown += cooldownDuration;
            cooldownDuration *= cooldownPersistent;
            states.Add(new BallState {
                velocity = Random.insideUnitCircle * maxStartStpeed,
                mass = BallState.masses[BallState.initialStage],
                targetRadius = BallState.radii[BallState.initialStage],
                type = Random.Range(0, ballPrefabs.Length),
                position = verifySpawnPositionJob.position,
                stage = BallState.initialStage,
                alive = true
            });
        }
    }

    public void UpdateVisualization(float dtExtrapolated) {
        for (int i = visualizations.Count; i < states.Length; i++) {
            visualizations.Add(ballPrefabs[states[i].type].Spawn());
        }

        for (int i = 0; i < visualizations.Count; i++) {
            BallState state = states[i];

            if (state.alive) {
                visualizations[i].UpdateVisualization(
                    state.position + state.velocity * dtExtrapolated,
                    Mathf.Min(state.radius + dtExtrapolated, state.targetRadius)
                );
            } else {
                int lastIndex = states.Length - 1;
                states[i] = states[lastIndex];
                states.Length -= 1;

                visualizations[i].Despawn();
                visualizations[i] = visualizations[lastIndex];
                visualizations.RemoveAt(lastIndex);
                i -= 1;
            }
        }
    }

}
