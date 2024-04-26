using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CreateAssetMenu(fileName = "EConfigHolder", menuName = "EConfig Management/EConfig Holder", order = 1)]
public class EConfigHolder : ScriptableObject
{
    public List<ScriptableObject> configs = new List<ScriptableObject>();
}