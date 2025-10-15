using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tower.defense {

    public class Game : MonoBehaviour {

        const float pausedTimeScale = 0f;

        static Game instance;

        [SerializeField] Vector2Int boardSize = new Vector2Int(11, 11);
        [SerializeField, Range(0, 100)] int startingPlayerHealth = 10;
        [SerializeField] GameBoard board;
        [SerializeField] GameTileContentFactory tileContentFactory;
        [SerializeField] WarFactory warFactory;
        [SerializeField] GameScenario scenario = default;
        [SerializeField] LayerMask enemyLayerMask = 1 << 8;
        [SerializeField, Range(1f, 10f)] float playSpeed = 1f;


        int playerHealth;
        TowerType selectTowerType;
        GameScenario.State activeScenario;
        GameBehaviorCollection enemies = new GameBehaviorCollection();
        GameBehaviorCollection nonEnemies = new GameBehaviorCollection();


        Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);


        private void Awake() {
            playerHealth = startingPlayerHealth;
            board.Initialize(boardSize, tileContentFactory);
            board.ShowGrid = true;
            activeScenario = scenario.Begin();
        }

        private void OnEnable() {
            TargetPoint.SetEnemyLayerMask(enemyLayerMask);
            instance = this;
        }


        private void OnValidate() {
            if (boardSize.x < 2) {
                boardSize.x = 2;
            }
            if (boardSize.y < 2) {
                boardSize.y = 2;
            }
        }


        private void Update() {

            if (Input.GetKeyDown(KeyCode.Space)) {
                Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
            } else if (Time.timeScale > pausedTimeScale) {
                Time.timeScale = playSpeed;
            }

            if (Input.GetMouseButtonDown(0)) {
                HandleTouch();
            } else if (Input.GetMouseButtonDown(1)) {
                HandleAlternativeTouch();
            }

            if (Input.GetKeyDown(KeyCode.V)) {
                board.ShowPaths = !board.ShowPaths;
            }

            if (Input.GetKeyDown(KeyCode.G)) {
                board.ShowGrid = !board.ShowGrid;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                selectTowerType = TowerType.Laser;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                selectTowerType = TowerType.Mortar;
            }

            if (Input.GetKeyDown(KeyCode.B)) {
                BeginNewGame();
            }

            if (playerHealth <= 0 && startingPlayerHealth > 0) {
                Debug.Log("Defeat!");
                BeginNewGame();
            }

            if (!activeScenario.Progress() && enemies.IsEmpty) {
                Debug.Log("Victory!");
                BeginNewGame();
                activeScenario.Progress();
            }

            enemies.GameUpdate();
            Physics.SyncTransforms();
            board.GameUpdate();
            nonEnemies.GameUpdate();
        }



        public static Shell SpawnShell() {
            Shell shell = instance.warFactory.Shell;
            instance.nonEnemies.Add(shell);
            return shell;
        }

        public static Explosion SpawnExplosion() {
            Explosion explosion = instance.warFactory.Explosion;
            instance.nonEnemies.Add(explosion);
            return explosion;
        }

        public static void SpawnEnemy(EnemyFactory factory, EnemyType type) {
            GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
            Enemy enemy = factory.Get(type);
            enemy.SpawnOn(spawnPoint);
            instance.enemies.Add(enemy);
        }

        public static void EnemyReachedDestination() {
            instance.playerHealth -= 1;
        }



        void HandleAlternativeTouch() {
            GameTile tile = board.GetTile(TouchRay);
            if (tile != null) {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    board.ToggleDestination(tile);
                } else {
                    board.ToggleSpawnPoint(tile);
                }
            }
        }

        void HandleTouch() {
            GameTile tile = board.GetTile(TouchRay);
            if (tile != null) {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    board.ToggleTower(tile, selectTowerType);
                } else {
                    board.ToggleWall(tile);
                }
            }
        }

        void BeginNewGame() {
            playerHealth = startingPlayerHealth;
            enemies.Clear();
            nonEnemies.Clear();
            board.Clear();
            activeScenario = scenario.Begin();
        }

    }

}