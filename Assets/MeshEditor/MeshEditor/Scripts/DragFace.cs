using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MeshEdit
{
    public class DragFace : MonoBehaviour
    {
        public Action<List<Vector3>, List<MeshEditor.Point>> ResetPlane;
        public List<MeshEditor.Point> points = new List<MeshEditor.Point>();
        public Dictionary<MeshEditor.Point, GameObject> vertices = new();

        private Vector3 offset; // To store the initial offset between the object and mouse position
        private Vector3 startLocalPosition; // To store the initial local position of the plane
        private Camera mainCamera; // Reference to the main camera for converting screen coordinates

        void Start()
        {
            mainCamera = Camera.main;
        }

        public void SetPoints(List<MeshEditor.Point> points)
        {
            foreach (var point in points.ToList())
            {
                var vertex = new GameObject();
                vertex.transform.parent = transform;
                vertex.transform.localPosition = point.position;
                this.points.Add(point);
                vertices.Add(point, vertex);
            }
        }

        public void ResetPoints()
        {
            foreach (var pointVecPair in vertices)
            {
                pointVecPair.Value.transform.localPosition = pointVecPair.Key.position;
            }
        }

        private void OnMouseDown()
        {
            // Calculate the offset between the mouse position and the plane's local position
            offset = transform.localPosition - GetMouseWorldPosition();
            startLocalPosition = transform.localPosition;
        }

        private void OnMouseDrag()
        {
            // Calculate the new mouse position in world space and apply the offset
            Vector3 newMouseWorldPos = GetMouseWorldPosition();
            Vector3 newLocalPos = newMouseWorldPos + offset;

            // Calculate delta based on the difference between current and initial position
            Vector3 delta = newLocalPos - startLocalPosition;

            // Move each vertex based on the calculated delta
            foreach (var go in vertices.Values)
            {
                go.transform.localPosition += delta;
            }

            // Update the plane's position
            transform.localPosition = newLocalPos;

            // Update the start position for the next frame
            startLocalPosition = transform.localPosition;

            // After dragging is done, update the positions of all vertices and call the ResetPlane action
            List<Vector3> positions = new List<Vector3>();
            foreach (var go in vertices.Values)
            {
                positions.Add(go.transform.localPosition);
            }

            ResetPlane(positions, points);
        }

        // Helper method to get the mouse position in world coordinates
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = mainCamera.WorldToScreenPoint(transform.position).z; // Preserve Z distance
            return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        }
    }
}