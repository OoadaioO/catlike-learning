using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace paddle.square {
    public class Game : MonoBehaviour {

        [SerializeField] private LivelyCamera livelyCamera;

        [SerializeField] private TextMeshPro countdownText;
        [Min(1f)]
        [SerializeField] private float newGameDelay = 3f;

        [Min(2)]
        [SerializeField] private int pointToWin = 3;
        [SerializeField] private Ball ball;
        [SerializeField] private Paddle topPaddle, bottomPaddle;
        [SerializeField] private Vector2 arenaExtents = new Vector2(10f, 10f);


        private float countdownUntilNewGame;

        private void Awake() => countdownUntilNewGame = newGameDelay;

        private void StartNewGame() {
            ball.StartNewGame();
            topPaddle.StartNewGame();
            bottomPaddle.StartNewGame();
        }

        private void Update() {
            bottomPaddle.Move(ball.Position.x, arenaExtents.x);
            topPaddle.Move(ball.Position.x, arenaExtents.x);

            if (countdownUntilNewGame <= 0f) {
                countdownText.gameObject.SetActive(false);
                UpdateGame();
            } else {
                UpdateCountDown();
            }

        }

        private void UpdateCountDown() {
            countdownUntilNewGame -= Time.deltaTime;
            if (countdownUntilNewGame <= 0f) {
                countdownText.gameObject.SetActive(true);
                StartNewGame();
            } else {
                float displayValue = Mathf.Ceil(countdownUntilNewGame);
                if (displayValue < newGameDelay) {
                    countdownText.SetText("{0}", displayValue);
                }
            }
        }

        private void UpdateGame() {
            ball.Move();
            BounceYIfNeeded();
            BounceXIfNeeded(ball.Position.x);
            ball.UpdateVisualization();
        }



        private void BounceYIfNeeded() {
            float yExtents = arenaExtents.y - ball.Extents;

            if (ball.Position.y < -yExtents) {
                BounceY(-yExtents, bottomPaddle, topPaddle);

            } else if (ball.Position.y > yExtents) {
                BounceY(yExtents, topPaddle, bottomPaddle);
            }
        }

        private void BounceY(float boundary, Paddle defender, Paddle attacker) {
            float durationAfterBounce = (ball.Position.y - boundary) / ball.Velocity.y;
            float bounceX = ball.Position.x - ball.Velocity.x * durationAfterBounce;

            BounceXIfNeeded(bounceX);
            bounceX = ball.Position.x - ball.Velocity.x * durationAfterBounce;
            livelyCamera.PushXZ(ball.Velocity);
            ball.BounceY(boundary);

            if (defender.HitBall(bounceX, ball.Extents, out float hitFactor)) {
                ball.SetXPositionAndSpeed(bounceX, hitFactor, durationAfterBounce);
            } else {
                livelyCamera.JostleY();
                if (attacker.ScorePoint(pointToWin)) {
                    EndGame();
                }
            }
        }


        private void BounceXIfNeeded(float x) {
            float xExtents = arenaExtents.x - ball.Extents;

            if (x < -xExtents) {
                livelyCamera.PushXZ(ball.Velocity);
                ball.BounceX(-xExtents);

            } else if (x > xExtents) {
                livelyCamera.PushXZ(ball.Velocity);
                ball.BounceX(xExtents);
            }
        }

        private void EndGame() {
            countdownUntilNewGame = newGameDelay;
            countdownText.SetText("GAME OVER");
            countdownText.gameObject.SetActive(true);
            ball.EndGame();
        }
    }
}