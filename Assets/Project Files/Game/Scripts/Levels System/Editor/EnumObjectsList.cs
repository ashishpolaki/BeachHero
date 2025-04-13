#pragma warning disable 649

using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using UnityEngine;

namespace Watermelon
{
    public class EnumObjectsList
    {
        private const string DEFAULT_ENUM_NAME = "Item";
        private const string REGEX_PATTERN = "^[A-Z][0-9a-zA-Z]+$";
        public SerializedProperty array;
        public string enumPropertyPath;
        public string objectReferencePropertyPath;
        private Regex regex;
        public string enumFilePath;
        public Action onBeforeEnumFileUpdate;
        public string enumName;
        private Type customType;
        private string tabLabel;
        private int acceptablePositionGap;

        public Type CustomType { get => customType; set => customType = value; }
        public string TabLabel { get => tabLabel; set => tabLabel = value; }
        public int AcceptablePositionGap { get => acceptablePositionGap; set => acceptablePositionGap = value; }

        //variables for display
        private const float ITEMS_MAX_WIDTH = 600;
        private const int FIRST_COLUMN_WIDTH = 80;
        private const int CUSTOM_LABEL_WIDTH = 45;
        private const string EDIT_ENUM = "Edit enum";
        private const string SAVE = "Save";
        private const string CANCEL = "Cancel";
        private const string ENUM = "Enum: ";
        private const string ENUM_NAME = "Name:";
        private const string ENUM_VALUE = "Value:";
        private const string ADD = "+";
        private const string REMOVE = "X";
        private const string MOVE = "↑↓";
        private const string SPAWN_POSITION_PROPERTY_PATH = "spawnPosition";
        private const string PREFAB_LABEL = "Prefab:";
        private const string SPAWN_POSITION_LABEL = "Spawn position:";


        private Vector2 scrollView;
        string[] enumDisplayNames;
        private SerializedProperty tempProperty;
        private string tempPropertyLabel;
        private bool editEnamModeEnabled;
        private bool moveEnumModeEnabled;
        private int movableElementIndex;
        private Dictionary<int, string> enumDictionary;
        private List<EditedEnumValue> values;
        private List<string> usedNames;
        private List<int> usedEnumIndexes;
        private List<string> errors;
        private float defaultLabelWidth;
        private GUIContent prefabContent;
        private GUIContent spawnPositionContent;

        //move enum view 
        private int maxPosition;
        private int currentIndex;

        //Enable category support
        bool isCategoryEnabled;
        string categotyPath;
         GUIContent categoryContent;

        public EnumObjectsList(SerializedProperty array, string enumPropertyPath, string objectReferencePropertyPath, string enumFilePath, Action onBeforeEnumFileUpdate, string enumName = DEFAULT_ENUM_NAME)
        {
            this.array = array;
            this.enumPropertyPath = enumPropertyPath;
            this.objectReferencePropertyPath = objectReferencePropertyPath;
            this.enumFilePath = enumFilePath;
            this.onBeforeEnumFileUpdate = onBeforeEnumFileUpdate;
            this.enumName = enumName;
            regex = new Regex(REGEX_PATTERN, RegexOptions.Singleline | RegexOptions.Compiled);
            customType = typeof(SavableItem);
            tabLabel = "Spawn items:";
            acceptablePositionGap = 8;
            prefabContent = new GUIContent(PREFAB_LABEL);
            spawnPositionContent = new GUIContent(SPAWN_POSITION_LABEL);
            SortAndAddMissingItems();
            isCategoryEnabled = false;
        }

        public void EnableCategories(string propertyPath)
        {
            isCategoryEnabled = true;
            categotyPath = propertyPath;
            categoryContent = new GUIContent("Category:");
        }



        private void SortAndAddMissingItems()
        {
            array.arraySize = Mathf.Max(array.arraySize, 1);
            enumDisplayNames = array.GetArrayElementAtIndex(0).FindPropertyRelative(enumPropertyPath).enumDisplayNames;

            bool found;
            int currentEnumValue;
            int tempEnumValue;
            array.arraySize = Mathf.Max(array.arraySize, enumDisplayNames.Length);

            for (int sortedIndex = 0; sortedIndex < enumDisplayNames.Length; sortedIndex++)
            {
                currentEnumValue = array.GetArrayElementAtIndex(sortedIndex).FindPropertyRelative(enumPropertyPath).enumValueIndex;

                if (currentEnumValue == sortedIndex) //value sorted
                {
                    continue;
                }
                else
                {
                    found = false;

                    for (int unsortedIndex = sortedIndex + 1; unsortedIndex < array.arraySize; unsortedIndex++)
                    {
                        tempEnumValue = array.GetArrayElementAtIndex(unsortedIndex).FindPropertyRelative(enumPropertyPath).enumValueIndex;

                        if (tempEnumValue == sortedIndex)
                        {
                            found = true;
                            array.MoveArrayElement(unsortedIndex, sortedIndex);
                            break;
                        }
                    }

                    if (!found) // insert new element if missing
                    {
                        array.InsertArrayElementAtIndex(sortedIndex);
                        array.GetArrayElementAtIndex(sortedIndex).FindPropertyRelative(enumPropertyPath).enumValueIndex = sortedIndex;
                        array.GetArrayElementAtIndex(sortedIndex).FindPropertyRelative(objectReferencePropertyPath).objectReferenceValue = null;
                    }
                }
            }

            array.arraySize = enumDisplayNames.Length; // remove extra elements
        }

        public void DisplayTab()
        {
            EditorGUILayout.BeginVertical();
            scrollView = EditorGUILayout.BeginScrollView(scrollView);
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(ITEMS_MAX_WIDTH));

            if (editEnamModeEnabled)
            {
                if (moveEnumModeEnabled)
                {
                    MoveEnumView();
                }
                else
                {
                    EditEnumView();
                }
            }
            else
            {
                DefaultView();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }



        private void DefaultView()
        {
            EditorGUILayout.LabelField(tabLabel, EditorCustomStyles.labelLargeBold);

            for (int index = 0; index < array.arraySize; index++)
            {
                tempProperty = array.GetArrayElementAtIndex(index);
                tempPropertyLabel = enumDisplayNames[tempProperty.FindPropertyRelative(enumPropertyPath).enumValueIndex];

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField(tempPropertyLabel, EditorCustomStyles.labelMedium);

                EditorGUILayout.PropertyField(tempProperty.FindPropertyRelative(objectReferencePropertyPath), prefabContent);
                EditorGUILayout.PropertyField(tempProperty.FindPropertyRelative(SPAWN_POSITION_PROPERTY_PATH), spawnPositionContent);

                if (isCategoryEnabled)
                {
                    EditorGUILayout.PropertyField(tempProperty.FindPropertyRelative(categotyPath), categoryContent);
                }

                EditorGUILayout.EndVertical();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EDIT_ENUM, EditorCustomStyles.button, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                StartEditMode();
            }

            GUILayout.EndHorizontal();
        }

        
        private void StartEditMode()
        {
            editEnamModeEnabled = true;
            values = new List<EditedEnumValue>();
            errors = new List<string>();
            usedEnumIndexes = new List<int>();
            usedNames = new List<string>();
            ParseEnumFile();

            foreach (KeyValuePair<int, string> pair in enumDictionary)
            {
                values.Add(new EditedEnumValue(pair.Value, pair.Key, true));
            }
        }

        private void ParseEnumFile()
        {
            string path = LevelEditorBase.GetProjectPath() + enumFilePath;
            string allText = File.ReadAllText(path);
            int firstIndex = allText.LastIndexOf('{');
            int secondIndex = allText.IndexOf('}');
            allText = allText.Substring(firstIndex + 1, secondIndex - firstIndex - 2);
            string[] parseArray = allText.Split(',');

            enumDictionary = new Dictionary<int, string>();
            int index;
            string leftPart;
            string rightPart;

            for (int i = 0; i < parseArray.Length; i++)
            {
                index = parseArray[i].IndexOf('=');

                if (index != -1) // for last element or direct enum editing
                {
                    leftPart = parseArray[i].Substring(0, index).Trim();
                    rightPart = parseArray[i].Substring(index + 1).Trim();
                    enumDictionary.Add(int.Parse(rightPart), leftPart);
                }
            }
        }

        private void EditEnumView()
        {
            EditorGUILayout.LabelField(ENUM, EditorCustomStyles.labelLargeBold);
            ClearValidation();

            defaultLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = CUSTOM_LABEL_WIDTH;
            //action buttons
            GUILayout.BeginVertical();

            for (int i = 0; i < values.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("#" + (i + 1) + " " + (values[i].old ? "(old)" : "(new)"), GUILayout.MaxWidth(FIRST_COLUMN_WIDTH));
                values[i].name = EditorGUILayout.TextField(ENUM_NAME, values[i].name);
                GUILayout.Space(10);

                if (values[i].old)
                {
                    EditorGUILayout.LabelField(ENUM_VALUE, values[i].enumIndex.ToString());
                }
                else
                {
                    values[i].enumIndex = EditorGUILayout.IntField(ENUM_VALUE, values[i].enumIndex);
                }



                EditorGUI.BeginDisabledGroup(values[i].old);

                if (GUILayout.Button(MOVE, GUILayout.Width(40)))
                {
                    GUIUtility.keyboardControl = int.MinValue;
                    maxPosition = 0;

                    foreach (EditedEnumValue item in values)
                    {
                        maxPosition = Mathf.Max(item.enumIndex, maxPosition);
                    }

                    maxPosition += AcceptablePositionGap;
                    movableElementIndex = i;
                    moveEnumModeEnabled = true;
                }

                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(REMOVE, GUILayout.Width(20)))
                {
                    GUIUtility.keyboardControl = int.MinValue;

                    if (EditorUtility.DisplayDialog("Warning dialog", $"Are you sure that you want to remove element #{(i + 1)}?", "Ok", "Cancel"))
                    {
                        values.RemoveAt(i);
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();


            for (int i = 0; i < values.Count; i++)
            {
                ValidataValue(i, values[i]);
            }

            EditorGUIUtility.labelWidth = defaultLabelWidth;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Sort by value"))
            {
                SortValues();
            }

            GUILayout.FlexibleSpace();


            if (GUILayout.Button(ADD))
            {
                values.Add(new EditedEnumValue(string.Empty, Mathf.Max(usedEnumIndexes.ToArray()) + 1, false));
            }



            GUILayout.EndHorizontal();


            if (errors.Count > 0)
            {
                EditorGUILayout.HelpBox(errors[0], MessageType.Error);
            }


            //cancel/save section
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(CANCEL, EditorCustomStyles.buttonRed, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                editEnamModeEnabled = false;
            }

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(errors.Count > 0);

            if (GUILayout.Button(SAVE, EditorCustomStyles.buttonGreen, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                UpdateEnumFile();
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void MoveEnumView()
        {
            currentIndex = 0;

            EditorGUILayout.LabelField(ENUM, EditorCustomStyles.labelLargeBold);
            ClearValidation();

            defaultLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = CUSTOM_LABEL_WIDTH;
            //action buttons
            GUILayout.BeginVertical();

            for (int index = 0; index < maxPosition; index++)
            {
                if ((currentIndex < values.Count) && (values[currentIndex].enumIndex == index))
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("#" + (currentIndex + 1) + " " + (values[currentIndex].old ? "(old)" : "(new)"), GUILayout.MaxWidth(FIRST_COLUMN_WIDTH));
                    EditorGUILayout.LabelField(ENUM_NAME, values[currentIndex].name);
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField(ENUM_VALUE, values[currentIndex].enumIndex.ToString());
                    GUILayout.EndHorizontal();
                    currentIndex++;
                }
                else
                {
                    if (GUILayout.Button($"Place here (new value: {index})", EditorCustomStyles.buttonBlue))
                    {
                        values[movableElementIndex].enumIndex = index;
                        SortValues();
                        moveEnumModeEnabled = false;
                    }
                }
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = defaultLabelWidth;
        }

        private void ClearValidation()
        {
            errors.Clear();
            usedNames.Clear();
            usedEnumIndexes.Clear();
        }

        private void ValidataValue(int index, EditedEnumValue editedEnumValue)
        {
            if (editedEnumValue.name.Length == 0)
            {
                errors.Add("Empty \"name\" in element  #" + (index + 1));
            }
            else if (usedNames.Contains(editedEnumValue.name))
            {
                errors.Add("You need to make \"name\" unique in element  #" + (index + 1));
            }
            else if (!regex.IsMatch(editedEnumValue.name))
            {
                errors.Add($"Name in element#{(index + 1)} should start with capital letter and uses only latin letters and numbers without spaces");
            }
            else
            {
                usedNames.Add(editedEnumValue.name);
            }

            if (usedEnumIndexes.Contains(editedEnumValue.enumIndex))
            {
                errors.Add("You need to make \"value\" unique in element  #" + (index + 1));
            }
            else
            {
                usedEnumIndexes.Add(editedEnumValue.enumIndex);
            }
        }

        private void SortValues()
        {
            values.Sort((x, y) => x.enumIndex.CompareTo(y.enumIndex));
        }

        private void UpdateEnumFile()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("namespace Watermelon");
            stringBuilder.AppendLine();
            stringBuilder.Append("{");
            stringBuilder.AppendLine();
            stringBuilder.Append("\tpublic enum " + enumName);
            stringBuilder.AppendLine();
            stringBuilder.Append("\t{");
            stringBuilder.AppendLine();

            SortValues(); // To make everything follow the same order

            for (int i = 0; i < values.Count; i++)
            {
                stringBuilder.Append("\t\t");
                stringBuilder.Append(values[i].name);
                stringBuilder.Append(" = ");
                stringBuilder.Append(values[i].enumIndex);
                stringBuilder.Append(",");
                stringBuilder.AppendLine();
            }

            stringBuilder.Append("\t}");
            stringBuilder.AppendLine();
            stringBuilder.Append("}");

            string path = Application.dataPath.Replace("Assets", string.Empty) + enumFilePath;
            onBeforeEnumFileUpdate?.Invoke();
            File.WriteAllText(path, stringBuilder.ToString(), Encoding.UTF8);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(enumFilePath);
        }

        private class EditedEnumValue
        {
            public string name;
            public int enumIndex;
            public bool old;

            public EditedEnumValue(string name, int enumIndex, bool old)
            {
                this.name = name;
                this.enumIndex = enumIndex;
                this.old = old;
            }
        }
    }
}
