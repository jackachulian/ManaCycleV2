using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using LevelSystem.Objectives;

[CustomPropertyDrawer(typeof(LevelObjectiveList))]
public class LevelObjectiveListDrawer : PropertyDrawer
{
    private Type[] objectiveTypes;
    private string[] typeNames;
    private int selectedIndex = 0;

    public LevelObjectiveListDrawer()
    {
        // Get all subclasses of LevelObjective
        objectiveTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(LevelObjective)) && !t.IsAbstract)
            .ToArray();

        typeNames = objectiveTypes.Select(t => t.Name).ToArray();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty objectivesProp = property.FindPropertyRelative("objectives");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 5f;
        float yOffset = position.y;

        // Draw label
        EditorGUI.LabelField(new Rect(position.x, yOffset, position.width, lineHeight), label, EditorStyles.boldLabel);
        yOffset += lineHeight + spacing;

        // Draw each objective manually (WITHOUT foldout)
        for (int i = 0; i < objectivesProp.arraySize; i++)
        {
            SerializedProperty element = objectivesProp.GetArrayElementAtIndex(i);
            if (element.managedReferenceValue != null)
            {
                float elementHeight = DrawObjectiveFields(position.x, yOffset, position.width - 30, element);

                // Remove button
                if (GUI.Button(new Rect(position.x + position.width - 25, yOffset, 20, lineHeight), "X"))
                {
                    objectivesProp.DeleteArrayElementAtIndex(i);
                }

                yOffset += elementHeight + spacing;
            }
        }

        // Dropdown to select new objective type
        selectedIndex = EditorGUI.Popup(new Rect(position.x, yOffset, position.width - 70, lineHeight),
            selectedIndex, typeNames);
        
        // Add button
        if (GUI.Button(new Rect(position.x + position.width - 65, yOffset, 60, lineHeight), "Add"))
        {
            Type selectedType = objectiveTypes[selectedIndex];
            LevelObjective newObjective = (LevelObjective)Activator.CreateInstance(selectedType);
            objectivesProp.InsertArrayElementAtIndex(objectivesProp.arraySize);
            objectivesProp.GetArrayElementAtIndex(objectivesProp.arraySize - 1).managedReferenceValue = newObjective;
        }

        EditorGUI.EndProperty();
    }

    private float DrawObjectiveFields(float x, float y, float width, SerializedProperty element)
    {
        float totalHeight = 0f;
        SerializedProperty prop = element.Copy();
        SerializedProperty endProp = prop.GetEndProperty();

        // Draw all properties inside the class (skip script reference)
        prop.NextVisible(true); // Move to first child
        while (!SerializedProperty.EqualContents(prop, endProp))
        {
            float propHeight = EditorGUI.GetPropertyHeight(prop, true);
            EditorGUI.PropertyField(new Rect(x, y + totalHeight, width, propHeight), prop, true);
            totalHeight += propHeight + 2;
            prop.NextVisible(false); // Move to next property
        }

        return totalHeight;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty objectivesProp = property.FindPropertyRelative("objectives");
        float height = EditorGUIUtility.singleLineHeight * 2 + 10; // Base height for label and dropdown

        // Accumulate height dynamically for each item
        for (int i = 0; i < objectivesProp.arraySize; i++)
        {
            SerializedProperty element = objectivesProp.GetArrayElementAtIndex(i);
            height += GetObjectiveHeight(element) + 5;
        }

        return height;
    }

    private float GetObjectiveHeight(SerializedProperty element)
    {
        float totalHeight = 0f;
        SerializedProperty prop = element.Copy();
        SerializedProperty endProp = prop.GetEndProperty();

        prop.NextVisible(true);
        while (!SerializedProperty.EqualContents(prop, endProp))
        {
            totalHeight += EditorGUI.GetPropertyHeight(prop, true) + 2;
            prop.NextVisible(false);
        }

        return totalHeight;
    }
}
