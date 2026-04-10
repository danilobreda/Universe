# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projeto

Mini-Universo Simulado — simulação de física emergente em C# .NET 10 com Raylib-cs. Regras simples no nível fundamental geram comportamentos complexos (órbitas, clusters, proto-átomos, regiões térmicas) sem programar esses conceitos diretamente.

`inicio.txt` é a fonte da verdade. Não adicione complexidade que não esteja prevista lá.

## Comandos

```bash
dotnet build              # compilar
dotnet run                # executar (ESC para sair)
```

# ⚠️ Regras de Ouro do Projeto

1. **Nunca programe o resultado final**
2. **Sempre programe regras locais**
3. **Tudo deve emergir**
4. **Sistema deve ser determinístico + pequenas variações**
5. **Evitar overengineering**

# 🧠 Filosofia do Sistema

* O universo não é feito de objetos complexos
* Ele é feito de **regras simples que permitem complexidade**

## Arquitetura

Loop N² direto com gravidade + força elétrica em universo toroidal 2D, com convenção de mínima imagem nas distâncias. Inicialização tipo Big Bang: tudo concentrado num pequeno disco central, expandindo radialmente. Damping radial em encontros próximos é a única forma de dissipação (Seção 8.2 do `inicio.txt`).

### Fluxo de um tick

1. **Forças** (paralelo): para cada partícula, soma `Gravity + Electric` de todas as outras → atualiza velocidade
2. **Damping radial** (sequencial): para cada par com distância < `CollisionRadius` que está se **aproximando**, dissipa parte da componente radial da velocidade relativa. A componente tangencial fica intacta — órbitas circulares são pontos fixos do operador
3. **Integração**: `ClampSpeed`, atualiza posição, wrap toroidal nas bordas

### Por que esse damping específico

- Em estado estável (órbita circular), `radialSpeed = 0` → nenhuma energia é tirada → átomo é eterno
- Órbita elíptica circulariza ao longo do tempo (perde energia só nas fases de aproximação)
- Encontros frontais entre partículas livres perdem energia → universo esfria
- É "perda de energia em impactos" da Seção 8.2 generalizada para qualquer encontro próximo

Sem isso, o Big Bang nunca esfriaria abaixo da energia de ligação, e átomos não emergem.

### Componentes

- **Vec2** (`Vec2.cs`) — struct 2D
- **Particle** (`Particle.cs`) — `Id`, posição, velocidade, massa base, carga (-1/0/+1), interação Higgs
- **Field** (`Field.cs`) — campo de Higgs uniforme (intensidade = 1.0)
- **Physics** (`Physics.cs`) — constantes (G, K, SpeedLimit, CollisionRadius, CollisionDamping, WorldSize), `Mass`, `Gravity`, `Electric`, `ClampSpeed`, `MinImage`
- **Simulation** (`Simulation.cs`) — Big Bang init com duas espécies, loop de simulação, damping radial, wrap toroidal
- **AtomDetector** (`AtomDetector.cs`) — **observador puro**, não toca na simulação. Aplica 4 critérios pra detectar pares ligados:
  1. **Vizinhança mútua** entre cargas opostas
  2. **Energia ligada**: `½·μ·v_rel² + K·q₁·q₂/r < 0` (μ = massa reduzida)
  3. **Persistência** por N ticks consecutivos
  4. **Isolamento** (nenhum terceiro corpo dentro de raio R)
- **Diagnostics** (`Diagnostics.cs`) — métricas derivadas (energia cinética, temperatura). Observações, NÃO variáveis do sistema
- **Renderer** (`Renderer.cs`) — Raylib-cs, partículas coloridas por carga, anéis brancos ao redor de átomos detectados, HUD com época cosmológica

### Inicialização Big Bang

- Posição: disco uniforme de raio `worldSize * 0.06` no centro
- Velocidade: radial pra fora com magnitude `bangSpeed` × jitter (0.7–1.3) + ruído tangencial
- Duas populações alternadas:
  - **Pesada+** (heavy): `BaseMass=8`, `Charge=+1`
  - **Leve-** (light): `BaseMass=0.4`, `Charge=-1`
- Razão de massa 20:1. Não são "prótons" e "elétrons" no código — são apenas pesadas-positivas e leves-negativas. O conceito "átomo" emerge depois, do estado dinâmico, não da etiqueta.

### O que esperar ao rodar

1. **Big Bang** (~tick 0–500): explosão central, temperatura altíssima, `H = 0`
2. **Resfriando** (~tick 500–3000): partículas se espalham pelo toro, encontros próximos dissipam energia, temperatura cai
3. **Recombinação**: temperatura cruza a escala da energia de ligação, contador `H:` começa a subir do zero
4. **Era Atômica**: a maioria dos pares mútuos ligados sobrevive, só sobram alguns livres

A época é classificada na HUD a partir de `Temperature` + estado do detector — também é observação derivada, não controle.

### Controles

| Tecla | Ação |
|---|---|
| SPACE | Pausar/continuar |
| A | Toggle visualização dos átomos detectados |
| Scroll | Zoom (em direção ao mouse) |
| Setas / Mouse direito | Pan |
| Home | Reset câmera |
| ESC | Sair |

## Filosofia / Regras de Ouro

1. **Nunca programe o resultado final** — apenas regras locais
2. **Tudo deve emergir** — energia, temperatura, átomos, épocas são derivados, não variáveis
3. **Sistema determinístico** com seed fixa
4. **Detector ≠ controle** — `AtomDetector` é um observador. Apertar [A] não muda a simulação, só esconde o desenho
5. **Evitar overengineering** — sem painel de tuning, sem propagação de campo, sem spatial hash. N² funciona bem até ~1000 partículas

## Evoluções Previstas (Seção 8 do inicio.txt)

- 8.1 Campo de Higgs não uniforme
- 8.2 Colisões e dissipação — **implementado** (damping radial)
- 8.3 Ligações (proto-química) — **implementado via emergência** (atração de Coulomb + damping = estados ligados estáveis, detectados pelo `AtomDetector`)
- 8.4 Energia potencial explícita
- 8.5 Limite de velocidade tipo relativístico — **implementado** (`ClampSpeed`)
