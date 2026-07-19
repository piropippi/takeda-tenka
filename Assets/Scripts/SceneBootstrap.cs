using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TakedaHarunobu
{
    internal static class SceneBootstrap
    {
        private const string RootName = "RuntimeScreenRoot";
        private static Font japaneseFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHandler()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildInitialScene()
        {
            BuildScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BuildScene(scene);
        }

        private static void BuildScene(Scene scene)
        {
            if (GameObject.Find(RootName) != null)
            {
                return;
            }

            switch (scene.name)
            {
                case "TitleScene":
                    BuildTitleScene();
                    break;
                case "StageSelectScene":
                    BuildStageSelectScene();
                    break;
            }
        }

        private static void BuildTitleScene()
        {
            RectTransform safeArea = CreateScreenRoot();
            Color ink = Hex("161414");
            Color paper = Hex("E7D7B5");
            Color vermilion = Hex("9D2D24");
            Color gold = Hex("C9A35B");

            CreatePanel("Background", safeArea, Vector2.zero, Vector2.one, ink);
            CreatePanel("TopWash", safeArea, new Vector2(0f, 0.68f), Vector2.one, Hex("291B1A"));
            CreatePanel("BottomWash", safeArea, Vector2.zero, new Vector2(1f, 0.23f), Hex("211A17"));

            Text windFire = CreateText(
                "WindFire",
                safeArea,
                "風  林  火  山",
                76,
                FontStyle.Bold,
                new Color(0.78f, 0.64f, 0.36f, 0.14f),
                TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.52f),
                new Vector2(0.95f, 0.68f));
            windFire.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.25f);

            CreateCrest(safeArea, gold);

            Text title = CreateText(
                "Title",
                safeArea,
                "武田晴信\n天下統一伝",
                96,
                FontStyle.Bold,
                paper,
                TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.64f),
                new Vector2(0.92f, 0.9f));
            Outline titleOutline = title.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            titleOutline.effectDistance = new Vector2(4f, -4f);

            RectTransform buttons = CreateRect(
                "Menu",
                safeArea,
                new Vector2(0.13f, 0.13f),
                new Vector2(0.87f, 0.47f));
            VerticalLayoutGroup layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 26f;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            Button startButton = CreateMenuButton(buttons, "はじめから", vermilion, paper, true);
            startButton.onClick.AddListener(() => SceneManager.LoadScene("StageSelectScene"));
            CreateMenuButton(buttons, "つづきから", Hex("38322E"), Hex("8F8678"), false);
            CreateMenuButton(buttons, "図鑑", Hex("38322E"), Hex("8F8678"), false);
        }

        private static void BuildStageSelectScene()
        {
            RectTransform safeArea = CreateScreenRoot();
            Color ink = Hex("171514");
            Color paper = Hex("E7D7B5");
            Color vermilion = Hex("9D2D24");
            Color gold = Hex("C9A35B");

            CreatePanel("Background", safeArea, Vector2.zero, Vector2.one, ink);
            CreatePanel("HeaderBand", safeArea, new Vector2(0f, 0.82f), Vector2.one, Hex("291B1A"));
            CreatePanel("HeaderRule", safeArea, new Vector2(0f, 0.815f), new Vector2(1f, 0.823f), gold);

            CreateText(
                "GameTitle",
                safeArea,
                "武田晴信 天下統一伝",
                38,
                FontStyle.Normal,
                gold,
                TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.91f),
                new Vector2(0.95f, 0.98f));

            CreateText(
                "PageTitle",
                safeArea,
                "章 選 択",
                70,
                FontStyle.Bold,
                paper,
                TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.82f),
                new Vector2(0.95f, 0.92f));

            RectTransform card = CreatePanel(
                "SuwaChapter",
                safeArea,
                new Vector2(0.08f, 0.42f),
                new Vector2(0.92f, 0.72f),
                Hex("302923"));
            Outline cardOutline = card.gameObject.AddComponent<Outline>();
            cardOutline.effectColor = gold;
            cardOutline.effectDistance = new Vector2(3f, -3f);

            CreatePanel(
                "ChapterBand",
                card,
                new Vector2(0f, 0.72f),
                Vector2.one,
                vermilion);
            CreateText(
                "Chapter",
                card,
                "第一章",
                50,
                FontStyle.Bold,
                paper,
                TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.72f),
                new Vector2(0.95f, 0.98f));
            CreateText(
                "StageName",
                card,
                "諏訪攻略",
                88,
                FontStyle.Bold,
                paper,
                TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.12f),
                new Vector2(0.95f, 0.72f));
        }

        private static RectTransform CreateScreenRoot()
        {
            GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject safeAreaObject = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            RectTransform safeArea = safeAreaObject.GetComponent<RectTransform>();
            safeArea.SetParent(root.transform, false);
            Stretch(safeArea, Vector2.zero, Vector2.one);

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            return safeArea;
        }

        private static Button CreateMenuButton(
            Transform parent,
            string label,
            Color background,
            Color foreground,
            bool interactable)
        {
            RectTransform rect = CreateRect(label + "Button", parent, Vector2.zero, Vector2.one);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = background;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.interactable = interactable;

            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.16f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.2f);
            colors.disabledColor = background;
            button.colors = colors;

            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = interactable ? Hex("C9A35B") : Hex("5A5249");
            outline.effectDistance = new Vector2(2f, -2f);

            CreateText(
                "Label",
                rect,
                label,
                48,
                FontStyle.Bold,
                foreground,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one);
            return button;
        }

        private static void CreateCrest(Transform parent, Color color)
        {
            RectTransform crest = CreateRect(
                "TakedaCrestPlaceholder",
                parent,
                new Vector2(0.37f, 0.48f),
                new Vector2(0.63f, 0.62f));

            Vector2[] anchors =
            {
                new Vector2(0.28f, 0.55f),
                new Vector2(0.5f, 0.55f),
                new Vector2(0.39f, 0.31f),
                new Vector2(0.61f, 0.31f)
            };

            foreach (Vector2 anchor in anchors)
            {
                RectTransform diamond = CreatePanel(
                    "Diamond",
                    crest,
                    anchor,
                    anchor,
                    new Color(color.r, color.g, color.b, 0.54f));
                diamond.sizeDelta = new Vector2(120f, 120f);
                diamond.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
            Text text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = GetJapaneseFont();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
            rect.gameObject.AddComponent<Image>().color = color;
            return rect;
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect, anchorMin, anchorMax);
            return rect;
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Font GetJapaneseFont()
        {
            if (japaneseFont != null)
            {
                return japaneseFont;
            }

            japaneseFont = Font.CreateDynamicFontFromOSFont(
                new[]
                {
                    "Yu Gothic",
                    "Yu Gothic UI",
                    "Meiryo",
                    "Hiragino Sans",
                    "Noto Sans CJK JP",
                    "Noto Sans JP",
                    "Arial"
                },
                64);

            if (japaneseFont == null)
            {
                japaneseFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return japaneseFont;
        }

        private static Color Hex(string value)
        {
            return ColorUtility.TryParseHtmlString("#" + value, out Color color)
                ? color
                : Color.white;
        }
    }

    internal sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea ||
                lastScreenSize.x != Screen.width ||
                lastScreenSize.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            Rect safeArea = Screen.safeArea;
            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(
                safeArea.xMin / Screen.width,
                safeArea.yMin / Screen.height);
            rect.anchorMax = new Vector2(
                safeArea.xMax / Screen.width,
                safeArea.yMax / Screen.height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
