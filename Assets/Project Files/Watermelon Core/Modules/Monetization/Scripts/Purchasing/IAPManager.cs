using System.Collections.Generic;
using UnityEngine;

#if MODULE_IAP
using UnityEngine.Purchasing;
#endif

namespace Watermelon
{
    public static class IAPManager
    {
        private static Dictionary<ProductKeyType, IAPItem> productsTypeToProductLink = new Dictionary<ProductKeyType, IAPItem>();

        private static bool isInitialized = false;
        public static bool IsInitialized => isInitialized;

        private static IAPWrapper wrapper;

        public static event SimpleCallback OnPurchaseModuleInitted;
        public static event ProductCallback OnPurchaseComplete;
        public static event ProductFailCallback OnPurchaseFailded;

        private static IAPSettings settings;

        public static void Init(MonetizationSettings monetizationSettings)
        {
            if(isInitialized)
            {
                Debug.LogError("[IAP Manager]: Module is already initialized!");

                return;
            }

            settings = monetizationSettings.IAPSettings;

            productsTypeToProductLink = new Dictionary<ProductKeyType, IAPItem>();

            IAPItem[] items = settings.StoreItems;
            for (int i = 0; i < items.Length; i++)
            {
                if(!productsTypeToProductLink.ContainsKey(items[i].ProductKeyType))
                {
                    productsTypeToProductLink.Add(items[i].ProductKeyType, items[i]);
                }
                else
                {
                    Debug.LogError(string.Format("[IAP Manager]: Product with the type {0} has duplicates in the list!", items[i].ProductKeyType), settings);
                }
            }

            wrapper = GetPlatformWrapper();
            wrapper.Init(settings);
        }

        public static IAPItem GetIAPItem(string productID)
        {
            foreach(IAPItem item in productsTypeToProductLink.Values)
            {
                if (item.ID == productID)
                    return item;
            }

            return null;
        }

        public static IAPItem GetIAPItem(ProductKeyType productKeyType)
        {
            if (productsTypeToProductLink.ContainsKey(productKeyType))
                return productsTypeToProductLink[productKeyType];

            return null;
        }

#if MODULE_IAP
        public static Product GetProduct(ProductKeyType productKeyType)
        {
            IAPItem iapItem = GetIAPItem(productKeyType);
            if (iapItem != null)
            {
                return UnityIAPWrapper.Controller.products.WithID(iapItem.ID);
            }

            return null;
        }
#endif

        public static void RestorePurchases()
        {
            if (!Monetization.IsActive || !isInitialized) return;

            wrapper.RestorePurchases();
        }

        public static void SubscribeOnPurchaseModuleInitted(SimpleCallback callback)
        {
            if (isInitialized)
            {
                callback?.Invoke();
            }
            else
            {
                OnPurchaseModuleInitted += callback;
            }
        }

        public static void BuyProduct(ProductKeyType productKeyType)
        {
            if (!Monetization.IsActive || !isInitialized)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!", settings);

                return;
            }

            wrapper.BuyProduct(productKeyType);
        }

        public static ProductData GetProductData(ProductKeyType productKeyType)
        {
            if (!Monetization.IsActive || !isInitialized) return new ProductData();

            ProductData product = wrapper.GetProductData(productKeyType);

            if(product == null)
            {
                Debug.LogWarning($"[IAP Manager]: Product of type '{productKeyType}' was not found in Monetization Settings. Please ensure it is added to the products list.", settings);
            }

            return product;
        }

        public static bool IsSubscribed(ProductKeyType productKeyType)
        {
            if (!Monetization.IsActive || !isInitialized) return false;

            return wrapper.IsSubscribed(productKeyType);
        }

        public static string GetProductLocalPriceString(ProductKeyType productKeyType)
        {
            ProductData product = GetProductData(productKeyType);

            if (product == null)
            {
                Debug.LogWarning($"[IAP Manager]: Product of type '{productKeyType}' was not found in Monetization Settings. Please ensure it is added to the products list.", settings);

                return string.Empty;
            }    

            return string.Format("{0} {1}", product.ISOCurrencyCode, product.Price);
        }

        public static void OnModuleInitialized()
        {
            isInitialized = true;

            OnPurchaseModuleInitted?.Invoke();

            if (Monetization.VerboseLogging)
                Debug.Log("[IAPManager]: Module is initialized!");
        }

        public static void OnPurchaseCompled(ProductKeyType productKey)
        {
            OnPurchaseComplete?.Invoke(productKey);
        }

        public static void OnPurchaseFailed(ProductKeyType productKey, Watermelon.PurchaseFailureReason failureReason)
        {
            OnPurchaseFailded?.Invoke(productKey, failureReason);
        }

        private static IAPWrapper GetPlatformWrapper()
        {
#if MODULE_IAP
            return new UnityIAPWrapper();
#else
            return new DummyIAPWrapper();
#endif
        }

        public delegate void ProductCallback(ProductKeyType productKeyType);
        public delegate void ProductFailCallback(ProductKeyType productKeyType, Watermelon.PurchaseFailureReason failureReason);
    }
}

// -----------------
// IAP Manager v 1.2.2
// -----------------

// Changelog
// v 1.2.2
// • Fixed serialization bug
// v 1.2.1
// • Added test mode
// v 1.2
// • Support of IAP version 4.11.0
// • Added Editor purchase wrapper
// v 1.1
// • Support of IAP version 4.9.3
// v 1.0.3
// • Support of IAP version 4.7.0
// v 1.0.2
// • Added quick access to the local price of IAP via GetProductLocalPriceString method
// v 1.0.1
// • Added restoration status messages
// v 1.0.0
// • Documentation added
// v 0.4
// • IAPStoreListener inheriting from MonoBehaviour
// v 0.3
// • Editor style update
// v 0.2
// • IAPManager structure changed
// • Enums from UnityEditor.Purchasing has duplicated to prevent serialization problems
// v 0.1
// • Added basic version
