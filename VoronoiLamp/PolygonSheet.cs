using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
namespace VoronoiLamp
{
    public class PolygonSheet : I3dObject
    {
        #region Fields
        private List<Vector3> verteces2d;
        private List<Side> sides;
        private float thickness, bulging;
        private string material;
        private bool invertNormals;
        #endregion
        #region Constructors
        public PolygonSheet(Vector3[] verteces, float thickness, float bulging = 0f)
        {
            this.thickness = thickness;
            this.bulging = bulging;
            verteces2d = verteces.ToList();
            if (verteces.Length < 3)
            {
                throw new Exception("need at least three points to construct a sheet");
            }
            Render();
        }
        public PolygonSheet(PolygonSheet pol)
        {
            thickness = pol.thickness;
            verteces2d = pol.BaseVerts;
            Intersect = pol.Intersect;
            Render();
            Material = pol.Material;
        }
        public static PolygonSheet Regular(int corners, float radius, float thickness, float bulging = 0f)
        {
            var verts = new List<Vector3>();
            for (var k = 0; k < corners; k++)
            {
                var phi = (float)k / (float)corners * 2 * (float)Math.PI;
                verts.Add(new Vector3((float)Math.Sin(phi) * radius, (float)Math.Cos(phi) * radius, 0));
            }
            return new PolygonSheet(verts.ToArray(), thickness, bulging);
        }
        public PolygonSheet(Vector3 v1, Vector3 v2, Vector3 v3, float thickness)
        {
            verteces2d = new List<Vector3> { v1, v2, v3 };
            this.thickness = thickness;
            Render();
        }
        public static PolygonSheet Cube(float l)
        {
            var verts = new List<Vector3>();
            verts.Add(new Vector3());
            verts.Add(new Vector3(l, 0, 0));
            verts.Add(new Vector3(l, l, 0));
            verts.Add(new Vector3(0, l, 0));
            var cube = new PolygonSheet(verts.ToArray(), l);
            cube.Move(new Vector3(-0.5f * l, -0.5f * l, 0));
            return cube;
        }
        public static PolygonSheet Line(Vector3 start, Vector3 end, float r = 0.0005f, float bulging = 0f, int edges = 10)
        {
            if (end.Z < start.Z)
            {
                var temp = start;
                start = end;
                end = temp;
            }
            var v = end - start;
            var l = (float)Math.Sqrt(Vector3.Dot(v, v));
            var line = Regular(edges, r, l, bulging);
            line.Move(start + new Vector3(0, 0, 0.5f * l));
            var angle = Vector3Util.Angle(v, Vector3Util.UZ);
            var axis = Vector3.Cross(Vector3Util.UZ, v);
            if (angle > 0.0001f && Vector3Util.Length(axis) > 0.0001f)
            {
                axis = Vector3.Normalize(axis);
                line.Rotate(start, axis, (float)angle);
            }
            return line;
        }

        #endregion
        #region Private Methods
        private void Render()
        {
            //SortVerteces();
            sides = new List<Side>();
            Triangles = new List<Triangle>();
            var offset = (thickness / 2.0f) * Normal;
            if (verteces2d.Count < 3)
            {
                throw new Exception("must have at least 3 verteces");
            }
            else if (verteces2d.Count == 3)
            {
                var v1 = verteces2d[0];
                var v2 = verteces2d[1];
                var v3 = verteces2d[2];
                var top = new Triangle(v1 + offset, v2 + offset, v3 + offset, this);
                Triangles.Add(top);
                sides.Add(new Side(new[] { top }));
                var bottom = new Triangle(v1 - offset, v3 - offset, v2 - offset, this);
                Triangles.Add(bottom);
                sides.Add(new Side(new[] { bottom }));
            }
            else
            {
                var top = new Side();
                var bottom = new Side();
                for (var k = 0; k < verteces2d.Count; k++)
                {
                    var v1 = verteces2d[k];
                    var v2 = verteces2d[(k + 1) % verteces2d.Count];
                    // top section
                    var t = new Triangle(v1 + offset, v2 + offset, Center + offset + bulging * Normal, this);
                    Triangles.Add(t);
                    top.Triangles.Add(t);
                    // bottom section
                    var b = new Triangle(v2 - offset, v1 - offset, Center - offset - bulging * Normal, this);
                    Triangles.Add(b);
                    bottom.Triangles.Add(b);
                }
                sides.Add(top);
                sides.Add(bottom);
            }
            for (var k = 0; k < verteces2d.Count; k++)
            {
                var v1 = verteces2d[k];
                var v2 = verteces2d[(k + 1) % verteces2d.Count];
                // outer section 1
                var o1 = new Triangle(v1 - offset, v2 + offset, v1 + offset, this);
                Triangles.Add(o1);
                // outer section 2
                var o2 = new Triangle(v1 - offset, v2 - offset, v2 + offset, this);
                Triangles.Add(o2);
                sides.Add(new Side(new[] { o1, o2 }));
            }
            foreach (var t in Triangles)
            {
                t.Material = Material;
            }
        }
        private void SortVerteces()
        {
            var vertAngles = new List<Tuple<float, Vector3>>();
            var v0 = verteces2d[0] - Center;
            var lv0 = Math.Sqrt(Vector3.Dot(v0, v0));
            for (var k = 1; k < verteces2d.Count; k++)
            {
                var v = verteces2d[k] - Center;
                var angle = (float)Math.Acos(Vector3.Dot(v0, v) / Math.Sqrt(Vector3.Dot(v, v)) / lv0);
                vertAngles.Add(new Tuple<float, Vector3>(angle, verteces2d[k]));
            }
            verteces2d = vertAngles.OrderBy(t => t.Item1).Select(t => t.Item2).ToList();
        }
        #endregion
        #region Public Methods
        public void Move(Vector3 move)
        {
            verteces2d = verteces2d.Select(v => v + move).ToList();
            Render();
        }
        public void AlignNormal(Vector3 n)
        {
            var axis = Vector3.Cross(Normal, n);
            var angle = (float)Vector3Util.Angle(n, Normal);
            if(Vector3Util.Length(axis) > 0.0001f)
            {
                Rotate(Center, axis, angle);
            }
        }

        public void Rotate(Vector3 origin, Vector3 axis, float rads)
        {
            var result = new List<Vector3>();
            axis = Vector3.Normalize(axis);
            foreach (var v in verteces2d)
            {
                var vd = v - origin;
                var vnew = axis * Vector3.Dot(axis, vd);
                var axvd = Vector3.Cross(axis, vd);
                vnew += (float)Math.Cos(rads) * Vector3.Cross(axvd, axis);
                vnew += (float)Math.Sin(rads) * axvd;
                result.Add(vnew);
            }
            verteces2d = result.Select(v => v + origin).ToList();
            Render();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public Tuple<Vector3, Vector3> PlaneIntersect(PolygonSheet poly)
        {
            var res = poly.Intersect ? PlaneIntersect(poly.Center, poly.Normal, true) :
                new Tuple<Vector3, Vector3>(new Vector3(), new Vector3());
            if (Vector3.Distance(res.Item1, res.Item2) < 0.00001f)
            {
                Partners.Add(poly);
            }
            return res;
        }
            

        public Tuple<Vector3, Vector3> PlaneIntersect(Vector3 origin, Vector3 normal) =>
            PlaneIntersect(origin, normal, false);

        public Tuple<Vector3, Vector3> PlaneIntersect(Vector3 origin, Vector3 normal, bool clip = false)
        {
            var intersects = new List<Vector3>();
            var clipFrom = -1;
            var clipTo = -1;
            for (var k = 0; k < verteces2d.Count; k++)
            {
                var v1 = verteces2d[k] - origin;
                var v2 = verteces2d[(k + 1) % verteces2d.Count] - origin;
                var negPen = Vector3.Dot(v1, normal) > 0 && Vector3.Dot(v2, normal) < 0;
                var posPen = Vector3.Dot(v2, normal) > 0 && Vector3.Dot(v1, normal) < 0;
                if (posPen || negPen)
                {
                    //var v = Vector3.Normalize(v2 - v1);
                    var v = v2 - v1;
                    var a = Vector3.Dot(origin - verteces2d[k], normal) / Vector3.Dot(v, normal);
                    intersects.Add(verteces2d[k] + a * v);
                    if (posPen) clipFrom = k;
                    if (negPen) clipTo = k;
                }
            }
            if (intersects.Count == 2)
            {
                if (clip)
                {
                    Clip(clipFrom, clipTo, intersects);
                }
                return new Tuple<Vector3, Vector3>(intersects[0], intersects[1]);
            }
            return new Tuple<Vector3, Vector3>(new Vector3(), new Vector3());
        }
        public void Clip(int from, int to, List<Vector3> replacements)
        {

            var newverteces = new List<Vector3>();
            //if(from > to)
            //{
            //    var temp = to;
            //    to = from;
            //    from = temp;
            //}
            for (var k = 0; k < verteces2d.Count; k++)
            {
                if (from < to)
                {
                    if (k <= from || k > to)
                    {
                        newverteces.Add(verteces2d[k]);
                    }
                    if (k == from)
                    {
                        newverteces.AddRange(replacements);
                    }
                }
                if (from > to)
                {
                    if (k > to && k <= from)
                    {
                        newverteces.Add(verteces2d[k]);
                    }
                    if (k == from)
                    {
                        replacements.Reverse();
                        newverteces.AddRange(replacements);
                    }
                }
            }
            if (newverteces.Any(v => v.X == float.NaN || v.Y == float.NaN || v.Z == float.NaN))
            {
                throw new Exception("something went wrong here");
            }
            if (newverteces.Count < 3)
            {
                throw new Exception("something went wrong here");
            }
            verteces2d = newverteces;
            Render();
        }
        public void PolygonIntersect(PolygonSheet poly)
        {
            for(var k = 0; k < BaseVerts.Count; k++)
            {

            }
        }
        public bool LineItersects(Vector3 p, Vector3 v)
        {
            var l = v - p;
            // length along line direction until plane intersection
            var a = -Vector3.Dot(p - Center, Normal) / Vector3.Dot(l, Normal);
            // point of line intersection plane
            if(a > 1f || a < 0f)
            {
                return false;
            }
            var i = p + l * a;
            return PointInside(i);
        }
        public Vector3 LineIntersection(Vector3 p, Vector3 v)
        {
            var l = v - p;
            // length along line direction until plane intersection
            var a = -Vector3.Dot(p - Center, Normal) / Vector3.Dot(l, Normal);
            // point of line intersection plane
            return p + l * a;
        }
        public bool PointInside(Vector3 p)
        {
            var rot = 0.0;
            for (var k = 0; k < BaseVerts.Count; k++)
            {
                var v1 = BaseVerts[k];
                var v2 = BaseVerts[(k + 1) % BaseVerts.Count];
                rot += Vector3Util.Angle(v1 - p, v2 - p);
            }
            return Math.Abs(2 * Math.PI - Math.Abs(rot)) < 0.0001f;
        }
        //public bool LineIntersects(Vector3 p, Vector3 v, out List<Vector3> intersections)
        //{
        //    var res = false;
        //    intersections = new List<Vector3>();
        //    for (var k = 0; k < BaseVerts.Count; k++)
        //    {
        //        var o = (BaseVerts[k] + BaseVerts[(k + 1) % BaseVerts.Count]) / 2f;
        //        var n = o - Center;
        //        var posPen = Vector3.Dot(p - o, n) < 0 && Vector3.Dot(v - o, n) > 0;
        //        var negPen = Vector3.Dot(p - o, n) > 0 && Vector3.Dot(v - o, n) < 0;
        //        if (posPen || negPen)
        //        {
        //            var l = v - p;
        //            // length along line direction until plane intersection
        //            var a = Vector3.Dot(p - o, n) / Vector3.Dot(l, n);
        //            // point of line intersection plane
        //            intersections.Add(p + l * a);
        //            res = true;
        //        }
        //    }

        //    return res;
        //}
        //public bool IsInside(Vector3 p)
        //{
        //    var inside = true;
        //    for(var k = 0; k < BaseVerts.Count; k++)
        //    {
        //        var o = (BaseVerts[k] + BaseVerts[(k + 1) % BaseVerts.Count]) / 2f;
        //        var n = o - Center;
        //        if(Vector3.Dot(p - o, n) < 0)
        //        {
        //            inside = false;
        //            break;
        //        }
        //    }
        //    return inside;
        //}
        #endregion
        #region Properties
        public Vector3 Center
        {
            get
            {
                return verteces2d.Aggregate((a, b) =>
                {
                    return a + b;
                }) / verteces2d.Count;
            }
        }
        public bool IsOpposite(Vector3 from, Vector3 target)
        {
            var d1 = Vector3.Dot(from - Center, Normal);
            var d2 = Vector3.Dot(target - Center, Normal);
            return d1 * d2 < 0;
        }
        public Vector3 Normal
        {
            get
            {
                var n = Vector3.Cross(verteces2d[1] - verteces2d[0], verteces2d[2] - verteces2d[0]);
                return Vector3.Normalize(n);
            }
        }
        public List<Triangle> Triangles { get; private set; }
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
        public bool Hide { get; set; }
        public List<Vector3> BaseVerts
        {
            get
            {
                var res = new List<Vector3>();
                foreach (var v in verteces2d)
                {
                    res.Add(v);
                }
                return res;
            }
        }
        public bool Intersect { get; set; } = true;
        public bool InvertNormals {
            get => invertNormals;
            set
            {
                invertNormals = value;
                foreach (var t in Triangles)
                {
                    t.InvertNormal = value;
                }
            }
        }
        public float Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                Render();
            }
        }
        public string Name { get; set; }
        public bool Inside(Vector3 v) => sides.TrueForAll(side => side.Inside(v));
        public List<PolygonSheet> Partners { get; } = new List<PolygonSheet>();
        #endregion
    }
}
