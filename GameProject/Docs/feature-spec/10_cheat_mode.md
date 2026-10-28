# 10. 치트 모드 (임시 — 제거 예정)

> 시연/테스트용 임시 기능. **나중에 한 번에 빼기 위해** 관련 코드·에셋 위치를 전부 이 문서에 모았다.
> 제거 절차는 맨 아래 "제거 체크리스트"만 따라가면 된다.

## 1. 동작

- **C 키**로 치트 모드 토글 (레거시 `Input.GetKeyDown(KeyCode.C)`). 켜고 끌 때 우측 하단에 팝업("치트가 사용됨" / "치트가 해제됨")이 2초 유지 후 0.5초 페이드아웃.
- 치트 모드 ON 상태에서 **전투 시작(`TurnType.StartBattle`) 드로우**가 바뀐다.
  - 고정 카드 4장: `겹습기사`(= 견습기사, 에셋명 오타 그대로), `기병`, `매복자`, `축복자` 를 드로우 더미 맨 위로 올린다.
  - 덱에 없는 카드는 건너뛴다 (만들어 넣지 않음).
  - 드로우 장수 = `Max(startDrawAmount, 실제로 올린 카드 수)`. `startDrawAmount`가 3이어도 4장이 모두 덱에 있으면 4장을 뽑는다.
  - 고정 카드 외 나머지는 기존 셔플 순서(랜덤) 그대로.
- 치트 OFF면 기존 동작과 완전히 동일 (`startDrawAmount`장 드로우).
- 모든 전투(일반/엘리트 구분 없음)에 적용. 주점(Tavern) 등 `StartBattle`이 없는 흐름에는 영향 없음.

## 2. 관련 항목 전체 목록

### 2.1 현재 사용 중 (치트 모드 본체)

| 구분 | 위치 | 내용 |
|---|---|---|
| 스크립트 | `Assets/Game/0_Scripts/1.Systems/CheatSystem.cs` | `Singleton<CheatSystem>`. C 키 토글, `IsCheatMode`, 팝업 연출. **파일 통째로 삭제 대상** |
| 스크립트 | `Assets/Game/0_Scripts/1.Systems/CardSystem.cs` | 아래 2.1.1의 3곳 |
| 프리팹 | `Assets/Game/3_Prefabs/--- GAMEMASTER ---.prefab` | 하위 `CheatSystem` 오브젝트(컴포넌트 `CheatSystem`) + 자식 `Popup`(CanvasGroup + 텍스트). **오브젝트째 삭제 대상** |

#### 2.1.1 `CardSystem.cs` 안의 치트 코드 (3곳)

1. 상수 (파일 상단)
   ```csharp
   private static readonly string[] CheatDrawNames = { "겹습기사", "기병", "매복자", "축복자" };   //치트 고정 드로우 ...
   ```
2. 메서드 `MoveCheatCardsToTop()` 전체 (`//치트 모드: 지정 카드를 드로우 더미 맨 위로 ...` 주석 포함). 올린 카드 수(int)를 반환.
3. `StartBattlePreReaction` 내부
   ```csharp
   // 현재
   int drawAmount = startDrawAmount;
   if (CheatSystem.Instance != null && CheatSystem.Instance.IsCheatMode)
       drawAmount = Mathf.Max(startDrawAmount, MoveCheatCardsToTop());   //고정 카드는 ...

   DrawCardsGA drawCardsGA = new(drawAmount);

   // 제거 후 (원래 형태)
   DrawCardsGA drawCardsGA = new(startDrawAmount);
   ```

### 2.2 옛 치트 잔재 (현재 동작과 무관, 같이 정리 후보)

현재 `CheatSystem`과 **연결되지 않은** 이전 시도의 흔적. 치트를 뺄 때 함께 지우는 것을 권장하지만, 지우기 전에 사용 여부를 한 번 확인할 것.

| 위치 | 내용 | 비고 |
|---|---|---|
| `Assets/Game/0_Scripts/1.Systems/InteractionSystem.cs` | 필드 `changeCheatMode`, `cheatUpdatedAction`, `Initialze()`의 `playerInput.actions["ChangeCheatMode"]`, `Update()`의 `cheatUpdatedAction?.Invoke(...)` | `cheatUpdatedAction`은 구독자가 없어 호출해도 아무 일 없음 |
| `Assets/Game/GameProjectInput.inputactions` | 액션 `ChangeCheatMode`(바인딩: →, Enter, Space), 액션 `StartCheat`(바인딩: C) | `InteractionSystem`이 `ChangeCheatMode`를 이름으로 조회하므로 **코드와 에셋을 함께** 지울 것. `StartCheat`는 코드 참조 없음. 현재 C 키 치트는 이 액션이 아니라 `Input.GetKeyDown`을 직접 사용 |
| `Assets/Game/2_Scenes/DevelopScene/MapTestScene.unity` | `CheatSystem`(구 스크립트 GUID `21d4cc26…`, 해당 `.cs`가 프로젝트에 없어 Missing Script 상태), `CheatUI`, `CheatTitle`, `CheatModeName` | 개발용 씬의 옛 UI. 필드명 `cheatUITrans`, `cardCheatModeText`, `Attack_Adjacent_CardDeck` |

## 3. 의존 관계 (제거 시 영향)

- `CheatSystem`을 참조하는 코드는 **`CardSystem.StartBattlePreReaction` 하나뿐**이다 (`CheatSystem.Instance != null` 가드 포함). 다른 시스템·UI는 참조하지 않는다.
- `CheatSystem`은 `Singleton<T>`이고 GAMEMASTER 하위라 씬을 넘나든다. 삭제해도 다른 `Singleton`/`ActionSystem` 등록에 영향 없음.
- 팝업 `Popup`은 `CheatSystem`의 자식이라 오브젝트를 지우면 함께 사라진다. 다른 곳에서 재사용하지 않음.

## 4. 제거 체크리스트

순서대로 하면 컴파일 에러 없이 빠진다.

1. `CardSystem.cs`: 2.1.1의 3곳을 제거/원복 (`CheatSystem` 참조가 먼저 없어져야 2번이 안전).
2. `--- GAMEMASTER ---.prefab`: `CheatSystem` 오브젝트(자식 `Popup` 포함) 삭제 후 저장.
3. `CheatSystem.cs` 와 `CheatSystem.cs.meta` 삭제.
4. (선택) 2.2의 잔재 정리: `InteractionSystem.cs`의 치트 코드 → `GameProjectInput.inputactions`의 `ChangeCheatMode`/`StartCheat` 액션·바인딩 → `MapTestScene`의 옛 `CheatSystem`/`CheatUI` 오브젝트.
5. Console 에러 0 확인, 전투 진입 시 드로우가 `startDrawAmount`장인지 확인.

검색 확인용: 제거 후 `Cheat` 문자열이 `Assets/` 코드·프리팹에 남아 있지 않아야 한다 (2.2를 지우지 않았다면 해당 항목만 남음).

## 5. 알려진 사항

- `CheatDrawNames`의 `"겹습기사"`는 견습기사 SO의 에셋명 오타를 그대로 따른 것. 에셋명을 고치면 이 상수도 함께 바꿔야 매칭된다.
- 카드 매칭은 `Card.Title` 문자열 비교라 이름이 바뀌면 조용히 무시된다.
- 실제 플레이 검증은 하지 않았다 (컴파일 에러 0만 확인).
