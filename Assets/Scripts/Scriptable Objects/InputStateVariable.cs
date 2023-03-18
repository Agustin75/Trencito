using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variables/Input State")]
public class InputStateVariable : ScriptableObject
{
    [SerializeField]
    private InputState value = InputState.ShowingFeedback;

    public InputState Value { get => value; set => this.value = value; }

    public static implicit operator InputState(InputStateVariable reference)
    {
        return reference.Value;
    }
}
