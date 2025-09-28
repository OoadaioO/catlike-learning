
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct DisperseScentJob : IJobFor {
    [ReadOnly]
    public Maze maze;

    [ReadOnly, NativeDisableParallelForRestriction]
    public NativeArray<float> oldScent;
    public NativeArray<float> newScent;

    public void Execute(int i) {
        MazeFlags cell = maze[i];
        float scent = oldScent[i];

        float fromNeighbours = 0f;
        float disperseFactor = 0f;

        if (cell.Has(MazeFlags.PassageE)) {
            fromNeighbours += oldScent[i + maze.StepE];
            disperseFactor += 1f;
        }
        if (cell.Has(MazeFlags.PassageS)) {
            fromNeighbours += oldScent[i + maze.StepS];
            disperseFactor += 1f;
        }
        if (cell.Has(MazeFlags.PassageW)) {
            fromNeighbours += oldScent[i + maze.StepW];
            disperseFactor += 1f;
        }
        if (cell.Has(MazeFlags.PassageN)) {
            fromNeighbours += oldScent[i + maze.StepN];
            disperseFactor += 1f;
        }
        scent += (fromNeighbours - scent * disperseFactor) * 0.2f;
        newScent[i] = scent * 0.5f;
    }
}
