# Folder.md — `Assets/Game` 폴더 구조

> `Assets/Game/` 전체(스크립트 + 에셋)를 전수 조사해서 정리한 폴더 지도다. 파일 개수·목록은 조사 시점 스냅샷이므로 자산이 추가/삭제되면 다시 검증해야 한다.
> 코드 네이밍·계약 규칙(클래스 시그니처, 인터페이스, SO 필드 등)은 이 문서가 아니라 **[convention.md](convention.md)** 참고. 이 문서는 "무엇이 어디에 있는가"만 다룬다.

---

## 최상위 개요

```
Assets/Game/
├─ 0_Scripts/       # C# 스크립트 전체 — 1절 참고
├─ 1_Datas/         # ScriptableObject 데이터 자산(.asset) — 0_Scripts/5.Data 클래스의 실제 인스턴스, 2절 참고
├─ 2_Scenes/        # 씬(.unity) — 3절 참고
├─ 3_Prefabs/       # 프리팹(.prefab) — 4절 참고
├─ 4_Medias/        # 스프라이트/이미지(.png) — 5절 참고
├─ 5_Materials/     # 머티리얼(.mat) — 6절 참고
├─ 6_AudioClips/    # 오디오 클립(.wav/.mp3) — 7절 참고
├─ GameAudioMixer.mixer          # 전역 오디오 믹서 — 8절 참고
├─ GameProjectInput.inputactions # Input System 액션 정의 — 8절 참고
└─ Move_Step.wav                 # 8절 참고 (중복 파일 의심)
```

## 1. `0_Scripts/` — C# 스크립트

```
0_Scripts/
├─ 1.Systems/              # 게임플레이 오케스트레이터(~System)/~Processor(상태 없는 GA 처리 전용)
│  ├─ Skills/               #   분류용 하위 폴더 — 네이밍/구조 규칙은 루트와 동일
│  └─ Skills/Player/
├─ 2.GameActions/          # GameAction(~GA) 서브클래스
│  └─ CardEffectRelative/  #   분류용
├─ 3.Views/                # CombatantView/HeroView/EnemyView 등 토큰의 뷰 레이어
├─ 4.Creators/             # TokenCreator/ProjectionCreator 등 생성 담당
├─ 5.Data/                 # ScriptableObject 데이터 클래스 정의(~Data 접미사, 11개) — 실 자산은 1_Datas/(2절)
├─ Effects/                # Effect(Models/Effect.cs) 서브클래스 — SkillAbility.Effects에 담기는 데이터+로직 홀더
│  └─ CloseRange_Attack/   #   분류용(ShieldBashEffect 등)
├─ Enemies/                # Enemy(Models/Enemy.cs) 서브클래스(Larva, Scarecrow 등)
│  └─ EnemyActions/        #   적 행동 로직(AttackEA 등)
├─ Enums/                  # 열거형 13개
├─ Extensions/             # 확장 메서드(ArrayExtenstion, ListExtension)
├─ General/
│  ├─ Singleton.cs         # 제공 인프라 — 상속만, 수정 금지
│  ├─ ActionSystem/        # 제공 인프라 — ActionSystem.cs, GameAction.cs, ReactionTiming.cs
│  ├─ Attributes/          # 커스텀 인스펙터 속성(ShowIfAttribute.cs, ShowIfDrawer.cs)
│  ├─ Util/                # 정적 헬퍼(Utility, UtilityBFS, DamageCaculator, GridLayoutImageResizer)
│  └─ DontDestroyedObject.cs
├─ Interfaces/             # I 접두사 인터페이스 7개: IHaveCaster, IDamageable, IHaveDamage, IHaveDistance, IUseCondition, IUseCustomRangeVG, IUseCustomTargetVG
├─ Models/                 # 도메인 모델(SO 아닌 순수 C# 클래스): Token/TokenGrid, Hero/HeroPreview/Enemy, Skill/SkillAbility/Effect(추상 베이스), Perk/PerkItem, EnemyAction
│  └─ Modes/               #   범위·타겟 선정 전략(RangeMode/TargetMode: 플레이어용, EnemyRangeMode/EnemyTargetMode: 적용)
├─ Perks/                  # Perk(Models/Perk.cs) 서브클래스: BloodyAxe, HeroArmor, MuscleTrophy
├─ Struct/                 # 값 타입 구조체(EnemyActionInfo)
├─ SubSystems/             # Singleton 파사드가 위임하는 실행 컴포넌트(TokenMainAPI/TokenServiceAPI/TokenSetup, GridSelector, TooltipTrigger)
├─ UI/                     # 화면별 UI 컴포넌트(~UI 접미사, SkillsUI/StatusEffectsUI/TurnPopUpUI 등) + AudioMixerController
└─ GameSystem.cs           # 메타 상태(골드/스테이지/영웅 로스터) 전담 Singleton(상세는 convention.md 6절)
```

세부 네이밍·계약 규칙은 `convention.md` 1~11절 참고.

## 2. `1_Datas/` — SO 데이터 자산 (0_Scripts/5.Data 클래스와 매칭)

| 폴더 | 자산 수 | 대응 스크립트 클래스 | 비고 |
|---|---|---|---|
| `1.Enemies/` | 1 | `EnemyData` | 허수아비 |
| `1.Heroes/` | 3 | `HeroData` | Hero1~3 |
| `2.Skills/` | 3 | `SkillData` | 타격/사격/방어 |
| `3.Stages/` | 1 | `StageData` | TestStage |
| `3.Stages/Waves/` | 1 | `WaveData` | Simple_Wave |
| `3.Stages/Waves/WaveCores/` | 1 | `WaveCoreData` | SimpleCore |
| `4.StatusEffects/` | 2 | `StatusEffectData` | Armor, Power |
| `6.VisualGrids/` | 19 | `VisualGridData` | 그리드 하이라이트 전부 |
| `7.Perks/` | 3 | `PerkData` | 피묻은 도끼 / 근육 트로피 / 용사의 갑옷 |
| `8.Sounds/` | 18 | `SoundData` | `SoundData.asset`/`SoundData 1.asset`은 이름 정리가 안 된 기본값으로 보임 |

**관찰(기록만, 수정 대상 아님):** 폴더 번호가 "5."는 건너뛰고 "1."을 `Enemies`/`Heroes` 둘 다에 쓴다 — `0_Scripts/5.Data`의 클래스 11개와 번호가 1:1로 대응하지는 않는다(`TokenData`는 추상 베이스라 자산이 없다).

## 3. `2_Scenes/` — 씬

- **실제 씬**(루트): `GameDemoScene.unity`, `StartScene.unity`
- **개발·테스트용**(`DevelopScene/`, 4개): `MapTestScene.unity`, `New Scene.unity`, `SpriteTestScene.unity`, `UIScene.unity`

## 4. `3_Prefabs/` — 프리팹 (35개)

| 폴더 | 개수 | 파일 |
|---|---|---|
| (루트) | 7 | `DataSystem`, `GameSystem`, `PlayerInput`, `SplinePrefab`, `TESTPREFAB`, `VisualGird`, `--- GAMEMASTER ---`(프로젝트 창 구분용 더미로 보임) |
| `Isometrics/` | 8 | `AoE_BurningEmbers`, `AoE_EmblemOfSanctuary`, `AoE_PoisonousSwamp`, `AoE_PoolOfLight`, `AoE_SpiderwebTrap`, `Grid_Tile`, `TestWorld_1`, `TileVisual` |
| `Models/` | 3 | `EnemyModel`, `HeroModel`, `WallModel` |
| `Projections/` | 2 | `RatGeneral_Stone`, `Skeleton_BoneArrow` |
| `UI/` | 6 | `DisPlayUI`, `EnemyUI`, `PerkUI`, `SettingUI`, `StatusEffectUI`, `StatusEffectUIAtWorld` |
| `Views/` | 9 | `ArrowView`, `CombatantViewBase`, `EnemyView`, `HeroPreView`, `HeroView`, `PopUp_Text_View`, `ProjectionView`, `SkillView`, `WaveCoreView` |

## 5. `4_Medias/` — 스프라이트/이미지 (98개 png)

| 폴더 | 개수 | 비고 |
|---|---|---|
| `1.Samples/` | 9 | 초기 샘플용 영웅/몬스터 아이콘 |
| `2.HUDUI/` | 35 | 가장 큰 폴더 — HUD/UI 스프라이트(그리드 아이콘, 코인 등) |
| `3.Skills/` | 3 | 스킬 아이콘 |
| `Enemies/` | 5 | 몬스터 스프라이트 |
| `Projections/` | 2 | 투사체 스프라이트 |
| `SlayTheSpireTuto/` | 11 | 이름상 타 프로젝트 튜토리얼 참고용 샘플 이미지로 보임 |
| `StatusEffects/` | 8 | 상태이상 아이콘 |
| `Traps/` | 2 | 함정 스프라이트 |
| `VisualGrids/` | 11 | 그리드 하이라이트 스프라이트 |

## 6. `5_Materials/` — 머티리얼 (6개)

`Floor.mat`, `NegativeMaterial.mat`, `NormalDice.mat`, `PositiveMaterial.mat`, `Masks/MaskHole.mat`, `Masks/MaskTarget.mat`

## 7. `6_AudioClips/` — 오디오 클립 (16개)

| 폴더 | 개수 |
|---|---|
| `Battle_SFX/` | 4 |
| `BGM/` | 0 (비어있음) |
| `Game_SFX/` | 1 |
| `Interaction_SFX/` | 6 |
| `SE_SFX/` | 2 |
| `Skill_SFX/` | 3 |

폴더명이 `Enums/AudioType.cs`의 `AudioEditorType` 열거형 멤버(`BGM`/`Battle_SFX`/`Game_SFX`/`Skill_SFX`/`Interaction_SFX`/`SE_SFX`)와 정확히 1:1 대응한다 — 코드에서 이 enum을 직접 소비하는 곳은 없지만(`convention.md` 9절 "정리 후보" 참고) 폴더 분류 의도와는 일치한다.

## 8. 루트 파일

- `GameAudioMixer.mixer` — 전역 오디오 믹서, `UI/AudioMixerController.cs`가 참조
- `GameProjectInput.inputactions` — Input System 액션 정의
- `Move_Step.wav` — `6_AudioClips/Battle_SFX/Move_Step.wav`와 이름이 같은 중복 파일로 보임(정리 후보, 이번 문서화 범위에서는 삭제하지 않음)
