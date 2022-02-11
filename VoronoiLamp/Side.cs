using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VoronoiLamp
{
    public class Side
    {
        public Side()
        {
            Triangles = new List<Triangle>();
        }
        public Side(IEnumerable<Triangle> triangles)
        {
            Triangles = new List<Triangle>();
            Triangles.AddRange(triangles);
        }

        public List<Triangle> Triangles { get; }
        public bool Inside(Vector3 v)
        {
            throw new NotImplementedException();
            foreach(var t in Triangles)
            {
                if (t.Inside(v)) return true;
            }
            return false;
        }
    }
}
