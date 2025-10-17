using System.IO;
using UnityEngine;

public class GameDataReader {

    public int Version{ get; }
    BinaryReader reader;

    public GameDataReader(BinaryReader reader,int version) {
        this.reader = reader;
        this.Version = version;
    }

    public float ReadFloat(){
        return reader.ReadSingle();
    }

    public int ReadInt(){
        return reader.ReadInt32();
    }

    public Vector3 ReadVector3(){
        Vector3 p;
        p.x = reader.ReadSingle();
        p.y = reader.ReadSingle();
        p.z = reader.ReadSingle();
        return p;
    }

    public Quaternion ReadQuaternion(){
        Quaternion r;
        r.x = reader.ReadSingle();
        r.y = reader.ReadSingle();
        r.z = reader.ReadSingle();
        r.w = reader.ReadSingle();
        return r;
    }

    public Color ReadColor(){
        Color c;
        c.r = reader.ReadSingle();
        c.g = reader.ReadSingle();
        c.b = reader.ReadSingle();
        c.a = reader.ReadSingle();
        return c;
    }
}