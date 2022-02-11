using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace VoronoiLamp
{
    public class VoronoiGenerator2
    {
        private List<VoronoiSeed> seeds;
        private Config config;
        private Environment env;

        public VoronoiGenerator2(Config config, Environment env)
        {
            this.env = env;
            this.config = config;
        }

        private void InitSeeds()
        {
            seeds = new List<VoronoiSeed>();
            if (config.SeedPositions != null && config.SeedPositions.Count > 0)
            {
                foreach (var v in config.SeedPositions)
                {
                    seeds.Add(new VoronoiSeed(config.SurfaceThickness, v));
                }
            }
            if (config.Seeds > seeds.Count)
            {
                for (var k = seeds.Count; k < config.Seeds; k++)
                {
                    seeds.Add(new VoronoiSeed(config.MaxDist));
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

        public void Run()
        {
            InitSeeds();
            GenerateSurfaces();
        }

        private void GenerateSurfaces()
        {
            var counter = 0;
            var nAreas = 2;
            foreach(var currentSeed in seeds)
            {
                counter++;
                var sortedRest = seeds.Where(s => s.Id != currentSeed.Id)
                    .OrderBy(s => Vector3.Distance(currentSeed.Center, s.Center))
                    .ToList();
                foreach(var other in sortedRest)
                {
                    currentSeed.R = Vector3.Distance(other.Center, currentSeed.Center);
                    var middle = 0.5f * (other.Center + currentSeed.Center);
                    if (!currentSeed.IsHiddenFrom(middle))
                    {
                        currentSeed.MakeSurface(other);
                    }
                }
                currentSeed.MakePolygons();
                // for testing
                //if(counter >= nAreas)break;
            }
        }
        public void Render()
        {
            var walls = Walls();
            foreach(var s in seeds)
            {
                var sphere = new Sphere(0.002f);
                sphere.Move(s.Center);
                sphere.Material = "redop";
                env.Meshes.Add(sphere);
                var polys = s.GetPolgons().Select(p =>
                {
                    foreach(var wall in walls)
                    {
                        p.PlaneIntersect(wall);
                    }
                    p.Material = "grey";
                    return p;
                });
                env.Meshes.AddRange(polys);
            }
            foreach(var seed in seeds)
            {
                foreach(var surf in seed.Surfaces.Values)
                {

                }
            }
        }


        private List<PolygonSheet> Walls()
        {
            var wall1 = Wall();
            wall1.Move(new Vector3(config.MaxDist, 0, 0));
            var wall2 = Wall();
            wall2.Rotate(wall2.Center, new Vector3(0, 0, 1f), (float)Math.PI);
            var wall3 = Wall();
            wall3.Rotate(new Vector3(), new Vector3(0, 0, 1f), (float)(1.5 * Math.PI));
            var wall4 = Wall();
            wall4.Rotate(new Vector3(0, config.MaxDist, 0), new Vector3(0, 0, 1f), (float)(Math.PI * 0.5));
            var wall5 = Wall();
            wall5.Rotate(new Vector3(), new Vector3(0, 1, 0), (float)(0.5 * Math.PI));
            var wall6 = new PolygonSheet(wall5);
            wall6.Rotate(new Vector3(config.MaxDist / 2f, config.MaxDist / 2f, config.MaxDist / 2f), new Vector3(0, 1, 0), (float)(Math.PI));
            return new List<PolygonSheet>() { wall1, wall2, wall3, wall4, wall5, wall6 };
        }
        private PolygonSheet Wall()
        {
            var verts = new List<Vector3>
            {
                new Vector3(),
                new Vector3(0f, config.MaxDist, 0),
                new Vector3(0f, config.MaxDist, config.MaxDist),
                new Vector3(0f, 0f, config.MaxDist)
            };
            var wall = new PolygonSheet(verts.ToArray(), 0.005f);
            wall.Material = "redop";
            return wall;
        }
    }
}
