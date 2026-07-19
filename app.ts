import "./styles.css";
import {
  BOARD_SIZE,
  CELL_COUNT,
  CHINO_YORIMITSU,
  ENEMY_MAX_TROOPS,
  MAX_MORALE,
  TAKEDA_MAX_TROOPS,
  TAKEDA_WARRIORS,
  createBattle,
  openCell,
  toggleFlag,
  type BattleState,
} from "./game-logic";

type Screen = "title" | "chapter" | "stages" | "battle";

interface AppState {
  screen: Screen;
  battle: BattleState | null;
  flagMode: boolean;
  resultDismissed: boolean;
}

const app = document.querySelector<HTMLElement>("#app")!;

const state: AppState = {
  screen: "title",
  battle: null,
  flagMode: false,
  resultDismissed: false,
};

const takedaBishi = `
  <div class="takeda-bishi" aria-label="武田菱">
    <i></i><i></i><i></i><i></i>
  </div>
`;

function render(): void {
  switch (state.screen) {
    case "title":
      renderTitle();
      break;
    case "chapter":
      renderChapter();
      break;
    case "stages":
      renderStages();
      break;
    case "battle":
      renderBattle();
      break;
  }
  window.scrollTo({ top: 0, behavior: "auto" });
}

function renderTitle(): void {
  app.innerHTML = `
    <section class="screen title-screen">
      <p class="eyebrow">甲斐より天下へ</p>
      <h1 class="game-title"><span>武田晴信</span><span>天下統一伝</span></h1>
      ${takedaBishi}
      <p class="title-concept">マインスイーパー × 戦国合戦</p>
      <div class="title-actions">
        <button class="sengoku-button primary" data-action="chapter">はじめから</button>
        <button class="sengoku-button" disabled>つづきから</button>
        <button class="sengoku-button" disabled>図鑑</button>
      </div>
      <p class="version">WEB 戦国絵巻・第一章</p>
    </section>
  `;
}

function renderChapter(): void {
  app.innerHTML = `
    <section class="screen chapter-screen">
      <button class="back-button corner-back" data-action="title" aria-label="タイトルへ戻る">‹</button>
      <div class="chapter-cloud chapter-cloud-top" aria-hidden="true"></div>
      <div class="chapter-intro">
        <p class="chapter-kicker">第一章</p>
        <h1>諏訪攻略</h1>
        <div class="chapter-rule"></div>
        <p>
          甲斐を固めた武田晴信は、信濃への道を開くため諏訪へ兵を進める。
          最初の戦場は、国境を守る千野頼満の砦であった。
        </p>
        <dl class="chapter-force">
          <div><dt>武田軍</dt><dd>一、二〇〇兵</dd></div>
          <div><dt>敵軍</dt><dd>七〇〇兵</dd></div>
        </dl>
        <button class="sengoku-button primary" data-action="stages">諏訪へ進軍</button>
      </div>
      <div class="chapter-cloud chapter-cloud-bottom" aria-hidden="true"></div>
    </section>
  `;
}

function renderStages(): void {
  app.innerHTML = `
    <section class="screen stage-screen">
      <header class="screen-header">
        <button class="back-button" data-action="chapter" aria-label="第一章へ戻る">‹</button>
        <div>
          <p class="chapter-number">第一章　諏訪攻略</p>
          <h1>戦場選択</h1>
        </div>
      </header>
      <div class="stage-map">
        <div class="route-line" aria-hidden="true"></div>
        <button class="stage-card" data-action="start-battle">
          <span class="stage-seal">1-1</span>
          <span class="stage-copy">
            <small>甲斐・信濃 国境</small>
            <strong>国境の砦</strong>
            <span>敵将　千野頼満　七〇〇兵</span>
          </span>
          <span class="stage-arrow">›</span>
        </button>
      </div>
      <aside class="stage-brief">
        <p><span>勝利条件</span>千野頼満隊700兵を0にする</p>
        <p><span>戦場</span>6×6・敵マス3</p>
        <p><span>出陣</span>武田晴信ほか六将</p>
      </aside>
    </section>
  `;
}

function renderBattle(): void {
  const battle = state.battle;
  if (!battle) {
    startBattle();
    return;
  }

  const currentWarrior = TAKEDA_WARRIORS[battle.attackerIndex];
  const takedaRatio = (battle.takedaTroops / TAKEDA_MAX_TROOPS) * 100;
  const enemyRatio = (battle.enemyTroops / ENEMY_MAX_TROOPS) * 100;
  const moraleRatio = (battle.morale / MAX_MORALE) * 100;

  app.innerHTML = `
    <section class="screen battle-screen">
      <header class="battle-header">
        <button class="back-button" data-action="stages" aria-label="戦場選択へ戻る">‹</button>
        <div>
          <p>第一章　諏訪攻略</p>
          <h1>1-1 国境の砦</h1>
        </div>
        <button class="restart-button" data-action="restart">再戦</button>
      </header>

      <section class="army-overview" aria-label="両軍兵力">
        ${armyGauge("武田軍総兵士数", battle.takedaTroops, TAKEDA_MAX_TROOPS, takedaRatio, "takeda")}
        ${armyGauge("敵軍総兵士数", battle.enemyTroops, ENEMY_MAX_TROOPS, enemyRatio, "enemy")}
      </section>

      <section class="commander-panel">
        <div class="portrait commander-portrait" aria-label="千野頼満の顔画像プレースホルダー">
          <span>千野</span>
        </div>
        <div class="commander-data">
          <small>敵将</small>
          <h2>${CHINO_YORIMITSU.name}</h2>
          <p>部隊兵士数 <strong>${battle.enemyTroops.toLocaleString("ja-JP")}</strong> / 700</p>
          <div class="stat-row"><span>攻 ${CHINO_YORIMITSU.attack}</span><span>防 ${CHINO_YORIMITSU.defense}</span></div>
        </div>
        <div class="enemy-marks" aria-label="敵マス状況">
          ${[0, 1, 2].map((_, enemyNumber) => enemyMark(enemyNumber, battle)).join("")}
        </div>
      </section>

      <section class="battle-status">
        <div class="turn-display">
          <span>現在の攻撃担当</span>
          <strong>${currentWarrior.name}</strong>
        </div>
        <div class="combo-display ${battle.combo >= 2 ? "active" : ""}">
          <span>連撃</span>
          <strong>${battle.combo}</strong>
        </div>
      </section>

      <section class="morale-panel">
        <div><span>武田軍 士気</span><strong>${battle.morale} / 100</strong></div>
        <div class="morale-meter"><i style="width:${moraleRatio}%"></i></div>
        <small>今回は蓄積のみ</small>
      </section>

      <section class="roster" aria-label="武田軍攻撃ローテーション">
        ${TAKEDA_WARRIORS.map((warrior, index) => warriorCard(warrior, index, battle.attackerIndex)).join("")}
      </section>

      <div class="battle-instruction">
        <strong>${battle.result ? resultInstruction(battle.result) : "未開封マスを選べ"}</strong>
        <span>${battle.firstMovePending ? "最初の一手で安全地帯と複数の数字を展開" : "数字から敵マスの位置を推理せよ"}</span>
      </div>

      <div class="board-toolbar">
        <span>敵マス × 3</span>
        <button class="flag-button ${state.flagMode ? "active" : ""}" data-action="toggle-flag-mode">
          ⚑ 旗モード ${state.flagMode ? "ON" : "OFF"}
        </button>
      </div>

      <div class="board" role="grid" aria-label="6×6の戦場">
        ${Array.from({ length: CELL_COUNT }, (_, index) => cellMarkup(index, battle)).join("")}
      </div>

      <section class="damage-strip" aria-label="直近の損害">
        <div><span>与ダメージ</span><strong>${battle.latestGivenDamage || "—"}</strong></div>
        <div><span>被ダメージ</span><strong>${battle.latestReceivedDamage || "—"}</strong></div>
      </section>

      <section class="battle-log" aria-label="戦闘ログ" aria-live="assertive">
        <h2>戦闘記録</h2>
        ${battle.logs.slice(-5).reverse().map((line) => `<p>${line}</p>`).join("")}
      </section>

      <p class="formula">通常損害 = max(10, round((攻撃 − 防御 ＋ 50) × 0.9〜1.1))</p>
      ${battle.result && !state.resultDismissed ? resultModal(battle) : ""}
    </section>
  `;
}

function armyGauge(
  label: string,
  troops: number,
  maxTroops: number,
  ratio: number,
  side: "takeda" | "enemy",
): string {
  return `
    <div class="army-gauge ${side}">
      <div><span>${label}</span><strong>${troops.toLocaleString("ja-JP")} / ${maxTroops.toLocaleString("ja-JP")} 兵</strong></div>
      <div class="troop-meter"><i style="width:${ratio}%"></i></div>
    </div>
  `;
}

function enemyMark(enemyNumber: number, battle: BattleState): string {
  const enemyIndices = [...battle.enemyIndices];
  const enemyIndex = enemyIndices[enemyNumber];
  const opened = enemyIndex !== undefined && battle.revealed.has(enemyIndex);
  return `<i class="${opened ? "opened" : ""}" title="敵マス${enemyNumber + 1}">${opened ? "開" : "伏"}</i>`;
}

function warriorCard(
  warrior: (typeof TAKEDA_WARRIORS)[number],
  index: number,
  attackerIndex: number,
): string {
  const current = index === attackerIndex;
  return `
    <article class="warrior-card ${current ? "current" : ""}">
      <span class="turn-number">${index + 1}</span>
      <div class="portrait warrior-portrait" aria-label="${warrior.name}の顔画像プレースホルダー">${warrior.shortName.slice(0, 1)}</div>
      <div>
        <h3>${warrior.name}</h3>
        <p><span>攻 ${warrior.attack}</span><span>防 ${warrior.defense}</span></p>
      </div>
    </article>
  `;
}

function cellMarkup(index: number, battle: BattleState): string {
  const revealed = battle.revealed.has(index);
  const enemy = battle.enemyIndices.has(index);
  const flagged = battle.flagged.has(index);
  const classes = ["cell"];
  let label = `${Math.floor(index / BOARD_SIZE) + 1}行${(index % BOARD_SIZE) + 1}列 未開封`;
  let content = '<span class="cell-mark">◆</span>';

  if (flagged && !revealed) {
    classes.push("flagged");
    label = `${Math.floor(index / BOARD_SIZE) + 1}行${(index % BOARD_SIZE) + 1}列 旗マス`;
    content = '<span class="flag-mark">⚑</span>';
  } else if (revealed) {
    classes.push("revealed");
    if (enemy) {
      classes.push("enemy-cell");
      label = "開封済みの敵マス 千野頼満";
      content = '<span class="enemy-mon">千野</span>';
    } else {
      const count = battle.adjacentCounts[index];
      classes.push(`hint-${count}`);
      label = count === 0 ? "開封済みの安全マス 周囲の敵0" : `開封済みの安全マス 周囲の敵${count}`;
      content = count === 0
        ? '<span class="hint hint-zero" aria-hidden="true"></span>'
        : `<span class="hint">${count}</span>`;
    }
  }

  if (battle.result && !revealed) classes.push("locked");
  return `
    <button
      class="${classes.join(" ")}"
      data-cell="${index}"
      role="gridcell"
      aria-label="${label}"
      ${revealed || battle.result ? "disabled" : ""}
    >${content}</button>
  `;
}

function resultInstruction(result: NonNullable<BattleState["result"]>): string {
  return result === "victory" ? "千野頼満隊を撃破" : "武田軍は壊滅";
}

function resultModal(battle: BattleState): string {
  const victory = battle.result === "victory";
  return `
    <div class="result-backdrop">
      <div class="result-panel ${victory ? "victory" : "defeat"}" role="dialog" aria-modal="true" aria-labelledby="result-title">
        <span class="result-kicker">${victory ? "合戦勝利" : "合戦敗北"}</span>
        <h2 id="result-title">${victory ? "千野頼満、討ち取ったり！" : "武田軍、全軍退却"}</h2>
        <p>
          ${victory
            ? `敵軍総兵士数 ${battle.enemyTroops}・千野頼満隊 ${battle.enemyTroops}`
            : `武田軍総兵士数 ${battle.takedaTroops}`}
        </p>
        <button class="sengoku-button primary" data-action="${victory ? "stages" : "restart"}">
          ${victory ? "戦場選択へ" : "再戦する"}
        </button>
        <button class="inspect-button" data-action="inspect-board">盤面を確認</button>
      </div>
    </div>
  `;
}

function startBattle(): void {
  state.battle = createBattle();
  state.flagMode = false;
  state.resultDismissed = false;
  state.screen = "battle";
  render();
}

app.addEventListener("click", (event) => {
  const target = event.target;
  if (!(target instanceof Element)) return;

  const actionElement = target.closest<HTMLElement>("[data-action]");
  if (actionElement) {
    const action = actionElement.dataset.action;
    if (action === "title" || action === "chapter" || action === "stages") {
      state.screen = action;
      render();
    } else if (action === "start-battle" || action === "restart") {
      startBattle();
    } else if (action === "toggle-flag-mode") {
      state.flagMode = !state.flagMode;
      render();
    } else if (action === "inspect-board") {
      state.resultDismissed = true;
      render();
    }
    return;
  }

  const cell = target.closest<HTMLElement>("[data-cell]");
  if (!cell || !state.battle) return;
  const index = Number(cell.dataset.cell);
  if (state.flagMode) {
    state.battle = toggleFlag(state.battle, index);
    render();
    return;
  }
  const outcome = openCell(state.battle, index);
  if (outcome.events.length === 0) return;
  state.battle = outcome.battle;
  render();
});

if ("serviceWorker" in navigator) {
  window.addEventListener("load", () => {
    navigator.serviceWorker.register("/service-worker.js").catch(() => {
      // PWA登録に失敗してもゲーム本体は継続する。
    });
  });
}

render();
