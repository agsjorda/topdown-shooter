using System.Collections.Generic;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Writes InventoryThemeSO textures onto the live visual tree.
    /// An empty sprite clears the inline background so the host USS shows through.
    /// </summary>
    public static class InventoryThemeApplier
    {
        public static void Apply(
            InventoryThemeSO theme,
            VisualElement documentRoot,
            VisualElement panelRoot,
            VisualElement slotsContainer,
            IReadOnlyList<SlotView> slots,
            IReadOnlyList<EquipmentSlotView> equipmentSlots,
            IReadOnlyList<InventoryTabElement> tabs)
        {
            ApplyThemeStyleSheet(documentRoot, theme);

            ApplyTexture(panelRoot, theme != null ? theme.panelBackground : null);
            ApplyTexture(slotsContainer, theme != null ? theme.slotsContainerBackground : null);

            if (slots != null) {
                for (int i = 0; i < slots.Count; i++) {
                    var slot = slots[i];
                    if (slot == null || slot.IsEquipmentSlot) continue;
                    ApplyTexture(slot, theme != null ? theme.slotBackground : null);
                }
            }

            if (equipmentSlots != null) {
                for (int i = 0; i < equipmentSlots.Count; i++) {
                    var slot = equipmentSlots[i];
                    if (slot == null) continue;
                    ApplyTexture(slot.BackgroundLayer, FindEquipmentTexture(theme, slot.SlotType));
                }
            }

            if (tabs != null) {
                for (int i = 0; i < tabs.Count; i++) {
                    var tab = tabs[i];
                    if (tab == null) continue;
                    ApplyTexture(tab.BackgroundLayer, theme != null ? theme.tabBackground : null);
                    ApplyTexture(tab.ActiveBackgroundLayer, theme != null ? theme.tabActiveBackground : null);
                }
            }
        }

        public static void ApplyTexture(VisualElement element, TextureStyle texture)
        {
            if (element == null) return;

            if (texture == null || texture.sprite == null) {
                element.style.backgroundImage = StyleKeyword.Null;
                element.style.unityBackgroundImageTintColor = StyleKeyword.Null;
                element.style.backgroundSize = StyleKeyword.Null;
                return;
            }

            element.style.backgroundImage = new StyleBackground(texture.sprite);
            element.style.unityBackgroundImageTintColor = texture.tint;
            element.style.backgroundSize = new BackgroundSize(texture.sizeMode);
        }

        private static TextureStyle FindEquipmentTexture(InventoryThemeSO theme, EquipmentSlotTypeSO slotType)
        {
            if (theme == null || slotType == null || theme.equipmentSlotBackgrounds == null) return null;
            for (int i = 0; i < theme.equipmentSlotBackgrounds.Count; i++) {
                var entry = theme.equipmentSlotBackgrounds[i];
                if (entry != null && entry.slotType == slotType) return entry.background;
            }
            return null;
        }

        private static void ApplyThemeStyleSheet(VisualElement documentRoot, InventoryThemeSO theme)
        {
            if (documentRoot == null || theme == null || theme.themeStyleSheet == null) return;
            if (!documentRoot.styleSheets.Contains(theme.themeStyleSheet)) {
                documentRoot.styleSheets.Add(theme.themeStyleSheet);
            }
        }
    }
}
