using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NostreetsExtensions.Extend.Form
{
    public static class Forms
    {
        public static void Alert(string text, string caption)
        {
            System.Windows.Forms.Form prompt = new System.Windows.Forms.Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };


            Label textLabel = new Label() { Left = 50, Top = 20, Width = 400, Text = text, TextAlign = ContentAlignment.TopCenter };
            Button confirmation = new Button() { Text = "OK", Left = 200, Width = 100, Top = 50, DialogResult = DialogResult.OK, TextAlign = ContentAlignment.BottomCenter };


            if (text.Contains('\n'))
            {
                string[] lines = text.Split('\n');
                foreach (string line in lines)
                {
                    int d = TextRenderer.MeasureText(line, textLabel.Font).Width / prompt.Width;
                    do {
                        prompt.Height += 15;
                        textLabel.Height += 15;
                        confirmation.Top += 15;
                        d--;
                    } while (d > -1);
                }
            }


            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);

            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.AcceptButton = confirmation;

            prompt.ShowDialog();
        }

        public static int CalcLabelHeight(this Label lbl)
        {
            Size sz = new Size(lbl.ClientSize.Width, int.MaxValue);
            sz = TextRenderer.MeasureText(lbl.Text, lbl.Font, sz, TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            int height = sz.Height;
            if (height < lbl.Font.Height) height = lbl.Font.Height;
            return height + lbl.Padding.Vertical;
        }
    }
}
