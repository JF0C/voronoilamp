using System;
using System.Numerics;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace VoronoiLamp
{
    public class Triangle
    {
        private Vector3 v1, v2, o;
        private Vector3 normal;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, I3dObject parent)
        {
            Parent = parent;
            Verteces = new List<Vector3>();
            Verteces.Add(p1);
            Verteces.Add(p2);
            Verteces.Add(p3);
            o = p1;
            v1 = p2 - p1;
            v2 = p3 - p1;
            Normal = Vector3.Cross(v1, v2);
            Normal = Vector3.Normalize(Normal);
        }
        private Vector3 Round(Vector3 vec)
        {
            var res = new Vector3();
            res.X = (float)Math.Round(vec.X, 4);
            res.Y = (float)Math.Round(vec.Y, 4);
            res.Z = (float)Math.Round(vec.Z, 4);
            return res;
        }

        public Vector3 Project(Vector3 v)
        {
            var v0 = v - o;
            v0 -= Normal * Vector3.Dot(Normal, v0);
            return v0 + o;
        }

        public bool Inside(Vector3 v)
        {
            v -= o;
            var v1 = Vector3.Normalize(this.v1);
            var m = DenseMatrix.OfArray(new double[,] {
                { v1.X, v1.Y, v1.Z },
                { v2.X, v2.Y, v2.Z },
                { Normal.X, Normal.Y, Normal.Z }
            });
            var s = m.Solve(Vector<double>.Build.Dense(new double[] { v.X, v.Y, v.Z }));
            var d = s[0] + s[1];
            return d < 1 && s[0] > 0 && s[1] > 0;
        }

        public Vector3 Normal { get => InvertNormal ? -normal : normal; private set => normal = value; }
        public List<Vector3> Verteces { get; }
        public string Material { get; set; }
        public bool InvertNormal { get; set; }
        public I3dObject Parent { get; }

        public List<Triangle> Triangles => throw new NotImplementedException();
    }
}
