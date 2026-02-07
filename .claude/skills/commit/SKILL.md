---
name: commit
description: Realiza commit das alteracoes no JJManager com mensagem descritiva e push automatico
argument-hint: [mensagem-opcional]
---

# Commit JJManager

Realiza commit das alteracoes no projeto JJManager com mensagem descritiva.

## Fluxo de Execucao

### 1. Sincronizar com Remote (Pull)

Antes de commitar, sincronizar com o repositorio remoto:

```bash
git pull
```

Se houver conflitos, abortar e informar o usuario.

### 2. Verificar Mudancas

Executar para ver o estado atual:

```bash
git status
git diff --stat
```

Se nao houver mudancas, informar e encerrar.

### 3. Analisar Mudancas

Para gerar mensagem automatica, analisar:
- Quais arquivos foram modificados
- Qual componente/dispositivo foi afetado (JJDB-01, JJM-01, JJLC-01, etc)
- Tipo de mudanca (correcao, adicao, melhoria, implementacao, remocao)

### 4. Definir Mensagem de Commit

#### Se argumento `$ARGUMENTS` foi fornecido:
- Usar a mensagem fornecida diretamente
- NAO perguntar confirmacao
- Pular para passo 6

#### Se sem argumento (mensagem automatica):
1. Gerar mensagem baseada na analise das mudancas
2. Perguntar ao usuario usando AskUserQuestion:
   - "A mensagem esta OK?"
   - Opcao 1: "Sim, usar essa mensagem" (Recomendado)
   - Opcao 2: "Nao, quero inserir minha propria mensagem"
3. Se usuario escolher inserir propria, solicitar a mensagem

### 5. Exibir Resumo e Confirmar Arquivos

Antes de commitar, mostrar ao usuario um resumo claro dos arquivos que serao afetados:

```
📋 Arquivos que serao commitados:
────────────────────────────────────────

  ✚ Novos (2):
    • .claude/skills/commit/SKILL.md
    • JJManager/Class/NewFile.cs

  ✎ Modificados (3):
    • JJManager/Class/Devices/JJM01.cs
    • JJManager/Pages/Devices/JJM01.cs
    • CLAUDE.md

  ✖ Removidos (1):
    • JJManager/Class/OldFile.cs

────────────────────────────────────────
  Total: 6 arquivos
```

**Categorias:**
- **Novos (✚)**: Arquivos untracked que serao adicionados
- **Modificados (✎)**: Arquivos existentes com alteracoes
- **Removidos (✖)**: Arquivos deletados

**Formato:**
- Agrupar por categoria
- Mostrar contagem por categoria
- Mostrar total de arquivos
- Usar caminho relativo ao projeto

**Apos exibir o resumo, perguntar ao usuario usando AskUserQuestion:**
- "Deseja enviar esses arquivos?"
- Opcao 1: "Sim, continuar com o commit" (Recomendado)
- Opcao 2: "Nao, cancelar operacao"

Se usuario escolher cancelar, encerrar sem fazer commit.

### 6. Executar Commit

```bash
git add <arquivos-modificados>
git commit -m "<mensagem>"
```

**IMPORTANTE**:
- NAO adicionar Co-Authored-By
- NAO usar `git add -A` ou `git add .` para evitar arquivos sensiveis
- Adicionar arquivos especificos pelo nome

### 7. Push Automatico

Apos commit bem-sucedido:

```bash
git push
```

Informar resultado do push ao usuario.

## Formato da Mensagem

### Estilo

- **Idioma**: Portugues
- **Tamanho**: Maximo ~72 caracteres
- **Formato**: Descritiva e enxuta
- **Sem**: Co-Authored-By, emojis, prefixos tipo "feat:", "fix:"

### Padrao para Geracao Automatica

1. Identificar componente afetado (JJDB-01, JJM-01, AudioController, etc)
2. Identificar tipo de mudanca:
   - `Adicionado` - nova funcionalidade/arquivo
   - `Corrigido` - bug fix
   - `Melhorias` - refatoracao, otimizacao
   - `Implementacao` - nova feature grande
   - `Removido` - codigo/arquivo removido
   - `Atualizado` - mudanca em existente
3. Descrever brevemente o que foi feito

### Exemplos de Mensagens Validas

Baseado no historico do projeto:
- "Melhorias na tela de edicao do Dashboard JJDB-01"
- "Adicionado botao de conexao na tela de edicao do JJM-01"
- "Corrigido bug do hover dos LEDs no JJDB-01"
- "Implementacao da comunicacao byte-based na JJLC-01"
- "Atualizado protocolo de comunicacao do JJB999"
- "Removido codigo legado de conexao Bluetooth"

## Arquivos Sensiveis (NAO commitar)

- `.env`
- `credentials.json`
- `*.key`
- `*.pem`
- Arquivos com senhas ou tokens

Se algum desses arquivos estiver modificado, avisar o usuario e NAO incluir no commit.

## Exemplo de Uso

### Com mensagem automatica:
```
/commit
```
1. Faz pull
2. Analisa mudancas
3. Sugere mensagem e pergunta confirmacao
4. Exibe resumo dos arquivos
5. Pergunta se deseja enviar
6. Commita
7. Faz push

### Com mensagem manual:
```
/commit Corrigido problema de conexao no JJM-01
```
1. Faz pull
2. Exibe resumo dos arquivos
3. Pergunta se deseja enviar
4. Commita com mensagem fornecida
5. Faz push

## Tratamento de Erros

### Pull com conflitos
- Abortar operacao
- Informar usuario sobre conflitos
- Sugerir resolver manualmente

### Nenhuma mudanca
- Informar que nao ha nada para commitar
- Encerrar sem erro

### Push falhou
- Informar erro
- Sugerir verificar conexao ou permissoes

## Verificacao Final

Apos execucao, mostrar:
1. Arquivos commitados
2. Mensagem do commit
3. Resultado do push (sucesso ou falha)
4. Hash do commit (se sucesso)
