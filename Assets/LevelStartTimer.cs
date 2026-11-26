using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using MoreMountains.CorgiEngine;

public class LevelStartTimer : MonoBehaviour
{
    [Header("Referencias de Texto (Asigna uno de los dos)")]
    public TMP_Text tmpTimerText;
    public Text legacyTimerText;

    [Header("Configuración de Cuenta Atrás")]
    [Tooltip("Segundos totales para completar el nivel")]
    public float startingSeconds = 30f;
    [Tooltip("Mostrar milisegundos (ej. 00:12.45)")]
    public bool showMilliseconds = true;

    [Header("Aviso y Reinicio al Perder")]
    [Tooltip("Segundos restantes para poner el texto en rojo")]
    public float warningThreshold = 5f;
    public Color warningColor = Color.red;
    [Tooltip("Breve pausa al llegar a 0 antes de reiniciar el nivel")]
    public float restartDelay = 0.8f;

    private float _currentTime = 0f;
    private bool _isRunning = false;
    private bool _hasFinished = false;

    private void Start()
    {
        _currentTime = startingSeconds;
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (_hasFinished) return;

        // 1. Espera en pausa hasta que cualquiera de los dos jugadores se mueva o salte
        if (!_isRunning)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                _isRunning = true;
            }
            else
            {
                return;
            }
        }

        // 2. Cuenta atrás
        _currentTime -= Time.deltaTime;

        // Cambia el texto a rojo en los últimos segundos
        if (_currentTime <= warningThreshold)
        {
            if (tmpTimerText != null) tmpTimerText.color = warningColor;
            if (legacyTimerText != null) legacyTimerText.color = warningColor;
        }

        // 3. Si llega a 0 -> Pierdes y reinicias
        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            _hasFinished = true;
            UpdateTimerDisplay();
            StartCoroutine(RestartLevelRoutine());
            return;
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(_currentTime / 60f);
        int seconds = Mathf.FloorToInt(_currentTime % 60f);

        string formattedTime;
        if (showMilliseconds)
        {
            int milliseconds = Mathf.FloorToInt((_currentTime * 100f) % 100f);
            formattedTime = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
        else
        {
            formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (tmpTimerText != null) tmpTimerText.text = formattedTime;
        if (legacyTimerText != null) legacyTimerText.text = formattedTime;
    }

    // Llama a este método cuando los dos personajes se encuentren para parar el tiempo
    public void StopTimer()
    {
        _isRunning = false;
        _hasFinished = true;
    }

    private IEnumerator RestartLevelRoutine()
    {
        // Congela a los jugadores de Corgi Engine al perder para que no sigan saltando
        if (LevelManager.Instance != null && LevelManager.Instance.Players != null)
        {
            foreach (Character player in LevelManager.Instance.Players)
            {
                if (player != null) player.Freeze();
            }
        }

        yield return new WaitForSeconds(restartDelay);

        // Reinicia la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
