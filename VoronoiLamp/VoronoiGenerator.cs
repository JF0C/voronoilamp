using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VoronoiLamp
{
    public class VoronoiGenerator
    {
        float r;
        private List<VoronoiSeed> seeds;
        private List<PolygonSheet> walls;
        private Config config;
        private Environment env;

        public VoronoiGenerator(Config config, Environment env)
        {
            this.env = env;
            this.config = config;
        }
        private void InitSeedsBounded()
        {
            var rnd = new Random((int)DateTime.Now.Ticks);
            seeds = new List<VoronoiSeed>();
            var dimbound = Math.Pow(config.Seeds, 0.33333);
            dimbound = Math.Floor(dimbound);
            var cellConst = config.MaxDist / dimbound;
            var maxvar = config.SeedVariation * cellConst;
            for (var z = 0; z < dimbound; z++)
            {
                for (var x = 0; x < dimbound; x++)
                {
                    for(var y = 0; y < dimbound; y++)
                    {
                        var comb = (z % 2) * 0.4f;
                        var origin = new Vector3(
                            (float)((x + 0.5 + comb) * cellConst),
                            (float)((y + 0.5 + comb) * cellConst),
                            (float)((z + 0.5) * cellConst));
                        var variation = new Vector3(
                            (float)((rnd.Next(10000) / 10000.0 - 0.5) * maxvar),
                            (float)((rnd.Next(10000) / 10000.0 - 0.5) * maxvar),
                            (float)((rnd.Next(10000) / 10000.0 - 0.5) * maxvar));
                        //if(z > 0)
                        //{
                        //    Vector3 below, center;
                        //    do
                        //    {
                        //        below = seeds[x * y * (z - 1)].Center;
                        //        variation = new Vector3(
                        //            (float)((rnd.Next(10000) / 10000.0 - 0.5) * maxvar),
                        //            (float)((rnd.Next(10000) / 10000.0 - 0.5) * maxvar),
                        //            (float)((rnd.Next(10000) / 10000.0 - 0.5) * maxvar));
                        //        center = origin + variation;
                        //    }
                        //    while (Vector2.Distance(new Vector2(below.X, below.Y), new Vector2(center.X, center.Y)) < 0.1f * cellConst);
                            
                        //}
                        if(walls.All(w => !w.IsOpposite(origin + variation, Center)))
                        {
                            seeds.Add(new VoronoiSeed(config.SurfaceThickness, origin + variation));
                        }
                    }
                }
            }
            config.SeedPositions = seeds.Aggregate(new List<Vector3>(), (l, s) =>
            {
                l.Add(s.Center);
                return l;
            });
            var conf = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("../../config.json", conf);

        }
        private void InitSeeds()
        {
            seeds = new List<VoronoiSeed>();
            if (config.SeedPositions != null && config.SeedPositions.Count > 0)
            {
                foreach(var v in config.SeedPositions)
                {
                    seeds.Add(new VoronoiSeed(config.SurfaceThickness, v));
                }
            }
            if(config.Seeds > seeds.Count)
            {
                for (var k = seeds.Count; k < config.Seeds; k++)
                {
                    seeds.Add(new VoronoiSeed(config.SurfaceThickness, config.MaxDist));
                }
                config.SeedPositions = seeds.Aggregate(new List<Vector3>(), (l, s) =>
                {
                    l.Add(s.Center);
                    return l;
                });
                var conf = JsonConvert.SerializeObject(config);
                File.WriteAllText("../../config.json", conf);
            }
        }
        private void InitSeedsFromConfig()
        {
            seeds = new List<VoronoiSeed>();
            foreach (var v in config.SeedPositions)
            {
                seeds.Add(new VoronoiSeed(config.SurfaceThickness, v));
            }
        }
        public void Run()
        {
            walls = WallsRoof();
            if (config.SeedPositions != null && config.SeedPositions.Count > 0 && !config.CreateNew)
            {
                InitSeedsFromConfig();
            }
            else
            {
                InitSeedsBounded();
            }
            for (var step = 0; step < config.Steps; step++)
            {
                Console.WriteLine($"Step {step + 1} / {config.Steps}");
                r = step * config.StepSize;
                foreach(var s in seeds)
                {
                    s.R = r;
                }
                for(var k = 0; k < seeds.Count; k++)
                {
                    for(var l = 0; l < seeds.Count; l++)
                    {
                        if (k == l) continue;
                        seeds[k].Intersect(seeds[l]);
                    }
                }
            }
        }
        public void Render()
        {
            //env.Meshes.AddRange(walls);
            var counter = 0;
            //env.Meshes.Add(walls.First(m => m.Name == "base"));
            env.Meshes.AddRange(Floor());
            foreach (var s in seeds)
            {
                Console.WriteLine($"Rendering Seeds: {++counter} / {seeds.Count}");
                
                var sphere = new Sphere(0.002f, 3);
                sphere.Move(s.Center);
                sphere.Material = "redop";
                //env.Meshes.Add(sphere);
                s.MakePolygons();
                foreach(var f in s.Surfaces)
                {
                    var pol = f.Value.Polygon;
                    pol.Material = "grey";
                    //s.Surfaces.Where(kv => kv.Value.Id != f.Value.Id)
                    //    .ToList().ForEach(kv => f.Value.Intersect(kv.Value));

                    foreach (var wall in walls)
                    {
                        pol.PlaneIntersect(wall);
                    }
                    env.Meshes.Add(pol);
                }
            }
            counter = 0;
            foreach (var s in seeds)
            {
                Console.WriteLine($"Cleaning up Seeds: {++counter} / {seeds.Count}");
                s.CleanUp();
            }
            // clean up lost surfaces
            var outsiders = env.Meshes.Where(m => walls.Any(w => w.IsOpposite(Center, m.Center))).ToList();
            foreach (var o in outsiders)
            {
                env.Meshes.Remove(o);
            }

            //var fixes = 0;
            //foreach(var s in seeds)
            //{
            //    var tofix = seeds.Where(s2 => s2.Id != s.Id && !s.Surfaces.Any(f => f.Value.IsOpposite(s.Center, s2.Center))).ToList();
            //    foreach(var fix in tofix)
            //    {
            //        var surf = new VoronoiSurface(0.5f*(s.Center + fix.Center), Vector3.Normalize(fix.Center - s.Center));
            //        s.Surfaces.Add(fix.Id, surf);
            //        surf.R = 0.1f;
            //        surf.Render();
            //        surf.Polygon.Material = "red";
            //        env.Meshes.Add(surf.Polygon);
            //        fixes++;
            //    }
            //}
            ClearLine();
            if (config.Pillars)
            {
                env.Meshes.AddRange(Pillars());
            }
            if (config.GridSegments > 0)
            {
                MakeHoneycomb();
            }
            //var supportline = PolygonSheet.Line(new Vector3(), new Vector3(0, 0, 0.037f), config.WallThickness, 0, 6);
            //var bottom = walls.First(w => w.Name == "base");
            //supportline.Material = "red";
            //supportline.Move(bottom.BaseVerts[3]);
            //env.Meshes.Add(supportline);
            if (config.RoofBorder)
            {
                RoofBorder();
            }

        }
        private void MakeHoneycomb()
        {
            var gridheight = config.MaxDist * 1.005f;
            var topseg = walls.First(w => w.Name == "topseg");
            var gridwidth = Vector3.Distance(topseg.BaseVerts[1], topseg.BaseVerts[0]);
            var phi = 2 * (float)Math.PI / config.OuterWalls;
            var angle = 0f;
            var gridseg = config.GridSegments;
            var edgelength = gridwidth / gridseg / (float)Math.Cos(Math.PI / 6);
            var bottom = walls.First(w => w.Name == "base");
            foreach (var w in walls.Where(w => w.Name == "side"))
            {
                var grid = SurfGrid(gridseg, gridwidth, gridheight, out var resheight);
                grid.ForEach(g =>
                {
                    g.Move(w.Center);
                    g.Rotate(w.Center, new Vector3(0, 1, 0), (float)Math.PI / 2f);
                    g.Rotate(w.Center, new Vector3(1, 0, 0), (float)Math.PI / 2f);
                    g.Move(new Vector3(0, 0, (resheight - gridheight) / 2f));
                    g.Move(new Vector3(0, 0, -edgelength));
                    g.Rotate(w.Center, new Vector3(0, 0, 1), angle);
                    if (bottom.IsOpposite(g.Center + new Vector3(0, 0, 0.001f), Center))
                    {
                        g.Hide = true;
                    }
                    if (topseg.IsOpposite(g.Center - new Vector3(0, 0, 0.001f), Center))
                    {
                        g.Hide = true;
                    }
                    if (Math.Abs(g.Center.Z) < 0.003f)
                    {
                        g.Move(new Vector3(0, 0, 0.25f * g.Thickness));
                        g.Thickness = 0.5f * g.Thickness;
                    }
                    if (Math.Abs(g.Center.Z - config.MaxDist) < 0.003f)
                    {
                        g.Move(new Vector3(0, 0, -0.25f * g.Thickness));
                        g.Thickness = 0.5f * g.Thickness;
                    }
                });
                angle += phi;
                grid = grid.Where(g => !g.Hide).ToList();
                env.Meshes.AddRange(grid);
            }
        }
        private void RoofBorder()
        {
            var phi = 2 * (float)Math.PI / config.OuterWalls;
            var angle = 0f;
            foreach (var w in walls.Where(w => w.Name == "topseg"))
            {
                var len = Vector3.Distance(w.BaseVerts[0], w.BaseVerts[1]);
                var line = PolygonSheet.Regular(4, (float)(config.WallThickness * Math.Sqrt(2)), len, config.WallThickness / 2f);
                line.Rotate(line.Center, new Vector3(0, 0, 1), angle + (float)(Math.PI / config.OuterWalls));
                var mid = (w.BaseVerts[0] + w.BaseVerts[1]) / 2f;
                line.Material = "grey";
                line.Move(mid - new Vector3(0,0, 0.5f*config.WallThickness));
                line.Rotate(line.Center, mid - w.BaseVerts[2], (float)(Math.PI / 2));
                //var line = PolygonSheet.Line(w.BaseVerts[0], w.BaseVerts[1], config.WallThickness, 0, 4);
                //var lineang = (float)Vector3Util.Angle(line.BaseVerts[2] - line.BaseVerts[0], new Vector3(0, 0, 1));
                //line.Rotate(line.Center, line.Normal, lineang + (float)(Math.PI / 4));
                env.Meshes.Add(line);
                angle += phi;
            }
            if (!config.RoofLineSupport) return;
            foreach (var w in walls.Where(w => w.Name == "topseg"))
            {
                var mid = (w.BaseVerts[0] + w.BaseVerts[1]) / 2f;
                var len = Vector3.Distance(w.BaseVerts[0], w.BaseVerts[1])/2f;
                var left = w.BaseVerts[0] + new Vector3(0, 0, -len);
                var right = w.BaseVerts[1] + new Vector3(0, 0, -len);
                //var leftline = PolygonSheet.Regular(config.OuterWalls,
                //    (float)(config.WallThickness * Math.Sqrt(2)),
                //    (float)(len * Math.Sqrt(2)));
                var leftline = PolygonSheet.Line(left, mid, config.WallThickness * 0.5f);
                leftline.Material = "blue";
                env.Meshes.Add(leftline);
                var rightline = PolygonSheet.Line(mid, right, config.WallThickness * 0.5f);
                rightline.Material = "blue";
                env.Meshes.Add(rightline);
                var leftmid = (w.BaseVerts[0] + mid) / 2;
                var leftleftbase = w.BaseVerts[0] + new Vector3(0, 0, -0.5f * len);
                var leftleft = PolygonSheet.Line(leftleftbase, leftmid, config.WallThickness * 0.5f);
                leftleft.Material = "blue";
                env.Meshes.Add(leftleft);
                var rightmid = (w.BaseVerts[1] + mid) / 2;
                var rightrightbase = w.BaseVerts[1] + new Vector3(0, 0, -0.5f * len);
                var rightright = PolygonSheet.Line(rightrightbase, rightmid, config.WallThickness * 0.5f);
                rightright.Material = "blue";
                env.Meshes.Add(rightright);
                var llm = leftmid * 0.5f + w.BaseVerts[0] * 0.5f;
                var llb = leftmid * 0.5f + leftleftbase * 0.5f;
                var llv = PolygonSheet.Line(llm, llb, config.WallThickness * 0.5f);
                llv.Material = "blue";
                env.Meshes.Add(llv);
                var lrv = new PolygonSheet(llv);
                lrv.Move(leftmid - w.BaseVerts[0]);
                env.Meshes.Add(lrv);
                var rrm = rightmid * 0.5f + w.BaseVerts[1] * 0.5f;
                var rrb = rightmid * 0.5f + rightrightbase * 0.5f;
                var rrv = PolygonSheet.Line(rrm, rrb, config.WallThickness * 0.5f);
                rrv.Material = "blue";
                env.Meshes.Add(rrv);
                var rlv = new PolygonSheet(rrv);
                rlv.Move(rightmid - w.BaseVerts[1]);
                env.Meshes.Add(rlv);
            }
        }
        private void ClearLine()
        {
            var n = 100;
            for(var k = 0; k < n; k++)
            {
                var phi = 2.0 * Math.PI * k / n;
                var offset = 0.015f * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0);
                var p1 = new Vector3(0.5f * config.MaxDist, 0.5f * config.MaxDist, 0) + offset;
                var p2 = new Vector3(0.5f * config.MaxDist, 0.5f * config.MaxDist, 0.5f * config.MaxDist) + offset;
                var line = PolygonSheet.Line(p1, p2);
                line.Material = "red";
                //env.Meshes.Add(line);
                foreach (var s in seeds)
                {
                    foreach (var p in s.Surfaces.Values)
                    {
                        if (p.Polygon.LineItersects(p1, p2))
                        {
                            p.Polygon.Material = "redop";
                            p.Polygon.Hide = true;
                        }
                    }
                }

            }
            
        }
        public List<I3dObject> Pillars()
        {
            var r = config.MaxDist * 0.5f;
            var res = new List<I3dObject>();
            var alpha = Math.PI / config.OuterWalls;
            var R = r / (float)Math.Cos(alpha);
            for(var k = 0; k < config.OuterWalls; k++)
            {
                var phi = (double)k / config.OuterWalls * Math.PI * 2;
                var v = new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0);
                var pillar = PolygonSheet.Regular(config.OuterWalls,
                    config.WallThickness,
                    config.MaxDist - config.RoofHeight,
                    0.5f * config.WallThickness * 1.41f);
                pillar.Material = "darkgrey";
                pillar.Move((R /*- config.WallThickness*/)*v + Center - new Vector3(0,0,config.RoofHeight*0.5f));
                pillar.Rotate(Center, new Vector3(0, 0, 1), -(float)alpha);
                res.Add(pillar);
                if (config.RoofLines)
                {
                    var roofLine = PolygonSheet.Line(
                        new Vector3(0.5f * config.MaxDist, 0.5f * config.MaxDist, config.MaxDist),
                        new Vector3(pillar.Center.X, pillar.Center.Y, config.MaxDist - config.RoofHeight),
                        config.WallThickness, 0.5f * config.WallThickness * 1.41f, config.OuterWalls);
                    roofLine.Material = "darkgrey";
                    var sp = new Sphere(config.WallThickness);
                    sp.Material = "darkgrey";
                    sp.Move(new Vector3(pillar.Center.X, pillar.Center.Y, config.MaxDist - config.RoofHeight));
                    res.Add(sp);
                    res.Add(roofLine);
                }
            }
            return res;
        }
        public List<I3dObject> Floor()
        {
            var res = new List<I3dObject>();
            var alpha = Math.PI / config.OuterWalls;
            var r = config.MaxDist * 0.5f;
            var R = r / (float)Math.Cos(alpha);
            for (var k = 0; k < config.OuterWalls; k++)
            {
                var phi = (double)k / config.OuterWalls * Math.PI * 2;
                var phi2 = (double)(k+1) / config.OuterWalls * Math.PI * 2;
                var v1 = new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0);
                var v2 = new Vector3((float)Math.Cos(phi2), (float)Math.Sin(phi2), 0);
                var segVerts = new List<Vector3>() { v1 * R, v2 * R, v2 * 0.5f * R, v1 * 0.5f * R };
                var hole = (v1 + v2) * 0.5f * 0.75f * R;
                var floorSeg = new HoleSheet(segVerts, hole, 0.005f, config.WallThickness);
                //var floorSeg = new PolygonSheet(new[] { v1 * R, v2 * R, v2 * 0.5f * R, v1 * 0.5f * R }, config.WallThickness);
                floorSeg.Move(new Vector3(0.5f * config.MaxDist, 0.5f * config.MaxDist, 0.5f*config.WallThickness));
                floorSeg.Rotate(Center, new Vector3(0, 0, 1), -(float)alpha);
                floorSeg.Material = "darkgrey";
                res.Add(floorSeg);
            }
            return res;
        }
        public List<PolygonSheet> Walls()
        {
            if(config.OuterWalls < 3)
            {
                throw new Exception("Must have at least 3 outer walls");
            }
            var result = new List<PolygonSheet>();
            var r = config.MaxDist * 0.5f;
            var R = r / (float)Math.Cos(Math.PI / config.OuterWalls);
            //var r = R * (float)Math.Cos(Math.PI / config.OuterWalls);
            var l = r;
            if(config.OuterWalls == 3)
            {
                l = 2 * (float)Math.Sqrt(R * R - r * r);
            }
            //var R = (float)(r / Math.Cos(Math.PI/config.OuterWalls));
            for (var k = 0; k < config.OuterWalls; k++)
            {
                var phi = (double)k / config.OuterWalls * Math.PI * 2;
                var v = new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0);
                var wall = PolygonSheet.Regular(4, (float)(l * Math.Sqrt(2)), config.WallThickness);
                wall.Rotate(wall.Center, new Vector3(0, 0, 1), (float)(phi + Math.PI / 4));
                wall.Rotate(wall.Center, Vector3.Cross(v, new Vector3(0, 0, 1)), (float)(Math.PI / 2.0));
                wall.Move(r*v);
                wall.Material = "yellowop";
                wall.Name = "side";
                result.Add(wall);
            }
            var top = PolygonSheet.Regular(config.OuterWalls, R, config.WallThickness);
            top.Move(new Vector3(0, 0, config.MaxDist * 0.5f));
            result.Add(top);
            var bot = PolygonSheet.Regular(config.OuterWalls, R, config.WallThickness);
            bot.Move(new Vector3(0, 0, -config.MaxDist * 0.5f));
            bot.Name = "base";
            double ang = Vector3Util.Angle(top.BaseVerts[0] - top.Center, new Vector3(-R, top.Center.Y, top.Center.Z));
            if (config.OuterWalls % 2 == 0)
            {
                ang += Math.PI / config.OuterWalls;
            }
            top.Rotate(top.Center, new Vector3(0, 0, 1), (float)ang);
            top.Rotate(top.Center, new Vector3(1, 0, 0), (float)Math.PI);
            bot.Rotate(bot.Center, new Vector3(0, 0, 1), (float)ang);
            result.Add(bot);
            for (var k = 0; k < config.OuterWalls; k++)
            {
                var w1 = result[k];
                var w2 = result[(k + 1) % config.OuterWalls];
                //top.SurfaceIntersect(w1);
                //bot.SurfaceIntersect(w1);
                w1.PlaneIntersect(w2);
                w2.PlaneIntersect(w1);
                if(config.OuterWalls == 3)
                {
                    w1.PlaneIntersect(top);
                    w1.PlaneIntersect(bot);
                }
            }
            foreach(var w in result)
            {
                w.Move(new Vector3(r));
            }
            return result;
        }
        public List<PolygonSheet> WallsRoof()
        {
            if (config.OuterWalls < 3)
            {
                throw new Exception("Must have at least 3 outer walls");
            }
            var result = new List<PolygonSheet>();
            var r = config.MaxDist * 0.5f;
            var R = r / (float)Math.Cos(Math.PI / config.OuterWalls);
            //var r = R * (float)Math.Cos(Math.PI / config.OuterWalls);
            var l = r;
            if (config.OuterWalls == 3)
            {
                l = 2 * (float)Math.Sqrt(R * R - r * r);
            }
            //var R = (float)(r / Math.Cos(Math.PI/config.OuterWalls));
            for (var k = 0; k < config.OuterWalls; k++)
            {
                var phi = (double)k / config.OuterWalls * Math.PI * 2;
                var v = new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0);
                var wall = PolygonSheet.Regular(4, (float)(l * Math.Sqrt(2)), config.WallThickness);
                wall.Name = "side";
                wall.Rotate(wall.Center, new Vector3(0, 0, 1), (float)(phi + Math.PI / 4));
                wall.Rotate(wall.Center, Vector3.Cross(v, new Vector3(0, 0, 1)), (float)(Math.PI / 2.0));
                wall.Move(r * v);
                wall.Material = "yellowop";

                result.Add(wall);
            }
            var top = PolygonSheet.Regular(config.OuterWalls, R, config.WallThickness);
            top.Move(new Vector3(0, 0, config.MaxDist * 0.5f));
            //result.Add(top);
            var bot = PolygonSheet.Regular(config.OuterWalls, R, config.WallThickness);
            bot.Move(new Vector3(0, 0, -config.MaxDist * 0.5f));
            bot.Name = "base";
            double ang = Vector3Util.Angle(top.BaseVerts[0] - top.Center, new Vector3(-R, top.Center.Y, top.Center.Z));
            if (config.OuterWalls % 2 == 0)
            {
                ang += Math.PI / config.OuterWalls;
            }
            top.Rotate(top.Center, new Vector3(0, 0, 1), (float)ang);
            top.Rotate(top.Center, new Vector3(1, 0, 0), (float)Math.PI);
            bot.Rotate(bot.Center, new Vector3(0, 0, 1), (float)ang);
            result.Add(bot);
            for (var k = 0; k < config.OuterWalls; k++)
            {
                var w1 = result[k];
                var w2 = result[(k + 1) % config.OuterWalls];
                //top.SurfaceIntersect(w1);
                //bot.SurfaceIntersect(w1);
                w1.PlaneIntersect(w2);
                w2.PlaneIntersect(w1);
                if (config.OuterWalls == 3)
                {
                    w1.PlaneIntersect(top);
                    w1.PlaneIntersect(bot);
                }
            }
            var topMid = new Vector3(0, 0, config.MaxDist * 0.5f);
            for(var k = 0; k < top.BaseVerts.Count; k++)
            {
                var v1 = top.BaseVerts[k] + new Vector3(0,0, -config.RoofHeight);
                var v2 = top.BaseVerts[(k + 1) % top.BaseVerts.Count] + new Vector3(0, 0, -config.RoofHeight);
                var topseg = new PolygonSheet(new[] { v1, v2, topMid }, config.WallThickness);
                topseg.Name = "topseg";
                topseg.Material = "yellowop";
                result.Add(topseg);
            }
            foreach (var w in result)
            {
                w.Move(new Vector3(r));
            }
            return result;
        }
        public List<PolygonSheet> SurfGrid(int div, float w, float h, out float resheight)
        {
            var res = new List<PolygonSheet>();
            // TODO: create honeycomb cover surface for outside walls
            var Points = new List<Vector2>();
            var cellwidth = w / div;
            var edgelength = 0.5f * cellwidth / (float)Math.Cos(Math.PI/6);
            float yoff = 0;
            for (var l = 0; l < h / 3 / edgelength; l++)
            {
                yoff = l * 3 * edgelength;
                for (var k = 0; k < div; k++)
                {
                    var start = new Vector3(0.5f * cellwidth + k * cellwidth, yoff, 0);
                    var mid = new Vector3(0.5f * cellwidth + k * cellwidth, yoff + edgelength, 0);
                    var vertical = GridLine(start, mid);
                    res.Add(vertical);

                    var leftend = new Vector3(k * cellwidth, yoff + 1.5f * edgelength, 0);
                    var left = GridLine(leftend, mid);
                    res.Add(left);

                    var rightend = new Vector3((k + 1) * cellwidth, yoff + 1.5f * edgelength, 0);
                    var right = GridLine(mid, rightend);
                    res.Add(right);

                    var lefttop = new Vector3(k * cellwidth, yoff + 2.5f * edgelength, 0);
                    var leftvert = GridLine(leftend, lefttop);
                    res.Add(leftvert);

                    var topmid = new Vector3((k + 0.5f) * cellwidth, yoff + 3f * edgelength, 0);
                    var leftroof = GridLine(lefttop, topmid);
                    res.Add(leftroof);

                    var righttop = new Vector3((k + 1) * cellwidth, yoff + 2.5f * edgelength, 0);
                    var rightroof = GridLine(topmid, righttop);
                    res.Add(rightroof);
                }
            }
            resheight = yoff + 3 * edgelength;
            var rh = resheight;
            res.ForEach(r => r.Move(new Vector3(-w / 2, -rh / 2, 0)));
            return res;
        }
        private PolygonSheet GridLine(Vector3 start, Vector3 end)
        {
            var line = PolygonSheet.Line(start, end, config.WallThickness, 0.5f * config.WallThickness, 4);
            var angle = (float)Vector3Util.Angle(line.BaseVerts[1] - line.BaseVerts[0], new Vector3(0, 0, 1));
            line.Rotate(line.Center, start - end, angle);
            
            line.Material = "grey";
            return line;
        }
        private Vector3 Center => new Vector3(config.MaxDist * 0.5f);
    }
}
