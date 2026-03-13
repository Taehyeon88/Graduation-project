using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
    public string conditionFieldName;
    public ShowIfAttribute(string conditionFieldName)
    {
        this.conditionFieldName = conditionFieldName;
    }
}
