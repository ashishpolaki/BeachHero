using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SkinStore
{
    public class SkinStoreProductContainer
    {
        public SkinStoreProductData ProductData { get; private set; }
        public ISkinData SkinData { get; private set; }

        public bool IsUnlocked => ProductData.IsDummy ? false : SkinData.IsUnlocked;

        public SkinStoreProductContainer(SkinStoreProductData data, ISkinData skinData)
        {
            ProductData = data;
            SkinData = skinData;
        }
    }
}
