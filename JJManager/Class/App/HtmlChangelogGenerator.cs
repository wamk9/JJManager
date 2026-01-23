using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MaterialSkin;

namespace JJManager.Class.App
{
    /// <summary>
    /// Generates HTML changelog pages using the generic HtmlTemplateEngine
    /// </summary>
    public static class HtmlChangelogGenerator
    {
        private static HtmlTemplateEngine _engine = null;

        // Badge colors for different change types
        private const string BADGE_NEW_COLOR = "#4CAF50";      // Green for new features
        private const string BADGE_NEW_TEXT_COLOR = "#FFFFFF";
        private const string BADGE_FIX_COLOR = "#FF9800";      // Orange for fixes
        private const string BADGE_FIX_TEXT_COLOR = "#FFFFFF";

        // Badge text labels
        private static readonly Dictionary<string, string> BadgeLabels = new Dictionary<string, string>
        {
            { "new", "Novidade" },
            { "fix", "Correção" }
        };

        /// <summary>
        /// Gets or creates the template engine instance
        /// </summary>
        private static HtmlTemplateEngine Engine
        {
            get
            {
                if (_engine == null)
                {
                    _engine = new HtmlTemplateEngine(Path.Combine("Pages", "App", "HTML"));
                }
                return _engine;
            }
        }

        /// <summary>
        /// Gets the badge CSS class based on the change type
        /// </summary>
        private static string GetBadgeClass(string type)
        {
            switch (type?.ToLowerInvariant())
            {
                case "new": return "badge-new";
                case "fix": return "badge-fix";
                default: return "badge-default";
            }
        }

        /// <summary>
        /// Gets the badge display text based on the change type
        /// </summary>
        private static string GetBadgeText(string type)
        {
            string typeKey = type?.ToLowerInvariant() ?? "";
            return BadgeLabels.ContainsKey(typeKey) ? BadgeLabels[typeKey] : "Mudança";
        }

        /// <summary>
        /// Generates a styled HTML changelog from a list of changelog entries
        /// </summary>
        /// <param name="changeLogEntries">List of string arrays containing [img_url, title, description, type]</param>
        /// <param name="isDarkTheme">Whether to use dark theme colors</param>
        /// <param name="colorScheme">Material design color scheme</param>
        /// <returns>Complete HTML document as string</returns>
        public static string GenerateHtml(List<string[]> changeLogEntries, bool isDarkTheme, ColorScheme colorScheme)
        {
            if (changeLogEntries == null || changeLogEntries.Count == 0)
            {
                return GenerateEmptyHtml(isDarkTheme);
            }

            try
            {
                // Get Material Design colors
                var colors = HtmlTemplateEngine.GetMaterialColors(isDarkTheme, colorScheme);

                // Add badge colors
                colors["BADGE_NEW_COLOR"] = BADGE_NEW_COLOR;
                colors["BADGE_NEW_TEXT_COLOR"] = BADGE_NEW_TEXT_COLOR;
                colors["BADGE_FIX_COLOR"] = BADGE_FIX_COLOR;
                colors["BADGE_FIX_TEXT_COLOR"] = BADGE_FIX_TEXT_COLOR;

                // Generate cards HTML
                var cardsHtml = new StringBuilder();
                int index = 1;

                foreach (var entry in changeLogEntries)
                {
                    if (entry == null || entry.Length < 3)
                        continue;

                    string imgUrl = entry[0];
                    string title = entry[1];
                    string description = entry[2];
                    string type = entry.Length >= 4 ? entry[3] : "";

                    // Generate image HTML
                    string imageHtml;
                    if (!string.IsNullOrEmpty(imgUrl) && imgUrl != "#")
                    {
                        var imageReplacements = new Dictionary<string, string>
                        {
                            { "IMAGE_URL", HtmlTemplateEngine.EscapeHtml(imgUrl) },
                            { "INDEX", index.ToString() }
                        };
                        imageHtml = Engine.LoadAndRender(
                            Path.Combine("Modules", "changelog_image.html"),
                            imageReplacements
                        );
                    }
                    else
                    {
                        var noImageReplacements = new Dictionary<string, string>
                        {
                            { "INDEX", index.ToString() }
                        };
                        imageHtml = Engine.LoadAndRender(
                            Path.Combine("Modules", "changelog_no_image.html"),
                            noImageReplacements
                        );
                    }

                    // Generate card HTML with badge
                    var cardReplacements = new Dictionary<string, string>
                    {
                        { "IMAGE_HTML", imageHtml },
                        { "INDEX", index.ToString() },
                        { "TITLE", HtmlTemplateEngine.EscapeHtml(title) },
                        { "DESCRIPTION", HtmlTemplateEngine.EscapeHtml(description) },
                        { "BADGE_CLASS", GetBadgeClass(type) },
                        { "BADGE_TEXT", GetBadgeText(type) }
                    };

                    string cardHtml = Engine.LoadAndRender(
                        Path.Combine("Modules", "changelog_card.html"),
                        cardReplacements
                    );

                    cardsHtml.AppendLine(cardHtml);
                    index++;
                }

                // Generate final page HTML
                colors["CARDS_HTML"] = cardsHtml.ToString();

                string finalHtml = Engine.LoadAndRender(
                    "changelog_page.html",
                    colors
                );

                return finalHtml;
            }
            catch (FileNotFoundException ex)
            {
                Log.Insert("HtmlChangelogGenerator", "Template HTML não encontrado", ex);

                // Show error to user
                Pages.App.MessageBox.Show(
                    null,
                    "Erro ao Carregar Changelog",
                    $"Template HTML não encontrado:\n{ex.Message}\n\nVerifique se os arquivos estão em:\nPages/App/HTML/"
                );

                return GenerateEmptyHtml(isDarkTheme);
            }
            catch (Exception ex)
            {
                Log.Insert("HtmlChangelogGenerator", "Erro ao gerar HTML do changelog", ex);

                // Show error to user
                Pages.App.MessageBox.Show(
                    null,
                    "Erro ao Gerar Changelog",
                    $"Erro inesperado ao gerar changelog:\n{ex.Message}"
                );

                return GenerateEmptyHtml(isDarkTheme);
            }
        }

        /// <summary>
        /// Generates an empty state HTML when no changelog is available
        /// </summary>
        private static string GenerateEmptyHtml(bool isDarkTheme)
        {
            var backgroundColor = isDarkTheme ? "#303030" : "#FAFAFA";
            var textColor = isDarkTheme ? "rgba(255, 255, 255, 0.60)" : "rgba(0, 0, 0, 0.60)";

            return $@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Roboto', 'Segoe UI', sans-serif;
            background-color: {backgroundColor};
            color: {textColor};
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100vh;
            margin: 0;
            text-align: center;
        }}
        .empty-message {{
            font-size: 18px;
        }}
    </style>
</head>
<body>
    <div class='empty-message'>Nenhum changelog disponível</div>
</body>
</html>";
        }
    }
}
