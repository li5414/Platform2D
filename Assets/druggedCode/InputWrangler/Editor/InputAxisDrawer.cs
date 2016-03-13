/*
The MIT License (MIT)

Copyright (c) 2015 Mitch Thompson

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 */

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(InputAxis))]
public class InputAxisDrawer : PropertyDrawer {
	static List<string> inputNames = new List<string>();

	static InputAxisDrawer () {
		RefreshInputs();
	}

	static void RefreshInputs () {
		
		Object manager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
		SerializedObject obj = new SerializedObject(manager);
		SerializedProperty axisArr = obj.FindProperty("m_Axes");

		inputNames.Clear();

		for (int i = 0; i < axisArr.arraySize; i++) {
			SerializedProperty entry = axisArr.GetArrayElementAtIndex(i);
			string name = GetChild(entry, "m_Name").stringValue;
			if (inputNames.Contains(name))
				continue;
			else
				inputNames.Add(name);
		}
	}

	static SerializedProperty GetChild (SerializedProperty p, string name) {
		SerializedProperty child = p.Copy();
		child.Next(true);

		if (child.name == name)
			return child;

		while (child.Next(false)) {
			if (child.name == name) {
				return child;
			}
		}

		return null;
	}

	static void OpenInputManager () {
		Object manager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
		Selection.activeObject = manager;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		if (property.propertyType != SerializedPropertyType.String) {
			EditorGUI.LabelField(position, "ERROR:", "May only apply to type string");
			return;
		}

		position = EditorGUI.PrefixLabel(position, label);

		string name = property.stringValue;
		Color previousColor = GUI.color;
		if (name == "")
			GUI.color = Color.grey;
		else if (!inputNames.Contains(name))
			GUI.color = Color.red;

		if (GUI.Button(position, name == "" ? "<None>" : name, EditorStyles.popup)) {
			Selector(property);
		}
		GUI.color = previousColor;
		
	}

	SerializedProperty currentProperty;
	void Selector (SerializedProperty property) {
		currentProperty = property;
		GenericMenu menu = new GenericMenu();

		menu.AddItem(new GUIContent("Refresh"), false, RefreshInputs);
		menu.AddItem(new GUIContent("InputManager"), false, OpenInputManager);
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("<None>"), property.stringValue == "", HandleSelect, "");
		for (int i = 0; i < inputNames.Count; i++) {
			string name = inputNames[i];
			menu.AddItem(new GUIContent(name), property.stringValue == name, HandleSelect, name);
		}
		

		menu.ShowAsContext();
	}

	void HandleSelect (object data) {
		currentProperty.stringValue = (string)data;
		currentProperty.serializedObject.ApplyModifiedProperties();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return 18;
	}
}
