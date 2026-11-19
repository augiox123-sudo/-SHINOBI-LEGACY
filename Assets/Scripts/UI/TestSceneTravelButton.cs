using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShinobiLegacy.UI
{
    /// <summary>
    /// Small scene switcher used only by the first-test build to move between the hub and combat arena.
    /// </summary>
    public sealed class TestSceneTravelButton : MonoBehaviour
    {
        [SerializeField] private string destinationScene;
        [SerializeField] private string buttonLabel = "CHANGE AREA";

        public void Configure(string sceneName, string label)
        {
            destinationScene = sceneName;
            buttonLabel = label;
        }

        private void Start()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject buttonObject = new GameObject("Test Scene Travel Button",
                typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(canvas.transform, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-28f, -28f);
            rect.sizeDelta = new Vector2(230f, 58f);

            Image background = buttonObject.GetComponent<Image>();
            background.color = new Color(0.08f, 0.28f, 0.42f, 0.92f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(LoadDestination);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObject.GetComponent<Text>();
            label.text = buttonLabel;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 21;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
        }

        private void LoadDestination()
        {
            if (!string.IsNullOrEmpty(destinationScene) && Application.CanStreamedLevelBeLoaded(destinationScene))
                SceneManager.LoadScene(destinationScene);
            else
                Debug.LogError("SHINOBI LEGACY: Test scene is not present in Build Settings: " + destinationScene);
        }
    }
}