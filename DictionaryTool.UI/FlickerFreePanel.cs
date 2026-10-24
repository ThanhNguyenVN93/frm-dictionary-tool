using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Guna2Panel/FlowLayoutPanel không bật double-buffering mặc định trên .NET
    /// Framework WinForms, gây nháy/chớp khi cuộn hoặc khi nhiều control con được
    /// thêm/layout lại liên tục (vd danh sách định nghĩa render lại mỗi lần tra từ).
    /// </summary>
    internal class FlickerFreePanel : Guna2Panel
    {
        public FlickerFreePanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
    }

    internal class FlickerFreeFlowLayoutPanel : FlowLayoutPanel
    {
        public FlickerFreeFlowLayoutPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
    }
}
