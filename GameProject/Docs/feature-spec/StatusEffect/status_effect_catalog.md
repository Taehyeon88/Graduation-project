# 상태효과(StatusEffect) 카탈로그 — 이름·타입·설명·판정 연결

> 상태효과 하나가 어떤 `StatusEffectType`과 `StatusEffectData` 에셋으로 정의되고, 어디서 읽혀 적용되는지 **상태효과 단위**로 정리한 문서. 제작 절차는 [statuseffect_howto.md](../../howto/statuseffect_howto.md)를 본다.
> 새 상태효과(`StatusEffectType` 값 + `StatusEffectData` 에셋)를 만들면 이 문서에 섹션을 추가한다. 원작에서 확인하지 못한 규칙은 `(원작 확인 필요)`로 표기한다.
> **현재 수록: 5개** (힘, 취약, 약화, 회피, 치명타). 수록 에셋은 3개(Power, 회피, 치명타)이며 취약·약화는 에셋이 없다. 열거형 값 1번은 삭제된 방어(`ARMOR`)의 자리라 비어 있다.

## 요약

| 열거형 값 | 타입 | 이름 | 설명 (`Discription`) | 에셋 | 지속·감소 |
|---|---|---|---|---|---|
| 0 | `POWER` | 힘 | 수치만큼 고정 피해 추가 | `Power.asset` | 없음 |
| 2 | `WEAK` | 취약 | (에셋 없음) 공격자 디버프 | 없음 | 영웅 턴 종료 시 −1 |
| 3 | `VULNERABLE` | 약화 | (에셋 없음) 피격자 디버프 | 없음 | 영웅 턴 종료 시 −1 |
| 4 | `DODGE` | 회피 | 이 용병을 대상으로 하는 다음 1회의 모든 공격과 피해가 빗나갑니다. 효과가 발동하거나 지속시간이 종료되면 이 상태가 해제됩니다. | `회피.asset` | 발동 시 −1 |
| 5 | `CRITICAL` | 치명타 | 자신의 공격력의 절반만큼 추가 공격력을 얻습니다. | `치명타.asset` | 발동 시 −1 |

- 에셋 위치: `Assets/Game/1_Datas/6.StatusEffects/`, 아이콘 위치: `Assets/Game/4_Medias/StatusEffects/`.
- 열거형 값은 에셋에 정수로 직렬화되므로 새 타입은 `StatusEffectType.cs`의 **끝에** 추가하고, 기존 멤버는 값을 명시해 고정한다 (`POWER = 0, WEAK = 2, VULNERABLE = 3, DODGE = 4, CRITICAL = 5`). 멤버를 지울 때도 남은 멤버의 값이 밀리지 않게 한다.
- 에셋은 씬의 `StatusEffectSystem.effectDatas`에 등록해야 부여할 수 있다 (현재 등록: Power, 회피, 치명타).

## 0 · 힘 (POWER)

에셋: `Assets/Game/1_Datas/6.StatusEffects/Power.asset`

| 항목 | 값 |
|---|---|
| 타입 | `POWER` (0) |
| 설명 (`Discription`) | 수치만큼 고정 피해 추가 |
| 스택 의미 | 수치 (스택 = 가산할 피해량) |
| 판정 위치 | `DamageCaculator.GetDamage` → `GetPower(attacker)` |
| 효과 동작 | 공격자의 `POWER` 스택을 `DealDamageGA.Amount`에 더한 뒤 취약·약화 계산으로 넘긴다 |
| 지속·감소 | 자동 감소 없음 (`ReduceSEWhenMyTurnEnd`에 없음) |
| 아이콘 | `spriteImage` 연결됨 |
| 코드 | `General/Util/DamageCaculator.cs` |

**연결 흐름**

```
DealDamageGA → DamageSystem.DealDamageGAPerformer
 └ DamageCaculator.GetDamage(Amount, 공격자 CombatantView, 대상)
    └ damage = Amount + 공격자.GetStatusEffectStacks(POWER)  → 이후 WEAK/VULNERABLE 보정 → CeilToInt
```

**주의·한계**
- 공격자가 `CombatantView`일 때만 가산된다 (`dealDamageGA.Caster as CombatantView`가 null이면 건너뜀).
- `AttackCoreGA`(코어 공격)도 `CoreSystem.AttackCoreGAPerformer`에서 `DamageCaculator.GetDamage`를 거치므로 힘이 적용된다 (치명타 도입 시 연결). 공격자가 `CombatantView`일 때만이며 플레이어 스킬(시전자 없음)은 제외. 코어 공격에도 힘·약화가 적용되는 것이 원작 규칙인지는 확인 필요 (원작 확인 필요).
- 스택 상한 없음. Play 테스트는 하지 않았다.

**에디터 설정**: 에셋 `EffectType`=`POWER`. 부여는 카드 `AddStatusEffectEffect` 또는 특성 `AddStatusTraitEffect`에서 타입 `POWER` 선택.

## 2 · 취약 (WEAK)

에셋: **없음** (`6.StatusEffects/`에 미작성)

| 항목 | 값 |
|---|---|
| 타입 | `WEAK` (2) |
| 설명 | 코드 주석: "취약 : 공격자 디버프" (에셋 `Discription` 없음) |
| 스택 의미 | 유무 (스택 > 0이면 적용, 크기 무관) |
| 판정 위치 | `DamageCaculator.CalculateWeak(damage, attacker)` |
| 효과 동작 | 공격자의 `WEAK` 스택이 0보다 크면 피해 ×0.75 |
| 지속·감소 | 영웅 턴 종료 시 −1 (`ReduceSEWhenMyTurnEnd`) |
| 코드 | `General/Util/DamageCaculator.cs`, `3.Views/CombatantView.cs` |

**주의·한계**
- **이름·효과 불일치 의심**: 한국어 이름은 "취약"인데 효과는 "공격자의 피해가 줄어드는" 약화형이다. 반대로 `VULNERABLE`("약화")은 피격자가 받는 피해가 늘어나는 취약형이다. 주석(`WEAK=취약`, `VULNERABLE=약화`)과 `ReduceSEWhenMyTurnEnd` 주석("취약 감소"/"약화 감소" 순서)이 서로 어긋나 있어 이름이 뒤바뀐 듯 보인다 — 코드 동작 기준으로 기록 (원작 확인 필요).
- 에셋이 없어 `AddStatusEffectGA`로 부여하면 `StatusEffectSystem` performer가 `KeyNotFoundException`을 낸다. 부여 전에 에셋 작성·`effectDatas` 등록이 필요하다.
- 감소는 영웅에게만 호출되므로 용병이 걸리면 줄지 않는다. Play 테스트는 하지 않았다.

## 3 · 약화 (VULNERABLE)

에셋: **없음** (`6.StatusEffects/`에 미작성)

| 항목 | 값 |
|---|---|
| 타입 | `VULNERABLE` (3) |
| 설명 | 코드 주석: "약화 : 피격자 디버프" (에셋 `Discription` 없음) |
| 스택 의미 | 유무 (스택 > 0이면 적용, 크기 무관) |
| 판정 위치 | `DamageCaculator.CalculateVulnerable(damage, target)` |
| 효과 동작 | 대상(`CombatantView`)의 `VULNERABLE` 스택이 0보다 크면 받는 피해 ×1.5 |
| 지속·감소 | 영웅 턴 종료 시 −1 (`ReduceSEWhenMyTurnEnd`) |
| 코드 | `General/Util/DamageCaculator.cs`, `3.Views/CombatantView.cs` |

**주의·한계**
- 대상이 `CombatantView`가 아닌 `IDamageable`(코어 등)이면 적용되지 않는다.
- 이름·효과 불일치 의심, 에셋 없음, 용병 감소 훅 없음은 위 `WEAK`와 동일하다.

## 4 · 회피 (DODGE)

에셋: `Assets/Game/1_Datas/6.StatusEffects/회피.asset` (아이콘 `Assets/Game/4_Medias/StatusEffects/회피.png`)

| 항목 | 값 |
|---|---|
| 타입 | `DODGE` (4) |
| 설명 (`Discription`) | 이 용병을 대상으로 하는 다음 1회의 모든 공격과 피해가 빗나갑니다. 효과가 발동하거나 지속시간이 종료되면 이 상태가 해제됩니다. |
| 스택 의미 | 횟수 (1스택 = 1회 회피, 중첩 시 횟수만큼) |
| 판정 위치 | `DamageSystem.DealDamageGAPerformer` — 대상별로 `GetDamage`/`PlayDamageVFX` **이전** |
| 효과 동작 | 대상(`CombatantView`)이 `DODGE`를 가졌으면 스택 1을 소모하고 피해 계산·피격 VFX·`Damage()`를 모두 건너뛴다. 단일 대상(`Target`)과 다중 대상(`Targets`) 모두 대상별로 판정 |
| 지속·감소 | 발동 시 −1. 지속시간(턴 종료 감소)은 두지 않음 (발동 전까지 유지, 사용자 결정) |
| 아이콘 | `spriteImage` 연결됨 (`회피.png`) |
| 코드 | `1.Systems/Skills/DamageSystem.cs` |

**연결 흐름**

```
DealDamageGA → DamageSystem.DealDamageGAPerformer
 ├ (기본 공격이면 공격 모션은 그대로 재생 — 공격자는 헛스윙)
 └ 대상 CombatantView.CheckStatusEffectExist(DODGE)
    ├ true : RemoveStatusEffect(DODGE, 1) → 피해·VFX 생략
    └ false: DamageCaculator → PlayDamageVFX → Damage()
```

**주의·한계**
- `DealDamageGA` 자체는 실행되므로 `DealDamageCondition`/`AttackedOpponentCondition`을 구독한 특성(공격 신호, 맹공, 반격 등)은 빗나가도 발동한다 (공격자 쪽 반응 유지, 사용자 결정). 피해량에 의존하는 특성은 어색할 수 있다.
- `AttackCoreGA`(코어 공격)는 대상이 코어라 회피와 무관하다.
- "MISS" 표시·회피 연출·사운드는 없다 (아트 제외).
- 설명문의 "지속시간이 종료되면"은 구현되지 않았다. 지속시간을 두려면 용병 턴 종료 감소 훅이 필요하다 (영웅만 `HeroSystem`이 호출).
- 부여 수단: 발판 '풀숲'(`TerrainSystem`) — 용병이 풀숲에 들어오면 `AddStatusEffectGA(DODGE, 1)`, 떠나면 1 회수 (출처 구분 없음, [08_terrain_obstacle.md](../08_terrain_obstacle.md) §5). 카드·특성 부여는 아직 연결하지 않았다. 풀숲 진입·이탈·재진입 시 스택 증감은 Play 모드에서 확인했다.

**에디터 설정**: 에셋 `EffectType`=`DODGE`(4), `Discription` 입력, `spriteImage`=`회피.png`. 씬 `StatusEffectSystem.effectDatas`에 등록 완료.

## 5 · 치명타 (CRITICAL)

에셋: `Assets/Game/1_Datas/6.StatusEffects/치명타.asset` (아이콘 **미연결** — `치명타.png` 없음)

| 항목 | 값 |
|---|---|
| 타입 | `CRITICAL` (5) |
| 설명 (`Discription`) | 자신의 공격력의 절반만큼 추가 공격력을 얻습니다. |
| 스택 의미 | 횟수 (1스택 = 피해가 적용되는 공격 1회분) |
| 판정 위치 (가산) | `DamageCaculator.GetDamage` → `GetCritical(attacker)` — 힘 가산 직후, 취약·약화 곱 **이전** |
| 판정 위치 (소모) | `DamageSystem.DealDamageGAPerformer`(공격 1회 처리 후), `CoreSystem.AttackCoreGAPerformer`(코어 피해 적용 후) |
| 효과 동작 | 공격자(`CombatantView`)가 `CRITICAL`을 가졌으면 피해에 `공격자.BaseDamage × 0.5`(`CriticalRatio`)를 더한다 |
| 지속·감소 | 피해가 적용된 공격 1회마다 −1. 지속시간(턴 종료 감소)은 두지 않음 → 영웅·용병 동일 동작 |
| 코드 | `General/Util/DamageCaculator.cs`, `1.Systems/Skills/DamageSystem.cs`, `1.Systems/CoreSystem.cs` |

**연결 흐름**

```
DealDamageGA → DamageSystem.DealDamageGAPerformer
 ├ 대상별: DamageCaculator.GetDamage(Amount, 공격자, 대상)
 │   └ damage = Amount + POWER + (CRITICAL 보유 ? 공격자.BaseDamage × 0.5 : 0) → 취약·약화 보정 → CeilToInt
 └ 모든 대상 처리 후, 피해가 한 번이라도 적용됐으면 공격자 RemoveStatusEffect(CRITICAL, 1)  (GA당 1회)

AttackCoreGA → CoreSystem.AttackCoreGAPerformer
 ├ DamageCaculator.GetDamage(Amount ?? BaseDamage, 공격자, null) → 코어 체력 감소
 └ !IsMissed 이면 공격자 RemoveStatusEffect(CRITICAL, 1)
```

**주의·한계**
- `GetDamage`는 순수 계산이라 소모를 하지 않는다 → `PopUpAmountUI`의 피해 미리보기에는 치명타 보너스가 반영되고 스택은 줄지 않는다.
- 보너스는 항상 **시전자 `BaseDamage`의 50%**다. 스킬·특성의 `Amount`가 `BaseDamage`와 달라도 `Amount` 기준이 아니다.
- **GA당 1회 소모**: 한 특성이 `DealDamageGA`와 `AttackCoreGA`를 연달아 발행하면(예: `DamageAllOpponentsBySpeedEffect`) 첫 GA만 보너스를 받고 소모되며 둘째는 받지 못한다.
- 전부 회피된 `DealDamageGA`, `IsMissed`인 `AttackCoreGA`는 피해가 적용되지 않으므로 소모하지 않는다.
- 다중 대상은 소모를 루프 이후 1회만 해서 대상 전원이 보너스를 받는다.
- **코어 공격 연결의 부수 효과**: 이전에는 `AttackCoreGA`가 `DamageCaculator`를 거치지 않았으나, 치명타를 위해 연결하면서 힘·약화도 코어 공격에 적용된다. 플레이어 스킬(`Caster == null`)은 공격자가 없어 보정 대상이 아니다 (원작 확인 필요).
- 획득: 자동 전투 턴(`AutoBattleTurnGA`)이 끝날 때마다 `HeroSystem.AutoBattleTurnPostReaction`이 영웅별로 `HeroData.Critical`(%) 확률을 굴려, 성공하면 `AddStatusEffectGA(CRITICAL, 1)`을 부여한다. 스택 상한·지속시간이 없어 쓰지 못한 치명타는 다음 턴으로 누적된다. 몬스터는 대상이 아니다.
- 아이콘·연출·사운드·"치명타" 표시는 없다 (아트 제외). Play 테스트는 하지 않았다.
- 부여 수단은 연결하지 않았다: 카드 `AddStatusEffectEffect` / 특성 `AddStatusTraitEffect`에서 타입 `CRITICAL`을 선택하면 된다.

**에디터 설정**: 에셋 `EffectType`=`CRITICAL`(5), `Discription` 입력 완료, `spriteImage` 비어 있음(미연결). 씬 `StatusEffectSystem.effectDatas`에 등록 완료.
