using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
    public string[] conditionFieldNames;
    public ShowIfAttribute(params string[] conditionFieldNames)
    {
        this.conditionFieldNames = conditionFieldNames;
    }
}
