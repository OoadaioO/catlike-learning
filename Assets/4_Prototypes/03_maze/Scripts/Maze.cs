using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;
public struct Maze {
    private int2 size;

    [NativeDisableParallelForRestriction]
    private NativeArray<MazeFlags> cells;

    public int Length => cells.Length;

    public Maze(int2 size) {
        this.size = size;
        this.cells = new NativeArray<MazeFlags>(size.x * size.y, Allocator.Persistent);
    }

    public void Dispose() {
        if (cells.IsCreated) {
            cells.Dispose();
        }
    }

    public int CoordinatesToIndex(int2 coordinates) {
        return coordinates.y * size.x + coordinates.x;
    }

    public int2 WorldPositionToCoordinates(Vector3 position) =>
        int2(
            (int)((position.x + size.x) * 0.5f),
            (int)((position.y + size.y) * 0.5f)
        );
    public int WorldPositionToIndex(Vector3 position) =>
        CoordinatesToIndex(WorldPositionToCoordinates(position));

    public int2 IndexToCoordinates(int index) {
        int2 coordinates;
        coordinates.y = index / size.x;
        coordinates.x = index - coordinates.y * size.x;
        return coordinates;
    }

    public Vector3 CoordinatesToWorldPosition(int2 coordinates, float y = 0f) =>
        new Vector3(
            2f * coordinates.x + 1 - size.x,
            y,
            2f * coordinates.y + 1 - size.y
        );


    public Vector3 IndexToWorldPosition(int index, float y = 0f) =>
        CoordinatesToWorldPosition(IndexToCoordinates(index), y);


    public MazeFlags this[int index] {
        get => cells[index];
        set => cells[index] = value;
    }

    public MazeFlags Set(int index, MazeFlags mask) => cells[index] = cells[index].With(mask);

    public MazeFlags Unset(int index, MazeFlags mask) => cells[index] = cells[index].WithOut(mask);

    public int SizeEW => size.x;
    public int SizeNS => size.y;
    public int StepN => size.x;
    public int StepE => 1;
    public int StepS => -size.x;
    public int StepW => -1;

}
