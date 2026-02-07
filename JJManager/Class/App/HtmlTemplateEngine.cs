using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Class.App
{
    /// <summary>
    /// Generic HTML template engine for loading and rendering HTML templates with placeholders
    /// </summary>
    public class HtmlTemplateEngine
    {
        private readonly string _basePath;
        private readonly Dictionary<string, string> _templateCache;
        private readonly bool _enableCache;
        private static Task<CoreWebView2Environment> _environmentTask;

        
        /// <summary>
        /// Creates a new HtmlTemplateEngine instance
        /// </summary>
        /// <param name="basePath">Base directory path for templates (relative to AppDomain.BaseDirectory)</param>
        /// <param name="enableCache">Enable template caching (default: true)</param>
        public HtmlTemplateEngine(string basePath, bool enableCache = false)
        {
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath.ToLower());
            _enableCache = enableCache;
            _templateCache = new Dictionary<string, string>();
        }

        /// <summary>
        /// Loads a template file
        /// </summary>
        /// <param name="templatePath">Relative path to template file (e.g., "changelog_page.html" or "Modules/changelog_card.html")</param>
        /// <returns>Template content as string</returns>
        /// <exception cref="FileNotFoundException">Thrown when template file is not found</exception>
        public string LoadTemplate(string templatePath)
        {
            // Check cache first
            if (_enableCache && _templateCache.ContainsKey(templatePath))
            {
                return _templateCache[templatePath];
            }

            string fullPath = Path.Combine(_basePath, templatePath);

            if (!File.Exists(fullPath))
            {
                string errorMsg = $"Template HTML não encontrado: {fullPath}";
                Log.Insert("HtmlTemplateEngine", errorMsg);
                throw new FileNotFoundException(errorMsg, fullPath);
            }

            try
            {
                string content = File.ReadAllText(fullPath);

                // Cache the template
                if (_enableCache)
                {
                    _templateCache[templatePath] = content;
                }

                return content;
            }
            catch (Exception ex)
            {
                Log.Insert("HtmlTemplateEngine", $"Erro ao carregar template '{templatePath}'", ex);
                throw;
            }
        }

        public static Task<CoreWebView2Environment> GetAsync()
        {
            if (_environmentTask != null)
                return _environmentTask;

            _environmentTask = CoreWebView2Environment.CreateAsync(
                userDataFolder: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "JJManager",
                    "WebView2"
                )
            );

            return _environmentTask;
        }

        /// <summary>
        /// Renders a template by replacing placeholders with values
        /// </summary>
        /// <param name="template">Template string with placeholders</param>
        /// <param name="replacements">Dictionary of placeholder → value replacements</param>
        /// <returns>Rendered HTML string</returns>
        public string Render(string template, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            if (replacements == null || replacements.Count == 0)
                return template;

            string result = template;

            foreach (var replacement in replacements)
            {
                string placeholder = $"{{{{{replacement.Key}}}}}"; // {{PLACEHOLDER}}
                result = result.Replace(placeholder, replacement.Value ?? string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Loads and renders a template in one call
        /// </summary>
        /// <param name="templatePath">Relative path to template file</param>
        /// <param name="replacements">Dictionary of placeholder → value replacements</param>
        /// <returns>Rendered HTML string</returns>
        /// <exception cref="FileNotFoundException">Thrown when template file is not found</exception>
        public string LoadAndRender(string templatePath, Dictionary<string, string> replacements)
        {
            string template = LoadTemplate(templatePath);
            return Render(template, replacements);
        }

        /// <summary>
        /// Clears the template cache
        /// </summary>
        public void ClearCache()
        {
            _templateCache.Clear();
        }

        /// <summary>
        /// Escapes HTML special characters
        /// </summary>
        public static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        /// <summary>
        /// Converts System.Drawing.Color to hex string
        /// </summary>
        public static string ColorToHex(System.Drawing.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Creates color dictionary for Material Design themes
        /// </summary>
        public static Dictionary<string, string> GetMaterialColors(bool isDarkTheme, MaterialSkin.ColorScheme colorScheme)
        {
            return new Dictionary<string, string>
            {
                { "BACKGROUND_COLOR", isDarkTheme ? "#303030" : "#FAFAFA" },
                { "CARD_BACKGROUND_COLOR", isDarkTheme ? "#424242" : "#FFFFFF" },
                { "TEXT_PRIMARY_COLOR", isDarkTheme ? "rgba(255, 255, 255, 0.87)" : "rgba(0, 0, 0, 0.87)" },
                { "TEXT_SECONDARY_COLOR", isDarkTheme ? "rgba(255, 255, 255, 0.60)" : "rgba(0, 0, 0, 0.60)" },
                { "DIVIDER_COLOR", isDarkTheme ? "rgba(255, 255, 255, 0.12)" : "rgba(0, 0, 0, 0.12)" },
                { "SHADOW_COLOR", isDarkTheme ? "rgba(0, 0, 0, 0.4)" : "rgba(0, 0, 0, 0.2)" },
                { "PRIMARY_COLOR", ColorToHex(colorScheme.PrimaryColor) },
                { "ACCENT_COLOR", ColorToHex(colorScheme.AccentColor) }
            };
        }
    }
}
