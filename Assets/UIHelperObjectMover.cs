using UnityEngine;
using UnityEngine.UI;

public class UIHelperObjectMover : MonoBehaviour
{
    [Header("Referencia a la Cámara del Jugador")]
    [Tooltip("Cámara principal o el centro de visión del jugador.")]
    public Transform playerCamera;

    public static UIHelperObjectMover Instance { get; private set; }

    // Si se asigna targetObject, se aplicarán las operaciones a éste; de lo contrario se usará el GameObject donde está este script.
    [Header("Target Object (Opcional)")]
    [Tooltip("Si se asigna, todas las operaciones se aplicarán a este objeto; de lo contrario se usará este GameObject.")]
    public GameObject targetObject;

    // Propiedad para obtener el transform efectivo
    private Transform EffectiveTransform
    {
        get { return (targetObject != null) ? targetObject.transform : transform; }
    }

    public Text textDebug;
    public enum AxisSelect { None, X, Y, Z }

    [Header("Configuración de Movimiento del Objeto")]
    [Tooltip("Velocidad de movimiento horizontal (adelante, atrás, izquierda, derecha).")]
    public float moveSpeed = 2f;
    [Tooltip("Velocidad de movimiento vertical (subir y bajar).")]
    public float verticalSpeed = 2f;
    [Tooltip("Velocidad de rotación en grados por segundo.")]
    public float rotationSpeed = 90f;

    // Banderas internas
    private bool isMoving = false;       // Indica si se está manipulando el objeto
    private bool rotationMode = false;   // Indica si se está usando el modo rotación

    [Header("Referencia al OVRPlayerController")]
    [Tooltip("Arrastra el GameObject que contenga el componente OVRPlayerController (por ejemplo, el rig del jugador).")]
    public OVRPlayerController playerController;

    [Header("Sistema de Modificación (para creación de la casa)")]
    [Tooltip("Si es true, se permitirá modificar la escala del objeto mediante InputField y botones.")]
    public bool isModifiable = false;
    [Tooltip("InputField donde se mostrará y escribirá el valor numérico.")]
    public InputField scaleInputField;

    // Eje seleccionado para modificar la escala.
    [Tooltip("Indica cuál eje se está modificando (se selecciona con botones dedicados).")]
    public AxisSelect currentAxis = AxisSelect.None;

    // Tag que determinará si un objeto es modificable.
    [Tooltip("Si el objeto target tiene este tag, se considerará modificable.")]
    public string modificableTag = "Modificable";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita que haya más de una instancia
        }
    }

    #region Movimiento y Rotación

    /// <summary>
    /// Inicia el modo de manipulación asignando el objeto target.
    /// Además, verifica si el objeto posee el tag indicado para activarlo como modificable.
    /// Para salir del modo manipulación (isMoving), se presiona el botón SecondaryHandTrigger.
    /// </summary>
    /// <param name="target">Objeto a manipular.</param>
    public void ToggleMovement(GameObject target)
    {
        // Si se está iniciando el modo
        if (!isMoving)
        {
            targetObject = target;
            isMoving = true;
            // Verifica si el objeto tiene el tag indicado
            isModifiable = target.CompareTag(modificableTag);
            Debug.Log("Iniciando manipulación del objeto: " + target.name + " | Modificable: " + isModifiable);
            if (textDebug != null)
                textDebug.text = "Manipulación: " + target.name;
            if (playerController != null)
            {
                // Desactiva la rotación del jugador para evitar interferencias
                playerController.EnableRotation = false;
            }
        }
    }

    void Update()
    {
        // Salir del modo manipulación presionando el Secondary Hand Trigger
        if (isMoving && OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            isMoving = false;
            rotationMode = false;
            Debug.Log("Salida de modo manipulación.");
            if (textDebug != null)
                textDebug.text = "Manipulación finalizada.";
            if (playerController != null)
                playerController.EnableRotation = true;
        }

        if (isMoving)
        {
            // Alternar entre modo rotación y movimiento con el thumbstick derecho
            if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
            {
                rotationMode = !rotationMode;
                Debug.Log("Modo rotación activo: " + rotationMode);
                if (textDebug != null)
                    textDebug.text = "Modo Rotación: " + rotationMode;
            }

            Vector2 rightInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            if (rotationMode)
            {
                // Rotación: invertir eje X para que al inclinar hacia la derecha el objeto gire hacia la derecha.
                float rotationAmount = -rightInput.x * rotationSpeed * Time.deltaTime;
                EffectiveTransform.Rotate(0f, rotationAmount, 0f, Space.World);
            }
            else
            {
                // Movimiento: se invierte el eje Y para que empujar hacia adelante mueva el objeto hacia adelante.
                // Calcula la dirección relativa a la cámara del jugador
                Vector3 camForward = playerCamera.forward;
                Vector3 camRight = playerCamera.right;

                // Ignora la inclinación vertical para mantenerlo en el plano horizontal
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();

                // Movimiento relativo al jugador
                Vector3 moveDirection = (rightInput.y * camForward) + (rightInput.x * camRight);
                targetObject.transform.position += moveDirection * moveSpeed * Time.deltaTime;


                // Movimiento vertical: Botón A baja; Botón B sube.
                if (OVRInput.Get(OVRInput.Button.One))
                {
                    EffectiveTransform.Translate(Vector3.down * verticalSpeed * Time.deltaTime, Space.World);
                }
                if (OVRInput.Get(OVRInput.Button.Two))
                {
                    EffectiveTransform.Translate(Vector3.up * verticalSpeed * Time.deltaTime, Space.World);
                }
            }
        }
    }

    #endregion

    #region Sistema de Escala (Teclado Numérico Personalizado)

    /// <summary>
    /// Agrega el dígito recibido al final del texto actual del InputField.
    /// Se invoca desde los botones del teclado numérico.
    /// </summary>
    /// <param name="digit">Dígito o caracter a agregar.</param>
    public void AddDigit(string digit)
    {
        if (!isModifiable) return;
        if (scaleInputField != null)
        {
            scaleInputField.text += digit;
            if (textDebug != null)
                textDebug.text = "Input: " + scaleInputField.text;
        }
    }

    /// <summary>
    /// Borra el último dígito del InputField.
    /// </summary>
    public void DeleteLastDigit()
    {
        if (!isModifiable) return;
        if (scaleInputField != null && scaleInputField.text.Length > 0)
        {
            scaleInputField.text = scaleInputField.text.Substring(0, scaleInputField.text.Length - 1);
            if (textDebug != null)
                textDebug.text = "Input: " + scaleInputField.text;
        }
    }

    /// <summary>
    /// Limpia el contenido del InputField.
    /// </summary>
    public void ClearInput()
    {
        if (!isModifiable) return;
        if (scaleInputField != null)
        {
            scaleInputField.text = "";
            if (textDebug != null)
                textDebug.text = "Input limpiado.";
        }
    }

    /// <summary>
    /// Selecciona el eje X para modificar la escala.
    /// </summary>
    public void SelectAxisX()
    {
        if (!isModifiable) return;
        currentAxis = AxisSelect.X;
        ClearInput();
        Debug.Log("Eje seleccionado: X");
        if (textDebug != null)
            textDebug.text = "Eje seleccionado: X";
    }

    /// <summary>
    /// Selecciona el eje Y para modificar la escala.
    /// </summary>
    public void SelectAxisY()
    {
        if (!isModifiable) return;
        currentAxis = AxisSelect.Y;
        ClearInput();
        Debug.Log("Eje seleccionado: Y");
        if (textDebug != null)
            textDebug.text = "Eje seleccionado: Y";
    }

    /// <summary>
    /// Selecciona el eje Z para modificar la escala.
    /// </summary>
    public void SelectAxisZ()
    {
        if (!isModifiable) return;
        currentAxis = AxisSelect.Z;
        ClearInput();
        Debug.Log("Eje seleccionado: Z");
        if (textDebug != null)
            textDebug.text = "Eje seleccionado: Z";
    }

    /// <summary>
    /// Al presionar "Enter", se parsea el valor del InputField y se aplica como escala en el eje seleccionado.
    /// </summary>
    public void OnEnterApplyScale()
    {
        if (!isModifiable) return;
        if (scaleInputField == null || currentAxis == AxisSelect.None)
        {
            Debug.LogWarning("No se ha asignado el InputField o no se seleccionó ningún eje.");
            if (textDebug != null)
                textDebug.text = "Error: No InputField o eje";
            return;
        }
        if (float.TryParse(scaleInputField.text, out float newScale))
        {
            Vector3 currentScale = EffectiveTransform.localScale;
            switch (currentAxis)
            {
                case AxisSelect.X:
                    currentScale.x = newScale;
                    Debug.Log("Nueva escala X: " + newScale);
                    if (textDebug != null)
                        textDebug.text = "Escala X: " + newScale;
                    break;
                case AxisSelect.Y:
                    currentScale.y = newScale;
                    Debug.Log("Nueva escala Y: " + newScale);
                    if (textDebug != null)
                        textDebug.text = "Escala Y: " + newScale;
                    break;
                case AxisSelect.Z:
                    currentScale.z = newScale;
                    Debug.Log("Nueva escala Z: " + newScale);
                    if (textDebug != null)
                        textDebug.text = "Escala Z: " + newScale;
                    break;
            }
            EffectiveTransform.localScale = currentScale;
            ClearInput();
            currentAxis = AxisSelect.None;
        }
        else
        {
            Debug.LogError("Error al parsear el valor ingresado: " + scaleInputField.text);
            if (textDebug != null)
                textDebug.text = "Error de parseo: " + scaleInputField.text;
        }
    }

    #endregion
}
