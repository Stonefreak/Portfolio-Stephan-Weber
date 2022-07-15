/******************************************************************************
 * Project: Unity
 * File: MyMinMaxDrawer
 * Author: Stephan Weber (SW)
 * 
 * Description:
 *      This Class is a Custom Property Drawer for the MyMinMax class.
 *      It places the Min and Max Value side by side instead of a dropdown.
 * 
 * ***************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(MyMinMax))]
public class MyMinMaxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, label);

        SerializedProperty minVal = property.FindPropertyRelative("minValue");
        SerializedProperty maxVal = property.FindPropertyRelative("maxValue");

        float[ ] vals = new float[ ] { minVal.floatValue, maxVal.floatValue };

        EditorGUI.MultiFloatField(
            position,
            new GUIContent[ ] { new GUIContent("Min"), new GUIContent("Max") },
            vals);

        minVal.floatValue = vals[0];
        maxVal.floatValue = vals[1];

        EditorGUI.EndProperty();
    }
}