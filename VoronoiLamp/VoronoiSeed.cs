using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace VoronoiLamp
{
    public class VoronoiSeed
    {
        private float radius;
        private static long idcounter;
        private List<long> rejectedSeeds = new List<long>();
        private Dictionary<long, VoronoiSurface> surfaces = new Dictionary<long, VoronoiSurface>();
        private float surfaceThickness;
        public VoronoiSeed(float surfaceThickness, float max = 0.2f)
        {
            this.surfaceThickness = surfaceThickness;
            var rnd = new Random((int)DateTime.Now.Ticks);
            X = rnd.Next((int)(1000f * max)) / 1000f;
            Y = rnd.Next((int)(1000f * max)) / 1000f;
            Z = rnd.Next((int)(1000f * max)) / 1000f;
            Id = idcounter++;
            Items.Add(this);
        }
        public VoronoiSeed(float surfaceThickness, Vector3 v)
        {
            this.surfaceThickness = surfaceThickness;
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            Id = idcounter++;
            Items.Add(this);
        }
        public void Intersect(VoronoiSeed seed)
        {
            var v = Vector3.Normalize(seed.Center - Center);
            if (Vector3Util.Length(seed.Center - Center) < R + seed.R)
            {
                if (Surfaces.ContainsKey(seed.Id))
                {
                    return;
                }
                if (rejectedSeeds.Contains(seed.Id))
                {
                    return;
                }
                if (Surfaces.Any(s => s.Value.Hides(seed.Center, Center)))
                {
                    rejectedSeeds.Add(seed.Id);
                    return;
                }
                var surface = new VoronoiSurface(0.5f * (seed.Center + Center), v, surfaceThickness);
                Surfaces.Add(seed.Id, surface);

                //seed.Surfaces.Add(Id, surface);
                //AllSurfaces.Add(surface);
            }
        }
        public void MakePolygons()
        {
            Surfaces = Surfaces.OrderByDescending(kv => Vector3.Distance(kv.Value.Origin, Center))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            var maxr = Vector3.Distance(Surfaces.First().Value.Origin, Center);
            foreach(var kv in Surfaces)
            {
                kv.Value.Gen2 = IsGen2(kv.Value.Origin);
                kv.Value.R = 5 * maxr;
                kv.Value.Render();
                foreach(var other in Surfaces.Values.Where(s => s.Id != kv.Value.Id))
                {
                    kv.Value.Polygon.PlaneIntersect(other.Origin, other.Normal, true);
                }
                foreach(var n in Neighbors)
                {
                    n.ClipStranger(kv.Value, this);
                }
            }
        }
        public List<PolygonSheet> GetPolgons() =>
            Surfaces.Values.Select(v => v.Polygon).ToList();

        public void MakeSurface(VoronoiSeed seed)
        {
            var v = Vector3.Normalize(seed.Center - Center);
            var surface = new VoronoiSurface(0.5f * (seed.Center + Center), v, surfaceThickness);
            Surfaces.Add(seed.Id, surface);
        }
        public bool IsHiddenFrom(Vector3 point)
        {
            return Surfaces.Any(s => s.Value.Hides(Center, point));
        }
        private bool IsGen2(Vector3 v)
        {
            return Surfaces.Any(s => s.Value.IsOpposite(Center, v));
        }
        public void RenderSurfaces()
        {
            foreach(var f in Surfaces)
            {
                Surfaces.Where(kv => kv.Value.Id != f.Value.Id)
                    .ToList().ForEach(kv => f.Value.Intersect(kv.Value));

            }
        }
        public void ClipStranger(VoronoiSurface surface, VoronoiSeed seed)
        {
            //foreach(var surf in Surfaces.Values)
            //{
            //    if(surf.IsOpposite(Center, seed.Center))
            //    {
            //        surface.Polygon.SurfaceIntersect(surf.Polygon);
            //    }
            //}
        }

        public void CleanUp()
        {
            Nodes = new List<Vector3>();
            var res = new Dictionary<long, VoronoiSurface>();
            
            foreach (var s in Surfaces)
            {
                var partner = Items.First(i => i.Id == s.Key);
                var dist = Vector3.Distance(Center, s.Value.Origin);
                //if (Items.Any(i =>
                //{
                //    return i.Id != partner.Id &&
                //        i.Id != Id &&
                //        Vector3.Distance(i.Center, s.Value.Polygon.Center) <= dist;
                //}))
                //{
                //    s.Value.Polygon.Hide = true;
                //    continue;
                //}
                foreach(var vert in s.Value.Polygon.BaseVerts)
                {
                    var d = Vector3.Distance(Center, vert);
                    var critials = Items.Where(i => i.Id != partner.Id && i.Id != Id && Vector3.Distance(vert, i.Center) + 0.002f < d).ToList();
                    foreach (var c in critials)
                    {
                        if (c.Surfaces.TryGetValue(s.Key, out var addSurf))
                        {
                            s.Value.Polygon.PlaneIntersect(addSurf.Polygon.Center, -addSurf.Polygon.Normal);
                        }
                    }

                    foreach (var i in Items)
                    {

                        if(i.Id != partner.Id && i.Id != Id && Vector3.Distance(vert, i.Center) + 0.002f < d)
                        {
                            s.Value.Polygon.Hide = true;


                            s.Value.Polygon.Material = "red";


                            //if (!s.Value.Gen2)
                            //{
                            //}
                            //else
                            //{
                            //    s.Value.Render();
                            //    foreach(var p in i.Surfaces.Values.Select(f => f.Polygon))
                            //    {
                            //        s.Value.Polygon.SurfaceIntersect(p);
                            //    }
                            //    foreach(var neighbor in Neighbors)
                            //    {
                            //        neighbor.ClipStranger(s.Value, this);
                            //    }
                            //}
                            //foreach(var fkv in i.Surfaces)
                            //{
                            //    if(fkv.Key == Id || fkv.Key == i.Id)
                            //    {
                            //        s.Value.Polygon.SurfaceIntersect(fkv.Value.Polygon);
                            //}
                            //}
                            //s.Value.Polygon.Material = "red";
                            break;
                        }
                    }
                }
                res.Add(s.Key, s.Value);
                /*
                var isValid = true;
                foreach(var seed in Items)
                {
                    if (seed.Id == Id) continue;
                    if(!seed.Surfaces.Any(s => s.Value.Hides(s.Value.Polygon.Center, seed.Center)))
                    {
                        isValid = false;
                    }
                }
                if (!isValid) continue;
                var isConnected = false;
                foreach(var s2 in Surfaces)
                {
                    if(s2.Value.HidesAfterRender(s1.Value.Origin, Center))
                    {
                        //Items.FirstOrDefault(i => i.Id == s1.Key)?.Surfaces.Remove(Id);
                        isValid = false;
                        break;
                    }
                    
                    if (s1.Value.Id == s2.Value.Id) continue;
                    var nodes = s1.Value.SharedVerteces(s2.Value);
                    if (nodes.Count == 2)
                    {
                        if (res.ContainsKey(s2.Key)) continue;
                        isConnected = true;
                        //res.Add(s2.Key, s2.Value);
                        //Nodes.AddRange(nodes);
                    }
                    
                }
                if (!isValid || !isConnected) continue;

                res.Add(s1.Key, s1.Value);
                */
            }

            Surfaces = res;
        }

        public Dictionary<long, VoronoiSurface> Surfaces
        {
            get
            {
                return surfaces;
            }
            private set => surfaces = value;
        }

        public List<VoronoiSeed> Neighbors
        {
            get => Items.Where(i => Surfaces.Keys.Contains(i.Id)).ToList();
        }

        public static List<VoronoiSurface> AllSurfaces { get; } = new List<VoronoiSurface>();

        public float R
        {
            get => radius;
            set
            {
                radius = value;
                foreach (var kv in Surfaces)
                {
                    var h = kv.Value.Origin - Center;
                    kv.Value.R = (float)Math.Sqrt(radius * radius - Vector3.Dot(h, h));
                }
            }
        }
        public long Id { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public static List<VoronoiSeed> Items { get; } = new List<VoronoiSeed>();
        public List<Vector3> Nodes {get; private set;}
        public Vector3 Center => new Vector3(X, Y, Z);
    }
}
