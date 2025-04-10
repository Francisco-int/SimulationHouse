using UnityEngine;

public class SimpleFurnitureSpawner : MonoBehaviour
{
    // Prefab del mueble a instanciar.
    public GameObject furniturePrefab;
    // Punto de spawn, por ejemplo la posición del controlador.
    public Transform spawnPoint;

    // Este método se llamará desde el botón de la UI.
    public void SpawnFurniture(GameObject furniture)
    {
        // Instancia el prefab en la posición y rotación del spawnPoint.
        GameObject newFurniture = Instantiate(furniture, spawnPoint.position, spawnPoint.rotation);

        // Opcional: Si deseas que el objeto se "agarre" de inmediato,
        // puedes asignarlo como hijo del spawnPoint (por ejemplo, el controlador).
        // newFurniture.transform.SetParent(spawnPoint);
    }
}
