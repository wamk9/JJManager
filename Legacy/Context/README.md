# Context - Documentação Técnica JJManager

Esta pasta contém contextos históricos e documentação técnica específica de dispositivos e componentes do JJManager.

## Estrutura

```
Context/
├── README.md                         # Este arquivo (guia de navegação)
├── App/                              # Componentes da aplicação
│   └── AudioController.md           # Sistema de controle de áudio do Windows
└── Devices/                          # Dispositivos JJManager
    ├── Connections/                 # Camadas de comunicação
    │   └── HID.md                   # Protocolo HID byte-based (especificação completa)
    ├── JJB01_V2.md                  # ButtonBox JJB-01 V2
    ├── JJB999.md                    # ButtonBox JJB-999
    └── JJM01.md                     # Mixer de Áudio JJM-01
```

## Documentação por Dispositivo

### ButtonBoxes

#### JJB01_V2.md
Contexto completo do ButtonBox JJB-01 V2:
- Migração para protocolo byte-based (2025-12-25)
- Correções de UI (FlowLayoutPanel, auto-save, slider)
- Sistema Keep-Alive (3s cliente / 5s timeout)
- SimHub Mode com prioridade de outputs
- Performance (latência < 100ms, CPU < 1%)

#### JJB999.md
Contexto do ButtonBox JJB-999:
- Protocolo **idêntico** ao JJB01_V2
- Mesmos comandos e sistema de keep-alive
- Referencia JJB01_V2.md para detalhes do protocolo

### Audio Mixer

#### JJM01.md
Contexto completo do Mixer de Áudio JJM-01:
- Implementação de protocolo byte-based request/response (2025-12-28)
- Performance 10x mais rápida (15-30s → 3-5s)
- RequestHIDBytes com timeout configurável
- Cache inteligente de PIDs (> 90% hit rate)
- Throttling de eventos (200+/s → 2/s)
- Detecção instantânea de apps (< 1 segundo)

## Componentes da Aplicação

### AudioController.md
Contexto do sistema de controle de áudio:
- Correções de controle de volume ao trocar dispositivo (2025-12-30)
- IDisposable pattern para cleanup adequado
- Busca em tempo real de sessions
- Proteção anti-loop infinito
- RefreshSessionsToControl com cache optimization (2026-01-02)
- SessionCreated event handler fix (2026-01-02)

## Camadas de Comunicação

### HID.md (Devices/Connections/)
Contexto da camada de comunicação HID:
- Criação da classe HIDMessage (2025-12-26)
- Correção de FLAGS no protocolo (FLAG_L = CMD_L)
- Implementação de RequestHIDBytes (2025-12-28)
- Migração Device.Net → HidSharp (2025-12)
- Lista completa de dispositivos com protocolo byte-based

## Protocolo HID Byte-Based

### HID.md (Devices/Connections/)
Especificação completa do protocolo byte-based para todos os dispositivos HID:
- Estrutura da mensagem `[CMD_H][CMD_L][PAYLOAD][FLAG_H][FLAG_L]`
- Comandos específicos por dispositivo (JJB01_V2, JJB999, JJM01, JJDB01)
- Sistema de FLAGS (CONTINUE, END)
- Chunking support para mensagens grandes
- Device Info Request/Response
- Troubleshooting e diagramas de sequência
- Exemplos de implementação (Firmware e JJManager)

## Como Usar Esta Documentação

### Para Desenvolvimento de Novos Dispositivos
1. Leia **HID.md** (Devices/Connections/) para entender a camada de comunicação e protocolo completo
2. Use **JJB01_V2.md** ou **JJB999.md** como template de implementação
3. Consulte seções "Comandos Específicos Detalhados" e "Exemplos de Uso" em **HID.md**
4. Siga padrões estabelecidos (Keep-Alive, Change Tracking, FLAGS corretos)

### Para Manutenção de Dispositivos Existentes
1. Consulte o arquivo específico do dispositivo (ex: **JJM01.md**)
2. Veja histórico de correções na seção "Bugs Corrigidos"
3. Verifique otimizações implementadas na seção "Performance"

### Para Debugging de Áudio
1. Leia **AudioController.md** para entender o sistema
2. Consulte "Fluxo Atual" para ver como events são processados
3. Veja "Bugs Corrigidos" para problemas conhecidos e soluções

### Para Implementar Novos Recursos
1. Verifique se recurso já foi implementado em outro dispositivo
2. Consulte documentação do componente relacionado
3. Siga padrões estabelecidos (Keep-Alive, Change Tracking, etc.)

## Convenções

### Formato dos Arquivos
- **Markdown** (.md) para fácil leitura e versionamento
- **Seções hierárquicas** (##, ###, ####) para navegação
- **Exemplos de código** com syntax highlighting
- **Tabelas** para comparações e listas estruturadas

### Informações Incluídas
- **Visão Geral**: Resumo do componente/dispositivo
- **Características**: Specs técnicas principais
- **Histórico de Desenvolvimento**: Mudanças cronológicas com datas
- **Bugs Corrigidos**: Problema + solução + código
- **Performance**: Benchmarks e otimizações
- **Arquivos Relacionados**: Referências cruzadas

### Referências Cruzadas
Use links relativos para referenciar outros arquivos:
```markdown
Ver [JJB01_V2.md](./JJB01_V2.md) para detalhes.
Ver [Protocolo Completo](../JJB01_V2_Protocol.md).
```

## Manutenção

### Adicionando Novo Dispositivo
1. Crie `Devices/[DEVICE_NAME].md`
2. Use template de JJB01_V2.md ou JJM01.md
3. Inclua todas as seções padrão
4. Atualize este README

### Atualizando Contexto Existente
1. Adicione data no título da seção (ex: "### 2026-01-02: Nova Feature")
2. Inclua código antes/depois quando relevante
3. Documente o problema/motivação
4. Explique a solução implementada

### Removendo Contexto Obsoleto
- Não remova histórico (importante para entender decisões)
- Marque seções obsoletas com **[OBSOLETO - YYYY-MM-DD]**
- Adicione link para nova implementação

## Histórico de Organização

### 2026-01-02: Criação da Estrutura Organizada
- Consolidação de 5 arquivos de contexto (CONTEXT_2025-12-*.md, RESUMO_*.md)
- Criação de estrutura hierárquica (App/, Devices/, Devices/Connections/)
- Extração de informações por dispositivo e componente
- Remoção de arquivos temporários da raiz
- Preservação apenas de CLAUDE.md e README.md na raiz

---

**Nota:** Para documentação geral do projeto, veja `../CLAUDE.md` e `../README.md` na raiz.
