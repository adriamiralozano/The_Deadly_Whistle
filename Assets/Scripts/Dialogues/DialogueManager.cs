using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public DialogueNode nodoInicial;
    private DialogueNode nodoActual;

    public TextMeshProUGUI textoUI;
    public GameObject contenedorOpciones;
    public Button botonOpcionPrefab;

    void Start()
    {
        MostrarNodo(nodoInicial);
    }

    void MostrarNodo(DialogueNode nodo)
    {
        nodoActual = nodo;
        textoUI.text = nodo.texto;

        // Limpiar botones anteriores
        foreach (Transform hijo in contenedorOpciones.transform)
        {
            Destroy(hijo.gameObject);
        }

        // Crear un botón por cada opción
        foreach (var opcion in nodo.opciones)
        {
            var boton = Instantiate(botonOpcionPrefab, contenedorOpciones.transform);
            boton.GetComponentInChildren<TextMeshProUGUI>().text = opcion.textoOpcion;

            DialogueNode siguienteNodo = opcion.siguienteNodo;

            boton.onClick.AddListener(() => {
                MostrarNodo(opcion.siguienteNodo);
            });
        }
    }
}
