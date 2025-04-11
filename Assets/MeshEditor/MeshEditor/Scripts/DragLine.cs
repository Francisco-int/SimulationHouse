using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEdit
{

    public class DragLine : MonoBehaviour
    {
        // Action that takes the new position and vertices indices for the line
        public Action<Vector3, MeshEditor.Point> ResetPoint;

        public MeshEditor.Point pointA, pointB;

        private Vector3 offset; // To store the initial offset between the object and mouse position
        private Camera mainCamera; // Reference to the main camera for converting screen coordinates

        void Start()
        {
            // Get reference to the main camera
            mainCamera = Camera.main;
        }

        private void OnMouseDown()
        {
            // Calculate the offset between the line's midpoint and the mouse position in world coordinates
            Vector3 midPoint = (pointA.go.transform.localPosition + pointB.go.transform.localPosition) / 2;
            offset = midPoint - GetMouseWorldPosition();
        }

        private void OnMouseDrag()
        {
            // Get the new midpoint of the line during drag
            Vector3 newMidPoint = GetMouseWorldPosition() + offset;

            // Calculate the movement delta
            Vector3 delta = newMidPoint - (pointA.go.transform.localPosition + pointB.go.transform.localPosition) / 2;

            // Move both vertices by the same delta
            pointA.go.transform.localPosition += delta;
            pointB.go.transform.localPosition += delta;

            // Update the position of the line (this object) to stay at the new midpoint
            transform.localPosition = (pointA.go.transform.localPosition + pointB.go.transform.localPosition) / 2;

            ResetPoint(pointA.go.transform.localPosition, pointA);
            ResetPoint(pointB.go.transform.localPosition, pointB);
        }

        // Helper method to get the mouse position in world coordinates
        private Vector3 GetMouseWorldPosition()
        {
            // Get the mouse position in screen space and convert it to world space
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = mainCamera.WorldToScreenPoint(transform.position).z; // Preserve z distance
            return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        }
    }
}