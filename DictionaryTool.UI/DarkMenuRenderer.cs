using System.Drawing;
using System.Windows.Forms;

namespace DictionaryTool.UI
{
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColorTable()) { }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? UiPalette.TextPrimary : UiPalette.TextSecondary;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = UiPalette.TextSecondary;
            base.OnRenderArrow(e);
        }
    }

    internal sealed class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => UiPalette.CardBackground;
        public override Color ImageMarginGradientBegin => UiPalette.CardBackground;
        public override Color ImageMarginGradientMiddle => UiPalette.CardBackground;
        public override Color ImageMarginGradientEnd => UiPalette.CardBackground;
        public override Color MenuBorder => UiPalette.BorderSoft;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuItemSelected => UiPalette.AccentSoftHover;
        public override Color MenuItemSelectedGradientBegin => UiPalette.AccentSoftHover;
        public override Color MenuItemSelectedGradientEnd => UiPalette.AccentSoftHover;
        public override Color MenuItemPressedGradientBegin => UiPalette.AccentSoft;
        public override Color MenuItemPressedGradientMiddle => UiPalette.AccentSoft;
        public override Color MenuItemPressedGradientEnd => UiPalette.AccentSoft;
        public override Color SeparatorDark => UiPalette.BorderSoft;
        public override Color SeparatorLight => UiPalette.BorderSoft;
        public override Color CheckBackground => UiPalette.AccentSoft;
        public override Color CheckPressedBackground => UiPalette.AccentSoftHover;
        public override Color CheckSelectedBackground => UiPalette.AccentSoftHover;
    }
}
