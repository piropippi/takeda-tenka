export const BOARD_SIZE = 6;
export const CELL_COUNT = BOARD_SIZE * BOARD_SIZE;
export const ENEMY_CELL_COUNT = 3;
export const TAKEDA_MAX_TROOPS = 1200;
export const ENEMY_MAX_TROOPS = 700;
export const MAX_MORALE = 100;

export type BattleResult = "victory" | "defeat" | null;

export interface Warrior {
  readonly name: string;
  readonly shortName: string;
  readonly attack: number;
  readonly defense: number;
}

export const TAKEDA_WARRIORS: readonly Warrior[] = Object.freeze([
  Object.freeze({ name: "武田晴信", shortName: "晴信", attack: 230, defense: 230 }),
  Object.freeze({ name: "武田信繁", shortName: "信繁", attack: 190, defense: 195 }),
  Object.freeze({ name: "板垣信方", shortName: "板垣", attack: 145, defense: 175 }),
  Object.freeze({ name: "甘利虎泰", shortName: "甘利", attack: 160, defense: 150 }),
  Object.freeze({ name: "飯富虎昌", shortName: "飯富", attack: 165, defense: 155 }),
  Object.freeze({ name: "原虎胤", shortName: "原", attack: 155, defense: 160 }),
]);

export const CHINO_YORIMITSU = Object.freeze({
  name: "千野頼満",
  attack: 120,
  defense: 115,
});

export interface DamageResult {
  damage: number;
  critical: boolean;
  variance: number;
  comboBonus: number;
}

export interface BattleEvent {
  type: "takeda-attack" | "chino-attack" | "victory" | "defeat";
  message: string;
  damage: number;
  critical: boolean;
  warriorName?: string;
  combo?: number;
}

export interface BattleState {
  enemyIndices: ReadonlySet<number>;
  revealed: ReadonlySet<number>;
  flagged: ReadonlySet<number>;
  adjacentCounts: readonly number[];
  firstMovePending: boolean;
  takedaTroops: number;
  enemyTroops: number;
  attackerIndex: number;
  combo: number;
  morale: number;
  result: BattleResult;
  latestGivenDamage: number;
  latestReceivedDamage: number;
  logs: readonly string[];
}

function shuffledEnemyIndices(random: () => number): number[] {
  const candidates = Array.from({ length: CELL_COUNT }, (_, index) => index);
  for (let index = candidates.length - 1; index > 0; index -= 1) {
    const randomIndex = Math.floor(Math.min(0.999999, Math.max(0, random())) * (index + 1));
    [candidates[index], candidates[randomIndex]] = [candidates[randomIndex], candidates[index]];
  }
  return candidates.slice(0, ENEMY_CELL_COUNT);
}

export function createBattle(
  random: () => number = Math.random,
  fixedEnemyIndices?: readonly number[],
): BattleState {
  const indices = fixedEnemyIndices ?? shuffledEnemyIndices(random);
  const uniqueIndices = new Set(
    indices.filter((index) => Number.isInteger(index) && index >= 0 && index < CELL_COUNT),
  );
  if (uniqueIndices.size !== ENEMY_CELL_COUNT) {
    throw new Error(`敵マスは重複なしで${ENEMY_CELL_COUNT}マス必要です。`);
  }

  return {
    enemyIndices: uniqueIndices,
    revealed: new Set<number>(),
    flagged: new Set<number>(),
    adjacentCounts: calculateAdjacentCounts(uniqueIndices),
    firstMovePending: true,
    takedaTroops: TAKEDA_MAX_TROOPS,
    enemyTroops: ENEMY_MAX_TROOPS,
    attackerIndex: 0,
    combo: 0,
    morale: 0,
    result: null,
    latestGivenDamage: 0,
    latestReceivedDamage: 0,
    logs: ["武田軍、国境の砦へ進軍開始。"],
  };
}

export function comboDamageBonus(combo: number): number {
  if (combo >= 5) return 0.4;
  if (combo === 4) return 0.3;
  if (combo === 3) return 0.2;
  if (combo === 2) return 0.1;
  return 0;
}

export function calculateDamage(
  attack: number,
  defense: number,
  random: () => number = Math.random,
  combo = 0,
): DamageResult {
  const varianceRoll = Math.min(1, Math.max(0, random()));
  const criticalRoll = Math.min(1, Math.max(0, random()));
  const variance = 0.9 + varianceRoll * 0.2;
  const critical = criticalRoll < 0.05;
  const comboBonus = comboDamageBonus(combo);
  const normalDamage = Math.max(10, Math.round((attack - defense + 50) * variance));
  const damage = Math.max(
    10,
    Math.round(normalDamage * (critical ? 1.5 : 1) * (1 + comboBonus)),
  );

  return { damage, critical, variance, comboBonus };
}

export function adjacentEnemyCount(
  index: number,
  enemyIndices: ReadonlySet<number>,
): number {
  const row = Math.floor(index / BOARD_SIZE);
  const column = index % BOARD_SIZE;
  let count = 0;

  for (let rowOffset = -1; rowOffset <= 1; rowOffset += 1) {
    for (let columnOffset = -1; columnOffset <= 1; columnOffset += 1) {
      if (rowOffset === 0 && columnOffset === 0) continue;
      const nextRow = row + rowOffset;
      const nextColumn = column + columnOffset;
      if (
        nextRow >= 0 &&
        nextRow < BOARD_SIZE &&
        nextColumn >= 0 &&
        nextColumn < BOARD_SIZE &&
        enemyIndices.has(nextRow * BOARD_SIZE + nextColumn)
      ) {
        count += 1;
      }
    }
  }
  return count;
}

export function calculateAdjacentCounts(
  enemyIndices: ReadonlySet<number>,
): number[] {
  return Array.from({ length: CELL_COUNT }, (_, index) =>
    enemyIndices.has(index) ? -1 : adjacentEnemyCount(index, enemyIndices),
  );
}

function adjacentIndices(index: number): number[] {
  const row = Math.floor(index / BOARD_SIZE);
  const column = index % BOARD_SIZE;
  const indices: number[] = [];

  for (let rowOffset = -1; rowOffset <= 1; rowOffset += 1) {
    for (let columnOffset = -1; columnOffset <= 1; columnOffset += 1) {
      if (rowOffset === 0 && columnOffset === 0) continue;
      const nextRow = row + rowOffset;
      const nextColumn = column + columnOffset;
      if (
        nextRow >= 0 &&
        nextRow < BOARD_SIZE &&
        nextColumn >= 0 &&
        nextColumn < BOARD_SIZE
      ) {
        indices.push(nextRow * BOARD_SIZE + nextColumn);
      }
    }
  }
  return indices;
}

function cloneBattle(battle: BattleState): BattleState & {
  enemyIndices: Set<number>;
  revealed: Set<number>;
  flagged: Set<number>;
  adjacentCounts: number[];
  logs: string[];
} {
  return {
    ...battle,
    enemyIndices: new Set(battle.enemyIndices),
    revealed: new Set(battle.revealed),
    flagged: new Set(battle.flagged),
    adjacentCounts: [...battle.adjacentCounts],
    logs: [...battle.logs],
  };
}

function criticalLabel(critical: boolean): string {
  return critical ? " 会心の一撃！" : "";
}

function finalizeFirstMoveLayout(
  battle: ReturnType<typeof cloneBattle>,
  firstIndex: number,
  random: () => number,
): void {
  if (battle.enemyIndices.has(firstIndex)) {
    const candidates = Array.from({ length: CELL_COUNT }, (_, index) => index).filter(
      (index) => index !== firstIndex && !battle.enemyIndices.has(index),
    );
    const destination =
      candidates[
        Math.floor(Math.min(0.999999, Math.max(0, random())) * candidates.length)
      ];
    battle.enemyIndices.delete(firstIndex);
    battle.enemyIndices.add(destination);
  }

  battle.adjacentCounts = calculateAdjacentCounts(battle.enemyIndices);
  battle.firstMovePending = false;
}

function collectSafeExpansion(
  battle: ReturnType<typeof cloneBattle>,
  startIndex: number,
): number[] {
  const opened: number[] = [];
  const queue = [startIndex];
  const queued = new Set(queue);

  while (queue.length) {
    const current = queue.shift();
    if (
      current === undefined ||
      battle.revealed.has(current) ||
      battle.enemyIndices.has(current) ||
      battle.flagged.has(current)
    ) {
      continue;
    }

    battle.revealed.add(current);
    opened.push(current);

    if (battle.adjacentCounts[current] === 0) {
      for (const neighbor of adjacentIndices(current)) {
        if (
          !queued.has(neighbor) &&
          !battle.revealed.has(neighbor) &&
          !battle.enemyIndices.has(neighbor) &&
          !battle.flagged.has(neighbor)
        ) {
          queued.add(neighbor);
          queue.push(neighbor);
        }
      }
    }
  }

  return opened;
}

export function toggleFlag(battle: BattleState, index: number): BattleState {
  if (
    battle.result ||
    battle.revealed.has(index) ||
    !Number.isInteger(index) ||
    index < 0 ||
    index >= CELL_COUNT
  ) {
    return battle;
  }

  const next = cloneBattle(battle);
  if (next.flagged.has(index)) {
    next.flagged.delete(index);
  } else {
    next.flagged.add(index);
  }
  return next;
}

export function openCell(
  battle: BattleState,
  index: number,
  random: () => number = Math.random,
): { battle: BattleState; events: BattleEvent[] } {
  if (
    battle.result ||
    battle.revealed.has(index) ||
    battle.flagged.has(index) ||
    !Number.isInteger(index) ||
    index < 0 ||
    index >= CELL_COUNT
  ) {
    return { battle, events: [] };
  }

  const next = cloneBattle(battle);
  const events: BattleEvent[] = [];

  if (next.firstMovePending) {
    finalizeFirstMoveLayout(next, index, random);
  }

  if (next.enemyIndices.has(index)) {
    next.revealed.add(index);
    next.combo = 0;
    next.morale = Math.min(MAX_MORALE, next.morale + 6);
    const defender = TAKEDA_WARRIORS[next.attackerIndex];
    const result = calculateDamage(CHINO_YORIMITSU.attack, defender.defense, random);
    next.takedaTroops = Math.max(0, next.takedaTroops - result.damage);
    next.latestReceivedDamage = result.damage;
    const message =
      `${CHINO_YORIMITSU.name}の反撃。${defender.name}が受け、武田軍に` +
      `${result.damage}の損害。${criticalLabel(result.critical)}`;
    next.logs.push(message);
    events.push({
      type: "chino-attack",
      message,
      damage: result.damage,
      critical: result.critical,
      warriorName: defender.name,
      combo: 0,
    });

    if (next.takedaTroops === 0) {
      next.result = "defeat";
      const defeatMessage = "武田軍総兵士数が尽きた。全軍退却！";
      next.logs.push(defeatMessage);
      events.push({
        type: "defeat",
        message: defeatMessage,
        damage: 0,
        critical: false,
      });
    }
    return { battle: next, events };
  }

  const openedSafeIndices = collectSafeExpansion(next, index);

  for (const current of openedSafeIndices) {
    if (next.result) break;
    next.combo += 1;
    next.morale = Math.min(MAX_MORALE, next.morale + 12);

    const attacker = TAKEDA_WARRIORS[next.attackerIndex];
    const result = calculateDamage(
      attacker.attack,
      CHINO_YORIMITSU.defense,
      random,
      next.combo,
    );
    next.enemyTroops = Math.max(0, next.enemyTroops - result.damage);
    next.latestGivenDamage = result.damage;
    const comboLabel = next.combo >= 2 ? ` ${next.combo}連撃！` : "";
    const message =
      `${attacker.name}の攻撃。千野頼満隊に${result.damage}の損害。` +
      `${comboLabel}${criticalLabel(result.critical)}`;
    next.logs.push(message);
    events.push({
      type: "takeda-attack",
      message,
      damage: result.damage,
      critical: result.critical,
      warriorName: attacker.name,
      combo: next.combo,
    });
    next.attackerIndex = (next.attackerIndex + 1) % TAKEDA_WARRIORS.length;

    if (next.enemyTroops === 0) {
      next.result = "victory";
      const victoryMessage = "千野頼満、討ち取ったり！";
      next.logs.push(victoryMessage);
      events.push({
        type: "victory",
        message: victoryMessage,
        damage: 0,
        critical: false,
      });
      break;
    }

  }

  return { battle: next, events };
}
