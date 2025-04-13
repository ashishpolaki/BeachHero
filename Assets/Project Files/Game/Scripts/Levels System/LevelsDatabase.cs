#pragma warning disable 0649

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BeachRescue
{
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Data/Levels Database", order = 1)]
    public class LevelsDatabase : ScriptableObject
    {
        private static LevelsDatabase instance;
        public static LevelsDatabase Data => instance;

        [SerializeField]
        private Level[] levelsList;

        public Level[] LevelsList
        {
            get { return levelsList; }
            set { levelsList = value; }
        }

        public int LevelsAmount
        {
            get { return levelsList.Length; }
        }

        [SerializeField]
        private LevelItem[] items;
        public LevelItem[] Items
        {
            get { return items; }
        }

        public void Init()
        {
            instance = this;
        }

        public Level GetLevelByIndex(int index)
        {
            return levelsList[index % levelsList.Length];
        }

        public LevelItem GetItem(Item itemType)
        {
            foreach (LevelItem item in Items)
            {

                if (item.Item == itemType)
                {
                    return item;
                }

            }

            return null;
        }

        public void AddMissingItems()
        {

            List<LevelItem> levelItems = new List<LevelItem>();
            levelItems.AddRange(Items);

            List<Item> itemsEnumList = new List<Item>();
            itemsEnumList.AddRange((Item[])Enum.GetValues(typeof(Item)));

            for (int i = levelItems.Count - 1; i >= 0; i--)
            {
                if (itemsEnumList.Contains(levelItems[i].Item))
                {
                    itemsEnumList.Remove(levelItems[i].Item);
                }
                else
                {
                    // in case there 2 LevelItem for same Item which wouldn`t happen unless some direct db modification
                    Debug.LogWarning("There was levelItems for same Item");
                    levelItems.RemoveAt(i);
                }
            }

            foreach (Item item in itemsEnumList)
            {
                levelItems.Add(new LevelItem(item));
            }

            items = levelItems.ToArray();
        }
    }
}