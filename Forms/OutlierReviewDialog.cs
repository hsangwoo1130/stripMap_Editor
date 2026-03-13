using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace stripMap_Editor.Forms
{
    /// <summary>
    /// MapArray 수정 시 BinCode 아웃라이어(MapArray≠2인데 BinCode=D) 검토 다이얼로그.
    /// 사용자가 각 자릿수의 수정 값을 입력하고 [수정]을 클릭하면 CorrectedBinCodes에 결과가 담깁니다.
    /// </summary>
    public partial class OutlierReviewDialog : Form
    {
        private readonly List<OutlierItem> _outliers;
        private readonly string _newBinCode;

        // OutlierItem → 사용자 입력 TextBox 매핑
        private readonly Dictionary<OutlierItem, TextBox> _textBoxMap
            = new Dictionary<OutlierItem, TextBox>();

        /// <summary>
        /// 키: stripNo, 값: 아웃라이어 보정이 반영된 최종 BinCode 문자열
        /// DialogResult.OK 후에만 유효합니다.
        /// </summary>
        public Dictionary<string, string> CorrectedBinCodes { get; private set; }

        public OutlierReviewDialog(List<OutlierItem> outliers, string newBinCode)
        {
            _outliers   = outliers;
            _newBinCode = newBinCode;
            InitializeComponent();
            BuildOutlierUI();
        }

        // ─────────────────────────────────────────────
        // 동적 UI 생성
        // ─────────────────────────────────────────────

        private void BuildOutlierUI()
        {
            int y = 8;
            const int ROW_H      = 28;
            const int STRIP_H    = 22;
            const int INDENT     = 12;
            const int PANEL_W    = 520;

            var byStrip = _outliers.GroupBy(o => o.StripNo);

            foreach (var group in byStrip)
            {
                // ── stripNo 헤더 ──
                var header = new Label
                {
                    Text      = group.Key,
                    Font      = new Font("맑은 고딕", 9.75f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 80, 160),
                    Location  = new Point(INDENT, y),
                    AutoSize  = true
                };
                panelScroll.Controls.Add(header);
                y += STRIP_H + 2;

                // ── 아웃라이어 행 ──
                foreach (var item in group)
                {
                    var lblInfo = new Label
                    {
                        Text     = $"  {item.Position}번째  MapArray: {item.MapArrayChar}  /  BinCode: D  →",
                        Font     = new Font("맑은 고딕", 9f),
                        Location = new Point(INDENT + 8, y + 4),
                        AutoSize = true
                    };
                    panelScroll.Controls.Add(lblInfo);

                    var tb = new TextBox
                    {
                        Font      = new Font("맑은 고딕", 9f),
                        MaxLength = 1,
                        Location  = new Point(PANEL_W - 70, y + 2),
                        Size      = new Size(50, 22),
                        TextAlign = HorizontalAlignment.Center
                    };
                    panelScroll.Controls.Add(tb);
                    _textBoxMap[item] = tb;

                    y += ROW_H;
                }

                y += 4; // 그룹 간 여백
            }

            // 스크롤 패널 내부 높이 조정
            panelScroll.AutoScrollMinSize = new Size(0, y + 8);
        }

        // ─────────────────────────────────────────────
        // 버튼 핸들러
        // ─────────────────────────────────────────────

        private void BtnApply_Click(object sender, EventArgs e)
        {
            CorrectedBinCodes = new Dictionary<string, string>();

            foreach (var group in _outliers.GroupBy(o => o.StripNo))
            {
                char[] chars = _newBinCode.ToCharArray();
                foreach (var item in group)
                {
                    string input = _textBoxMap[item].Text.Trim();
                    if (input.Length > 0)
                        chars[item.Position - 1] = input[0];
                    // 비워두면 'D' 그대로 유지
                }
                CorrectedBinCodes[group.Key] = new string(chars);
            }

            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }

    /// <summary>
    /// 아웃라이어 항목 (한 자릿수 단위)
    /// </summary>
    public class OutlierItem
    {
        public string StripNo     { get; set; }
        public int    Position    { get; set; } // 1-based
        public char   MapArrayChar { get; set; }
    }
}
