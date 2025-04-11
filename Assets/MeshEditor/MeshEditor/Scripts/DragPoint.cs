using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEdit
{
    public class DragPoint : MonoBehaviour
    {
        // Action that takes the new position and the list of vertices indices
        public Action<Vector3, MeshEditor.Point> ResetPoint;

        public MeshEditor.Point point;

        private Vector3 offset; // To store the initial offset between the object and mouse position
        private Camera mainCamera; // Reference to the main camera for converting screen coordinates

        void Start()
        {
            // Get reference to the main camera
            mainCamera = Camera.main;
        }

        private void OnMouseDown()
        {
            // Calculate the offset between the object's position and the mouse position in world coordinates
            offset = transform.position - GetMouseWorldPosition();
        }

        private void OnMouseDrag()
        {
            // Update the position of the object while dragging, considering the initial offset
            transform.position = GetMouseWorldPosition() + offset;


            // When the mouse button is released, call the ResetPoint function with the new position and indices
            ResetPoint(transform.localPosition, point);
        }

        private void OnMouseUp()
        {
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