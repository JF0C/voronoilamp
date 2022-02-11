using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace VoronoiLamp
{
    public class VoronoiSurface
    {
        private List<long> knownSurfaces;
        private static long idcount;
        private float thickness;
        private PolygonSheet polygon;
        public VoronoiSurface(Vector3 origin, Vector3 normal, float thickness)
        {
            this.thickness = thickness;
            knownSurfaces = new List<long>();
            Origin = origin;
            Normal = normal;
            Id = idcount++;
        }
        public bool Hides(Vector3 target, Vector3 from)
        {
            // line direction
            var l = target - from;
            // length along line direction until plane intersection
            var a = Vector3.Dot(from - Origin, Normal) / Vector3.Dot(l, Normal);
            // point of line intersection plane
            var p = from + l * a;
            return Vector3.Distance(p, Origin) < R;
            /*
            var rot = 0.0;
            for(var k = 0; k < Polygon.Verteces.Count; k++)
            {
                var v1 = Polygon.Verteces[k];
                var v2 = Polygon.Verteces[(k + 1) % Polygon.Verteces.Count];
                rot += Vector3Util.Angle(v1 - p, v2 - p);
            }
            return Math.Abs(2 * Math.PI - rot) < 0.0001f;
            */
        }
        public bool IsOpposite(Vector3 from, Vector3 target)
        {
            var d1 = Vector3.Dot(from - Origin, Normal);
            var d2 = Vector3.Dot(target - Origin, Normal);
            return d1 * d2 < 0;
        }
        public bool HidesAfterRender(Vector3 target, Vector3 from)
        {
            var l = target - from;
            var a = Vector3.Dot(from - Origin, Normal) / Vector3.Dot(l, Normal);
            var p = from + l * a;
            var rot = 0.0;
            for(var k = 0; k < Polygon.BaseVerts.Count; k++)
            {
                var v1 = Polygon.BaseVerts[k];
                var v2 = Polygon.BaseVerts[(k + 1) % Polygon.BaseVerts.Count];
                rot += Vector3Util.Angle(v1 - p, v2 - p);
            }
            return Math.Abs(2 * Math.PI - Math.Abs(rot)) < 0.0001f;

        }
        public bool Intersect(VoronoiSurface surface)
        {
            var res = Polygon.PlaneIntersect(surface.Polygon);
            return Vector3Util.Length(res.Item1 - res.Item2) > 0.0001f;
        }
        public List<Vector3> SharedVerteces(VoronoiSurface surface)
        {
            var shared = new List<Vector3>();
            foreach(var vert in surface.Polygon.BaseVerts)
            {
                if (Polygon.BaseVerts.Any(v => Math.Abs(Vector3.Distance(v, vert)) < 0.0001f))
                {
                    shared.Add(vert);
                }
            }
            return shared;
        }
        public void Render()
        {
            Polygon = PolygonSheet.Regular(100, R, thickness);
            Polygon.Move(Origin);
            Polygon.AlignNormal(Normal);
        }
        public PolygonSheet Polygon
        {
            get
            {
                if(polygon == null)
                {
                    Render();
                }
                return polygon;
            }
            private set => polygon = value;
        }
        public bool Gen2 { get; set; }
        public float R { get; set; }
        public Vector3 Origin { get; private set; }
        public Vector3 Normal { get; private set; }
        public long Id { get; }
    }
}
