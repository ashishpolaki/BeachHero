using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Watermelon
{
    public class PromotionWindow : EditorWindow
    {
        private const string PREFS_KEY = "PromotionMD5";

        private const string PROMOTION_URL = @"https://wmelongames.com/promotion/promo.php";

        private const float MAX_WIDTH = 500;
        private const float EXTRA_HEIGHT = 70;

        [SerializeField] Texture2D assetStoreButtonTexture;

        private static string url;
        private static Texture2D promotionTexture;

        private static PromotionCase promotionCase;

        private static GUIStyle backgroundStyle;

        [InitializeOnLoadMethod]
        public static void StartupCheck()
        {
            if (!CustomCoreSettings.ShowWatermelonPromotions) return;

            string storedMD5 = EditorPrefs.GetString(PREFS_KEY, "");

            if (promotionCase != null) return;

            promotionCase = new PromotionCase();

            EditorCoroutines.Execute(promotionCase.GetRequest(PROMOTION_URL, () =>
            {
                if(promotionCase.MD5 != storedMD5)
                {
                    EditorCoroutines.Execute(promotionCase.GetTexture((texture) =>
                    {
                        EditorPrefs.SetString(PREFS_KEY, promotionCase.MD5);

                        Color windowBackgroundColor = new Color(0.96f, 0.96f, 0.96f, 1.0f);

                        Texture2D backgroundTexture = new Texture2D(4, 4);
                        for(int x = 0; x < backgroundTexture.width; x++)
                        {
                            for(int y = 0; y < backgroundTexture.height; y++)
                            {
                                backgroundTexture.SetPixel(x, y, windowBackgroundColor);
                            }
                        }

                        backgroundTexture.filterMode = FilterMode.Point;
                        backgroundTexture.wrapMode = TextureWrapMode.Repeat;
                        backgroundTexture.Apply();

                        backgroundStyle = new GUIStyle();
                        backgroundStyle.normal.background = backgroundTexture;

                        Vector2 windowWidth = GetWidth(texture);

                        promotionTexture = texture;
                        url = promotionCase.Url;

                        PromotionWindow window = ScriptableObject.CreateInstance<PromotionWindow>();

                        window.minSize = windowWidth;
                        window.maxSize = windowWidth;
                        window.titleContent = new GUIContent(promotionCase.Name, EditorCustomStyles.GetIcon("icon_title"));

                        window.ShowModal();
                    }));
                }
            }));
        }

        private static Vector2 GetWidth(Texture2D texture)
        {
            // Calculate aspect ratio
            float aspectRatio = (float)texture.height / texture.width;

            // Calculate height based on locked width and aspect ratio
            float calculatedHeight = MAX_WIDTH * aspectRatio;

            // Set window size
            return new Vector2(MAX_WIDTH, calculatedHeight + EXTRA_HEIGHT);
        }

        protected void OnEnable()
        {
            EditorCustomStyles.CheckStyles();
        }

        private void OnGUI()
        {
            if(promotionTexture == null)
            {
                Close();

                return;
            }

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, backgroundStyle);

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height - EXTRA_HEIGHT), promotionTexture);

            if (GUI.Button(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, GUIStyle.none))
            {
                Application.OpenURL(url);
            }

            if (assetStoreButtonTexture != null)
            {
                // Calculate the position to draw the texture
                float textureWidth = assetStoreButtonTexture.width;
                float textureHeight = assetStoreButtonTexture.height;
                float x = (position.width - textureWidth) / 2; // Center the texture horizontally
                float y = position.height - textureHeight - (EXTRA_HEIGHT / 2 - textureHeight / 2);

                // Draw the texture
                GUI.DrawTexture(new Rect(x, y, textureWidth, textureHeight), assetStoreButtonTexture, ScaleMode.ScaleToFit, true);
            }
        }

        private class PromotionCase
        {
            public string Name { get; private set; }
            public string Url { get; private set; }
            public string MD5 { get; private set; }
            public Texture2D Texture { get; private set; }

            public bool IsLoaded { get; private set; }

            private string imageURL;

            public PromotionCase()
            {
                IsLoaded = false;
            }

            public IEnumerator GetRequest(string uri, SimpleCallback completeCallback)
            {
                UnityWebRequest www = UnityWebRequest.Get(uri);
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    // Or retrieve results as binary data
                    byte[] results = www.downloadHandler.data;

                    // For that you will need to add reference to System.Runtime.Serialization
                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(results, new System.Xml.XmlDictionaryReaderQuotas());

                    // For that you will need to add reference to System.Xml and System.Xml.Linq
                    var root = XElement.Load(jsonReader);

                    Name = root.XPathSelectElement("name").Value;
                    Url = root.XPathSelectElement("link").Value;
                    MD5 = root.XPathSelectElement("md5").Value;

                    imageURL = root.XPathSelectElement("url").Value;

                    completeCallback?.Invoke();
                }
            }

            public IEnumerator GetTexture(System.Action<Texture2D> loadCallback)
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageURL);
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    if (myTexture != null)
                    {
                        myTexture.filterMode = FilterMode.Bilinear;  // Use Bilinear for smoother but sharper scaling
                        myTexture.wrapMode = TextureWrapMode.Clamp;  // Prevents tiling, which can cause visible seams
                        myTexture.anisoLevel = 2;                    // Improves quality at steep angles
                        myTexture.Apply(updateMipmaps: false);       // Update without generating mipmaps for sharper detail

                        loadCallback.Invoke(myTexture);
                    }
                }
            }
        }
    }
}