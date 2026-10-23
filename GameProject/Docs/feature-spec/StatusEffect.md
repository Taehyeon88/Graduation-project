# 상태효과(StatusEffect) 카탈로그

> 상태효과 하나가 어떤 `StatusEffectType`으로 정의되고, 어디서 부여·소비·감소되는지 **상태효과 단위**로 정리한 문서. 코드 기준으로 작성했다 (원작 규칙 확인 필요한 부분은 `(원작 확인 필요)`).
> 새 상태효과(`StatusEffectData` 에셋 + `StatusEffectType` 값)를 만들면 이 문서에 섹션을 추가한다.

## 공통 구조

- 데이터: `StatusEffectData`(SO) — `EffectType`, `Discription`, `spriteImage`. 에셋 위치 `Assets/Game/1_Datas/6.StatusEffects/`.
- 등록: 씬의 `StatusEffectSystem.effectDatas` 배열에 에셋을 넣어야 한다 (`EffectType` 중복 시 `LogError`, 미등록 시 부여 단계에서 `KeyNotFoundException`).
- 부여: `AddStatusEffectGA` → `StatusEffectSystem` → `CombatantView.AddStatusEffect` (스택 누적, `StatusEffectsUI`가 아이콘+스택 표시).
- 감소: 영웅만 `HeroSystem`이 `CombatantView.ReduceSEWhenMyTurnEnd`를 호출한다. 용병은 턴 종료 감소 훅이 없다.
- 부여 수단: 카드는 `AddStatusEffectEffect`, 특성은 `AddStatusTraitEffect`에서 타입만 고른다.

## 요약

| 이름 | `StatusEffectType` | 설명 | 판정 위치 | 감소 규칙 |
|---|---|---|---|---|
| 힘 | `POWER` | 수치만큼 고정 피해 증가 | `DamageCaculator.GetPower` | 자동 감소 없음 |
| 취약 | `WEAK` | 공격자 디버프: 이 효과를 가진 공격자의 피해 ×0.75 | `DamageCaculator.CalculateWeak` | 영웅 턴 종료 시 1 감소 |
| 약화 | `VULNERABLE` | 피격자 디버프: 이 효과를 가진 대상이 받는 피해 ×1.5 | `DamageCaculator.CalculateVulnerable` | 영웅 턴 종료 시 1 감소 |
| 회피 | `DODGE` | 이 용병을 대상으로 하는 다음 1회의 모든 공격과 피해가 빗나감. 발동하면 해제 | `DamageSystem.DealDamageGAPerformer` | 발동 시 스택 1 소모, 지속시간 없음 |

## POWER · 힘

- 에셋: `Power.asset`
- 효과: 공격자의 스택 수를 `DealDamageGA.Amount`에 더한다 (`Amount + 스택`) 후 취약 계산을 거친다.
- 비고: 용병·영웅 모두 `CombatantView`라 동일하게 적용.

## WEAK · 취약

- 효과: 공격자가 스택 > 0이면 피해 ×0.75 (스택 크기와 무관, 유무만 판정).
- 감소: `ReduceSEWhenMyTurnEnd`(영웅)에서 1 감소.
- **불일치 주의**: 코드 주석은 `WEAK = 취약 : 공격자 디버프`, `VULNERABLE = 약화 : 피격자 디버프`로 적혀 있고 `ReduceSEWhenMyTurnEnd`의 주석("취약 감소"/"약화 감소")은 반대로 적혀 있다. 한국어 이름과 효과 방향이 뒤바뀐 듯 보이나(원작 용어상 취약=받는 피해 증가) 현재는 코드 동작 기준으로 기록했다 (원작 확인 필요).
- 에셋 미작성: `6.StatusEffects/`에 WEAK·VULNERABLE 에셋이 없어 현재 UI 표시(아이콘)가 불가하다.

## VULNERABLE · 약화

- 효과: 대상이 스택 > 0이면 받는 피해 ×1.5 (유무만 판정).
- 감소: `ReduceSEWhenMyTurnEnd`(영웅)에서 1 감소.
- 에셋 미작성 (위 WEAK와 동일).

## DODGE · 회피

- 에셋: `회피.asset` (이미지 `4_Medias/StatusEffects/회피.png`)
- 설명: 이 용병을 대상으로 하는 다음 1회의 모든 공격과 피해가 빗나갑니다. 효과가 발동하거나 지속시간이 종료되면 이 상태가 해제됩니다.
- 판정: `DamageSystem.DealDamageGAPerformer`에서 대상(`CombatantView`)이 `DODGE`를 가졌으면 스택 1을 소모하고 **피해 계산·피격 VFX·`Damage()`를 모두 건너뛴다**. 단일 대상(`Target`)과 다중 대상(`Targets`) 모두 대상별로 판정한다.
- 흐름:

```
DealDamageGA → DamageSystem.DealDamageGAPerformer
 ├ (기본 공격이면 공격 모션은 그대로 재생 — 공격자는 헛스윙)
 └ 대상 CombatantView.CheckStatusEffectExist(DODGE)
    ├ true : RemoveStatusEffect(DODGE, 1) → 피해·VFX 생략
    └ false: 기존 피해 처리
```

- 주의·한계:
  - 지속시간(턴 종료 감소)은 두지 않았다. 발동 전까지 유지된다 (사용자 결정). 스택 N = N회 회피.
  - `DealDamageGA` 자체는 실행되므로 `DealDamageCondition`/`AttackedOpponentCondition`을 구독한 특성(공격 신호, 맹공, 반격 등)은 빗나가도 발동한다 (공격자 쪽 반응 유지, 사용자 결정).
  - `AttackCoreGA`(코어 공격)는 대상이 코어라 회피와 무관하다.
  - "MISS" 표시·회피 연출·사운드는 없다 (아트 제외).
  - 부여 수단: 발판 '풀숲' — `TerrainSystem`이 용병이 풀숲에 들어오면(`MoveGA`/`PlayCardGA` POST) `AddStatusEffectGA(DODGE, 1)`을 부여하고, 떠나면 1을 회수한다. 출처 구분은 없다 ([08_terrain_obstacle.md](08_terrain_obstacle.md) §5).
  - 에디터: 에셋의 `EffectType`=`DODGE`, `Discription` 입력, `spriteImage` 연결, `StatusEffectSystem.effectDatas`에 등록 필요. Play 테스트는 아직 하지 않았다.
