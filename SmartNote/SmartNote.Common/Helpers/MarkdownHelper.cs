using System.Text.RegularExpressions;

namespace SmartNote.Common.Helpers
{
    public static class MarkdownHelper
    {
        /// <summary>
        /// 将 Markdown 文本粗略转换为纯文本（用于摘要/检索等场景）。
        /// </summary>
        public static string ToPlainText(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            var text = markdown;

            // 移除代码块
            text = Regex.Replace(text, "```[\\s\\S]*?```", " ", RegexOptions.Multiline);
            // 移除行内 code
            text = Regex.Replace(text, "`[^`]*`", " ");

            // 图片：![alt](url) -> alt
            text = Regex.Replace(text, "!\\[([^\\]]*)\\]\\([^\\)]*\\)", "$1");
            // 链接：[text](url) -> text
            text = Regex.Replace(text, "\\[([^\\]]+)\\]\\([^\\)]*\\)", "$1");

            // 标题/列表/引用符号
            text = Regex.Replace(text, "^[#>\\-*+\\s]+", "", RegexOptions.Multiline);
            text = Regex.Replace(text, "^\\d+\\.[\\s]+", "", RegexOptions.Multiline);

            // 粗体/斜体
            text = text.Replace("**", " ").Replace("__", " ");
            text = text.Replace("*", " ").Replace("_", " ");

            // 多余空白
            text = Regex.Replace(text, "\\s+", " ").Trim();

            return text;
        }

        /// <summary>
        /// 生成不超过 maxLength 的摘要（不做 AI，总是稳定、快速）。
        /// </summary>
        public static string BuildSummary(string markdownOrText, int maxLength = 100)
        {
            maxLength = maxLength <= 0 ? 100 : maxLength;
            var text = ToPlainText(markdownOrText);
            if (text.Length <= maxLength) return text;
            return text[..maxLength];
        }
    }
}
