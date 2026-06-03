# CLAUDE.md — 졸업작품 게임 프로젝트

## 기본 규칙

- **한국어**로만 답변한다.
- 코드/에셋을 수정하기 전에, **변경할 내용과 이유를 먼저 설명하고 확인**을 받는다.
- 단, 단순 1~2줄 수정(오타, 수치 변경 등)은 바로 적용해도 된다.
- 3개 이상의 파일을 동시에 수정하는 작업은 **계획(Plan)을 먼저 보여준 뒤 승인을 받고** 실행한다.
- `git push`는 절대 하지 않는다. `git commit`도 명시적으로 요청받을 때만 한다.

## 씬/에셋 수정 규칙

- `.unity` 씬 파일을 직접 수정할 때는 **반드시 먼저 무엇을 바꾸는지 설명하고 확인**을 받는다.
- `.prefab`, `.asset` 파일을 새로 만들 때는 기존 파일의 YAML 구조와 네이밍 컨벤션을 따른다.
- SoundData 에셋 이름은 반드시 `{숫자}.{설명}.asset` 형식으로 만든다 (SoundId가 이름에서 파싱됨).

## 아키텍처 패턴

### ActionSystem (핵심)
모든 게임 로직은 `ActionSystem`을 통해 흐른다.
- GameAction 클래스: 순수 데이터, 접미사 `GA` (예: `PlayCardGA`)
- System lifecycle: `OnEnable`에서 Performer/Reaction 등록, `OnDisable`에서 해제
- 새 게임 로직은 반드시 이 패턴을 따른다

### 클래스 네이밍
| 종류 | 패턴 | 예시 |
|------|------|------|
| GameAction | `VerbNounGA` | `DealDamageGA` |
| System | `NounSystem` | `SoundSystem` |
| View | `NounView` | `HeroView` |
| Data(SO) | `NounData` | `CardData` |
| Effect | `VerbNounEffect` | `HealEffect` |
| Enum 파일 | 파일명 = enum명 | `AudioType.cs` |

### 주요 경로
| 경로 | 역할 |
|------|------|
| `Assets/Game/0_Scripts/1.Systems/` | 게임 시스템 (~30개, Singleton) |
| `Assets/Game/0_Scripts/2.GameActions/` | GameAction 서브클래스 |
| `Assets/Game/0_Scripts/5.Data/` | ScriptableObject 데이터 |
| `Assets/Game/1_Datas/Sounds/` | SoundData 에셋 (ID 1~33) |
| `Assets/Game/6_AudioClips/` | 오디오 클립 파일 |
| `Assets/Game/2_Scenes/GameDemoScene.unity` | 메인 배틀 씬 |

## 스프라이트 크기 조정 규칙

- 몬스터, 함정 등 오브젝트의 **시각적 크기를 바꿀 때는 프리팹 Transform scale을 건드리지 않는다**.
- 해당 TokenData의 `<Sprite>k__BackingField` GUID를 찾아 스프라이트 `.meta` 파일의 `spritePixelsToUnits` 값을 수정한다.
- 크기를 N배 **키우려면** `현재 PPU ÷ N`, **줄이려면** `현재 PPU × N`
- 예: 현재 PPU 100에서 1.3배 크게 → `100 ÷ 1.3 ≈ 77`

## 코드 작성 규칙

- 주석은 **이유(WHY)가 명확히 비자명할 때만** 작성한다. "무엇을 하는지" 설명하는 주석은 쓰지 않는다.
- `using UnityEditor`를 런타임 스크립트에 추가하지 않는다 (빌드 오류 원인).
- 에러 처리는 시스템 경계(외부 입력 등)에서만 한다. 내부 로직에는 불필요한 null 체크를 남발하지 않는다.
- 기존 패턴에서 벗어나는 구현을 할 때는 이유를 먼저 설명한다.

## 사운드 시스템 (현재 구조)

- `SoundSystem` : 씬에 존재하는 Singleton, BGM/SFX 모두 관리
- `SoundData` : ScriptableObject, `SoundId`는 에셋 이름 앞 숫자에서 자동 파싱
- BGM ID: 32 (메인 BGM 1), 33 (보스 BGM)
- BGM 재생: `SoundSystem.Instance.PlayBGM(id)` — 기존 BGM 자동 정지 후 재생
- SFX 재생: `SoundSystem.Instance.PlaySound(id)`
- `RoomData.IsBossRoom == true`이면 보스 BGM(33), 아니면 메인 BGM(32) 재생
