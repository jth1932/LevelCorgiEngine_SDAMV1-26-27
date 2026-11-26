using UnityEngine;
using System.Collections;

public class PopInOnStart : MonoBehaviour
{
    [Header("Configuración Pop-Out")]
    [Tooltip("Retraso antes de aparecer al empezar el nivel")]
    public float startDelay = 0.2f;

    [Tooltip("Duración de la animación de rebote")]
    public float animDuration = 0.35f;

    [Header("¿Desaparecer después? (Opcional)")]
    public bool hideAfterDelay = false;
    public float visibleDuration = 4f;

    private Vector3 _originalScale;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // Guarda la escala original del cartel
        _originalScale = transform.localScale;

        // Añade CanvasGroup para suavizar la transparencia
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Empieza invisible y encogido desde el fotograma 0
        transform.localScale = Vector3.zero;
        _canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        // Animación Pop-Out (crece con rebote desde el centro)
        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            // Curva EaseOutBack para el efecto pop-out
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeOutBack = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            transform.localScale = _originalScale * easeOutBack;
            _canvasGroup.alpha = t;

            yield return null;
        }

        transform.localScale = _originalScale;
        _canvasGroup.alpha = 1f;

        // Si marcas la casilla opcional, también se ocultará solo
        if (hideAfterDelay)
        {
            yield return new WaitForSeconds(visibleDuration);

            elapsed = 0f;
            while (elapsed < animDuration * 0.7f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / (animDuration * 0.7f));
                transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, t * t);
                _canvasGroup.alpha = 1f - t;
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
