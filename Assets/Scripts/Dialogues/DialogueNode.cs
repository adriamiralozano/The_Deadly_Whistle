using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NuevoNodo", menuName = "Dialogo/Nodo")]
public class DialogueNode : ScriptableObject
{
    [TextArea(2, 5)]
    public string texto;

    public List<Opcion> opciones;
}

[System.Serializable]
public class Opcion
{
    public string textoOpcion;
    public DialogueNode siguienteNodo;
}
