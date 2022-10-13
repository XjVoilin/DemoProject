using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LIBII.Light2D
{
    public class PolygonTriangulator2D : MonoBehaviour
    {
        public enum Triangulation
        {
            Advanced,
            Legacy
        };

        public static Mesh TriangulateSimple(Polygon2D polygon, Vector2 UVScale, Vector2 UVOffset)
        {
            Mesh result = null;

            //result = TriangulateAdvanced(polygon, UVScale, UVOffset);

            polygon.Normalize();

            Polygon2 poly = new Polygon2(polygon);
            result = UnityDefaultTriangulator.Create(poly.points);

            return (result);
        }


        public static Mesh Triangulate(Polygon2D polygon, Vector2 UVScale, Vector2 UVOffset,
            Triangulation triangulation)
        {
            Mesh result = null;

            polygon.Normalize();

            result = TriangulateAdvanced(polygon, UVScale, UVOffset);

            return (result);
        }

        public static Mesh TriangulateAdvanced(Polygon2D polygon, Vector2 UVScale, Vector2 UVOffset)
        {
            TriangulationWrapper.Polygon poly = new TriangulationWrapper.Polygon();

            List<Vector2> pointsList = null;
            List<Vector2> UVpointsList = null;

            Vector3 v = Vector3.zero;

            foreach (Vector2D p in polygon.pointsList)
            {
                v = p.ToVector2();
                poly.outside.Add(v);
                poly.outsideUVs.Add(new Vector2(v.x / UVScale.x + .5f + UVOffset.x,
                    v.y / UVScale.y + .5f + UVOffset.y));

                // poly.outsideUVs.Add(Vector2.zero);
            }

            foreach (Polygon2D hole in polygon.holesList)
            {
                pointsList = new List<Vector2>();
                UVpointsList = new List<Vector2>();

                foreach (Vector2D p in hole.pointsList)
                {
                    v = p.ToVector2();
                    pointsList.Add(v);
                    UVpointsList.Add(new Vector2(v.x / UVScale.x + .5f, v.y / UVScale.y + .5f));

                    // UVpointsList.Add(Vector2.zero);
                }

                poly.holes.Add(pointsList);
                poly.holesUVs.Add(UVpointsList);
            }

            return (TriangulationWrapper.CreateMesh(poly));
        }
    }

    public class PolygonTriangulator2 : MonoBehaviour
    {
        public enum Triangulation
        {
            Advanced,
            Legacy
        };

        public static Mesh TriangulateSimple(Polygon2 polygon, Vector2 UVScale, Vector2 UVOffset)
        {
            Mesh result = null;

            //result = TriangulateAdvanced(polygon, UVScale, UVOffset);

            polygon.Normalize();

            result = UnityDefaultTriangulator.Create(polygon.points);

            return (result);
        }


        public static Mesh Triangulate(Polygon2 polygon, Vector2 UVScale, Vector2 UVOffset, Triangulation triangulation,
            bool keepHoleTriangles = false)
        {
            Mesh result = null;

            polygon.Normalize();

            result = TriangulateAdvanced(polygon, UVScale, UVOffset, keepHoleTriangles);

            return (result);
        }

        public static Mesh TriangulateAdvanced(Polygon2 polygon, Vector2 UVScale, Vector2 UVOffset,
            bool keepHoleTriangles = false)
        {
            TriangulationWrapper.Polygon poly = new TriangulationWrapper.Polygon();

            List<Vector2> pointsList = null;
            List<Vector2> UVpointsList = null;

            Vector3 v = Vector3.zero;

            for (var index = 0; index < polygon.points.Length; index++)
            {
                Vector2 p = polygon.points[index];
                poly.outside.Add(p);
                // poly.outsideUVs.Add(new Vector2(v.x / UVScale.x + .5f + UVOffset.x,
                // v.y / UVScale.y + .5f + UVOffset.y));

                poly.outsideUVs.Add(Vector2.one * .5f * polygon.strengthes[index]);
            }

            if (polygon.holes == null)
            {
                polygon.holes = new Polygon2[0];
            }

            foreach (Polygon2 hole in polygon.holes)
            {
                pointsList = new List<Vector2>();
                UVpointsList = new List<Vector2>();

                for (var index = 0; index < hole.points.Length; index++)
                {
                    Vector2 p = hole.points[index];
                    pointsList.Add(p);
                    //UVpointsList.Add (new Vector2(v.x / UVScale.x + .5f, v.y / UVScale.y + .5f));

                    UVpointsList.Add(Vector2.one  * .5f* hole.strengthes[index]);
                }

                poly.holes.Add(pointsList);
                poly.holesUVs.Add(UVpointsList);
            }

            return (TriangulationWrapper.CreateMesh(poly, keepHoleTriangles));
        }
    }
}