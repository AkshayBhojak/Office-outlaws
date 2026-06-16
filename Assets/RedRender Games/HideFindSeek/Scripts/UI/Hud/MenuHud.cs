using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.UI
{
    public sealed class MenuHud : MonoBehaviour
    {
        [SerializeField] public Button BtnSeek;
        [SerializeField] public Button BtnHide;
        [SerializeField] public TextMeshProUGUI TxtLevelName;

        private TextMeshProUGUI _txtTitle;

        public void SetupTitle()
        {
            if (_txtTitle != null) return;

            // Create a Title GameObject under MenuHud's transform
            GameObject titleGo = new GameObject("GameTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(transform, false);

            _txtTitle = titleGo.GetComponent<TextMeshProUGUI>();
            _txtTitle.text = "<color=#ffbb33>OFFICE</color> <color=#ff4444>OUTLAWS</color>\n<size=45%><color=#ffffff>BOSS ATTACK</color></size>";
            _txtTitle.font = TxtLevelName.font; // Reuse same TMP font asset
            _txtTitle.fontSize = 90;
            _txtTitle.alignment = TextAlignmentOptions.Center;
            _txtTitle.enableVertexGradient = true;

            // Positioning Title above Level text
            RectTransform rect = titleGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 320f);
            rect.sizeDelta = new Vector2(800f, 250f);

            // Shift TxtLevelName slightly below the Title to make it look clean
            RectTransform levelRect = TxtLevelName.GetComponent<RectTransform>();
            levelRect.anchoredPosition = new Vector2(0f, 150f);
        }

        public void AnimateMenu()
        {
            SetupTitle();

            // Adjust button dimensions and positions to fit the text cleanly
            RectTransform hideRect = BtnHide.GetComponent<RectTransform>();
            RectTransform seekRect = BtnSeek.GetComponent<RectTransform>();
            if (hideRect != null && seekRect != null)
            {
                hideRect.sizeDelta = new Vector2(230f, 100f);
                seekRect.sizeDelta = new Vector2(230f, 100f);

                // Space them out symmetrically on the horizontal axis
                hideRect.anchoredPosition = new Vector2(-135f, hideRect.anchoredPosition.y);
                seekRect.anchoredPosition = new Vector2(135f, seekRect.anchoredPosition.y);
            }

            // Dynamic renaming of buttons to match the Office Outlaws theme!
            var seekText = BtnSeek.GetComponentInChildren<TextMeshProUGUI>();
            if (seekText != null)
            {
                seekText.text = "BOSS";
                seekText.fontSize = 28f;
                seekText.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                var legacySeek = BtnSeek.GetComponentInChildren<Text>();
                if (legacySeek != null)
                {
                    legacySeek.text = "BOSS";
                    legacySeek.fontSize = 28;
                    legacySeek.alignment = TextAnchor.MiddleCenter;
                }
            }

            var hideText = BtnHide.GetComponentInChildren<TextMeshProUGUI>();
            if (hideText != null)
            {
                hideText.text = "EMPLOYEE";
                hideText.fontSize = 28f;
                hideText.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                var legacyHide = BtnHide.GetComponentInChildren<Text>();
                if (legacyHide != null)
                {
                    legacyHide.text = "EMPLOYEE";
                    legacyHide.fontSize = 28;
                    legacyHide.alignment = TextAnchor.MiddleCenter;
                }
            }

            // 1. Animate Title
            if (_txtTitle != null)
            {
                _txtTitle.transform.localScale = Vector3.zero;
                _txtTitle.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack).SetUpdate(true);

                // Idle floating animation
                _txtTitle.transform.DOLocalMoveY(335f, 1.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            }

            // 2. Animate Level Name
            TxtLevelName.transform.localScale = Vector3.zero;
            TxtLevelName.transform.DOScale(Vector3.one, 0.6f).SetDelay(0.2f).SetEase(Ease.OutBack).SetUpdate(true);

            // 3. Animate Buttons
            BtnHide.transform.localScale = Vector3.zero;
            BtnSeek.transform.localScale = Vector3.zero;

            BtnHide.transform.DOScale(Vector3.one, 0.6f).SetDelay(0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            BtnSeek.transform.DOScale(Vector3.one, 0.6f).SetDelay(0.4f).SetEase(Ease.OutBack).SetUpdate(true);

            // 4. Attach Hover and Click Animators if not present
            if (BtnHide.GetComponent<UIButtonAnimator>() == null)
            {
                BtnHide.gameObject.AddComponent<UIButtonAnimator>();
            }
            if (BtnSeek.GetComponent<UIButtonAnimator>() == null)
            {
                BtnSeek.gameObject.AddComponent<UIButtonAnimator>();
            }

            // 5. Setup and display Difficulty Selector buttons!
            SetupDifficultyButtons();

            // 6. Setup and display Levels selection button!
            SetupLevelsButton();
        }

        private Button _btnLevels;
        private GameObject _levelPanel;
        private System.Collections.Generic.List<Button> _levelButtons = new System.Collections.Generic.List<Button>();
        public System.Action<int> LevelSelected;

        private void SetupLevelsButton()
        {
            if (_btnLevels != null) return;

            // Duplicate BtnSeek to copy all visual styles
            GameObject levelsGo = Instantiate(BtnSeek.gameObject, transform);
            levelsGo.name = "BtnLevels";

            // Positioning & Sizing: neat horizontal button positioned vertically in the center
            RectTransform rectLevels = levelsGo.GetComponent<RectTransform>();
            rectLevels.sizeDelta = new Vector2(230f, 85f);
            rectLevels.anchoredPosition = new Vector2(0f, -125f); // perfectly centered between difficulty and mode selection

            _btnLevels = levelsGo.GetComponent<Button>();
            _btnLevels.onClick.RemoveAllListeners();
            _btnLevels.onClick.AddListener(OpenLevelSelectionPanel);

            // Style levels button text
            var levelsText = levelsGo.GetComponentInChildren<TextMeshProUGUI>();
            if (levelsText != null)
            {
                levelsText.text = "LEVELS";
                levelsText.fontSize = 26f;
                levelsText.alignment = TextAlignmentOptions.Center;
            }

            // Style levels button image color: professional light grey/steel
            var img = levelsGo.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.35f, 0.35f, 0.4f, 0.9f);
            }

            if (levelsGo.GetComponent<UIButtonAnimator>() == null)
            {
                levelsGo.AddComponent<UIButtonAnimator>();
            }

            // Animate entry
            levelsGo.transform.localScale = Vector3.zero;
            levelsGo.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void OpenLevelSelectionPanel()
        {
            if (_levelPanel == null)
            {
                SetupLevelSelectionPanel();
            }

            RefreshLevelButtons();

            _levelPanel.SetActive(true);
            
            var panelBox = _levelPanel.transform.Find("PanelBox");
            if (panelBox != null)
            {
                panelBox.localScale = Vector3.zero;
                panelBox.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        private void SetupLevelSelectionPanel()
        {
            // 1. Create main panel root
            GameObject panelGo = new GameObject("LevelSelectionPanel", typeof(RectTransform));
            panelGo.transform.SetParent(transform, false);
            _levelPanel = panelGo;

            RectTransform rectPanel = panelGo.GetComponent<RectTransform>();
            rectPanel.anchorMin = Vector2.zero;
            rectPanel.anchorMax = Vector2.one;
            rectPanel.sizeDelta = Vector2.zero;
            rectPanel.anchoredPosition = Vector2.zero;

            Image bgOverlay = panelGo.AddComponent<Image>();
            bgOverlay.color = new Color(0f, 0f, 0f, 0.85f);
            bgOverlay.raycastTarget = true;

            // 2. Create PanelBox container
            GameObject boxGo = new GameObject("PanelBox", typeof(RectTransform), typeof(Image));
            boxGo.transform.SetParent(panelGo.transform, false);
            RectTransform rectBox = boxGo.GetComponent<RectTransform>();
            rectBox.anchorMin = new Vector2(0.5f, 0.5f);
            rectBox.anchorMax = new Vector2(0.5f, 0.5f);
            rectBox.sizeDelta = new Vector2(600f, 850f);
            rectBox.anchoredPosition = Vector2.zero;

            Image boxImg = boxGo.GetComponent<Image>();
            boxImg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);
            
            var outline = boxGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.35f, 0.4f, 0.5f);
            outline.effectDistance = new Vector2(3f, 3f);

            // 3. Header Text
            GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(boxGo.transform, false);
            RectTransform rectTitle = titleGo.GetComponent<RectTransform>();
            rectTitle.anchorMin = new Vector2(0.5f, 1f);
            rectTitle.anchorMax = new Vector2(0.5f, 1f);
            rectTitle.anchoredPosition = new Vector2(0f, -60f);
            rectTitle.sizeDelta = new Vector2(500f, 60f);

            var titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
            titleTxt.text = "<color=#ffbb33>SELECT LEVEL</color>";
            titleTxt.font = TxtLevelName.font;
            titleTxt.fontSize = 42f;
            titleTxt.alignment = TextAlignmentOptions.Center;

            // 4. ScrollView container
            GameObject scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(boxGo.transform, false);
            RectTransform rectScroll = scrollGo.GetComponent<RectTransform>();
            rectScroll.anchorMin = new Vector2(0.5f, 0.5f);
            rectScroll.anchorMax = new Vector2(0.5f, 0.5f);
            rectScroll.anchoredPosition = new Vector2(0f, 10f);
            rectScroll.sizeDelta = new Vector2(540f, 580f);

            ScrollRect scrollRect = scrollGo.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // 5. Viewport
            GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            RectTransform rectViewport = viewportGo.GetComponent<RectTransform>();
            rectViewport.anchorMin = Vector2.zero;
            rectViewport.anchorMax = Vector2.one;
            rectViewport.sizeDelta = Vector2.zero;
            rectViewport.anchoredPosition = Vector2.zero;

            // 6. Content
            GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            RectTransform rectContent = contentGo.GetComponent<RectTransform>();
            rectContent.anchorMin = new Vector2(0.5f, 1f);
            rectContent.anchorMax = new Vector2(0.5f, 1f);
            rectContent.pivot = new Vector2(0.5f, 1f);
            rectContent.anchoredPosition = Vector2.zero;
            rectContent.sizeDelta = new Vector2(520f, 0f);

            scrollRect.viewport = rectViewport;
            scrollRect.content = rectContent;

            GridLayoutGroup grid = contentGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(95f, 95f);
            grid.spacing = new Vector2(25f, 25f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.padding = new RectOffset(25, 25, 20, 20);

            ContentSizeFitter fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // 7. Instantiate 100 Level Buttons
            _levelButtons.Clear();
            for (int i = 1; i <= 100; i++)
            {
                int levelIndex = i;
                GameObject btnGo = Instantiate(BtnSeek.gameObject, contentGo.transform);
                btnGo.name = "LevelBtn_" + levelIndex;

                Button btn = btnGo.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));

                RectTransform rectBtn = btnGo.GetComponent<RectTransform>();
                rectBtn.sizeDelta = new Vector2(95f, 95f);

                if (btnGo.GetComponent<UIButtonAnimator>() == null)
                {
                    btnGo.AddComponent<UIButtonAnimator>();
                }

                _levelButtons.Add(btn);
            }

            // 8. Close Button
            GameObject closeGo = Instantiate(BtnSeek.gameObject, boxGo.transform);
            closeGo.name = "BtnCloseLevels";
            RectTransform rectClose = closeGo.GetComponent<RectTransform>();
            rectClose.anchorMin = new Vector2(0.5f, 0f);
            rectClose.anchorMax = new Vector2(0.5f, 0f);
            rectClose.anchoredPosition = new Vector2(0f, 50f);
            rectClose.sizeDelta = new Vector2(200f, 65f);

            Button closeBtn = closeGo.GetComponent<Button>();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() => _levelPanel.SetActive(false));

            var closeText = closeGo.GetComponentInChildren<TextMeshProUGUI>();
            if (closeText != null)
            {
                closeText.text = "CLOSE";
                closeText.fontSize = 24f;
                closeText.alignment = TextAlignmentOptions.Center;
            }

            var closeImg = closeGo.GetComponent<Image>();
            if (closeImg != null)
            {
                closeImg.color = new Color(0.35f, 0.35f, 0.4f, 0.9f);
            }

            if (closeGo.GetComponent<UIButtonAnimator>() == null)
            {
                closeGo.AddComponent<UIButtonAnimator>();
            }

            _levelPanel.SetActive(false);
        }

        private void RefreshLevelButtons()
        {
            if (_levelButtons == null || _levelButtons.Count == 0) return;

            int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
            
            Color unlockedColor = new Color(1f, 0.62f, 0.15f, 1f); // Vibrant Orange
            Color lockedColor = new Color(0.2f, 0.2f, 0.22f, 0.6f); // Dimmed Dark Grey

            for (int i = 0; i < _levelButtons.Count; i++)
            {
                int levelNum = i + 1;
                var btn = _levelButtons[i];
                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                var img = btn.GetComponent<Image>();

                if (levelNum <= maxUnlocked)
                {
                    btn.interactable = true;
                    if (img != null) img.color = unlockedColor;
                    if (text != null)
                    {
                        text.text = levelNum.ToString();
                        text.fontSize = 28f;
                    }
                }
                else
                {
                    btn.interactable = false;
                    if (img != null) img.color = lockedColor;
                    if (text != null)
                    {
                        text.text = levelNum + "\n<size=70%>🔒</size>";
                        text.fontSize = 24f;
                    }
                }
            }
        }

        private void OnLevelButtonClicked(int levelIndex)
        {
            _levelPanel.SetActive(false);
            LevelSelected?.Invoke(levelIndex);
        }

        private Button _btnEasy;
        private Button _btnMedium;
        private Button _btnHard;
        private TextMeshProUGUI _txtEasyLabel;
        private TextMeshProUGUI _txtMediumLabel;
        private TextMeshProUGUI _txtHardLabel;

        private void SetupDifficultyButtons()
        {
            if (_btnEasy != null) return;

            // Duplicate BtnSeek to copy all visual styles
            GameObject easyGo = Instantiate(BtnSeek.gameObject, transform);
            GameObject mediumGo = Instantiate(BtnSeek.gameObject, transform);
            GameObject hardGo = Instantiate(BtnSeek.gameObject, transform);

            easyGo.name = "BtnEasy";
            mediumGo.name = "BtnMedium";
            hardGo.name = "BtnHard";

            // Get RectTransforms
            RectTransform rectEasy = easyGo.GetComponent<RectTransform>();
            RectTransform rectMedium = mediumGo.GetComponent<RectTransform>();
            RectTransform rectHard = hardGo.GetComponent<RectTransform>();

            // Sizing: small neat buttons
            Vector2 btnSize = new Vector2(160f, 75f);
            rectEasy.sizeDelta = btnSize;
            rectMedium.sizeDelta = btnSize;
            rectHard.sizeDelta = btnSize;

            // Position them horizontally in a row between Level text and Mode buttons
            float yPos = 10f; // Perfect center y-position
            rectEasy.anchoredPosition = new Vector2(-180f, yPos);
            rectMedium.anchoredPosition = new Vector2(0f, yPos);
            rectHard.anchoredPosition = new Vector2(180f, yPos);

            // Configure Buttons and Text Labels
            _btnEasy = easyGo.GetComponent<Button>();
            _btnMedium = mediumGo.GetComponent<Button>();
            _btnHard = hardGo.GetComponent<Button>();

            _btnEasy.onClick.RemoveAllListeners();
            _btnMedium.onClick.RemoveAllListeners();
            _btnHard.onClick.RemoveAllListeners();

            _btnEasy.onClick.AddListener(() => SetDifficulty(0));
            _btnMedium.onClick.AddListener(() => SetDifficulty(1));
            _btnHard.onClick.AddListener(() => SetDifficulty(2));

            _txtEasyLabel = easyGo.GetComponentInChildren<TextMeshProUGUI>();
            _txtMediumLabel = mediumGo.GetComponentInChildren<TextMeshProUGUI>();
            _txtHardLabel = hardGo.GetComponentInChildren<TextMeshProUGUI>();

            if (_txtEasyLabel != null) { _txtEasyLabel.text = "EASY"; _txtEasyLabel.fontSize = 24; }
            if (_txtMediumLabel != null) { _txtMediumLabel.text = "MEDIUM"; _txtMediumLabel.fontSize = 24; }
            if (_txtHardLabel != null) { _txtHardLabel.text = "HARD"; _txtHardLabel.fontSize = 24; }

            // Add animators
            if (easyGo.GetComponent<UIButtonAnimator>() == null) easyGo.AddComponent<UIButtonAnimator>();
            if (mediumGo.GetComponent<UIButtonAnimator>() == null) mediumGo.AddComponent<UIButtonAnimator>();
            if (hardGo.GetComponent<UIButtonAnimator>() == null) hardGo.AddComponent<UIButtonAnimator>();

            // Animate their entry
            easyGo.transform.localScale = Vector3.zero;
            mediumGo.transform.localScale = Vector3.zero;
            hardGo.transform.localScale = Vector3.zero;

            easyGo.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            mediumGo.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            hardGo.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.4f).SetEase(Ease.OutBack).SetUpdate(true);

            // Apply active states colors
            RefreshDifficultyUI();
        }

        private void SetDifficulty(int diff)
        {
            PlayerPrefs.SetInt("Difficulty", diff);
            PlayerPrefs.Save();
            RefreshDifficultyUI();
            Debug.Log($"[MenuHud] Difficulty changed to: {(diff == 0 ? "EASY" : diff == 1 ? "MEDIUM" : "HARD")}");
        }

        private void RefreshDifficultyUI()
        {
            int currentDiff = PlayerPrefs.GetInt("Difficulty", 1);

            // Active vs Inactive Colors
            Color activeColor = new Color(1f, 0.62f, 0.15f, 1f); // Vibrant Orange
            Color inactiveColor = new Color(0.25f, 0.25f, 0.3f, 0.85f); // Dim Steel Grey

            if (_btnEasy != null) _btnEasy.GetComponent<Image>().color = (currentDiff == 0) ? activeColor : inactiveColor;
            if (_btnMedium != null) _btnMedium.GetComponent<Image>().color = (currentDiff == 1) ? activeColor : inactiveColor;
            if (_btnHard != null) _btnHard.GetComponent<Image>().color = (currentDiff == 2) ? activeColor : inactiveColor;
        }
    }
}