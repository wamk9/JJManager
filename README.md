# JJManager

[![Version](https://img.shields.io/badge/version-1.2.9-blue.svg)](https://github.com/johnjohn3d/jjmanager)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple.svg)](https://dotnet.microsoft.com/download/dotnet-framework)

**JJManager** é o software oficial de gerenciamento para dispositivos de hardware JohnJohn3D, incluindo ButtonBoxes, Mixers de Áudio, Dashboards, LoadCells e muito mais.

Com uma interface intuitiva em Material Design, o JJManager permite configurar completamente seus dispositivos, criar perfis personalizados, programar macros, controlar áudio do sistema e integrar com simuladores de corrida através do SimHub.

---

## O que é o JJManager?

O JJManager é uma aplicação desktop para Windows que permite:

- **Configurar dispositivos JohnJohn3D** com total controle sobre inputs e outputs
- **Criar perfis personalizados** para diferentes aplicações e jogos
- **Programar macros de teclado/mouse** para automatizar tarefas
- **Controlar áudio do sistema** diretamente pelos seus dispositivos
- **Integrar com simuladores** de corrida via SimHub (telemetria em tempo real)
- **Atualizar firmware** dos dispositivos de forma automática

---

## Dispositivos Suportados

### ButtonBoxes
- **JJB-01** (V1 e V2)
- **JJB-999**
- **JJBP-06**
- **JJB-Slim Type A**

### Outros Dispositivos
- **JJM-01** - Mixer de Áudio
- **JJDB-01** - Dashboard para Simuladores
- **JJSD-01** - Streamdeck
- **JJLC-01** - LoadCell

---

## Principais Funcionalidades

### Gerenciamento de Perfis
Crie perfis diferentes para cada aplicação ou jogo. Troque entre perfis com um clique e tenha configurações específicas para cada uso.

### Sistema de Macros
- Grave sequências de teclas e cliques do mouse
- Execute atalhos complexos com um único botão
- Suporte a delays e repetições

### Controle de Áudio
- Controle o volume de aplicações individuais
- Mude dispositivos de áudio (fones, caixas de som)
- Mute/unmute rápido de aplicações específicas

### Integração SimHub
- Conecte ao SimHub para dados de telemetria em tempo real
- Configure LEDs que reagem a RPM, velocidade, marcha, etc.
- Suporte a múltiplos simuladores (iRacing, ACC, F1, etc.)

### Atualização de Firmware
- Atualizações automáticas via internet
- Update de firmware dos dispositivos de forma fácil
- Notificações quando novas versões estão disponíveis

---

## Requisitos do Sistema

### Mínimos
- **Sistema Operacional:** Windows 10 ou superior (64-bit)
- **.NET Framework:** 4.7.2 ou superior
- **Portas USB:** Para conexão dos dispositivos
- **Espaço em Disco:** 500 MB

### Recomendados
- **Sistema Operacional:** Windows 11 (64-bit)
- **.NET Framework:** 4.8
- **RAM:** 4 GB ou mais
- **SimHub:** Para integração com simuladores (opcional)

---

## Como Usar

### Instalação

1. **Baixe** a última versão do JJManager do [site oficial](https://johnjohn3d.com.br/jjmanager) ou nos sites específicos dos dispositivos
2. **Execute** o instalador
3. **Siga** as instruções na tela
4. **Conecte** seu dispositivo JohnJohn3D via USB
5. **Abra** o JJManager - seus dispositivos serão detectados automaticamente

### Primeiro Uso

1. **Conecte seu dispositivo** - O JJManager detectará automaticamente
2. **Clique no dispositivo** para habilitar a tela de configuração
3. **Crie um perfil** - Dê um nome e salve-o
4. **Configure os inputs/outputs** - Atribua funções a cada botão/controle no perfil selecionado
5. **Teste!** - Todos as suas configurações estarão funcionando imediatamente

## Integração SimHub

### Configuração

1. **Instale o SimHub** (https://www.simhubdash.com)
1. **Instale o plugin JJManager Sync** no SimHub, presente no [site oficial do JJManager](https://johnjohn3d.com.br/jjmanager)
2. **Abra o SimHub** e vá em "Add/remove feature"
3. **Ative** a integração com o JJManager (JJManager Sync)
4. **Configure** quais propriedades do simulador deseja usar no seu dispositivo junto ao JJManager

### Algumas Propriedades Suportadas

- Velocidade, RPM, Marcha
- Pressão e temperatura dos pneus
- Nível de combustível
- Flags (bandeiras de corrida)
- Posição na corrida
- Tempo de volta
- E muito mais!

---

## Suporte e Documentação

### Links Úteis

- **Website Oficial:** [johnjohn3d.com.br](https://johnjohn3d.com.br)
- **Manuais dos Dispositivos:** Disponíveis no site oficial de cada dispositivo
- **Tutoriais em Vídeo:** Canal do YouTube JohnJohn3D
- **Fórum da Comunidade:** Entre em contato via redes sociais

### Problemas Comuns

**Dispositivo não é reconhecido:**
- Verifique se o cabo USB está bem conectado
- Tente outra porta USB
- Reinicie o JJManager

**Firmware desatualizado:**
- Abra o JJManager
- Vá em Menu > Atualizações
- Siga as instruções para atualizar

**Macro não funciona:**
- Verifique se o perfil correto está ativo
- Regrave a macro se necessário
- Alguns jogos podem bloquear macros (anti-cheat)

### Perguntas Frequentes (FAQ)

**P: O JJM-01 demora muito para conectar. É normal?**
R: Não! Com as últimas atualizações, a conexão deve levar apenas 3-5 segundos. Se demorar mais, tente:
- Fechar apps de áudio desnecessários antes de conectar
- Reiniciar o JJManager
- Verificar se há atualizações disponíveis

**P: O app que abri não está sendo detectado pelo dispositivo na configuração de som. O que fazer?**
R: O JJManager detecta novos apps automaticamente em menos de 1 segundo. Se não detectar:
- Verifique se o nome do executável está correto (ex: "spotify.exe", não "Spotify")
- Aguarde 1-2 segundos após abrir o app
- Clique no encoder do JJM-01 para forçar uma atualização

**P: Posso controlar múltiplas janelas do Chrome/Firefox ao mesmo tempo?**
R: Sim! Configure o executável do navegador (ex: "chrome.exe") e TODAS as janelas/abas serão controladas simultaneamente. Perfeito para quem tem várias abas com áudio.

**P: O LED do meu JJB-999/JJB-01 V2 apaga sozinho. Por quê?**
R: O sistema Keep-Alive desliga o LED automaticamente após 5 segundos sem comunicação para economizar energia. Certifique-se de que:
- O JJManager está aberto e conectado ao dispositivo
- O cabo USB está bem conectado
- O perfil está ativo

**P: Como sei se o protocolo byte-based está funcionando?**
R: Dispositivos com protocolo byte-based (JJB-999, JJB-01 V2) têm resposta instantânea do LED (< 100ms). Se o LED demora para reagir, pode haver problema de conexão.

**P: O controle de áudio consome muita CPU?**
R: Não! Implementamos throttling de eventos que reduziu de 200+ eventos/segundo para apenas 2/segundo. O uso de CPU é mínimo (<1%).

**P: Posso usar o JJManager sem internet?**
R: Sim! O JJManager funciona completamente offline. A internet é necessária apenas para:
- Baixar atualizações de firmware
- Baixar atualizações do software
- Integração SimHub (que pode ser local)

---

## Versão Atual

**Versão em Produção:** 1.2.9.0
**Versão em Desenvolvimento:** 1.3.0.0

### Novidades e Melhorias Recentes

#### 🚀 Performance Extremamente Melhorada no JJM-01 (Audio Mixer)
- **Inicialização até 10x mais rápida** - De 15-30 segundos para 3-5 segundos
- **Cache inteligente de sessões de áudio** - Evita chamadas desnecessárias ao sistema
- **Processamento paralelo** - Múltiplos inputs são configurados simultaneamente
- **Detecção instantânea de novos apps** - Quando você abre um app de áudio, ele é reconhecido em menos de 1 segundo

#### 🎯 Controle de Áudio Inteligente
- **Detecção automática de novas sessões** - Abriu o Spotify, Discord ou qualquer app? O JJManager detecta automaticamente
- **Aplicação instantânea de volume** - Apps novos já iniciam com o volume configurado
- **Suporte a múltiplas instâncias** - Controle todos os Firefox/Chrome abertos de uma vez
- **Persistência de configurações** - O volume configurado é aplicado mesmo após reiniciar o app

#### ⚡ Protocolo de Comunicação Otimizado (JJB-999, JJB-01 V2)
- **Protocolo byte-based de alta velocidade** - Substituiu JSON por comunicação binária
- **Sistema Keep-Alive** - LED sempre sincronizado (timeout de 5s, keep-alive a cada 3s)
- **Change Tracking** - Envia apenas quando configurações mudam (menos tráfego USB)
- **4 modos de LED**: Desligado, Sempre Ligado, Pulsando, Piscando, SimHub Sync

#### 💡 Feedback Visual Durante Atualizações
- **Barras de progresso** agora aparecem na barra de status durante downloads de firmware e plugins
- **Acompanhamento em tempo real** do progresso de upload de firmware para seus dispositivos
- Você sempre saberá exatamente quanto tempo falta para concluir uma atualização

#### 🎨 Interface Mais Moderna
- **Caixas de diálogo renovadas** com design Material Design em todo o aplicativo
- **Aparência consistente** em todas as mensagens e notificações
- **Melhor experiência visual** que combina perfeitamente com o tema do JJManager

#### 🧹 Otimização Automática de Espaço
- **Limpeza automática** de arquivos temporários de download ao iniciar o JJManager
- **Economia de espaço em disco** - Não precisa mais se preocupar com arquivos antigos acumulando
- **Inicialização mais limpa** sem arquivos desnecessários

#### 📋 Compartilhamento de Perfis
- **Perfil padrão compartilhado** entre dispositivos do mesmo modelo
- **Configuração mais rápida** ao conectar novos dispositivos
- **Consistência** nas configurações entre múltiplos dispositivos

#### 🔧 Melhorias Técnicas de Estabilidade
- **Correção de race conditions** em conexão/desconexão rápida de dispositivos
- **Thread-safe** em operações de áudio críticas
- **Throttling de eventos** para evitar sobrecarga de CPU (de 200+ eventos/s para 2/s)
- **Cleanup automático** de recursos ao desconectar dispositivos

---

## Recursos Técnicos Avançados

### Arquitetura de Comunicação

**Protocolo HID Byte-Based (JJB-999, JJB-01 V2):**
- Comunicação binária de alta velocidade substituindo JSON
- Formato: `[CMD_H][CMD_L][PAYLOAD][FLAG_H][FLAG_L]`
- Keep-Alive: 3 segundos (client) / 5 segundos timeout (firmware)
- Change Tracking: Envia apenas quando valores mudam

**Sistema de Audio Controller:**
- Cache inteligente de PIDs para evitar WMI calls
- Processamento paralelo de múltiplos inputs (Task.WhenAll)
- Detecção automática de novas sessões via NAudio callbacks
- Throttling de eventos: 500ms entre mudanças de propriedade

### Performance

**Benchmarks JJM-01:**
- Inicialização: ~3-5 segundos (5 inputs)
- Cache hit rate: >90% após primeira inicialização
- Detecção de novo app: <1 segundo
- CPU usage: <1% em operação normal
- Memória: ~150MB (inclui cache de sessões)

**Otimizações Implementadas:**
- Static shared variables para cache entre inputs
- Thread-safe operations (lock-based synchronization)
- Early exit em event handlers quando não há listeners
- Cleanup automático de recursos COM (MMDeviceEnumerator)

### Database

**Tipo:** SQL Server LocalDB (.mdf)
**Localização:** `%APPDATA%\JohnJohn3D\JJManager\JJManagerDB.mdf`
**Migrações:** Automáticas (v1.1.13 até v1.2.9)
**Backup:** Automático antes de cada migração

### Logs

**Localização:** `%APPDATA%\JohnJohn3D\JJManager\Log\`
**Formato:** Um arquivo por módulo (`Log_<ModuleName>.txt`)
**Conteúdo:** Apenas erros (catch blocks) - logs de debug foram removidos para performance

---

## Licença e Direitos

© 2026 JohnJohn 3D - Todos os direitos reservados.

Este software é proprietário e destinado exclusivamente para uso com dispositivos JohnJohn3D.

---

## Contato

**JohnJohn 3D**
- Website: https://johnjohn3d.com.br
- Email: contato@johnjohn3d.com.br
- Instagram: [@johnjohn.3d](https://instagram.com/johnjohn.3d)

---

*Para informações técnicas de desenvolvimento, consulte o arquivo `claude.md`*
