using System;
using System.Numerics;
using System.Collections.Generic;
namespace VoronoiLamp
{
    public interface I3dObject
    {
        List<Vector3> Verteces { get; }
        List<Triangle> Triangles { get; }
        Tuple<Vector3, Vector3> PlaneIntersect(Vector3 origin, Vector3 normal);
        bool Hide { get; set; }
        string Name { get; set; }
        string Material { get; set; }
        void Move(Vector3 v);
        Vector3 Center { get; }
    }
}
