---
name: update-version
description: Atualiza a versao do JJManager em todos os arquivos necessarios (AssemblyInfo, csproj, Migrate, Resources, SQL, CLAUDE.md, README.md)
argument-hint: [nova-versao]
---

# Atualizar Versao do JJManager

Atualiza todos os arquivos de versao do projeto JJManager para a versao `$ARGUMENTS`.

## Formato da Versao

A versao deve estar no formato:
- **4 partes**: `MAJOR.MINOR.PATCH.REVISION` (ex: 1.3.0.1)
- **3 partes**: `MAJOR.MINOR.PATCH` (ex: 1.3.0) - sera convertida para 4 partes com REVISION = 0

## Arquivos a Atualizar

### 1. AssemblyInfo.cs
**Caminho**: `JJManager/Properties/AssemblyInfo.cs`

Atualizar as linhas:
```csharp
[assembly: AssemblyVersion("X.Y.Z.W")]
[assembly: AssemblyFileVersion("X.Y.Z.W")]
```

### 2. JJManager.csproj
**Caminho**: `JJManager/JJManager.csproj`

Atualizar a tag ApplicationVersion:
```xml
<ApplicationVersion>X.Y.Z.W</ApplicationVersion>
```

### 3. Migrate.cs
**Caminho**: `JJManager/Class/Migrate.cs`

Adicionar nova versao na lista `_versions` ANTES do comentario "// Last Version":
```csharp
_versions.Add(new Version(X, Y, Z));        // se REVISION = 0
_versions.Add(new Version(X, Y, Z, W));     // se REVISION > 0
```

Mover o comentario "// Last Version" para a nova linha.

### 4. Resources.resx
**Caminho**: `JJManager/Properties/Resources.resx`

Adicionar entrada para o novo arquivo SQL ANTES de `</root>`:
```xml
<data name="SQL_X_Y_Z" type="System.Resources.ResXFileRef, System.Windows.Forms">
  <value>..\MigrateQuerys\X_Y_Z.sql;System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089;Windows-1252</value>
</data>
```

Se REVISION > 0, usar `SQL_X_Y_Z_W` e `X_Y_Z_W.sql`.

### 5. Arquivo SQL de Migracao
**Criar arquivo**: `JJManager/MigrateQuerys/X_Y_Z.sql` (ou `X_Y_Z_W.sql` se REVISION > 0)

Conteudo:
```sql
-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

-- Migration X.Y.Z.W: [Descricao das alteracoes]

UPDATE dbo.configs SET software_version = 'X.Y.Z.W';
GO

SET ANSI_WARNINGS on
GO
```

### 6. JJManager.csproj - Referencia ao SQL
**Caminho**: `JJManager/JJManager.csproj`

Adicionar referencia ao novo arquivo SQL na secao de arquivos, proximo aos outros MigrateQuerys:
```xml
<None Include="MigrateQuerys\X_Y_Z.sql" />
```

### 7. CLAUDE.md
**Caminho**: `CLAUDE.md`

Atualizar a linha de versao:
```markdown
- **Versao Atual:** X.Y.Z.W
```

### 8. README.md
**Caminho**: `README.md`

Atualizar as seguintes referencias de versao (se existirem):

#### 8.1 Badge de Versao (linha ~3)
```markdown
[![Version](https://img.shields.io/badge/version-X.Y.Z-blue.svg)]
```
Nota: Badge usa formato 3 partes (X.Y.Z), sem REVISION.

#### 8.2 Versao em Producao (linha ~190)
```markdown
**Versao em Producao:** X.Y.Z.W
```

#### 8.3 Versao em Desenvolvimento (linha ~191)
Atualizar para a PROXIMA versao provavel:
```markdown
**Versao em Desenvolvimento:** X.Y.(Z+1).0
```
Ou manter a mesma se a versao em desenvolvimento ja for maior.

#### 8.4 Range de Migracoes (linha ~276)
```markdown
**Migracoes:** Automaticas (v1.1.13 ate vX.Y.Z)
```
Nota: Usar formato 3 partes (X.Y.Z).

## Passos de Execucao

1. **Validar formato** da versao fornecida
2. **Ler versao atual** do AssemblyInfo.cs
3. **Verificar se versao e maior** que a atual (nao permitir downgrade)
4. **Atualizar AssemblyInfo.cs** com nova versao
5. **Atualizar JJManager.csproj** ApplicationVersion
6. **Atualizar Migrate.cs** adicionando nova versao na lista
7. **Atualizar Resources.resx** com referencia ao novo SQL
8. **Criar arquivo SQL** de migracao
9. **Atualizar JJManager.csproj** com referencia ao arquivo SQL
10. **Atualizar CLAUDE.md** com nova versao

## Exemplo de Uso

```
/update-version 1.3.1.0
```

Isso ira:
- Atualizar de 1.3.0.1 para 1.3.1.0
- Criar arquivo `1_3_1.sql` (REVISION = 0, omite o W)
- Adicionar `SQL_1_3_1` no Resources.resx
- Adicionar `new Version(1, 3, 1)` no Migrate.cs

```
/update-version 1.3.1.1
```

Isso ira:
- Atualizar de 1.3.1.0 para 1.3.1.1
- Criar arquivo `1_3_1_1.sql` (REVISION > 0, inclui o W)
- Adicionar `SQL_1_3_1_1` no Resources.resx
- Adicionar `new Version(1, 3, 1, 1)` no Migrate.cs

## Verificacao Final

Apos todas as alteracoes, mostrar:
1. Resumo de arquivos modificados
2. Versao anterior vs nova versao
3. Lista de todos os arquivos criados/alterados

## Notas Importantes

- NAO fazer commit automaticamente - deixar para o usuario decidir
- Se algum arquivo de origem nao existir (AssemblyInfo.cs, etc), reportar erro
- Manter encoding Windows-1252 nos arquivos SQL
- Usar underscores (_) no nome dos arquivos SQL, nao pontos

## Tratamento de Arquivos Existentes

Se algum arquivo de migracao ou entrada ja existir, NAO acusar erro. Apenas:

1. **Arquivo SQL ja existe**: Verificar se o conteudo esta correto (tem UPDATE para a versao certa). Se estiver, pular criacao. Se nao estiver, atualizar.

2. **Versao ja existe no Migrate.cs**: Verificar se a versao esta na lista. Se estiver, pular adicao. Se nao estiver, adicionar.

3. **Entrada ja existe no Resources.resx**: Verificar se a entrada SQL_X_Y_Z existe. Se existir, pular. Se nao existir, adicionar.

4. **Referencia ja existe no csproj**: Verificar se a referencia ao arquivo SQL existe. Se existir, pular. Se nao existir, adicionar.

5. **Versao ja esta correta nos arquivos**: Se AssemblyInfo.cs, csproj ou CLAUDE.md ja tem a versao correta, pular atualizacao desse arquivo.

## Checagens Obrigatorias

Antes de finalizar, verificar:
- [ ] AssemblyInfo.cs tem a versao correta
- [ ] JJManager.csproj tem ApplicationVersion correta
- [ ] Migrate.cs tem a versao na lista
- [ ] Resources.resx tem entrada para o SQL
- [ ] Arquivo SQL existe e tem UPDATE correto
- [ ] csproj tem referencia ao arquivo SQL
- [ ] CLAUDE.md tem versao correta

Reportar no final quais arquivos foram:
- **Criados**: arquivos novos
- **Atualizados**: arquivos modificados
- **Ignorados**: arquivos que ja estavam corretos
