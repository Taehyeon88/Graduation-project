# hierarchy.md — `GameDemoScene` Hierarchy 스냅샷

> 이 문서는 `Assets/Game/2_Scenes/GameDemoScene.unity`의 Hierarchy 구조를 Unity MCP(`manage_scene get_hierarchy`)로 조사한 스냅샷이다. 씬이 바뀌면 이 문서도 다시 검증한다.
> 루트에는 빈 GameObject를 `--- 이름 ---` 형태로 만들어 폴더처럼 쓰는 관례가 이미 있다. 이 문서는 그 관례를 그대로 따라 카탈로그화하고, 끝에 개선 제안을 덧붙인다.

## 1. 루트 카테고리 목록

| 카테고리 | 자식 수 | 용도 |
|---|---|---|
| `--- SETUP ---` | 4 | 씬 기본 설정(조명·카메라·이벤트 시스템) |
| `--- UI ---` | 6 | Canvas 전체 |
| `--- VIEWS ---` | 2 | 토큰 월드 뷰 · VFX |
| `--- SYSTEMS ---` | 10 | 전투/스킬 처리 Singleton System |
| `--- PROCESSORS ---` | 3 | 상태 없이 GA만 처리하는 `~Processor` |
| `--- CREATORS ---` | 3 | 생성 담당(`4.Creators/`와 1:1 대응) |
| `--- MANAGERS ---` | 10 | 그 외 전역 매니저 |
| `--- INTERACTIONS ---` | 2 | 입력 처리 |
| `--- GAMEMASTER ---` | 1 | `DontDestroyOnLoad` 대상(`DontDestroyedObject` 컴포넌트) |
| (카테고리 없음, 루트) | — | `GameObject`(`Test`+`AudioSource`) — 테스트용으로 추정 |

## 2. 카테고리별 상세

### `--- SETUP --- `
| GameObject | 컴포넌트 |
|---|---|
| Directional Light | Light, UniversalAdditionalLightData |
| Global Volume | Volume |
| EventSystem | EventSystem, InputSystemUIInputModule |
| Main Camera | Camera, AudioListener, UniversalAdditionalCameraData |

### `--- UI ---`
| GameObject | 자식 수 | 비고 |
|---|---|---|
| Left Canvas | 3 | Canvas, CanvasScaler, GraphicRaycaster |
| Bottom Canvas | 6 | 〃 |
| Right Canvas | 2 | 〃, Layer 5 |
| Tooltip Canvas | 2 | Canvas, CanvasScaler (GraphicRaycaster 없음) |
| Middle Canvas | 1 | Canvas, CanvasScaler, GraphicRaycaster |
| Canvas | 1 | **비활성(`activeSelf: false`)**. 자식이 스크립트 없는 일반 `Image` 하나뿐 — 3절 참고 |

(각 Canvas의 손자 이하 UGUI 구조는 이번 조사에서 펼쳐보지 않음.)

### `--- VIEWS ---`
| GameObject | 자식 수 | 컴포넌트 |
|---|---|---|
| IsoGridWorldView | 5 | IsoWorld |
| VFX | 1 | (Transform만) |

### `--- SYSTEMS ---` (10개)
| GameObject | 컴포넌트 | 비고 |
|---|---|---|
| DamageSystem | DamageSystem | |
| EnemySystem | EnemySystem | |
| HeroSystem | HeroSystem | |
| SkillSystem | SkillSystem | |
| StatusEffectSystem | StatusEffectSystem | |
| AnimationSystem | AnimationSystem | |
| MP/APSystem | APSystem, MPSystem | 두 System을 한 GameObject에 번들 |
| MoveSystem | MoveSystem | |
| HealSystem | HealSystem | |
| WaveSystem | WaveSystem | |

### `--- PROCESSORS ---` (3개)
| GameObject | 컴포넌트 |
|---|---|
| CardComboProcessors | `AttackEnemyProcessor`, `ShieldBashProcessor`, `ShoulderBashProcessor`, `SplashProcessor` |
| EffectProcessor | `EffectProcessor` |
| KnockBackProcessor | `KnockBackProcessor` |

`--- SYSTEMS ---`에서 분리된 카테고리 — 4절 참고.

### `--- CREATORS ---`
| GameObject | 컴포넌트 |
|---|---|
| TokenCreator | TokenCreator |
| VisualGridCreator | VisualGridCreator |
| ProjectionCreator | ProjectionCreator |

`Assets/Game/0_Scripts/4.Creators/`의 클래스 3개와 1:1로 정확히 대응 — 가장 깔끔한 카테고리.

### `--- MANAGERS ---` (10개)
| GameObject | 컴포넌트 | 비고 |
|---|---|---|
| ActionSystem | ActionSystem | |
| TurnSystem | TurnSystem | |
| MatchSetupSystem | MatchSetupSystem | |
| TokenSystem | TokenSystem, TokenSetup, TokenMainAPI, TokenServiceAPI, TokenGrid | 5개 컴포넌트 번들 |
| UISystem | UISystem | |
| PauseSystem | PauseSystem | |
| SoundSystem | SoundSystem | 자식 3개(BGM/SFX/Interaction용 Transform으로 추정, 미확인) |
| TooltipSystem | TooltipSystem | |
| VisualEffectSystem | **(없음, Transform만)** | 대응 스크립트 없는 빈 GameObject — 3절 참고 |
| RewardSystem | RewardSystem | |

### `--- INTERACTIONS ---`
| GameObject | 컴포넌트 |
|---|---|
| PlayerInput | PlayerInput, Interactions |
| GridSelector | GridSelector |

### `--- GAMEMASTER ---`
루트 자체에 `DontDestroyedObject` 컴포넌트가 붙어 있음.
| GameObject | 컴포넌트 |
|---|---|
| GameSystem | GameSystem |

## 3. 발견한 이슈

1. ~~Processor 6개가 `--- SYSTEMS ---`에서 진짜 Singleton System과 섞여 있음.~~ **해결됨** — 4절 참고.
2. ~~GameObject 이름이 컴포넌트 리네이밍을 따라가지 못함(`EffectSystem`/`KnockBackSystem`).~~ **해결됨** — 4절 참고.
3. **`--- UI ---/Canvas`(비활성)** — 유일한 자식이 스크립트 없는 일반 `Image` 하나뿐. 기능적으로 연결된 곳이 없어 죽은 플레이스홀더로 추정된다. 삭제 여부는 별도 판단 필요(이번엔 확인만 함).
4. **`--- MANAGERS ---/VisualEffectSystem`** — 스크립트가 하나도 안 붙은 빈 Transform. 코드베이스 전체에 "VisualEffectSystem"이라는 클래스가 존재하지 않고, `Assets/Game/0_Scripts/3.Views/ProjectionView.cs`의 주석 처리된 죽은 코드(`HeroVisualEffectSystem`, 이름도 다름)만 유일한 관련 흔적이다 — 구현되지 않은 기능의 잔재로 추정된다. 삭제 여부는 별도 판단 필요(이번엔 확인만 함).
5. **카테고리 밖 루트의 `GameObject`**(`Test`+`AudioSource`) — 어느 `--- 이름 ---` 카테고리에도 속하지 않고 테스트용으로 보인다. 이번 조사 범위 밖.

## 4. `--- PROCESSORS ---` 카테고리 신설 (적용 완료)

이슈 1·2를 해결한 변경. Unity MCP로 씬에 직접 적용하고 저장했다.

- 새 루트 카테고리 `--- PROCESSORS ---`를 `--- SYSTEMS ---` 다음에 생성.
- `--- SYSTEMS ---`에서 아래 3개 GameObject를 `--- PROCESSORS ---`로 옮기고 이름도 컴포넌트에 맞게 바꿈:

| 이전 위치:이름 | 이동 후 이름 | 보유 컴포넌트 |
|---|---|---|
| `--- SYSTEMS ---/HerosSkillAbilitySystems` | `CardComboProcessors` | AttackEnemyProcessor, ShieldBashProcessor, ShoulderBashProcessor, SplashProcessor |
| `--- SYSTEMS ---/EffectSystem` | `EffectProcessor` | EffectProcessor |
| `--- SYSTEMS ---/KnockBackSystem` | `KnockBackProcessor` | KnockBackProcessor |

- 컴포넌트 구성(어떤 GameObject에 몇 개가 붙어있는지)은 바꾸지 않았다 — 위치·이름만 정리.
- 이슈 3·4(비활성 Canvas, 빈 VisualEffectSystem)는 이 변경에 포함하지 않았다 — 삭제 여부를 먼저 정해야 한다.

## 5. 문서 유지보수

- 이 문서는 스냅샷이다. Hierarchy 구조가 바뀌거나(카테고리 추가/이동) 이슈가 해결되면 즉시 갱신한다.
