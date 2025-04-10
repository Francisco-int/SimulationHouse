using UnityEngine;
using UnityEngine.UI;

public class UIHelperObjectMover : MonoBehaviour
{
    [Header("Referencia a la C�mara del Jugador")]
    [Tooltip("C�mara principal o el centro de visi�n del jugador.")]
    public Transform playerCamera;

    public static UIHelperObjectMover Instance { get; private set; }

    // Si se asigna targetObject, se aplicar�n las operaciones a �ste; de lo contrario se usar� el GameObject donde est� este script.
    [Header("Target Object (Opcional)")]
    [Tooltip("Si se asigna, todas las operaciones se aplicar�n a este objeto; de lo contrario se usar� este GameObject.")]
    public GameObject targetObject;

    // Propiedad para obtener el transform efectivo
    private Transform EffectiveTransform
    {
        get { return (targetObject != null) ? targetObject.transform : transform; }
    }

    public Text textDebug;
    public enum AxisSelect { None, X, Y, Z }

    [Header("Configuraci�n de Movimiento del Objeto")]
    [Tooltip("Velocidad de movimiento horizontal (adelante, atr�s, izquierda, derecha).")]
    public float moveSpeed = 2f;
    [Tooltip("Velocidad de movimiento vertical (subir y bajar).")]
    public float verticalSpeed = 2f;
    [Tooltip("Velocidad de rotaci�n en grados por segundo.")]
    public float rotationSpeed = 90f;

    // Banderas internas
    private bool isMoving = false;       // Indica si se est� manipulando el objeto
    private bool rotationMode = false;   // Indica si se est� usando el modo rotaci�n

    [Header("Referencia al OVRPlayerController")]
    [Tooltip("Arrastra el GameObject que contenga el componente OVRPlayerController (por ejemplo, el rig del jugador).")]
    public OVRPlayerController playerController;

    [Header("Sistema de Modificaci�n (para creaci�n de la casa)")]
    [Tooltip("Si es true, se permitir� modificar la escala del objeto mediante InputField y botones.")]
    public bool isModifiable = false;
    [Tooltip("InputField donde se mostrar� y escribir� el valor num�rico.")]
    public InputField scaleInputField;

    // Eje seleccionado para modificar la escala.
    [Tooltip("Indica cu�l eje se est� modificando (se selecciona con botones dedicados).")]
    public AxisSelect currentAxis = AxisSelect.None;

    // Tag que determinar� si un objeto es modificable.
    [Tooltip("Si el objeto target tiene este tag, se considerar� modificable.")]
    public string modificableTag = "Modificable";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita que haya m�s de una instancia
        }
    }

    #region Movimiento y Rotaci�n

    /// <summary>
    /// Inicia el modo de manipulaci�n asignando el objeto target.
    /// Adem�s, verifica si el objeto posee el tag indicado para activarlo como modificable.
    /// Para salir del modo manipulaci�n (isMoving), se presiona el bot�n SecondaryHandTrigger.
    /// </summary>
    /// <param name="target">Objeto a manipular.</param>
    public void ToggleMovement(GameObject target)
    {
        // Si se est� iniciando el modo
        if (!isMoving)
        {
            targetObject = target;
            isMoving = true;
            // Verifica si el objeto tiene el tag indicado
            isModifiable = target.CompareTag(modificableTag);
            Debug.Log("Iniciando manipulaci�n del objeto: " + target.name + " | Modificable: " + isModifiable);
            if (textDebug != null)
                textDebug.text = "Manipulaci�n: " + target.name;
            if (playerController != null)
            {
                // Desactiva la rotaci�n del jugador para evitar interferencias
                playerController.EnableRotation = false;
            }
        }
    }

    void Update()
    {
        // Salir del modo manipulaci�n presionando el Secondary Hand Trigger
        if (isMoving && OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            isMoving = false;
            rotationMode = false;
            Debug.Log("Salida de modo manipulaci�n.");
            if (textDebug != null)
                textDebug.text = "Manipulaci�n finalizada.";
            if (playerController != null)
                playerController.EnableRotation = true;
        }

        if (isMoving)
        {
            // Alternar entre modo rotaci�n y movimiento con el thumbstick derecho
            if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
            {
                rotationMode = !rotationMode;
                Debug.Log("Modo rotaci�n activo: " + rotationMode);
                if (textDebug != null)
                    textDebug.text = "Modo Rotaci�n: " + rotationMode;
            }

            Vector2 rightInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            if (rotationMode)
            {
                // Rotaci�n: invertir eje X para que al inclinar hacia la derecha el objeto gire hacia la derecha.
                float rotationAmount = -rightInput.x * rotationSpeed * Time.deltaTime;
                EffectiveTransform.Rotate(0f, rotationAmount, 0f, Space.World);
            }
            else
            {
                // Movimiento: se invierte el eje Y para que empujar hacia adelante mueva el objeto hacia adelante.
                // Calcula la direcci�n relativa a la c�mara del jugador
                Vector3 camForward = playerCamera.forward;
                Vector3 camRight = playerCamera.right;

                // Ignora la inclinaci�n vertical para mantenerlo en el plano horizontal
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();

                // Movimiento relativo al jugador
                Vector3 moveDirection = (rightInput.y * camForward) + (rightInput.x * camRight);
                targetObject.transform.position += moveDirection * moveSpeed * Time.deltaTime;


                // Movimiento vertical: Bot�n A baja; Bot�n B sube.
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

    #region Sistema de Escala (Teclado Num�rico Personalizado)

    /// <summary>
    /// Agrega el d�gito recibido al final del texto actual del InputField.
    /// Se invoca desde los botones del teclado num�rico.
    /// </summary>
    /// <param name="digit">D�gito o caracter a agregar.</param>
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
    /// Borra el �ltimo d�gito del InputField.
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
            Debug.LogWarning("No se ha asignado el InputField o no se seleccion� ning�n eje.");
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
