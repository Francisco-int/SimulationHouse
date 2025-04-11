using UnityEditor;
using UnityEngine;

namespace MeshEdit
{
#if UNITY_EDITOR

    public static class MeshEditorMounter
    {
        [MenuItem("GameObject/Mount MeshEditor", false, 1)]
        public static void AddMeshEditor()
        {
            GameObject selectedGO = Selection.activeGameObject;

            if (selectedGO == null)
            {
                Debug.Log("No GameObject Selected");
                return;
            }

            if (selectedGO.GetComponent<MeshEditor>())
            {
                Debug.Log("MeshEditor already exist under this GameObject");
                return;
            }

            if (!selectedGO.GetComponent<MeshFilter>())
            {
                Debug.Log("Mesh Editor can only be added under a GameObject which has a MeshFilter");
                return;
            }

            if (!selectedGO.GetComponent<MeshRenderer>())
            {
                Debug.Log("Mesh Editor can only be added under a GameObject which has a MeshRenderer");
                return;
            }

            selectedGO.AddComponent<MeshEditor>();
        }
    }

#endif
}