/******************************************************************************
 * Project: Unity
 * File: MyMinMaxDrawer
 * Author: Stephan Weber (SW)
 * 
 * Description:
 *      This Class is a Custom Property Drawer for the MyMinMaxInt class.
 *      It places the Min and Max Value side by side instead of a dropdown.
 * 
 * ***************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(MyMinMaxInt))]
public class MyMinMaxIntDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, label);

        SerializedProperty minVal = property.FindPropertyRelative("minValue");
        SerializedProperty maxVal = property.FindPropertyRelative("maxValue");

        int[] vals = new int[] { minVal.intValue, maxVal.intValue };

        EditorGUI.MultiIntField(
            position,
            new GUIContent[ ] { new GUIContent("Min"), new GUIContent("Max") },
            vals);

        minVal.intValue = vals[0];
        maxVal.intValue = vals[1];

        EditorGUI.EndProperty();
    }
}