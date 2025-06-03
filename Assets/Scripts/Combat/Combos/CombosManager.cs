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

    private List<QTEPoint> puntos = new List<QTEPoint>();
    private int currentIndex = 0;
    private bool terminado = true;
    private bool falloDetectado = false;

    private IEnumerator GenerarQTECoroutine()
    {
        terminado = false;
        falloDetectado = false;
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
            } while (!EsPosicionValida(pos, puntos, radioMinimo) && intentos < maxIntentos);

            GameObject puntoGO = Instantiate(puntoPrefab, contenedor);
            var qtePoint = puntoGO.GetComponent<QTEPoint>();
            qtePoint.Init(i, this);
            var rect = puntoGO.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            puntos.Add(qtePoint);

            // Si ya se ha fallado, pinta este punto en rojo directamente
            if (falloDetectado)
                qtePoint.SetFail();

            ultimoPunto = pos;
            ultimoAngulo = Random.Range(0, 360);

            yield return new WaitForSeconds(tiempoEntrePuntos);
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
                terminado = true;
                // Todos correctos, puedes mostrar mensaje de éxito aquí si quieres
            }
        }
        else
        {
            terminado = true;
            falloDetectado = true; // <-- Marca que se ha fallado
            // Todos en rojo
            foreach (var p in puntos)
            {
                p.SetFail();
            }
            // Puedes mostrar mensaje de fallo aquí si quieres
        }
    }

    public void EmpezarQuickTimeEvents()
    {
        GenerarQTE();
    }

    private bool EsPosicionValida(Vector2 nuevaPos, List<QTEPoint> puntos, float radioMinimo)
    {
        foreach (var p in puntos)
        {
            if (Vector2.Distance(nuevaPos, ((RectTransform)p.transform).anchoredPosition) < radioMinimo)
                return false;
        }
        return true;
    }

    public void GenerarQTE()
    {
        if (!terminado) return; // No permite iniciar si el anterior no ha terminado

        foreach (var p in puntos)
            if (p != null) Destroy(p.gameObject);
        puntos.Clear();
        currentIndex = 0;

        Debug.Log("Generando Quick Time Events...");


        StopAllCoroutines(); // Opcional: por si quieres evitar solapamientos
        StartCoroutine(GenerarQTECoroutine());
    }
}