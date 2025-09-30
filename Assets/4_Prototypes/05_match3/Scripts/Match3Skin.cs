using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class Match3Skin : MonoBehaviour {
    [SerializeField] private Tile[] tilePrefabs;

    [SerializeField] private Match3Game game;

    [Range(0.1f, 1f)]
    [SerializeField] private float dragThrehold = 0.5f;
    [SerializeField] private TileSwapper tileSwapper;

    [Range(0.1f, 20f)]
    [SerializeField] private float dropSpeed = 8f;

    [Range(0f, 10f)]
    [SerializeField] private float newDropOffset = 2f;

    [SerializeField] private TextMeshPro totalScoreText, gameOverText;

    [SerializeField] private FloatingScore floatingScorePrefab;


    private Grid2D<Tile> tiles;
    private float2 tileOffset;

    private float busyDuration;
    private float floatingScoreZ;

    public bool IsPlaying => IsBusy || game.PossibleMove.IsValid;
    public bool IsBusy => busyDuration > 0f;

    public void StartNewGame() {
        busyDuration = 0f;
        totalScoreText.SetText("0");
        gameOverText.gameObject.SetActive(false);

        game.StartNewGame();
        tileOffset = -0.5f * (float2)(game.Size - 1);
        if (tiles.IsUndefined) {
            tiles = new Grid2D<Tile>(game.Size);
        } else {
            for (int y = 0; y < tiles.SizeY; y++) {
                for (int x = 0; x < tiles.SizeX; x++) {
                    tiles[x, y].Despawn();
                    tiles[x, y] = null;
                }
            }
        }

        for (int y = 0; y < tiles.SizeY; y++) {
            for (int x = 0; x < tiles.SizeX; x++) {
                tiles[x, y] = SpawnTile(game[x, y], x, y);
            }
        }

    }

    private Tile SpawnTile(TileState t, float x, float y) =>
        tilePrefabs[(int)t - 1].Spawn(new Vector3(x + tileOffset.x, y + tileOffset.y));

    public void DoWork() {

        if (busyDuration > 0f) {
            tileSwapper.Update();
            busyDuration -= Time.deltaTime;
            if (busyDuration > 0f) {
                return;
            }
        }

        if (game.HasMatches) {
            ProcessMatches();
        } else if (game.NeedFilling) {
            DropTiles();
        } else if (!IsPlaying) {
            gameOverText.gameObject.SetActive(true);
        }
    }

    private void DropTiles() {
        game.DropTiles();

        for (int i = 0; i < game.DroppedTiles.Count; i++) {
            TileDrop drop = game.DroppedTiles[i];
            Tile tile;
            if (drop.fromY < tiles.SizeY) {
                tile = tiles[drop.coordinates.x, drop.fromY];
            } else {
                tile = SpawnTile(
                    game[drop.coordinates], drop.coordinates.x, drop.fromY + newDropOffset
                );
            }
            tiles[drop.coordinates] = tile;
            busyDuration = Mathf.Max(
                tile.Fall(drop.coordinates.y + tileOffset.y, dropSpeed)
                , busyDuration
            );
        }


    }

    private void ProcessMatches() {
        game.ProcessMatches();
        for (int i = 0; i < game.ClearedTileCoordinates.Count; i++) {
            int2 c = game.ClearedTileCoordinates[i];
            busyDuration = Mathf.Max(tiles[c].Disappear(), busyDuration);
            tiles[c] = null;
        }

        for (int i = 0; i < game.Scores.Count; i++) {
            SingleScore score = game.Scores[i];
            floatingScorePrefab.Show(
                new Vector3(
                    score.position.x + tileOffset.x,
                    score.position.y + tileOffset.y,
                    floatingScoreZ
                ),
                score.value
            );
            floatingScoreZ = floatingScoreZ < -0.02f ? 0f : floatingScoreZ - 0.001f;
        }

        totalScoreText.SetText("{0}", game.TotalScore);
    }

    public bool EvaluateDrag(Vector3 start, Vector3 end) {
        float2 a = ScreenToTileSpace(start), b = ScreenToTileSpace(end);
        Move move = new Move(
            (int2)a,
            (b - a) switch {
                float2 d when d.x > dragThrehold => MoveDirection.Right,
                float2 d when d.x < -dragThrehold => MoveDirection.Left,
                float2 d when d.y > dragThrehold => MoveDirection.Up,
                float2 d when d.y < -dragThrehold => MoveDirection.Down,
                _ => MoveDirection.None
            }
        );
        if (move.IsValid && tiles.AreValidCoordinates(move.From) && tiles.AreValidCoordinates(move.To)) {
            DoMove(move);
            return false;
        }
        return true;
    }

    private void DoMove(Move move) {
        bool success = game.TryMove(move);
        Tile a = tiles[move.From], b = tiles[move.To];
        busyDuration = tileSwapper.Swap(a, b, !success);
        if (success) {
            tiles[move.From] = b;
            tiles[move.To] = a;
        }
    }

    private float2 ScreenToTileSpace(Vector3 screenPosition) {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Vector3 p = ray.origin - ray.direction * (ray.origin.z / ray.direction.z);
        return float2(p.x - tileOffset.x + 0.5f, p.y - tileOffset.y + 0.5f);
    }

    private float2 ScreenToTileSpace2(Vector3 screenPosition) {
        screenPosition.z = -Camera.main.transform.position.z;
        Vector3 p = Camera.main.ScreenToWorldPoint(screenPosition);
        return float2(p.x - tileOffset.x + 0.5f, p.y - tileOffset.y + 0.5f);
    }

    public void DoAutomaticMove() => DoMove(game.PossibleMove);



}
