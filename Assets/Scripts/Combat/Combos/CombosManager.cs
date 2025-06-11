using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombosManager : MonoBehaviour
{
    [Header("QTE Settings")]
    [SerializeField] private RectTransform contenedor;
    [SerializeField] private GameObject puntoPrefab;
    [SerializeField] private int cantidadPuntos = 5;
    [SerializeField] private float distanciaMin = 100f;
    [SerializeField] private float distanciaMax = 300f;
    [SerializeField] private float anguloMin = 30f;
    [SerializeField] private float anguloMax = 150f;
    [SerializeField] private float tiempoEntrePuntos = 0.3f;
    [SerializeField] private float tiempoLimite = 2.5f;
    [SerializeField] private Slider tiempoSlider;

    [Header("Panel Animation")]
    [SerializeField] private QTEPanelAnimator panelAnimator;

    private List<QTEPoint> puntos = new List<QTEPoint>();
    private int currentIndex = 0;
    private bool terminado = true;
    public bool Terminado => terminado;
    private bool falloDetectado = false;
    private Coroutine timeoutCoroutine;
    private Coroutine mostrarQTECoroutine;
    private bool exitoCombo = false;
    public bool ExitoCombo => exitoCombo;
    private bool desvaneciendo = false;
    public bool Desvaneciendo => desvaneciendo;
    private bool permitirClicks = false; // NUEVA variable para controlar clicks

    private List<Vector2> posicionesQTE = new List<Vector2>();

    public void EmpezarQuickTimeEvents()
    {
        if (!terminado) return;

        if (panelAnimator != null)
        {
            panelAnimator.ShowPanel();
        }

        if (tiempoSlider != null)
        {
            tiempoSlider.maxValue = tiempoLimite;
            tiempoSlider.value = tiempoLimite;
            tiempoSlider.gameObject.SetActive(true);
        }

        foreach (var p in puntos)
            if (p != null) Destroy(p.gameObject);
        puntos.Clear();
        currentIndex = 0;
        permitirClicks = false;

        GenerarPosicionesQTE();

        Debug.Log("Generando Quick Time Events...");

        if (mostrarQTECoroutine != null)
            StopCoroutine(mostrarQTECoroutine);
        mostrarQTECoroutine = StartCoroutine(MostrarQTECoroutine());

        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(QTETimeoutCoroutine());
        
        // PERMITIR clicks después de un pequeño delay
        StartCoroutine(HabilitarClicksConDelay());
    }

    public void ShowQTEPanel()
    {
        // Solo muestra el panel, sin iniciar QTEs
        if (panelAnimator != null)
        {
            panelAnimator.ShowPanel();
        }
        
        // NUEVO: Forzar que el contenedor de puntos QTE esté por encima del panel
        EnsureQTEPointsOnTop();
    }

    public void HideQTEPanel()
    {
        // Solo oculta el panel
        if (panelAnimator != null)
        {
            panelAnimator.HidePanel();
        }
    }

    public void EmpezarQuickTimeEventsWithoutPanel()
    {
        if (!terminado) return;

        // Asegurar que los puntos estén por encima antes de crearlos
        EnsureQTEPointsOnTop();
        
        if (tiempoSlider != null)
        {
            tiempoSlider.maxValue = tiempoLimite;
            tiempoSlider.value = tiempoLimite;
            tiempoSlider.gameObject.SetActive(true);
        }

        foreach (var p in puntos)
            if (p != null) Destroy(p.gameObject);
        puntos.Clear();
        currentIndex = 0;
        permitirClicks = false; // RESETEAR

        GenerarPosicionesQTE();

        Debug.Log("Generando Quick Time Events...");

        if (mostrarQTECoroutine != null)
            StopCoroutine(mostrarQTECoroutine);
        mostrarQTECoroutine = StartCoroutine(MostrarQTECoroutine());

        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(QTETimeoutCoroutine());
        
        // PERMITIR clicks después de un pequeño delay
        StartCoroutine(HabilitarClicksConDelay());
    }

    // Asegurar que los puntos estén por encima antes de crearlos
    private void EnsureQTEPointsOnTop()
    {
        if (contenedor != null)
        {
            // Solo mover al final de la jerarquía para asegurar render order
            contenedor.SetAsLastSibling();
            Debug.Log("[CombosManager] Contenedor QTE movido al final de la jerarquía");
        }
    }

    private IEnumerator HabilitarClicksConDelay()
    {
        yield return new WaitForSeconds(0.3f); // Esperar a que se cree el primer punto
        permitirClicks = true;
        Debug.Log("[CombosManager] ¡Clicks habilitados!");
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    private IEnumerator QTETimeoutCoroutine()
    {
        float t = 0f;
        while (t < tiempoLimite && !terminado)
        {
            t += Time.deltaTime;
            if (tiempoSlider != null)
                tiempoSlider.value = Mathf.Clamp(tiempoLimite - t, 0, tiempoLimite);
            yield return null;
        }
        if (!terminado)
        {
            exitoCombo = false;
            falloDetectado = true;
            permitirClicks = false;
            foreach (var p in puntos)
                p.SetFail();
            if (mostrarQTECoroutine != null)
                StopCoroutine(mostrarQTECoroutine);
            StartCoroutine(MostrarRestantesFallidos());
            StartCoroutine(DesvanecerYPurgarPuntos(1f));
            terminado = true;
        }
        if (tiempoSlider != null)
            tiempoSlider.gameObject.SetActive(false);
    }

    private void GenerarPosicionesQTE()
    {
        posicionesQTE.Clear();
        Vector2 centro = contenedor.rect.center;
        Vector2 ultimoPunto = centro;
        float ultimoAngulo = Random.Range(0, 360);
        float puntoRadio = ((RectTransform)puntoPrefab.transform).rect.width * 0.5f;
        float radioMinimo = puntoRadio * 2f;

        // Calcula los márgenes internos
        float margen = puntoRadio;

        for (int i = 0; i < cantidadPuntos; i++)
        {
            Vector2 pos;
            int intentos = 0;
            const int maxIntentos = 100;
            do
            {
                float angulo = ultimoAngulo + Random.Range(anguloMin, anguloMax) * (Random.value > 0.5f ? 1 : -1);
                float distancia = Random.Range(distanciaMin, distanciaMax);

                Vector2 offset = new Vector2(
                    Mathf.Cos(angulo * Mathf.Deg2Rad),
                    Mathf.Sin(angulo * Mathf.Deg2Rad)
                ) * distancia;

                pos = ultimoPunto + offset;

                // Aplica el margen para que no se salgan del contenedor
                pos.x = Mathf.Clamp(
                    pos.x,
                    contenedor.rect.xMin + margen,
                    contenedor.rect.xMax - margen
                );
                pos.y = Mathf.Clamp(
                    pos.y,
                    contenedor.rect.yMin + margen,
                    contenedor.rect.yMax - margen
                );

                intentos++;
            } while (!EsPosicionValida(pos, posicionesQTE, radioMinimo) && intentos < maxIntentos);

            posicionesQTE.Add(pos);
            ultimoPunto = pos;
            ultimoAngulo = Random.Range(0, 360);
        }
    }

    private bool EsPosicionValida(Vector2 nuevaPos, List<Vector2> posiciones, float radioMinimo)
    {
        foreach (var p in posiciones)
        {
            if (Vector2.Distance(nuevaPos, p) < radioMinimo)
                return false;
        }
        return true;
    }

    private IEnumerator MostrarQTECoroutine()
    {
        terminado = false;
        falloDetectado = false;
        exitoCombo = false;

        // VOLVER al método original: crear puntos progresivamente
        for (int i = 0; i < posicionesQTE.Count; i++)
        {
            if (falloDetectado) break;

            GameObject puntoGO = Instantiate(puntoPrefab, contenedor);
            var qtePoint = puntoGO.GetComponent<QTEPoint>();
            qtePoint.Init(i, this);
            var rect = puntoGO.GetComponent<RectTransform>();
            rect.anchoredPosition = posicionesQTE[i];
            puntos.Add(qtePoint);

            Debug.Log($"[CombosManager] Punto QTE {i} creado en posición {posicionesQTE[i]}");

            if (i < posicionesQTE.Count - 1)
                yield return new WaitForSeconds(tiempoEntrePuntos);
        }

        Debug.Log($"[CombosManager] Todos los puntos QTE creados. Total: {puntos.Count}");
    }

    // Llama esto desde QTEPoint
    public void PulsarPunto(int idx)
    {
        Debug.Log($"[CombosManager] PulsarPunto llamado - Índice: {idx}, CurrentIndex: {currentIndex}, Terminado: {terminado}, PermitirClicks: {permitirClicks}");
        
        if (terminado || !permitirClicks) 
        {
            Debug.LogWarning("[CombosManager] Combo terminado o clicks deshabilitados, ignorando click");
            return;
        }

        if (idx == currentIndex)
        {
            Debug.Log($"[CombosManager] ¡Click CORRECTO! Punto {idx} acertado");
            if (idx < puntos.Count && puntos[idx] != null)
            {
                puntos[idx].SetCorrect();
            }
            currentIndex++;
            
            if (currentIndex >= puntos.Count)
            {
                Debug.Log("[CombosManager] ¡Combo COMPLETADO exitosamente!");
                exitoCombo = true;
                terminado = true;
                permitirClicks = false;
                if (timeoutCoroutine != null)
                    StopCoroutine(timeoutCoroutine);
                if (tiempoSlider != null)
                    tiempoSlider.gameObject.SetActive(false);
                StartCoroutine(DesvanecerYPurgarPuntos(0.5f));
            }
        }
        else
        {
            Debug.Log($"[CombosManager] ¡¡¡ FALLO DETECTADO !!! Se esperaba índice {currentIndex}, pero se clickeó {idx}");
            exitoCombo = false;
            falloDetectado = true;
            permitirClicks = false;
            
            foreach (var p in puntos)
            {
                if (p != null) p.SetFail();
            }

            if (mostrarQTECoroutine != null)
                StopCoroutine(mostrarQTECoroutine);
            if (timeoutCoroutine != null)
                StopCoroutine(timeoutCoroutine);
            if (tiempoSlider != null)
                tiempoSlider.gameObject.SetActive(false);
            
            StartCoroutine(MostrarRestantesFallidos());
            StartCoroutine(DesvanecerYPurgarPuntos(0.5f));
            terminado = true;
        }
    }

    // Instancia los puntos que faltan tras el fallo, en rojo y alpha reducido
    private IEnumerator MostrarRestantesFallidos()
    {
        for (int i = puntos.Count; i < posicionesQTE.Count; i++)
        {
            GameObject puntoGO = Instantiate(puntoPrefab, contenedor);
            var qtePoint = puntoGO.GetComponent<QTEPoint>();
            qtePoint.Init(i, this);
            var rect = puntoGO.GetComponent<RectTransform>();
            rect.anchoredPosition = posicionesQTE[i];
            puntos.Add(qtePoint);
            qtePoint.SetFail(0.5f);
            // No hay yield aquí, todos aparecen instantáneamente
        }
        yield break;
    }

    private IEnumerator DesvanecerYPurgarPuntos(float duracion)
    {
        desvaneciendo = true;
        Debug.Log("Combo terminado. ¿Éxito?: " + exitoCombo);
        float tiempo = 0f;
        // Guarda el color inicial de cada punto
        List<Image> images = new List<Image>();
        List<Color> coloresIniciales = new List<Color>();
        foreach (var p in puntos)
        {
            if (p != null && p.image != null)
            {
                images.Add(p.image);
                coloresIniciales.Add(p.image.color);
            }
        }

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;
            for (int i = 0; i < images.Count; i++)
            {
                // Verifica que la imagen y su GameObject sigan existiendo y activos
                if (images[i] != null && images[i].gameObject != null && images[i].gameObject.activeInHierarchy)
                {
                    var color = coloresIniciales[i];
                    color.a = Mathf.Lerp(coloresIniciales[i].a, 0f, t);
                    images[i].color = color;
                }
            }
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Asegura alpha 0 y desactiva
        foreach (var p in puntos)
            if (p != null) p.gameObject.SetActive(false);
        puntos.Clear();

        terminado = true;
        desvaneciendo = false;
    }
}