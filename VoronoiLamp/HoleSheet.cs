using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace VoronoiLamp
{
    public class HoleSheet :I3dObject
    {
        private string material;
        private List<Vector3> innerVerts;
        public HoleSheet(List<Vector3> verts, Vector3 holecenter, float r, float h)
        {
            BaseVerts = verts;
            Thickness = h;
            Triangles = new List<Triangle>();
            innerVerts = new List<Vector3>();
            var n = 20;
            for(var k = 0; k < n; k++)
            {
                var phi = Math.PI * 2 * k / n;
                var rot = new Vector3((float)Math.Cos(phi) * r, (float)Math.Sin(phi) * r, 0);
                innerVerts.Add(rot + holecenter);
            }
            Render();
        }
        public HoleSheet(List<Vector3> verts, List<Vector3> hole, float h)
        {
            BaseVerts = verts;
            innerVerts = hole;
            Triangles = new List<Triangle>();
            Thickness = h;
            Render();
        }
        private void Render()
        {
            Triangles = new List<Triangle>();
            Normal = Vector3.Cross(BaseVerts[0] - Center, BaseVerts[1] - Center);
            Normal = Vector3.Normalize(Normal);
            var offset = Thickness * 0.5f * Normal;
            for (var k = 0; k < innerVerts.Count; k++)
            {
                var h1 = innerVerts[k];
                var h2 = innerVerts[(k + 1) % innerVerts.Count];
                var h3 = innerVerts[(k + 2) % innerVerts.Count];
                var n1 = ClosestVert(h1, Vector3.Cross(h2 - h1, Normal));
                var n2 = ClosestVert(h2, Vector3.Cross(h3 - h2, Normal));
                Triangles.Add(new Triangle(h1 + offset, n1 + offset, h2 + offset, this));
                Triangles.Add(new Triangle(n1 - offset, h1 - offset, h2 - offset, this));

                Triangles.Add(new Triangle(h1 + offset, h2 + offset, h2 - offset, this));
                Triangles.Add(new Triangle(h1 - offset, h1 + offset, h2 - offset, this));
                if (n1 != n2)
                {
                    Triangles.Add(new Triangle(n1 + offset, n2 + offset, h1 + offset, this));
                    Triangles.Add(new Triangle(n2 - offset, n1 - offset, h1 - offset, this));
                    Triangles.Add(new Triangle(n1 + offset, n2 + offset, h2 + offset, this));
                    Triangles.Add(new Triangle(n2 - offset, n1 - offset, h2 - offset, this));

                    Triangles.Add(new Triangle(n1 + offset, n2 - offset, n2 + offset, this));
                    Triangles.Add(new Triangle(n1 - offset, n2 - offset, n1 + offset, this));
                }
            }
            Material = Material;
        }
        public void Rotate(Vector3 origin, Vector3 axis, float rads)
        {
            var result = new List<Vector3>();
            axis = Vector3.Normalize(axis);
            foreach (var v in BaseVerts)
            {
                var vd = v - origin;
                var vnew = axis * Vector3.Dot(axis, vd);
                var axvd = Vector3.Cross(axis, vd);
                vnew += (float)Math.Cos(rads) * Vector3.Cross(axvd, axis);
                vnew += (float)Math.Sin(rads) * axvd;
                result.Add(vnew);
            }
            BaseVerts = result.Select(v => v + origin).ToList();
            result.Clear();
            foreach (var v in innerVerts)
            {
                var vd = v - origin;
                var vnew = axis * Vector3.Dot(axis, vd);
                var axvd = Vector3.Cross(axis, vd);
                vnew += (float)Math.Cos(rads) * Vector3.Cross(axvd, axis);
                vnew += (float)Math.Sin(rads) * axvd;
                result.Add(vnew);
            }
            innerVerts = result.Select(v => v + origin).ToList();
            Render();
        }
        private Vector3 ClosestVert(Vector3 v, Vector3 n)
        {
            var res = BaseVerts[0];
            var dist = float.PositiveInfinity;
            foreach(var vert in BaseVerts)
            {
                var dir = vert - v;
                //if (Vector3.Dot(dir, n) < 0) continue;
                //var d1 = Vector3.Distance(vert, v);
                var d1 = Vector3.Dot(dir, -n);
                if (d1 < dist)
                {
                    res = vert;
                    dist = d1;
                }
            }
            return res;
        }

        public List<Vector3> Verteces
        {
            get
            {
                var res = Triangles.Aggregate(new List<Vector3>(), (a, b) => {
                    a.AddRange(b.Verteces);
                    return a;
                });
                res = res.Distinct().ToList();
                return res;
            }
        }
        public List<Vector3> BaseVerts { get; private set; }

        public List<Triangle> Triangles { get; private set; }
        public Vector3 Normal { get; private set; }

        public bool Hide { get; set; }
        public string Name { get; set; }
        public string Material
        {
            get => material;
            set
            {
                material = value;
                foreach (var t in Triangles)
                {
                    t.Material = Material;
                }
            }
        }

        public Vector3 Center
        {
            get
            {
                return BaseVerts.Aggregate((a, b) =>
                {
                    return a + b;
                }) / BaseVerts.Count;
            }
        }
        public float Thickness { get; private set; }

        public void Move(Vector3 v)
        {
            BaseVerts = BaseVerts.Select(b => b + v).ToList();
            innerVerts = innerVerts.Select(i => i + v).ToList();
            Render();
        }

        public Tuple<Vector3, Vector3> PlaneIntersect(Vector3 origin, Vector3 normal)
        {
            throw new NotImplementedException();
        }
    }
}
