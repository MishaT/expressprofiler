using System;
using System.Drawing;
using System.Windows.Forms;

namespace ExpressProfiler
{
    /// <summary>
    /// Helpers for per-monitor DPI. Point-size fonts are converted to pixels using a DPI
    /// that can differ from the window's actual DPI — typically in Remote Desktop sessions,
    /// or on a second monitor with different scaling. ToolStrips compensate for this
    /// themselves; for other controls we use pixel-size fonts computed from the window's
    /// DPI, which render at exactly that size whatever the system DPI is.
    /// </summary>
    internal static class DpiUtil
    {
        public static int SystemDpi
        {
            get
            {
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    return (int)Math.Round(g.DpiY);
                }
            }
        }

        /// <summary>
        /// The font the forms were designed with (WinForms default, 8.25pt — matches
        /// AutoScaleDimensions 6x13). Use this rather than reading Form.Font at runtime:
        /// in some sessions (seen over RDP) WinForms has already rescaled the inherited
        /// default font by the wrong factor before our code runs.
        /// </summary>
        public static Font DesignFont
        {
            get { return new Font(Control.DefaultFont.FontFamily, 8.25f, FontStyle.Regular, GraphicsUnit.Point); }
        }

        /// <summary>Ratio between the window's DPI and the DPI that point-size fonts render at.</summary>
        public static float FontCorrection(int deviceDpi)
        {
            return (float)deviceDpi / SystemDpi;
        }

        /// <summary>A pixel-size copy of <paramref name="designFont"/> for a window at <paramref name="deviceDpi"/>.</summary>
        public static Font ForDpi(Font designFont, int deviceDpi)
        {
            return new Font(designFont.FontFamily, designFont.SizeInPoints * deviceDpi / 72f,
                            designFont.Style, GraphicsUnit.Pixel);
        }

        /// <summary>Sets <paramref name="control"/>'s font for <paramref name="deviceDpi"/>, if it differs.</summary>
        public static void ApplyFont(Control control, Font designFont, int deviceDpi)
        {
            Font font = ForDpi(designFont, deviceDpi);
            Font current = control.Font;
            if (current.Unit != GraphicsUnit.Pixel || Math.Abs(current.Size - font.Size) > 0.01f)
                control.Font = font; // with AutoScaleMode.Font, a form also rescales its layout
            else
                font.Dispose();
        }

        /// <summary>For dialogs: correct the form font once its window (and real DPI) exists.</summary>
        public static void FixFontOnLoad(Form form)
        {
            Font designFont = DesignFont;
            form.Load += delegate { ApplyFont(form, designFont, form.DeviceDpi); };
        }
    }
}
