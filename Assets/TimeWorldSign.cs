using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TimedWorldSign : MonoBehaviour
{
    [Header("Configuración del Cartel")]
    public GameObject visualPanel;
    public float visibleDuration = 4f;
    public bool canRetrigger = true;

    [Header("Animación de Aparición y Salida")]
    public float animDuration = 0.25f;

    private Coroutine _sequenceCoroutine;
    private bool _hasTriggered = false;
    private Vector3 _originalScale;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (visualPanel != null)
        {
            // Guarda la escala real que configuraste en el editor (0.8, 0.74, 1)
            _originalScale = visualPanel.transform.localScale;

            // Añade automáticamente un CanvasGroup para animar la transparencia (Fade)
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Character character = other.GetComponentInParent<Character>();

        if (character != null && character.CharacterType == Character.CharacterTypes.Player)
        {
            if (!canRetrigger && _hasTriggered) return;

            _hasTriggered = true;
            ShowSign();
        }
    }

    private void ShowSign()
    {
        if (visualPanel == null) return;

        if (_sequenceCoroutine != null)
        {
            StopCoroutine(_sequenceCoroutine);
        }
        _sequenceCoroutine = StartCoroutine(AnimateSignSequence());
    }

    private IEnumerator AnimateSignSequence()
    {
        visualPanel.SetActive(true);

        // 1. ANIMACIÓN DE ENTRADA (Escala con ligero rebote + Fade In)
        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            // Curva "EaseOutBack" para un pequeño efecto pop/rebote elegante
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeOutBack = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            visualPanel.transform.localScale = _originalScale * easeOutBack;
            _canvasGroup.alpha = t;

            yield return null;
        }
        visualPanel.transform.localScale = _originalScale;
        _canvasGroup.alpha = 1f;

        // 2. ESPERA EN PANTALLA
        yield return new WaitForSeconds(visibleDuration);

        // 3. ANIMACIÓN DE SALIDA (Se encoge suavemente + Fade Out)
        elapsed = 0f;
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
