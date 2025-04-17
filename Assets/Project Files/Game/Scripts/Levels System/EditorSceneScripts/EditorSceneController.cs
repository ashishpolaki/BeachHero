#pragma warning disable 649

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Bokka.BeachRescue;

namespace Bokka
{
    public class EditorSceneController : MonoBehaviour
    {

#if UNITY_EDITOR
        private static EditorSceneController instance;
        public static EditorSceneController Instance { get => instance; }

        [SerializeField] private GameObject container;

        public EditorSceneController()
        {
            instance = this;
        }


        public void SpawnStartPoint(UnityEngine.Object prefabRef, Vector3 position)
        {
            GameObject newGameobject = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef, container.transform);
            newGameobject.transform.localPosition = position;
            Selection.activeGameObject = newGameobject;
            newGameobject.AddComponent<StartPointSave>();
        }

        public void SpawnItem(UnityEngine.Object prefabRef, ItemSave itemSave)
        {
            GameObject newGameobject = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef, container.transform);
            newGameobject.transform.localPosition = itemSave.Position;
            newGameobject.transform.localEulerAngles = itemSave.Rotation;
            newGameobject.transform.localScale = itemSave.Scale;
            Selection.activeGameObject = newGameobject;

            SavableItem component = newGameobject.AddComponent<SavableItem>();
            component.Item = itemSave.Type;
        }

        public void SpawnCharacter(UnityEngine.Object prefabRef, ItemSave itemSave,float waitingPecentage)
        {
            GameObject newGameobject = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef, container.transform);
            newGameobject.transform.localPosition = itemSave.Position;
            newGameobject.transform.localEulerAngles = itemSave.Rotation;
            newGameobject.transform.localScale = itemSave.Scale;
            Selection.activeGameObject = newGameobject;

            CharacterSavableItem component = newGameobject.AddComponent<CharacterSavableItem>();
            component.Item = itemSave.Type;
            component.WaitingPercentage = waitingPecentage;

        }


        public void SpawnMovingObstacle(UnityEngine.Object prefabRef, MovingObstacleSave movingObstacleSave)
        {
            GameObject newGameobject = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef, container.transform);
            newGameobject.transform.localPosition = new Vector3(0, 0, -50);
            Selection.activeGameObject = newGameobject;

            MovingObstacleSavableItem component = newGameobject.AddComponent<MovingObstacleSavableItem>();
            component.Save = movingObstacleSave;
        }


        public Vector3 CollectStartPoint()
        {
            StartPointSave data = container.GetComponentInChildren<StartPointSave>();
            return data.transform.localPosition;
        }

        public ItemSave[] CollectItems()
        {
            SavableItem[] data = container.GetComponentsInChildren<SavableItem>();
            ItemSave[] result = new ItemSave[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = new ItemSave();
                result[i].Type = data[i].Item;
                result[i].Position = data[i].transform.localPosition;
                result[i].Rotation = data[i].transform.localEulerAngles;
                result[i].Scale = data[i].transform.localScale;
            }


            Array.Sort(result, SortByTypeAndZ);
            return result;
        }

        private int SortByTypeAndZ(ItemSave x, ItemSave y)
        {
            if(x.Type == y.Type)
            {
                return x.Position.z.CompareTo(y.Position.z);
            }
            else
            {
                return x.Type.CompareTo(y.Type);
            }
            
        }

        public CharacterSave[] CollectCharacters()
        {
            CharacterSavableItem[] data = container.GetComponentsInChildren<CharacterSavableItem>();
            CharacterSave[] result = new CharacterSave[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = new CharacterSave();
                result[i].WaitingPercentage = data[i].WaitingPercentage;
                result[i].ItemSave = new ItemSave();
                result[i].ItemSave.Type = data[i].Item;
                result[i].ItemSave.Position = data[i].transform.localPosition;
                result[i].ItemSave.Rotation = data[i].transform.localEulerAngles;
                result[i].ItemSave.Scale = data[i].transform.localScale;
            }

            return result;
        }

        public MovingObstacleSave[] CollectMovingObstacles()
        {
            MovingObstacleSavableItem[] data = container.GetComponentsInChildren<MovingObstacleSavableItem>();
            MovingObstacleSave[] result = new MovingObstacleSave[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = data[i].Save;
            }

            return result;
        }

        public void SelectGameObject(GameObject selectedGameObject)
        {
            Selection.activeGameObject = selectedGameObject;
        }

        

        public void Clear()
        {
            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }
        }

        public ItemSave[] GetLevelItems()
        {
            SavableItem[] savableItems = FindObjectsByType<SavableItem>(FindObjectsSortMode.None);
            List<ItemSave> result = new List<ItemSave>();

            for (int i = 0; i < savableItems.Length; i++)
            {
                result.Add(HandleParse(savableItems[i]));
            }

            return result.ToArray();
        }

        private ItemSave HandleParse(SavableItem savableItem)
        {
            return new ItemSave(savableItem.Item, savableItem.gameObject.transform.position, savableItem.gameObject.transform.rotation.eulerAngles, savableItem.gameObject.transform.localScale);
        }

        
#endif
    }
}
