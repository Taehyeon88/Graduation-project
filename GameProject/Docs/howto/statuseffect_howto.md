# 상태효과(StatusEffect) 만들기 — 설계 → 코드 → 에셋 → 문서

> 상태효과 하나를 처음부터 끝까지 만드는 레시피. 현재 상태효과 목록·판정 위치·감소 규칙은 [StatusEffect.md](../feature-spec/StatusEffect.md)가 갖고 있으므로 여기서는 **만드는 순서와 판단 기준**만 다룬다. 이 문서는 커스텀 슬래시 커맨드(예: `/statuseffect 이름, 효과 설명`)가 따를 절차이며, 견본은 회피(`DODGE`) 작업이다(부록).

## 1. 언제 쓰나
"~상태가 걸리면 ~한다" 형태의 상태효과를 새로 만들 때. 예: "이 용병을 대상으로 하는 다음 1회의 모든 공격과 피해가 빗나갑니다. 효과가 발동하거나 지속시간이 종료되면 이 상태가 해제됩니다."
- 이미 있는 상태효과를 **걸어 주는 수단**(카드·특성)만 필요하면 코드 추가 없이 `AddStatusEffectEffect`(카드) / `AddStatusTraitEffect`(특성)에서 타입만 고른다. §5의 에셋 등록 여부만 확인한다.

## 2. 사전 읽기 (관련 부분만)
- `Docs/feature-spec/StatusEffect.md` — **가장 먼저.** 기존 상태효과의 판정 위치·감소 규칙. 같은 방식으로 풀 수 있으면 그 위치를 따른다
- `Assets/Game/0_Scripts/Enums/StatusEffectType.cs`, `5.Data/StatusEffectData.cs`
- `Assets/Game/0_Scripts/1.Systems/StatusEffectSystem.cs` — 에셋 캐싱·`AddStatusEffectGA` performer
- `Assets/Game/0_Scripts/3.Views/CombatantView.cs` — `AddStatusEffect`/`RemoveStatusEffect`/`CheckStatusEffectExist`/`GetStatusEffectStacks`, `ReduceSEWhenMyTurnEnd`, `Damage`
- `Assets/Game/0_Scripts/General/Util/DamageCaculator.cs`, `1.Systems/Skills/DamageSystem.cs` — 피해 파이프라인
- `Assets/Game/0_Scripts/UI/StatusEffectsUI.cs` — 표시 방식(모든 타입이 아이콘+스택 일반 경로)
- `Docs/convention.md` (코드 규칙)

## 3. 설계 단계 — 효과를 어디서 판정할지 결정

### 3-1. 문장 분해
한 문장을 다음 셋으로 쪼갠다. 애매하면 **구현 전에 사용자에게 확인**한다.

| 항목 | 질문 | 회피의 답 |
|---|---|---|
| 스택의 의미 | 수치인가, 횟수인가, 유무만 보는가 | 횟수 (1스택 = 1회 회피) |
| 지속·감소 | 턴 종료 감소 / 발동 시 소모 / 무기한 | 발동 시 1 소모, 지속시간 없음 (사용자 결정) |
| 판정 지점 | 어느 코드가 이 효과를 읽는가 | `DamageSystem.DealDamageGAPerformer` |

### 3-2. 판정 지점 선택

| 효과 종류 | 판정 위치 | 기존 예 |
|---|---|---|
| 피해량 수치 보정 | `DamageCaculator` | POWER, WEAK, VULNERABLE |
| 피해를 흡수 | `CombatantView.Damage` | (현재 없음 — 방어 ARMOR는 삭제됨) |
| 피해·피격 연출 자체를 막음 | `DamageSystem.DealDamageGAPerformer` — `GetDamage`/`PlayDamageVFX` **이전**, `Targets` 루프와 `Target` 단일 분기 **두 곳 모두** | DODGE |
| 턴 시작/종료 시 감소 | `CombatantView.ReduceSEWhenMyTurnEnd` | WEAK, VULNERABLE |

- **`Damage()` 안에서 막지 않는다.** `DamageSystem`이 `Damage()` 호출 전에 `PlayDamageVFX`를 재생하므로 빗나가도 피격 연출이 나온다 (회피 검토에서 확인).
- 모든 공격·피해(기본 공격, 반격, 저격, 전체 피해)는 `DealDamageGA` → `DamageSystem` 한 곳을 지난다. `AttackCoreGA`는 코어 대상이라 `CombatantView` 상태효과와 무관하다.
- 판정 대상이 `CombatantView`(용병·영웅 공통)인지 확인한다. 코어 등 `IDamageable`은 상태효과가 없다.

### 3-3. 반응(특성)과의 상호작용을 먼저 확인
`DealDamageGA`가 실행되면 `DealDamageCondition`/`AttackedOpponentCondition`을 구독한 특성(공격 신호, 맹공, 반격, 피해 시 회복 등)이 발동한다. 피해를 막는 효과는 **막힌 뒤에도 이 반응을 유지할지** 사용자에게 확인한다 (회피는 유지로 결정). 반응까지 취소하려면 `ActionSystem`(핵심 인프라, 수정 금지)에 취소 개념이 필요해 계약 변경이 된다.

## 4. 코드 단계

### 4-1. `StatusEffectType` 추가
- `Enums/StatusEffectType.cs`의 **끝에** 추가하고 한 줄 주석을 단다 (기존 값의 직렬화 순서 유지). 앞 값에 쉼표가 붙는지 확인. 멤버를 삭제한 적이 있어 값이 명시되어 있으면(`POWER = 0, WEAK = 2, …`) 새 멤버에도 다음 정수를 명시한다.
- 열거형 값은 `StatusEffectData` 에셋에 정수로 저장된다. 중간 삽입·순서 변경 금지.

### 4-2. 판정 로직
- 요청한 효과만 만든다. 이번 효과에 쓰이지 않는 유연성·설정은 넣지 않는다.
- 사용자가 새 메서드를 원하지 않으면 판정을 호출 지점에 **인라인**한다 (회피: `TryConsumeDodge()` 같은 메서드 미작성). 인라인이 두 곳에 중복되는 것은 허용 범위다.
- 회피의 형태:
```csharp
//회피 : 스택 1 소모하고 피해·피격 연출 생략
if (target is CombatantView dodger && dodger.CheckStatusEffectExist(StatusEffectType.DODGE))
{
    dodger.RemoveStatusEffect(StatusEffectType.DODGE, 1);
    continue;   //단일 대상 분기에서는 else if로 스킵
}
```
- 새 GA·시스템·UI는 보통 필요 없다: 부여는 `AddStatusEffectGA` → `StatusEffectSystem` → `CombatantView.AddStatusEffect` 일반 경로, 표시는 `StatusEffectsUI`가 모든 타입을 아이콘+스택으로 그린다.

### 4-3. 감소 훅이 필요한 경우
- 영웅은 `HeroSystem`이 `ReduceSEWhenMyTurnEnd`를 호출한다. **용병에는 호출하는 훅이 없다.** 용병의 턴 종료 감소가 필요하면 새 호출 지점이 필요하니 구현 전에 사용자에게 알린다.

### 4-4. 코드를 지울 때
- 소비처가 없는 열거형·메서드를 정리하라는 요청이면 먼저 `grep`으로 참조(코드·`.asset`·문서)를 확인한다. 파일이 비UTF-8(레거시 인코딩)이면 Edit/Write가 거부하므로 삭제는 shell `rm`으로 `.cs`와 `.meta`를 함께 지운다.
- 에셋에 남은 직렬화 필드는 무해하다 (Unity가 다음 저장 시 정리). 에셋은 편집하지 않는다.

### 4-5. 계약 변경이 필요하면 먼저 알린다
`Singleton`/`ActionSystem` 등 핵심 인프라, GA 시그니처, SO 필드 형태를 바꿔야 하면 **구현 전에 차이를 짚고 승인받는다.** GA를 바꾸면 `action_command.md`도 갱신한다.

## 5. 에셋·에디터 단계

에디터 작업은 원칙적으로 사용자 몫이다 (클릭 순서를 안내하고 멈춘다). **사용자가 직접 요청했을 때만** Unity MCP로 수행한다.

1. `Assets/Game/1_Datas/6.StatusEffects/`의 `StatusEffectData` 에셋 — `EffectType`, `Discription`, `spriteImage`. 스프라이트는 `Assets/Game/4_Medias/StatusEffects/`.
2. 씬 `StatusEffectSystem.effectDatas` 배열에 에셋을 등록한다. 누락 시 부여 단계에서 `effectDataByType[...]`가 `KeyNotFoundException`, `EffectType`이 중복이면 `LogError`.

Unity MCP로 할 때 (회피에서 확인한 방법):
- 에셋 값: `manage_scriptable_object` `modify`, `target`={path}, `patches`=`[{"propertyPath":"<EffectType>k__BackingField","value":<enum 정수>}, {"propertyPath":"<Discription>k__BackingField","value":"..."}, {"propertyPath":"<spriteImage>k__BackingField","ref":{"path":"Assets/.../이름.png"}}]`
- 씬 객체: `find_gameobjects`(`by_component`, `StatusEffectSystem`) → instance ID
- 배열 등록: `manage_components` `set_property`, `property`=`effectDatas`, `value`=에셋 경로 배열. **배열을 통째로 덮어쓰므로 기존 값을 먼저 확인해 모두 포함한다.** (회피 때는 기존 Power·Armor 두 항목이 있다고 가정하고 썼다가 이후 diff로 확인했다 — 먼저 읽는 것이 정석)
- `manage_scene` `save` 후 `git diff -- Assets/Game/2_Scenes/GameDemoScene.unity`로 추가분만 바뀌었는지 확인.
- 부여 수단(카드/특성)을 에셋에 연결하는 작업은 이 문서 범위 밖이다.

## 6. 문서 갱신
- [StatusEffect.md](../feature-spec/StatusEffect.md) — 요약 표에 행 추가 + 상세 섹션(에셋·효과·판정·흐름 도식·주의·한계·에디터 설정). 회피 섹션이 견본이다.
- 새 문서를 만들었거나 챕터가 늘었으면 `Docs/feature-spec.md` 목차에 한 줄.
- 열거형·메서드를 지웠다면 `Docs/convention.md`의 관련 언급(이번엔 262·294행 `SEMachanicsType`)을 갱신한다.
- 다른 문서의 낡은 서술은 조용히 고치지 말고 보고만 한다.

## 7. 검증
1. `refresh_unity`(compile 요청) → `read_console`로 에러 0 확인.
2. 열거형·메서드를 지웠다면 잔여 참조 `grep`(Library 제외) 0건 확인.
3. Play 확인이 필요한 항목은 직접 한 척 하지 말고 사용자에게 안내한다. 회피 기준 시나리오:
   - 효과를 가진 용병이 적 기본 공격 1회를 맞아도 체력 불변·피격 VFX 없음·아이콘 제거 → 다음 공격은 정상 피해
   - 전체 피해로 보유/미보유 대상을 섞어 때리면 보유 대상만 면제
   - 스택 2면 2회 연속 빗나감

## 8. 함정·알려진 한계
- 피격 VFX는 `Damage()` 호출 전에 재생된다 — 판정이 늦으면 연출이 새어 나온다 (§3-2).
- 용병에는 턴 종료 감소 훅이 없다 (영웅만 `HeroSystem`). 지속시간이 있는 효과는 용병에서 줄지 않는다.
- 막힌 공격에서도 `DealDamageGA` 구독 특성은 발동한다 (§3-3).
- `WEAK`(취약)/`VULNERABLE`(약화)는 이름과 효과 방향이 뒤바뀐 듯 보인다 — 코드는 WEAK = 공격자 피해 ×0.75, VULNERABLE = 피격자 피해 ×1.5 (원작 확인 필요). 이 둘은 에셋도 없어 UI 아이콘이 표시되지 않는다.
- "MISS" 표시·회피 연출·사운드는 아트 범위 제외라 만들지 않는다.
- `effectDatas` 배열 덮어쓰기 주의 (§5).
- 원작 규칙을 모르는 수치(지속시간, 스택 상한 등)는 임의로 단정하지 말고 `(원작 확인 필요)`로 표기한다.

## 9. 완료 보고 형식
- 추가한 `StatusEffectType` 값과 판정 위치(파일·메서드)
- 계약·시그니처 변경 여부 (있으면 갱신한 문서)
- 에셋·씬 연결 내용 (또는 사용자에게 안내한 클릭 순서)
- Console 결과 (에러·경고 수)
- 남긴 한계와 Play 확인 항목

## 부록. 회피(DODGE) 작업 요약 — 견본

| 단계 | 내용 |
|---|---|
| 설계 | 스택 = 횟수, 무기한·발동 시 1 소모, 빗나가도 공격자 쪽 반응 유지 (사용자 결정). 판정 지점은 `DamageSystem` (VFX 이전) |
| 코드 | `StatusEffectType.cs` 끝에 `DODGE`; `DamageSystem.cs` 다중 대상 루프 + 단일 대상 분기에 인라인 판정; 불필요해진 `SEMachanicsType`(`.cs`+`.meta`, `StatusEffectData` 필드, `StatusEffectSystem.GetMachanicsType`) 제거 |
| 에셋 | `회피.asset`: `EffectType`=4(`DODGE`), `Discription`, `spriteImage`=`회피.png`; 씬 `StatusEffectSystem.effectDatas`에 등록 후 씬 저장 |
| 문서 | `feature-spec/StatusEffect.md` 신규(상태효과 5종), `feature-spec.md` 목차 06 행, `convention.md` `SEMachanicsType` 언급 갱신 |
| 검증 | Console 에러 0 확인. Play 테스트는 하지 않음 (§7 시나리오는 사용자 확인 대기) |
