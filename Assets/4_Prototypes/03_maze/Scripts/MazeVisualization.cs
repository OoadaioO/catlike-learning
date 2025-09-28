using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class MazeVisualization : MonoBehaviour {

    static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f,90f,0f),
        Quaternion.Euler(0f,180f,0f),
        Quaternion.Euler(0f,270f,0f)
    };

    [SerializeField]
    private MazeCellObject deadend, straight, cornerClosed, cornerOpen,
        tJunctionClosed, tJunctionOpenNE, tJunctionOpenSE, tJunctionOpen,
        xJunctionClosed, xJunctionOpenNE, xJunctionOpenNE_SE, xJunctionOpenNE_SW, xJunctionClosedNE, xJunctionOpen;

    public void Visualize(Maze maze, MazeCellObject[] cellObjects) {
        for (int i = 0; i < maze.Length; i++) {
            (MazeCellObject, int) prefabWithRotation = GetPrefab(maze[i]);
            MazeCellObject instance = cellObjects[i] = prefabWithRotation.Item1.GetInstance();
            instance.transform.SetParent(transform, false);
            instance.transform.SetPositionAndRotation(
                maze.IndexToWorldPosition(i), rotations[prefabWithRotation.Item2]
            );
        }
    }


    private (MazeCellObject, int) GetPrefab(MazeFlags flags) => flags.StraightPassages() switch {
        MazeFlags.PassageN => (deadend, 0),
        MazeFlags.PassageE => (deadend, 1),
        MazeFlags.PassageS => (deadend, 2),
        MazeFlags.PassageW => (deadend, 3),

        MazeFlags.PassageN | MazeFlags.PassageS => (straight, 0),
        MazeFlags.PassageE | MazeFlags.PassageW => (straight, 1),

        MazeFlags.PassageN | MazeFlags.PassageE => GetCorner(flags, 0),
        MazeFlags.PassageE | MazeFlags.PassageS => GetCorner(flags, 1),
        MazeFlags.PassageS | MazeFlags.PassageW => GetCorner(flags, 2),
        MazeFlags.PassageW | MazeFlags.PassageN => GetCorner(flags, 3),

        MazeFlags.PassagesStraight & ~MazeFlags.PassageW => GetTJunction(flags, 0),
        MazeFlags.PassagesStraight & ~MazeFlags.PassageN => GetTJunction(flags, 1),
        MazeFlags.PassagesStraight & ~MazeFlags.PassageE => GetTJunction(flags, 2),
        MazeFlags.PassagesStraight & ~MazeFlags.PassageS => GetTJunction(flags, 3),

        _ => GetXJunction(flags)
    };

    private (MazeCellObject, int) GetCorner(MazeFlags flags, int rotation) => (
        flags.HasAny(MazeFlags.PassagesDiagonal) ? cornerOpen : cornerClosed, rotation
    );


    private (MazeCellObject, int) GetTJunction(MazeFlags flags, int rotation) => (
        flags.RotateDiagonalPassages(rotation) switch {
            MazeFlags.Empty => tJunctionClosed,
            MazeFlags.PassageNE => tJunctionOpenNE,
            MazeFlags.PassageSE => tJunctionOpenSE,
            _ => tJunctionOpen
        },
        rotation
    );

    private (MazeCellObject, int) GetXJunction(MazeFlags flags) =>
        flags.DiagonalPassages() switch {
            MazeFlags.Empty => (xJunctionClosed, 0),

            MazeFlags.PassageNE => (xJunctionOpenNE, 0),
            MazeFlags.PassageSE => (xJunctionOpenNE, 1),
            MazeFlags.PassageSW => (xJunctionOpenNE, 2),
            MazeFlags.PassageNW => (xJunctionOpenNE, 3),

            MazeFlags.PassageNE | MazeFlags.PassageSE => (xJunctionOpenNE_SE, 0),
            MazeFlags.PassageSE | MazeFlags.PassageSW => (xJunctionOpenNE_SE, 1),
            MazeFlags.PassageSW | MazeFlags.PassageNW => (xJunctionOpenNE_SE, 2),
            MazeFlags.PassageNW | MazeFlags.PassageNE => (xJunctionOpenNE_SE, 3),

            MazeFlags.PassageNE | MazeFlags.PassageSW => (xJunctionOpenNE_SW, 0),
            MazeFlags.PassageSE | MazeFlags.PassageNW => (xJunctionOpenNE_SW, 1),

            MazeFlags.PassagesDiagonal & ~MazeFlags.PassageNE => (xJunctionClosedNE, 0),
            MazeFlags.PassagesDiagonal & ~MazeFlags.PassageSE => (xJunctionClosedNE, 1),
            MazeFlags.PassagesDiagonal & ~MazeFlags.PassageSW => (xJunctionClosedNE, 2),
            MazeFlags.PassagesDiagonal & ~MazeFlags.PassageNW => (xJunctionClosedNE, 3),

            _ => (xJunctionOpen, 0),
        };
}
