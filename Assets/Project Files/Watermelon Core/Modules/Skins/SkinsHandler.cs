using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SkinsHandler
    {
        [SerializeField] AbstractSkinsDatabase[] skinProviders;
        public AbstractSkinsDatabase[] SkinsProviders => skinProviders;

        public int ProvidersCount => skinProviders.Length;

        public AbstractSkinsDatabase GetSkinsProvider(int index)
        {
            return skinProviders[index];
        }

        public AbstractSkinsDatabase GetSkinsProvider(System.Type providerType)
        {
            if (!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinsDatabase skinProvider in skinProviders)
                {
                    if (skinProvider.GetType() == providerType)
                        return skinProvider;
                }
            }

            return null;
        }

        public bool HasSkinsProvider(System.Type providerType)
        {
            if (!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinsDatabase skinProvider in skinProviders)
                {
                    if (skinProvider.GetType() == providerType)
                        return true;
                }
            }

            return false;
        }

        public bool HasSkinsProvider(AbstractSkinsDatabase provider)
        {
            if(!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinsDatabase skinProvider in skinProviders)
                {
                    if (skinProvider == provider)
                        return true;
                }
            }

            return false;
        }
    }
}
