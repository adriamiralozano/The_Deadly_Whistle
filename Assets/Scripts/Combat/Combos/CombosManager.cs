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

    private List<Vector2> posicionesQTE = new List<Vector2>();

    public void EmpezarQuickTimeEvents()
    {
        if (!terminado) return;

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

        GenerarPosicionesQTE();

        Debug.Log("Generando Quick Time Events...");

        if (mostrarQTECoroutine != null)
            StopCoroutine(mostrarQTECoroutine);
        mostrarQTECoroutine = StartCoroutine(MostrarQTECoroutine());

        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(QTETimeoutCoroutine());
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

        for (int i = 0; i < cantidadPuntos; i++)
        {
            Vector2 pos;
            int intentos = 0;
            const int maxIntentos = 50;
            do
            {
                float angulo = ultimoAngulo + Random.Range(anguloMin, anguloMax) * (Random.value > 0.5f ? 1 : -1);
                float distancia = Random.Range(distanciaMin, distanciaMax);

                Vector2 offset = new Vector2(
                    Mathf.Cos(angulo * Mathf.Deg2Rad),
                    Mathf.Sin(angulo * Mathf.Deg2Rad)
                ) * distancia;

                pos = ultimoPunto + offset;
                pos.x = Mathf.Clamp(pos.x, contenedor.rect.xMin, contenedor.rect.xMax);
                pos.y = Mathf.Clamp(pos.y, contenedor.rect.yMin, contenedor.rect.yMax);

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
        puntos.Clear();
        terminado = false;
        falloDetectado = false;

        for (int i = 0; i < posicionesQTE.Count; i++)
        {
            GameObject puntoGO = Instantiate(puntoPrefab, contenedor);
            var qtePoint = puntoGO.GetComponent<QTEPoint>();
            qtePoint.Init(i, this);
            var rect = puntoGO.GetComponent<RectTransform>();
            rect.anchoredPosition = posicionesQTE[i];
            puntos.Add(qtePoint);

            // Espera entre cada aparición, o muestra todos de golpe si falloDetectado
            if (!falloDetectado)
                yield return new WaitForSeconds(tiempoEntrePuntos);

            // Si ya se ha fallado, los nuevos puntos aparecen en rojo y con alpha reducido
            if (falloDetectado)
            {
                if (mostrarQTECoroutine != null)
                    StopCoroutine(mostrarQTECoroutine);

            }

        }
    }

    // Llama esto desde QTEPoint
    public void PulsarPunto(int idx)
    {
        if (terminado) return;

        if (idx == currentIndex)
        {
            puntos[idx].SetCorrect();
            currentIndex++;
            if (currentIndex >= puntos.Count)
            {
                exitoCombo = true;
                terminado = true;
                if (timeoutCoroutine != null)
                    StopCoroutine(timeoutCoroutine);
                StartCoroutine(DesvanecerYPurgarPuntos(0.5f));
            }
        }
        else
        {
            exitoCombo = false;
            falloDetectado = true;
            foreach (var p in puntos)
                p.SetFail();

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