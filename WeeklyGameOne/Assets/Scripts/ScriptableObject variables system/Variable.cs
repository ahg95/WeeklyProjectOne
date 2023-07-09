using UnityEngine;

public abstract class Variable<T> : VariableBase
{
    [SerializeField]
    private T DefaultValue;

    public T RuntimeValue { get; set; }

    protected void OnEnable()
    {
        RuntimeValue = DefaultValue;
    }

    public override string GetRuntimeValueString()
    {
        return RuntimeValue.ToString();
    }
}
