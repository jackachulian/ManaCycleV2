#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using LevelSystem.Objectives;
using UnityEngine.UIElements;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    // private Type[] objectiveTypes;
    // private string[] objectiveTypeNames;
    // private int selectedTypeIndex = 0;

    // private void OnEnable()
    // {
    //     // Find all types that extend LevelObjective
    //     objectiveTypes = AppDomain.CurrentDomain.GetAssemblies()
    //         .SelectMany(assembly => assembly.GetTypes())
    //         .Where(t => t.IsClass && !t.IsAbstract && typeof(LevelObjective).IsAssignableFrom(t))
    //         .ToArray();

    //     // Create readable names
    //     objectiveTypeNames = objectiveTypes.Select(t => t.Name).ToArray();
    // }

    // public override void OnInspectorGUI()
    // {
    //     serializedObject.Update();

    //     // Get the list property
    //     SerializedProperty objectivesProp = serializedObject.FindProperty("objectives");

    //     // Display the list
    //     EditorGUILayout.PropertyField(objectivesProp, true);

    //     // Dropdown to select type
    //     selectedTypeIndex = EditorGUILayout.Popup("Add Objective", selectedTypeIndex, objectiveTypeNames);

    //     if (GUILayout.Button("Add Objective"))
    //     {
    //         if (selectedTypeIndex >= 0 && selectedTypeIndex < objectiveTypes.Length)
    //         {
    //             Type selectedType = objectiveTypes[selectedTypeIndex];
    //             LevelObjective newObjective = (LevelObjective)Activator.CreateInstance(selectedType);

    //             objectivesProp.arraySize++;
    //             objectivesProp.GetArrayElementAtIndex(objectivesProp.arraySize - 1).managedReferenceValue = newObjective;
    //             serializedObject.ApplyModifiedProperties();
    //         }
    //     }

    //     serializedObject.ApplyModifiedProperties();
    // }

    // public override VisualElement CreateInspectorGUI()
    // {
    //     VisualElement myInspector = new VisualElement();

    //     myInspector.Add(new Label("This is a custom Inspector"));

    //     return myInspector;
    // }
}
#endif