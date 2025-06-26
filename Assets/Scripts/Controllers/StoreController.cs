using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace BeachHero
{
    public class StoreController : MonoBehaviour,IDetailedStoreListener
    {
        [SerializeField] private StoreDatabaseSO storeDatabase;

        public StoreDatabaseSO StoreDatabase
        {
            get => storeDatabase;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            throw new System.NotImplementedException();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            throw new System.NotImplementedException();
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            throw new System.NotImplementedException();
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            throw new System.NotImplementedException();
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            throw new System.NotImplementedException();
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}
