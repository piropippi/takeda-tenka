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
            Color paper = Hex("E7D7B5");
            Color vermilion = Hex("9D2D24");
            Color gold = Hex("C9A35B");

            CreateTitleBackground(safeArea);

            Text windFire = CreateText(
                "WindFire",
                safeArea,
                "風  林  火  山",
                62,
                FontStyle.Bold,
                new Color(0.78f, 0.64f, 0.36f, 0.1f),
                TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.46f),
                new Vector2(0.95f, 0.54f));
            windFire.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.25f);

            CreateCrest(safeArea, gold);

            Text title = CreateText(
                "Title",
                safeArea,
                "武田晴信\n天下統一伝",
                122,
                FontStyle.Bold,
                gold,
                TextAnchor.MiddleCenter,
                new Vector2(0.06f, 0.68f),
                new Vector2(0.94f, 0.95f));
            title.lineSpacing = 0.82f;
            Outline titleOutline = title.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0f, 0f, 0f, 0.96f);
            titleOutline.effectDistance = new Vector2(6f, -6f);

            RectTransform buttons = CreateRect(
                "Menu",
                safeArea,
                new Vector2(0.12f, 0.1f),
                new Vector2(0.88f, 0.41f));
            VerticalLayoutGroup layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 38f;
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
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.24f);
            colors.disabledColor = background;
            button.colors = colors;

            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = interactable ? Hex("B88A45") : Hex("4A4139");
            outline.effectDistance = new Vector2(3f, -3f);

            Color frameColor = interactable ? Hex("A77B3E") : Hex("4C433B");
            CreateFrameStrip("TopFrame", rect, new Vector2(0f, 0.93f), Vector2.one, frameColor);
            CreateFrameStrip("BottomFrame", rect, Vector2.zero, new Vector2(1f, 0.07f), frameColor);
            CreateFrameStrip("LeftFrame", rect, Vector2.zero, new Vector2(0.025f, 1f), frameColor);
            CreateFrameStrip("RightFrame", rect, new Vector2(0.975f, 0f), Vector2.one, frameColor);

            Text buttonLabel = CreateText(
                "Label",
                rect,
                label,
                50,
                FontStyle.Bold,
                foreground,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one);
            Shadow labelShadow = buttonLabel.gameObject.AddComponent<Shadow>();
            labelShadow.effectColor = new Color(0f, 0f, 0f, interactable ? 0.72f : 0.4f);
            labelShadow.effectDistance = new Vector2(2f, -2f);
            return button;
        }

        private static void CreateCrest(Transform parent, Color color)
        {
            RectTransform crest = CreateRect(
                "TakedaBishi",
                parent,
                new Vector2(0.28f, 0.43f),
                new Vector2(0.72f, 0.66f));

            Vector2[] positions =
            {
                new Vector2(-105f, 105f),
                new Vector2(105f, 105f),
                new Vector2(-105f, -105f),
                new Vector2(105f, -105f)
            };

            foreach (Vector2 position in positions)
            {
                CreateCrestDiamond(crest, position, color);
            }
        }

        private static void CreateTitleBackground(Transform parent)
        {
            RectTransform rect = CreateRect("KaiMorningBackground", parent, Vector2.zero, Vector2.one);
            rect.gameObject.AddComponent<CanvasRenderer>();
            TitleBackgroundGraphic background = rect.gameObject.AddComponent<TitleBackgroundGraphic>();
            background.color = Color.white;
            background.raycastTarget = false;
            background.SetAllDirty();
        }

        private static void CreateCrestDiamond(Transform parent, Vector2 position, Color color)
        {
            RectTransform glow = CreatePanel(
                "DiamondGlow",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Color(color.r, color.g, color.b, 0.1f));
            glow.sizeDelta = new Vector2(161f, 161f);
            glow.anchoredPosition = position;
            glow.localRotation = Quaternion.Euler(0f, 0f, 45f);
            glow.GetComponent<Image>().raycastTarget = false;

            RectTransform diamond = CreatePanel(
                "Diamond",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Color(color.r, color.g, color.b, 0.96f));
            diamond.sizeDelta = new Vector2(145f, 145f);
            diamond.anchoredPosition = position;
            diamond.localRotation = Quaternion.Euler(0f, 0f, 45f);
            diamond.GetComponent<Image>().raycastTarget = false;

            Outline outline = diamond.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.05f, 0.04f, 0.03f, 0.82f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static void CreateFrameStrip(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            RectTransform strip = CreatePanel(name, parent, anchorMin, anchorMax, color);
            strip.GetComponent<Image>().raycastTarget = false;
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

    internal sealed class TitleBackgroundGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rect = GetPixelAdjustedRect();

            AddQuad(
                vertexHelper,
                rect,
                0f,
                0.36f,
                HexColor("42282A"),
                HexColor("42282A"),
                HexColor("724139"),
                HexColor("724139"));
            AddQuad(
                vertexHelper,
                rect,
                0.36f,
                0.7f,
                HexColor("724139"),
                HexColor("724139"),
                HexColor("29212A"),
                HexColor("29212A"));
            AddQuad(
                vertexHelper,
                rect,
                0.7f,
                1f,
                HexColor("29212A"),
                HexColor("29212A"),
                HexColor("11141B"),
                HexColor("11141B"));

            AddQuad(
                vertexHelper,
                rect,
                0.28f,
                0.43f,
                new Color32(119, 61, 49, 0),
                new Color32(119, 61, 49, 0),
                new Color32(190, 101, 66, 54),
                new Color32(190, 101, 66, 54));

            Color32 farMountain = new Color32(44, 43, 51, 245);
            AddMountain(vertexHelper, rect, -0.1f, 0.18f, 0.13f, 0.53f, 0.35f, 0.17f, farMountain);
            AddMountain(vertexHelper, rect, 0.12f, 0.17f, 0.37f, 0.47f, 0.62f, 0.16f, farMountain);
            AddMountain(vertexHelper, rect, 0.45f, 0.16f, 0.72f, 0.51f, 1.06f, 0.17f, farMountain);

            Color32 middleMountain = new Color32(29, 31, 38, 255);
            AddMountain(vertexHelper, rect, -0.08f, 0.1f, 0.2f, 0.38f, 0.5f, 0.1f, middleMountain);
            AddMountain(vertexHelper, rect, 0.25f, 0.1f, 0.53f, 0.35f, 0.82f, 0.1f, middleMountain);
            AddMountain(vertexHelper, rect, 0.58f, 0.1f, 0.83f, 0.4f, 1.1f, 0.1f, middleMountain);

            Color32 foreground = new Color32(19, 20, 24, 255);
            AddMountain(vertexHelper, rect, -0.12f, 0f, 0.12f, 0.25f, 0.4f, 0f, foreground);
            AddMountain(vertexHelper, rect, 0.18f, 0f, 0.47f, 0.29f, 0.73f, 0f, foreground);
            AddMountain(vertexHelper, rect, 0.55f, 0f, 0.81f, 0.23f, 1.12f, 0f, foreground);

            AddQuad(
                vertexHelper,
                rect,
                0.22f,
                0.27f,
                new Color32(197, 183, 165, 8),
                new Color32(197, 183, 165, 8),
                new Color32(197, 183, 165, 35),
                new Color32(197, 183, 165, 35));
            AddQuad(
                vertexHelper,
                rect,
                0.31f,
                0.355f,
                new Color32(211, 193, 174, 28),
                new Color32(211, 193, 174, 28),
                new Color32(211, 193, 174, 5),
                new Color32(211, 193, 174, 5));
            AddQuad(
                vertexHelper,
                rect,
                0f,
                0.12f,
                new Color32(8, 9, 12, 255),
                new Color32(8, 9, 12, 255),
                new Color32(18, 18, 21, 225),
                new Color32(18, 18, 21, 225));
        }

        private static void AddMountain(
            VertexHelper vertexHelper,
            Rect rect,
            float leftX,
            float baseY,
            float peakX,
            float peakY,
            float rightX,
            float rightY,
            Color32 color)
        {
            int start = vertexHelper.currentVertCount;
            vertexHelper.AddVert(Point(rect, leftX, baseY), color, Vector2.zero);
            vertexHelper.AddVert(Point(rect, peakX, peakY), color, Vector2.zero);
            vertexHelper.AddVert(Point(rect, rightX, rightY), color, Vector2.zero);
            vertexHelper.AddTriangle(start, start + 1, start + 2);
        }

        private static void AddQuad(
            VertexHelper vertexHelper,
            Rect rect,
            float bottom,
            float top,
            Color32 bottomLeft,
            Color32 bottomRight,
            Color32 topLeft,
            Color32 topRight)
        {
            int start = vertexHelper.currentVertCount;
            vertexHelper.AddVert(Point(rect, 0f, bottom), bottomLeft, Vector2.zero);
            vertexHelper.AddVert(Point(rect, 0f, top), topLeft, Vector2.up);
            vertexHelper.AddVert(Point(rect, 1f, top), topRight, Vector2.one);
            vertexHelper.AddVert(Point(rect, 1f, bottom), bottomRight, Vector2.right);
            vertexHelper.AddTriangle(start, start + 1, start + 2);
            vertexHelper.AddTriangle(start + 2, start + 3, start);
        }

        private static Vector2 Point(Rect rect, float normalizedX, float normalizedY)
        {
            return new Vector2(
                Mathf.Lerp(rect.xMin, rect.xMax, normalizedX),
                Mathf.Lerp(rect.yMin, rect.yMax, normalizedY));
        }

        private static Color32 HexColor(string value)
        {
            return ColorUtility.TryParseHtmlString("#" + value, out Color color)
                ? color
                : Color.black;
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
