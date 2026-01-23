# JJManager - HTML Template System

Sistema de templates HTML genérico para renderização de conteúdo dinâmico no JJManager.

## 📂 Estrutura de Pastas

```
Pages/App/HTML/
├── changelog_page.html        # Template principal da página de changelog
├── Modules/                   # Módulos de templates específicos
│   ├── changelog_card.html    # Template de card individual
│   ├── changelog_image.html   # Template de imagem
│   └── changelog_no_image.html # Template de placeholder
└── README.md                  # Este arquivo
```

## 🎯 Como Funciona

### 1. HtmlTemplateEngine (Classe Genérica)

Localização: `Class/App/HtmlTemplateEngine.cs`

**Funcionalidades:**
- Carregamento de templates de qualquer pasta
- Sistema de placeholders `{{VARIAVEL}}`
- Cache automático de templates
- Fallback para conteúdo hardcoded
- Utilitários para escape HTML e conversão de cores

**Exemplo de uso:**
```csharp
var engine = new HtmlTemplateEngine(Path.Combine("Pages", "App", "HTML"));

var replacements = new Dictionary<string, string>
{
    { "TITLE", "Meu Título" },
    { "DESCRIPTION", "Minha descrição" }
};

string html = engine.LoadAndRender("changelog_page.html", replacements);
```

### 2. HtmlChangelogGenerator (Implementação Específica)

Localização: `Class/App/HtmlChangelogGenerator.cs`

Usa o `HtmlTemplateEngine` para gerar páginas de changelog.

## 📝 Placeholders Disponíveis

### Template Principal (`changelog_page.html`)

| Placeholder | Descrição | Exemplo |
|-------------|-----------|---------|
| `{{BACKGROUND_COLOR}}` | Cor de fundo | `#303030` (dark) / `#FAFAFA` (light) |
| `{{CARD_BACKGROUND_COLOR}}` | Cor de fundo dos cards | `#424242` / `#FFFFFF` |
| `{{TEXT_PRIMARY_COLOR}}` | Cor do texto principal | `rgba(255, 255, 255, 0.87)` |
| `{{TEXT_SECONDARY_COLOR}}` | Cor do texto secundário | `rgba(255, 255, 255, 0.60)` |
| `{{DIVIDER_COLOR}}` | Cor dos divisores | `rgba(255, 255, 255, 0.12)` |
| `{{SHADOW_COLOR}}` | Cor das sombras | `rgba(0, 0, 0, 0.4)` |
| `{{PRIMARY_COLOR}}` | Cor primária MaterialSkin | `#2196F3` |
| `{{ACCENT_COLOR}}` | Cor de destaque | `#FF5722` |
| `{{CARDS_HTML}}` | HTML dos cards gerados | (gerado dinamicamente) |

### Módulo Card (`Modules/changelog_card.html`)

| Placeholder | Descrição |
|-------------|-----------|
| `{{IMAGE_HTML}}` | HTML da imagem (gerado pelo módulo de imagem) |
| `{{INDEX}}` | Número do changelog (1, 2, 3...) |
| `{{TITLE}}` | Título do changelog |
| `{{DESCRIPTION}}` | Descrição detalhada |

### Módulo Imagem (`Modules/changelog_image.html`)

| Placeholder | Descrição |
|-------------|-----------|
| `{{IMAGE_URL}}` | URL da imagem |
| `{{INDEX}}` | Número do changelog |

### Módulo No-Image (`Modules/changelog_no_image.html`)

| Placeholder | Descrição |
|-------------|-----------|
| `{{INDEX}}` | Número do changelog (exibido como placeholder) |

## 🎨 Personalização

### Editando Templates (Sem Recompilar!)

Você pode editar qualquer template HTML e as mudanças serão aplicadas na próxima execução:

#### Exemplo: Mudar altura da imagem
```css
.changelog-image {
    height: 250px; /* Era 200px */
}
```

#### Exemplo: Mudar layout do grid
```css
.changelog-grid {
    grid-template-columns: repeat(auto-fit, minmax(350px, 1fr)); /* Era 280px */
    gap: 30px; /* Era 20px */
}
```

#### Exemplo: Adicionar animação customizada
```css
@keyframes meuEfeito {
    from { opacity: 0; }
    to { opacity: 1; }
}

.changelog-card {
    animation: meuEfeito 0.5s ease;
}
```

## 🚀 Criando Novos Módulos

### Exemplo: Módulo de Firmware Update

1. **Criar templates:**
   - `firmware_page.html` (na raiz)
   - `Modules/firmware_card.html`

2. **Criar classe geradora:**
```csharp
public static class HtmlFirmwareGenerator
{
    private static HtmlTemplateEngine _engine = null;

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

    public static string GenerateHtml(List<FirmwareInfo> firmwares, bool isDarkTheme, ColorScheme colorScheme)
    {
        var colors = HtmlTemplateEngine.GetMaterialColors(isDarkTheme, colorScheme);

        // Gerar cards...
        var cardsHtml = new StringBuilder();
        foreach (var fw in firmwares)
        {
            var replacements = new Dictionary<string, string>
            {
                { "DEVICE_NAME", fw.DeviceName },
                { "VERSION", fw.Version },
                { "DESCRIPTION", fw.Description }
            };
            cardsHtml.AppendLine(Engine.LoadAndRender(
                Path.Combine("Modules", "firmware_card.html"),
                replacements
            ));
        }

        colors["CARDS_HTML"] = cardsHtml.ToString();
        return Engine.LoadAndRender("firmware_page.html", colors);
    }
}
```

## ⚙️ Configuração do Projeto

No `JJManager.csproj`, garanta que os templates sejam copiados para a saída:

```xml
<ItemGroup>
  <None Update="Pages\App\HTML\*.html">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="Pages\App\HTML\Modules\*.html">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## 🔧 API do HtmlTemplateEngine

### Métodos Principais

```csharp
// Carregar template
string template = engine.LoadTemplate("changelog_page.html");

// Renderizar com replacements
string html = engine.Render(template, replacements);

// Carregar e renderizar em uma chamada
string html = engine.LoadAndRender("changelog_page.html", replacements);

// Limpar cache (forçar reload)
engine.ClearCache();
```

### Métodos Utilitários Estáticos

```csharp
// Escape HTML
string safe = HtmlTemplateEngine.EscapeHtml("<script>alert('xss')</script>");
// Output: &lt;script&gt;alert('xss')&lt;/script&gt;

// Cor para Hex
string hex = HtmlTemplateEngine.ColorToHex(Color.FromArgb(33, 150, 243));
// Output: #2196F3

// Obter cores Material Design
var colors = HtmlTemplateEngine.GetMaterialColors(isDarkTheme, colorScheme);
// Output: Dictionary com 8 cores prontas para usar
```

## 📊 Performance

- **Cache**: Templates carregados 1x e mantidos em memória
- **Lazy Loading**: Engine criado apenas quando necessário
- **Fallback Rápido**: Templates hardcoded se arquivos não existirem

## ⚠️ Importante

- **Sempre use UTF-8** encoding nos arquivos HTML
- **Não remova placeholders** se não souber o que está fazendo
- **Teste suas mudanças** localmente antes de distribuir
- **Mantenha backup** dos templates originais

## 🐛 Troubleshooting

### Templates não carregam
- Verifique se estão em `bin/Release/Pages/App/HTML/`
- Confirme configuração no `.csproj`
- Veja logs em `%APPDATA%\JohnJohn3D\JJManager\Log\`

### Placeholders não são substituídos
- Verifique se usa `{{VARIAVEL}}` (duas chaves)
- Confirme que o placeholder existe no dictionary de replacements

### Mudanças não aparecem
- Limpe cache: `engine.ClearCache()`
- Reinicie o JJManager
- Verifique se está editando o arquivo correto (não o fonte, mas o da build)

## 📚 Recursos Adicionais

- **Material Design Colors**: https://material.io/design/color
- **CSS Grid**: https://css-tricks.com/snippets/css/complete-guide-grid/
- **WebView2**: https://learn.microsoft.com/en-us/microsoft-edge/webview2/

---

**Versão:** 1.0
**Última atualização:** 2026-01-10
**Autor:** Sistema de Templates JJManager
