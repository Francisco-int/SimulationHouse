using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MeshEdit
{
    public class MeshEditor : MonoBehaviour
    {
        [SerializeField] private GameObject dragPointTemplate;
        [SerializeField] private GameObject dragLineTemplate;
        [SerializeField] private GameObject dragFaceTemplate;

        [SerializeField] private float pointSize = 0.1f;
        [SerializeField] private float edgeThickness = 1f;

        [SerializeField] private bool pointDraggable = true;
        [SerializeField] private bool edgeDraggable = true;
        [SerializeField] private bool planeDraggable = false;

        private Dictionary<Vector3, Point> points = new();
        private Dictionary<EdgePoints, Edge> edges = new();
        private List<Plane> planes = new();
        private Mesh mesh;

        private Dictionary<string, EdgePoints> indexEdges = new();
        private Dictionary<Vector3, List<Vector3>> normals = new();

        public class EdgePoints
        {
            public Point pointA;
            public Point pointB;
            public Vector3 normal;
        }

        public class Point
        {
            public Vector3 position;
            public List<int> indices;
            public GameObject go;
            public string name;
        }

        public class Edge
        {
            public List<Point> points;
            public GameObject go;
        }

        public class Plane
        {
            public List<Point> points;
            public List<Edge> Edges;
            public GameObject go;
        }


        private void Start()
        {
            var meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;
            if (meshFilter != null)
            {
                InitPoints();
                InitEdges();
                UpdateAllPlanes();
            }
        }

        private void InitPoints()
        {
            var vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (!points.ContainsKey(vertices[i]))
                {
                    // Instantiate GameObject
                    GameObject go = Instantiate(dragPointTemplate, transform);
                    go.transform.localPosition = vertices[i];
                    go.name = i.ToString();
                    points[vertices[i]] = new Point
                        { position = vertices[i], indices = new List<int> { i }, go = go, name = i.ToString() };
                    go.GetComponent<DragPoint>().ResetPoint = ResetPoint;
                    go.GetComponent<DragPoint>().point = points[vertices[i]];
                    go.transform.localScale = Vector3.one * pointSize;
                    go.SetActive(pointDraggable);
                }
                else
                {
                    points[vertices[i]].indices.Add(i);
                }
            }
        }

        private GameObject AddVisibleEdge(EdgePoints edge)
        {
            GameObject go = Instantiate(dragLineTemplate, transform);
            Vector3 vertex1 = edge.pointA.position;
            Vector3 vertex2 = edge.pointB.position;
            Vector3 mid = (vertex1 + vertex2) / 2;
            go.transform.localPosition = mid;
            // Calculate the direction from vertex1 to vertex2
            Vector3 direction = vertex2 - vertex1;

            // Set the rotation of the cylinder to face the direction of the line
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);

            // Calculate the distance between the two vertices
            float distance = Vector3.Distance(vertex1, vertex2);

            // Set the scale of the cylinder to match the distance
            Vector3 newScale = go.transform.localScale * edgeThickness;
            newScale.y = distance / 2; // Assuming the cylinder's height is along the Y-axis
            go.transform.localScale = newScale;
            go.name = $"({edge.pointA.name},{edge.pointB.name})";

            var dragLine = go.GetComponent<DragLine>();
            dragLine.ResetPoint = ResetPoint;
            dragLine.pointA = edge.pointA;
            dragLine.pointB = edge.pointB;
            go.SetActive(edgeDraggable);

            return go;
        }

        private GameObject AddVisibleNormal(Vector3 position, Vector3 normal, List<Vector3> vecs)
        {
            GameObject go = Instantiate(dragFaceTemplate, transform);
            go.transform.localPosition = position;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal.normalized);
            go.name = normal.ToString();

            var dragFace = go.GetComponent<DragFace>();
            List<Point> list = new();
            foreach (var vec in vecs)
            {
                list.Add(points[vec]);
            }

            Plane plane = new Plane
            {
                points = list,
                go = go
            };
            dragFace.SetPoints(list);
            dragFace.ResetPlane = ResetPlane;
            planes.Add(plane);
            go.SetActive(planeDraggable);
            return go;
        }

        private void UpdateVisibleNormal(Plane plane, Vector3 position, Vector3 normal)
        {
            var go = plane.go;
            go.transform.localPosition = position;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal.normalized);
            go.name = normal.ToString();

            go.GetComponent<DragFace>().ResetPoints();
        }

        void InitEdges()
        {
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var normals = mesh.normals;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 faceNormal = (normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]) /
                                     3;

                AddEdge(triangles[i], triangles[i + 1], faceNormal, vertices);
                AddEdge(triangles[i + 1], triangles[i + 2], faceNormal, vertices);
                AddEdge(triangles[i + 2], triangles[i], faceNormal, vertices);

                if (this.normals.TryGetValue(faceNormal, out _))
                {
                    this.normals[faceNormal].Add(vertices[triangles[i]]);
                    this.normals[faceNormal].Add(vertices[triangles[i + 1]]);
                    this.normals[faceNormal].Add(vertices[triangles[i + 2]]);
                }
                else
                {
                    this.normals.Add(faceNormal,
                        new List<Vector3>
                            { vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]] });
                }
            }

            foreach (var pos in this.normals.Keys)
            {
                List<Vector3> vecs = new List<Vector3>(new HashSet<Vector3>(this.normals[pos]));
                Vector3 sum = Vector3.zero;
                foreach (var vec in vecs)
                {
                    sum += vec;
                }

                sum /= points.Count;

                AddVisibleNormal(sum, pos, vecs);
            }
        }

        void AddEdge(int index1, int index2, Vector3 normal, Vector3[] vertices)
        {
            string edgeKey = index1 < index2 ? $"{index1}-{index2}" : $"{index2}-{index1}";

            Point pointA = points[vertices[index1]];
            Point pointB = points[vertices[index2]];

            EdgePoints edgePoints = new EdgePoints { pointA = pointA, pointB = pointB, normal = normal };
            EdgePoints reverseEdgePoints = new EdgePoints { pointA = pointB, pointB = pointA, normal = normal };
            bool edge1 = edges.TryGetValue(edgePoints, out _);
            bool edge2 = edges.TryGetValue(reverseEdgePoints, out _);

            if (indexEdges.TryGetValue(edgeKey, out EdgePoints data))
            {
                if (Vector3.Dot(data.normal, normal) < 0.99f)
                {
                    indexEdges[edgeKey] = edgePoints;
                }
                else
                {
                    Destroy(edges[data].go);
                    edges.Remove(edgePoints);
                    edges.Remove(data, out _);
                    indexEdges.Remove(edgeKey);
                }
            }
            else
            {
                indexEdges.Add(edgeKey, edgePoints);
                if (!(edge1 || edge2))
                {
                    GameObject go = AddVisibleEdge(edgePoints);
                    edges.Add(edgePoints,
                        new Edge { points = new List<Point> { edgePoints.pointA, edgePoints.pointB }, go = go });
                }
            }
        }

        public void ResetPosition(Vector3 pos, List<int> indices)
        {
            foreach (var index in indices)
            {
                Point point = points[mesh.vertices[index]];

                // Pass the point by reference to ResetPoint
                ResetPoint(pos, point);
            }
        }

        public void ResetPoint(Vector3 pos, Point point)
        {
            var newVertices = mesh.vertices;
            var newTriangles = mesh.triangles;
            foreach (var index in point.indices)
            {
                newVertices[index] = pos;
            }

            if (points.TryAdd(pos, point))
            {
                points.Remove(point.position);
            }

            point.go.transform.localPosition = pos;
            point.position = pos;

            mesh.Clear();

            mesh.vertices = newVertices;
            mesh.triangles = newTriangles;


            mesh.RecalculateNormals();
            ResetAll();
        }

        public void ResetLine(Vector3 position1, Vector3 position2, Point point1, Point point2)
        {
            ResetPosition(position1, point1.indices);
            ResetPosition(position2, point2.indices);
            ResetAll();
        }

        public void ResetPlane(List<Vector3> positions, List<Point> points)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                ResetPosition(positions[i], points[i].indices);
            }

            ResetAll();
        }

        public void UpdateAllEdges()
        {
            foreach (var edge in edges)
            {
                GameObject go = edge.Value.go;
                Vector3 vertex1 = edge.Value.points[0].position;
                Vector3 vertex2 = edge.Value.points[1].position;
                Vector3 mid = (vertex1 + vertex2) / 2;
                go.transform.localPosition = mid;
                // Calculate the direction from vertex1 to vertex2
                Vector3 direction = vertex2 - vertex1;

                // Set the rotation of the cylinder to face the direction of the line
                go.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);

                // Calculate the distance between the two vertices
                float distance = Vector3.Distance(vertex1, vertex2);

                // Set the scale of the cylinder to match the distance
                Vector3 newScale = go.transform.localScale * edgeThickness;
                newScale.y = distance / 2;
                go.transform.localScale = newScale;
            }
        }

        public void UpdateAllPlanes()
        {
            foreach (var plane in planes)
            {
                Vector3 sum = Vector3.zero;
                foreach (var point in plane.points)
                {
                    sum += point.position;
                }

                sum /= plane.points.Count;

                Vector3 normalSum = Vector3.zero;

                for (int i = 1; i < plane.points.Count - 1; i++)
                {
                    // Create two vectors using the first point and two consecutive points
                    Vector3 vector1 = plane.points[i].position - plane.points[0].position;
                    Vector3 vector2 = plane.points[i + 1].position - plane.points[0].position;

                    // Compute the cross product of the two vectors
                    Vector3 normal = Vector3.Cross(vector1, vector2);

                    // Add the normal to the sum
                    normalSum += normal.normalized;
                }

                UpdateVisibleNormal(plane, sum, normalSum.normalized);
            }

        }

        public void ResetAll()
        {
            UpdateAllEdges();
            UpdateAllPlanes();
        }
    }
}