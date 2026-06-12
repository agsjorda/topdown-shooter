using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [UxmlElement]
    public partial class InventoryScrollElement : VisualElement
    {
        public ScrollView InnerScroll { get; private set; }
        public VisualElement SlotsContainer { get; private set; }

        public InventoryScrollElement()
        {
            AddToClassList("inventory-scroll-wrapper"); // optional USS hook

            InnerScroll = new ScrollView {
                verticalScrollerVisibility = ScrollerVisibility.Auto,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden
            };
            //InnerScroll.AddToClassList("inventory-scroll-view");

            // ensure content wraps so vertical overflow occurs
            InnerScroll.contentContainer.style.flexDirection = FlexDirection.Row;
            InnerScroll.contentContainer.style.flexWrap = Wrap.Wrap;
            InnerScroll.contentContainer.style.alignContent = Align.Center;
            InnerScroll.contentContainer.style.justifyContent = Justify.Center;

            SlotsContainer = new VisualElement();
            SlotsContainer.AddToClassList("inventory-slots-container");

            InnerScroll.Add(SlotsContainer);
            Add(InnerScroll);

            style.flexShrink = 0;
        }

        /// <summary>
        /// Set scroll container background (applies to ScrollView, not individual slots).
        /// </summary>
        public void SetBackground(Texture2D texture, Sprite sprite, Color tint, bool useBackground)
        {
            if (!useBackground) {
                InnerScroll.style.backgroundImage = null;
                InnerScroll.style.backgroundColor = StyleKeyword.Null;
                return;
            }

            Texture2D tex = texture;
            if (tex == null && sprite != null)
                tex = sprite.texture;

            if (tex != null) {
                InnerScroll.style.backgroundImage = new StyleBackground(tex);
                InnerScroll.style.unityBackgroundImageTintColor = tint;
            } else {
                InnerScroll.style.backgroundImage = null;
                InnerScroll.style.backgroundColor = tint;
            }
        }

        /// <summary>
        /// Set explicit pixel size. 0 = let USS / parent layout decide.
        /// Applies to wrapper and inner ScrollView.
        /// </summary>
        public void SetSize(int widthPx, int heightPx)
        {
            if (widthPx > 0) {
                style.width = widthPx;
                InnerScroll.style.width = widthPx;
            } else {
                style.width = StyleKeyword.Null;
                InnerScroll.style.width = StyleKeyword.Null;
            }

            if (heightPx > 0) {
                style.height = heightPx;
                InnerScroll.style.height = heightPx;
            } else {
                style.height = StyleKeyword.Null;
                InnerScroll.style.height = StyleKeyword.Null;
            }
        }


        public void SetScrollerVisibility(ScrollerVisibility vertical)
        {
            InnerScroll.verticalScrollerVisibility = vertical;
        }
    }
}
