using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ControlsTutorialSign : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("Arrastra aquí el TextPanel hijo de Controles")]
    public GameObject visualPanel;
    public float animDuration = 0.25f;

    [Header("Opciones de Activación")]
    [Tooltip("Si está marcado, aparece nada más empezar sin necesitar collider")]
    public bool showOnStart = false;
    [Tooltip("Tiempo mínimo que debe verse antes de poder cerrarse al pulsar una tecla")]
    public float minVisibleTime = 0.5f;

    private Vector3 _originalScale;
    private CanvasGroup _canvasGroup;
    private bool _isShowing = false;
    private bool _hasCompleted = false;
    private float _shownTimer = 0f;

    private void Awake()
    {
        // Asegura que el Rigidbody2D sea Kinematic para que no caiga
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (visualPanel != null)
        {
            _originalScale = visualPanel.transform.localScale;
            _canvasGroup = visualPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = visualPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
        }

        if (showOnStart)
        {
            ShowControls();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasCompleted || _isShowing) return;

        Character character = other.GetComponentInParent<Character>();
        if (character != null && character.CharacterType == Character.CharacterTypes.Player)
        {
            ShowControls();
        }
    }

    private void Update()
    {
        if (!_isShowing) return;

        _shownTimer += Time.deltaTime;

        // Espera un pequeño margen para que el jugador alcance a leerlo
        if (_shownTimer < minVisibleTime) return;

        // Detecta W, A, D o las Flechas (Arriba, Izquierda, Derecha)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            HideControls();
        }
    }

    public void ShowControls()
    {
        if (visualPanel == null || _isShowing || _hasCompleted) return;
        StartCoroutine(AnimateIn());
    }

    private void HideControls()
    {
        if (!_isShowing) return;
        _isShowing = false;
        _hasCompleted = true;
        StartCoroutine(AnimateOut());
    }

    private IEnumerator AnimateIn()
    {
        _isShowing = true;
        _shownTimer = 0f;
        visualPanel.SetActive(true);

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeOutBack = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            visualPanel.transform.localScale = _originalScale * easeOutBack;
            _canvasGroup.alpha = t;
            yield return null;
        }
        visualPanel.transform.localScale = _originalScale;
        _canvasGroup.alpha = 1f;
    }

    private IEnumerator AnimateOut()
    {
        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            float easeIn = t * t;

            visualPanel.transform.localScale = Vector3.Lerp(_originalScale, _originalScale * 0.8f, easeIn);
            _canvasGroup.alpha = 1f - t;
            yield return null;
        }
        visualPanel.SetActive(false);
    }
}
