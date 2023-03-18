using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variables/Boolean")]
public class BoolVariable : ScriptableObject
{
    [SerializeField]
    private bool value = false;

    public bool Value { get => value; set => this.value = value; }

    public static implicit operator bool(BoolVariable reference)
    {
        return reference.Value;
    }
}
