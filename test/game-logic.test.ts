import { describe, expect, it } from "vitest";

import {
  CHINO_YORIMITSU,
  ENEMY_MAX_TROOPS,
  TAKEDA_WARRIORS,
  adjacentEnemyCount,
  calculateDamage,
  comboDamageBonus,
  createBattle,
  openCell,
} from "../game-logic";

const enemyCells = [0, 7, 35] as const;
const noCriticalAverage = () => {
  const values = [0.5, 0.5];
  return () => values.shift() ?? 0.5;
};

describe("1-1 国境の砦", () => {
  it("敵兵士数700と同一部隊を示す敵マス3個を生成する", () => {
    const battle = createBattle(Math.random, enemyCells);
    expect(battle.enemyTroops).toBe(700);
    expect([...battle.enemyIndices]).toEqual(enemyCells);
    expect(battle.enemyIndices.size).toBe(3);
  });

  it("能力値から通常ダメージを計算し兵士数を式に使わない", () => {
    const result = calculateDamage(230, 115, noCriticalAverage());
    expect(result.damage).toBe(165);
    expect(result.critical).toBe(false);
  });

  it("5%のクリティカルでダメージを1.5倍にする", () => {
    const values = [0.5, 0.049];
    const result = calculateDamage(230, 115, () => values.shift() ?? 0.5);
    expect(result.damage).toBe(248);
    expect(result.critical).toBe(true);
  });

  it("最低ダメージを10にする", () => {
    expect(calculateDamage(10, 999, noCriticalAverage()).damage).toBe(10);
  });

  it("連撃補正を2～5連撃以上へ適用する", () => {
    expect(comboDamageBonus(1)).toBe(0);
    expect(comboDamageBonus(2)).toBe(0.1);
    expect(comboDamageBonus(3)).toBe(0.2);
    expect(comboDamageBonus(4)).toBe(0.3);
    expect(comboDamageBonus(5)).toBe(0.4);
    expect(comboDamageBonus(9)).toBe(0.4);
  });

  it("周囲8マスの敵マスを同一部隊でも個数として数える", () => {
    const enemies = new Set([0, 2, 7]);
    expect(adjacentEnemyCount(1, enemies)).toBe(3);
  });

  it("安全マスごとに攻撃・ローテーション・士気・連撃を進める", () => {
    const initial = createBattle(Math.random, enemyCells);
    const outcome = openCell(initial, 1, noCriticalAverage());
    const firstAttack = outcome.events[0];
    expect(firstAttack.type).toBe("takeda-attack");
    expect(firstAttack.warriorName).toBe("武田晴信");
    expect(outcome.battle.enemyTroops).toBe(535);
    expect(outcome.battle.attackerIndex).toBe(1);
    expect(outcome.battle.combo).toBe(1);
    expect(outcome.battle.morale).toBe(12);
  });

  it("敵マスでは千野頼満が現在担当武将の防御へ攻撃し順番を進めない", () => {
    const initial = createBattle(Math.random, enemyCells);
    const outcome = openCell(initial, 0, noCriticalAverage());
    expect(outcome.events[0].type).toBe("chino-attack");
    expect(outcome.events[0].warriorName).toBe("武田晴信");
    expect(outcome.battle.takedaTroops).toBe(1190);
    expect(outcome.battle.enemyTroops).toBe(ENEMY_MAX_TROOPS);
    expect(outcome.battle.attackerIndex).toBe(0);
    expect(outcome.battle.combo).toBe(0);
    expect(outcome.battle.morale).toBe(6);
  });

  it("開いた敵マスは再攻撃せず、敵を自動討ち取りしない", () => {
    const initial = createBattle(Math.random, enemyCells);
    const first = openCell(initial, 0, noCriticalAverage()).battle;
    const second = openCell(first, 0, noCriticalAverage());
    expect(second.events).toEqual([]);
    expect(second.battle.takedaTroops).toBe(1190);
    expect(second.battle.enemyTroops).toBe(700);
    expect(second.battle.result).toBeNull();
  });

  it("敵マスを開くと既存の連撃を0へ戻す", () => {
    const initial = {
      ...createBattle(Math.random, enemyCells),
      combo: 4,
      attackerIndex: 3,
    };
    const outcome = openCell(initial, 7, noCriticalAverage());
    expect(outcome.battle.combo).toBe(0);
    expect(outcome.battle.attackerIndex).toBe(3);
  });

  it("0マスから敵以外の周辺安全マスを自動展開する", () => {
    const clusteredEnemies = [0, 1, 6] as const;
    const initial = createBattle(Math.random, clusteredEnemies);
    const outcome = openCell(initial, 35, noCriticalAverage());
    expect(outcome.battle.revealed.size).toBeGreaterThan(1);
    expect([...outcome.battle.revealed].some((index) => clusteredEnemies.includes(index as 0 | 1 | 6))).toBe(false);
  });

  it("武田軍攻撃は敵軍総兵士数と千野頼満部隊兵士数の共通値を減らす", () => {
    const initial = createBattle(Math.random, enemyCells);
    const outcome = openCell(initial, 1, noCriticalAverage());
    expect(outcome.battle.enemyTroops).toBe(700 - 165);
  });

  it("千野頼満部隊兵士数が0になった時だけ討ち取り・勝利にする", () => {
    const initial = {
      ...createBattle(Math.random, enemyCells),
      enemyTroops: 100,
    };
    const outcome = openCell(initial, 1, noCriticalAverage());
    expect(outcome.battle.enemyTroops).toBe(0);
    expect(outcome.battle.result).toBe("victory");
    expect(outcome.battle.logs.at(-1)).toBe("千野頼満、討ち取ったり！");
  });

  it("武田軍総兵士数が0になると敗北する", () => {
    const defender = TAKEDA_WARRIORS[0];
    expect(CHINO_YORIMITSU.attack).toBe(120);
    const initial = {
      ...createBattle(Math.random, enemyCells),
      takedaTroops: 10,
      attackerIndex: 0,
    };
    const outcome = openCell(initial, 0, noCriticalAverage());
    expect(defender.defense).toBe(230);
    expect(outcome.battle.takedaTroops).toBe(0);
    expect(outcome.battle.result).toBe("defeat");
  });
});
