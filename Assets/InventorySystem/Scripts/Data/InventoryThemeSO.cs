using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// One assignable texture. A null sprite means "leave this to USS".
    /// </summary>
    [Serializable]
    public class TextureStyle
    {
        public Sprite sprite;
        public Color tint = Color.white;
        public BackgroundSizeType sizeMode = BackgroundSizeType.Contain;
    }

    [Serializable]
    public class EquipmentSlotTexture
    {
        public EquipmentSlotTypeSO slotType;
        public TextureStyle background = new TextureStyle();
    }

    /// <summary>
    /// Inspector-authored look for an inventory panel. Assign sprites here to override
    /// the host stylesheet; leave a sprite empty and the USS background shows through.
    /// </summary>
    [CreateAssetMenu(fileName = "InventoryTheme", menuName = "Inventory System/Inventory Theme")]
    public class InventoryThemeSO : ScriptableObject
    {
        [Header("Backgrounds")]
        public TextureStyle panelBackground = new TextureStyle();
        public TextureStyle slotsContainerBackground = new TextureStyle();
        public TextureStyle slotBackground = new TextureStyle();
        public TextureStyle tabBackground = new TextureStyle();
        public TextureStyle tabActiveBackground = new TextureStyle();

        [Header("Equipment")]
        [Tooltip("Empty-state texture per equipment slot type. Applied to the slot's background layer.")]
        public List<EquipmentSlotTexture> equipmentSlotBackgrounds = new List<EquipmentSlotTexture>();

        [Header("Optional stylesheet")]
        [Tooltip("Extra USS added to the document root while this theme is active.")]
        public StyleSheet themeStyleSheet;

        private void OnEnable()
        {
            panelBackground ??= new TextureStyle();
            slotsContainerBackground ??= new TextureStyle();
            slotBackground ??= new TextureStyle();
            tabBackground ??= new TextureStyle();
            tabActiveBackground ??= new TextureStyle();
            equipmentSlotBackgrounds ??= new List<EquipmentSlotTexture>();
        }
    }
}
