using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SkinStore
{
    public class UISkinItemsGrid : MonoBehaviour
    {
        [SerializeField] GridLayoutGroup gridLayourGroup;

        private List<UISkinItem> storeItemsList = new List<UISkinItem>();
        public List<UISkinItem> StoreItemsList => storeItemsList;

        private IPool storeItemPool;

        public void Init(SkinStoreController Controller, List<SkinStoreProductContainer> products, string selectedProductId)
        {
            UISkinStore uiStore = UIController.GetPage<UISkinStore>();

            storeItemPool = PoolManager.GetPoolByName(uiStore.STORE_ITEM_POOL_NAME);
            storeItemsList.Clear();

            for (int i = 0; i < products.Count; i++)
            {
                UISkinItem item = storeItemPool.GetPooledObject().SetParent(transform).GetComponent<UISkinItem>();
                storeItemsList.Add(item);

                item.transform.localScale = Vector3.one;
                item.transform.SetParent(transform);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;

                item.Init(Controller, products[i], products[i].ProductData.UniqueId == selectedProductId);
            }

            bool isEven = products.Count % 2 == 0;
            int widthCount = isEven ? products.Count / gridLayourGroup.constraintCount : products.Count / gridLayourGroup.constraintCount + 1;

            var width = gridLayourGroup.padding.left + gridLayourGroup.cellSize.x * widthCount + gridLayourGroup.spacing.x * (widthCount - 1) + gridLayourGroup.padding.right;

            var rect = GetComponent<RectTransform>();

            rect.sizeDelta = rect.sizeDelta.SetX(width);
        }

        public void UpdateItems(string selectedProductId)
        {
            for (int i = 0; i < storeItemsList.Count; i++)
            {
                storeItemsList[i].SetSelectedStatus(storeItemsList[i].Data.ProductData.UniqueId == selectedProductId);
                storeItemsList[i].UpdatePriceText();
            }
        }
    }
}

