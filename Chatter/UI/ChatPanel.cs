﻿using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Chatter {
  public class ChatPanel {
    public GameObject Panel { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }

    public GameObject Grabber { get; private set; }
    public GameObject Viewport { get; private set; }
    public GameObject Content { get; private set; }
    public Image ContentImage { get; private set; }
    public ScrollRect ScrollRect { get; private set; }
    public GameObject TextPrefab { get; private set; }
    public InputField InputField { get; private set; }

    public ChatPanel(Transform parentTransform, Text parentText) {
      Panel = CreatePanel(parentTransform);
      CanvasGroup = Panel.GetComponent<CanvasGroup>();

      Viewport = CreateViewport(Panel.transform);
      Content = CreateContent(Viewport.transform);
      ContentImage = Content.GetComponent<Image>();
      ScrollRect = CreateScrollRect(Panel, Viewport, Content);
      TextPrefab = CreateTextPrefab(parentText);
      InputField = CreateChatInputField(Panel.transform);
      Grabber = CreateGrabber(Panel.transform);

      _panelRectTransform = Panel.GetComponent<RectTransform>();
      _viewportRectTransform = Viewport.GetComponent<RectTransform>();
      _viewportRectMask = Viewport.GetComponent<RectMask2D>();
      _viewportImage = Viewport.GetComponent<Image>();
      _contentLayoutGroup = Content.GetComponent<VerticalLayoutGroup>();
      _inputFieldImage = InputField.GetComponentInParent<Image>();
      _textPrefabText = TextPrefab.GetComponent<Text>();
      _grabberCanvasGroup = Grabber.GetComponent<CanvasGroup>();
      _contentWidthOffset = _contentLayoutGroup.padding.horizontal * -1f;
      _panelRectTransform.SetAsFirstSibling();
    }

    const string ContentRowBodyName = "Message.Row.Text";

    readonly RectTransform _panelRectTransform;
    readonly RectTransform _viewportRectTransform;
    readonly RectMask2D _viewportRectMask;
    readonly Image _viewportImage;
    readonly VerticalLayoutGroup _contentLayoutGroup;
    readonly Image _inputFieldImage;
    readonly Text _textPrefabText;
    readonly CanvasGroup _grabberCanvasGroup;

    float _contentWidthOffset = 0f;

    public void SetFont(Font font) {
      _textPrefabText.font = font;

      foreach (Text text in Panel.GetComponentsInChildren<Text>()) {
        text.font = font;
      }
    }

    public void SetFontSize(int fontSize) {
      _textPrefabText.fontSize = fontSize;

      foreach (Text text in Panel.GetComponentsInChildren<Text>()) {
        text.fontSize = text.name == ContentRowBodyName ? fontSize : fontSize - 2;
      }
    }

    public void SetPanelBackgroundColor(Color color) {
      _viewportImage.color = color;
      _inputFieldImage.color = color;
    }

    public void SetPanelRectMaskSoftness(Vector2 softness) {
      _viewportRectMask.softness = Vector2Int.RoundToInt(softness);
    }

    public void SetPanelPosition(Vector2 position) {
      _panelRectTransform.anchoredPosition = position;
    }

    public void SetPanelSize(Vector2 sizeDelta) {
      _panelRectTransform.sizeDelta = sizeDelta;
      _viewportRectTransform.sizeDelta = sizeDelta;
      SetRowLayoutPreferredWidths();
    }

    public void SetContentWidthOffset(float widthOffset) {
      _contentWidthOffset = widthOffset;
      SetRowLayoutPreferredWidths();
    }

    void SetRowLayoutPreferredWidths() {
      float preferredWidth = _panelRectTransform.sizeDelta.x + _contentWidthOffset;

      foreach (
          LayoutElement layout
              in Content.GetComponentsInChildren<LayoutElement>().Where(layout => layout.name == ContentRowBodyName)) {
        layout.preferredWidth = preferredWidth;
      }
    }

    public void SetContentSpacing(float spacing) {
      _contentLayoutGroup.spacing = spacing;
    }

    public void ToggleGrabber(bool toggle) {
      _grabberCanvasGroup.alpha = toggle ? 1f : 0f;
      _grabberCanvasGroup.blocksRaycasts = toggle;
    }

    public void SetVerticalScrollPosition(float position) {
      ScrollRect.verticalNormalizedPosition = position;
    }

    GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = new("ChatPanel", typeof(RectTransform));
      panel.SetParent(parentTransform);

      panel.GetComponent<RectTransform>()
          .SetAnchorMin(new(1f, 0f))
          .SetAnchorMax(new(1f, 0f))
          .SetPivot(new(1f, 0f))
          .SetPosition(Vector2.zero);

      panel.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false);

      panel.AddComponent<CanvasGroup>();

      return panel;
    }

    static GameObject CreateGrabber(Transform parentTransform) {
      GameObject grabber = new("ChatPanel.Grabber", typeof(RectTransform));
      grabber.SetParent(parentTransform);

      grabber.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      grabber.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(height: 5f);

      grabber.AddComponent<Image>()
          .SetColor(new Color32(255, 255, 255, 32))
          .SetRaycastTarget(true);

      grabber.AddComponent<CanvasGroup>();

      return grabber;
    }

    static GameObject CreateViewport(Transform parentTransform) {
      GameObject viewport = new("ChatPanel.Viewport", typeof(RectTransform));
      viewport.SetParent(parentTransform);

      viewport.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      viewport.AddComponent<Image>()
          .SetColor(PluginConfig.ChatPanelBackgroundColor.Value)
          .SetSprite(CreateGradientSprite())
          .SetRaycastTarget(false);

      RectMask2D viewportRectMask = viewport.AddComponent<RectMask2D>();
      viewportRectMask.softness = Vector2Int.RoundToInt(PluginConfig.ChatPanelRectMaskSoftness.Value);

      viewport.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f, height: 1f);

      return viewport;
    }

    InputField CreateChatInputField(Transform parentTransform) {
      GameObject row = new("ChatPanel.InputField", typeof(RectTransform));
      row.SetParent(parentTransform, worldPositionStays: false);

      row.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: true, height: false)
          .SetChildAlignment(TextAnchor.MiddleLeft)
          .SetPadding(left: 10, right: 10, top: 8, bottom: 8);

      Image rowImage = row.AddComponent<Image>().SetColor(PluginConfig.ChatPanelBackgroundColor.Value);

      InputField inputField = CreateInputField(row.transform);
      inputField.targetGraphic = rowImage;
      inputField.transition = Selectable.Transition.ColorTint;

      return inputField;
    }

    InputField CreateInputField(Transform parentTransform) {
      GameObject inputFieldRow = new("ChatPanel.InputField.Row", typeof(RectTransform));
      inputFieldRow.transform.SetParent(parentTransform, worldPositionStays: false);

      inputFieldRow.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      inputFieldRow.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: true, height: false)
          .SetChildAlignment(TextAnchor.MiddleLeft);

      GameObject inputFieldText = Object.Instantiate(TextPrefab, inputFieldRow.transform, worldPositionStays: false);
      inputFieldText.name = "ChatPanel.InputField.Row.Text";

      inputFieldText.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      InputField inputField = inputFieldRow.AddComponent<InputField>();
      inputField.textComponent = inputFieldText.GetComponent<Text>();

      LayoutElement textLayout = inputFieldRow.AddComponent<LayoutElement>();
      textLayout.flexibleWidth = 1f;

      return inputField;
    }

    static GameObject CreateContent(Transform parentTransform) {
      GameObject content = new("ChatPanel.Content", typeof(RectTransform));
      content.SetParent(parentTransform);

      content.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(new(1f, 0f))
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      content.AddComponent<Image>()
          .SetColor(Color.clear)
          .SetRaycastTarget(true);

      content.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetSpacing(PluginConfig.ChatPanelContentSpacing.Value)
          .SetPadding(left: 20, right: 20, top: 20, bottom: 20);

      content.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      return content;
    }

    static Sprite CreateGradientSprite() {
      Texture2D texture = new(width: 1, height: 2);
      texture.wrapMode = TextureWrapMode.Clamp;
      texture.SetPixel(0, 0, Color.white);
      texture.SetPixel(0, 1, Color.clear);
      texture.Apply();

      return Sprite.Create(texture, new(0, 0, 1, 2), Vector2.zero);
    }

    static ScrollRect CreateScrollRect(GameObject panel, GameObject viewport, GameObject content) {
      ScrollRect panelScroll = panel.AddComponent<ScrollRect>();
      panelScroll.viewport = viewport.GetComponent<RectTransform>();
      panelScroll.content = content.GetComponent<RectTransform>();
      panelScroll.horizontal = false;
      panelScroll.vertical = true;
      panelScroll.scrollSensitivity = 30f;

      return panelScroll;
    }

    static GameObject CreateTextPrefab(Text parentText) {
      GameObject textPrefab = new("Text", typeof(RectTransform));

      textPrefab.AddComponent<Text>()
          .SetFont(PluginConfig.MessageFont)
          .SetFontSize(PluginConfig.ChatMessageFontSize.Value);

      if (parentText.TryGetComponent(out Outline parentTextOutline)) {
        textPrefab.AddComponent<Outline>()
            .SetEffectColor(parentTextOutline.effectColor)
            .SetEffectDistance(parentTextOutline.effectDistance);
      }

      return textPrefab;
    }

    public GameObject AddChatMessage(ChatMessage message) {
      GameObject row = CreateChatMessageRow(Content.transform);
      CreateChatMessageRowHeader(row.transform, message);
      CreateChatMessageRowBody(row.transform, GetMessageText(message));

      return row;
    }

    public static string GetMessageText(ChatMessage message) {
      return message.Type switch {
        Talker.Type.Normal => $"{message.Text}",
        Talker.Type.Shout => $"<color=yellow>{message.Text}</color>",
        Talker.Type.Whisper => $"<color=purple>{message.Text}</color>",
        Talker.Type.Ping => $"Ping! <color=cyan>{message.Position}</color>",
        _ => string.Empty,
      };
    }

    public GameObject CreateMessageDivider(Transform parentTransform) {
      GameObject divider = new("Message.Divider", typeof(RectTransform));
      divider.SetParent(parentTransform, worldPositionStays: false);

      divider.AddComponent<Image>()
          .SetColor(new Color32(255, 255, 255, 16))
          .SetRaycastTarget(true)
          .SetMaskable(true);

      divider.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(height: 1f);

      return divider;
    }

    // TODO: get the script from here and attach it to each child element instead of the content-size fitter?
    // https://sushanta1991.blogspot.com/2019/09/force-expand-child-width-in-vertical.html
    public GameObject CreateChatMessageRow(Transform parentTransform) {
      GameObject row = new("Message.Row", typeof(RectTransform));
      row.SetParent(parentTransform, worldPositionStays: false);

      row.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0)
          .SetSpacing(5f);

      row.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      return row;
    }

    public GameObject CreateChatMessageRowHeader(Transform parentTransform, ChatMessage message) {
      return CreateChatMessageRowHeader(parentTransform, message.User, message.Timestamp.ToString("T"));
    }

    public GameObject CreateChatMessageRowHeader(Transform parentTransform, string leftText, string rightText) {
      GameObject header = new("Message.Row.Header", typeof(RectTransform));
      header.SetParent(parentTransform, worldPositionStays: false);

      header.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0);

      GameObject leftCell = Object.Instantiate(TextPrefab, header.transform, worldPositionStays: false);
      leftCell.name = "Header.LeftCell";

      Text leftCellText = leftCell.GetComponent<Text>();
      leftCellText.text = leftText;
      leftCellText.alignment = TextAnchor.MiddleLeft;
      leftCellText.fontSize -= 2;

      leftCell.AddComponent<LayoutElement>();

      GameObject spacer = new("Header.Spacer", typeof(RectTransform));
      spacer.SetParent(header.transform);
      spacer.AddComponent<LayoutElement>().SetFlexible(width: 1f);

      GameObject rightCell = Object.Instantiate(TextPrefab, header.transform, worldPositionStays: false);
      rightCell.name = "Header.RightCell";

      Text rightCellText = rightCell.GetComponent<Text>();
      rightCellText.text = rightText;
      rightCellText.alignment = TextAnchor.MiddleRight;
      rightCellText.fontSize -= 2;

      rightCell.AddComponent<LayoutElement>();

      return header;
    }

    public GameObject CreateChatMessageRowBody(Transform parentTransform, string text) {
      GameObject body = Object.Instantiate(TextPrefab, parentTransform, worldPositionStays: false);
      body.name = ContentRowBodyName;

      body.GetComponent<Text>()
          .SetText(text)
          .SetAlignment(TextAnchor.MiddleLeft);

      body.AddComponent<LayoutElement>()
          .SetPreferred(width: _panelRectTransform.sizeDelta.x + _contentWidthOffset);

      return body;
    }
  }
}
