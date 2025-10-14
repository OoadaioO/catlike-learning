using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour {


    [SerializeField]
    Transform arrow = default;


    GameTile north, east, south, west, nextOnPath;

    int distance;

    GameTileContent content;

    public GameTileContent Content {
        get => content;
        set {
            Debug.Assert(value != null, "Null assign to Content");
            if (content != null) {
                content.Recycle();
            }
            content = value;
            content.transform.localPosition = transform.localPosition;
        }
    }

    public bool IsAlternative { get; set; }

    public Vector3 ExitPoint { get; private set; }
    public Direction PathDirection { get; private set; }

    public GameTile NextTileOnPath => nextOnPath;


    public void ClearPath() {
        distance = int.MaxValue;
        nextOnPath = null;
    }

    public void BecomeDestination() {
        distance = 0;
        nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    public bool HasPath => distance != int.MaxValue;


    GameTile GrowPathTo(GameTile neighbor, Direction direction) {
        Debug.Assert(HasPath, "No Path");
        if (neighbor == null || neighbor.HasPath) {
            return null;
        }
        neighbor.distance = neighbor.distance + 1;
        neighbor.nextOnPath = this;
        neighbor.ExitPoint = neighbor.transform.localPosition + direction.GetHalfVector();
        neighbor.PathDirection = direction;
        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }

    public GameTile GrowPathNorth() => GrowPathTo(north, Direction.South);
    public GameTile GrowPathEast() => GrowPathTo(east, Direction.West);
    public GameTile GrowPathSouth() => GrowPathTo(south, Direction.North);
    public GameTile GrowPathWest() => GrowPathTo(west, Direction.East);

    public void ShowPath() {
        if (distance == 0) {
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);
        arrow.localRotation = nextOnPath switch {
            _ when nextOnPath == north => northRotation,
            _ when nextOnPath == east => eastRotation,
            _ when nextOnPath == south => southRotation,
            _ => westRotation
        };
    }

    public void HidePath() {
        arrow.gameObject.SetActive(false);
    }



    static Quaternion northRotation = Quaternion.Euler(90f, 0f, 0f),
     eastRotation = Quaternion.Euler(90f, 90f, 0f),
     southRotation = Quaternion.Euler(90f, 180f, 0f),
     westRotation = Quaternion.Euler(90f, 270f, 0f);

    public static void MakeEastWestNeighbours(GameTile east, GameTile west) {
        Debug.Assert(west.east == null && east.west == null, "Redefined Neighbors");
        west.east = east;
        east.west = west;
    }

    public static void MakeNorthSouthNeighbours(GameTile north, GameTile south) {
        Debug.Assert(north.south == null && south.north == null, "Redefined Neighbors");
        north.south = south;
        south.north = north;
    }
}
