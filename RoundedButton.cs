using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace stripMap_Editor.Controls
{
    public class RoundedButton : Button
    {
        private int borderRadius = 0;

        public int BorderRadius
        {
            get { return borderRadius; }
            set
            {
                borderRadius = value;

                if (borderRadius <= 0)
                {
                    // 일반 버튼으로 전환
                    this.SetStyle(ControlStyles.UserPaint, false);
                    this.Region = null;
                }
                else
                {
                    // 커스텀 버튼으로 전환
                    this.SetStyle(ControlStyles.UserPaint, true);
                }

                Invalidate();
            }
        }

        public RoundedButton()
        {
            // 기본 Flat 스타일
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;

            // 초기에는 일반 버튼
            this.SetStyle(ControlStyles.UserPaint, false);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (borderRadius <= 0)
            {
                // 기본 Button 렌더링 사용
                base.OnPaint(e);
                return;
            }

            // 커스텀 둥근 버튼 렌더링
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = GetRoundedRectangle(rect, borderRadius))
            {
                this.Region = new Region(path);

                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    this.Text,
                    this.Font,
                    new Rectangle(0, 0, this.Width, this.Height),
                    this.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius < 2)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}