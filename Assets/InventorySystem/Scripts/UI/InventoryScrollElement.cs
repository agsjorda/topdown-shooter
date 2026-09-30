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
            AddToClassList("inventory-scroll-wrapper");

            InnerScroll = new ScrollView {
                verticalScrollerVisibility = ScrollerVisibility.Auto,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden
            };

            SlotsContainer = new VisualElement();
            SlotsContainer.AddToClassList("inventory-slots-container");

            InnerScroll.Add(SlotsContainer);
            Add(InnerScroll);
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
