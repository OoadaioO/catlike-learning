using Unity.Mathematics;
using System;

[Serializable]
public struct Grid2D<T> {
    private T[] cells;
    private int2 size;


    public int2 Size => size;
    public int SizeX => size.x;
    public int SizeY => size.y;

    public bool IsUndefined => cells == null || cells.Length == 0;

    public T this[int x, int y] {
        get => cells[x + size.x * y];
        set => cells[x + size.x * y] = value;
    }

    public T this[int2 c] {
        get => cells[c.x + c.y * size.x];
        set => cells[c.x + c.y * size.x] = value;
    }

    public Grid2D(int2 size) {
        this.size = size;
        this.cells = new T[size.x * size.y];
    }


    public bool AreValidCoordinates(int2 c) =>
        0 <= c.x && c.x < size.x && 0 <= c.y && c.y < size.y;

    public void Swap(int2 a, int2 b) => (this[a], this[b]) = (this[b], this[a]);

}
