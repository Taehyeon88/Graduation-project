# DataSystem 미참조 에셋 동기화

DataSystem 컴포넌트에 등록되지 않은 ScriptableObject 에셋을 찾아 자동으로 추가한다.

## 작업 절차

### 1단계: 현재 DataSystem 참조 상태 확인

Unity MCP의 `find_gameobjects` 또는 `manage_components` 툴로 씬 내 DataSystem 컴포넌트가 가진 현재 직렬화 필드를 읽는다. DataSystem이 씬에 없으면 프리팹 경로를 탐색한다.

`manage_components` 툴 사용 예시:
```
action: "get"
gameObjectName: "DataSystem"
```

### 2단계: 디스크 에셋 수집

아래 경로별로 Glob 툴을 사용해 `.asset` 파일 목록을 수집한다.

| DataSystem 필드 | 에셋 타입 | 폴더 경로 |
|---|---|---|
| `cards` | CardData | `Assets/Game/1_Datas/2.Cards/` |
| `heroes` | HeroData | `Assets/Game/1_Datas/1.Tokens/Heroes/` |
| `enemies` | EnemyData | `Assets/Game/1_Datas/1.Tokens/Enemies/` |
| `walls` | WallData | `Assets/Game/1_Datas/1.Tokens/Obstacles/` (Wall 포함) |
| `traps` | TrapData | `Assets/Game/1_Datas/1.Tokens/Obstacles/` (Trap 포함) |
| `destructibles` | DestructibleData | `Assets/Game/1_Datas/1.Tokens/Obstacles/` (Destructible 포함) |
| `statusEffects` | StatusEffectData | `Assets/Game/1_Datas/4.StatusEffects/` |
| `visualEffects` | VisualEffectData | `Assets/Game/1_Datas/5.VisualEffects/` |
| `visualGrids` | VisualGridData | `Assets/Game/1_Datas/6.VisualGrids/` |
| `aoEs` | AoEData | `Assets/Game/1_Datas/AoEs/` |
| `dices` | DiceData | `Assets/Game/1_Datas/Dices/` |
| `rooms` | RoomData | `Assets/Game/1_Datas/Rooms/` |
| `sounds` | SoundData | `Assets/Game/1_Datas/Sounds/` |

> **주의:** `walls`, `traps`, `destructibles`는 모두 `1.Tokens/Obstacles/` 폴더를 공유한다.
> 각 `.asset` 파일의 YAML 헤더에서 `m_Script` GUID를 읽거나, 에셋 이름 패턴으로 타입을 구별한다.
> 확실하지 않을 경우 `manage_asset` 툴로 실제 타입을 확인한다.

### 3단계: 미참조 에셋 비교

- 1단계에서 얻은 현재 참조 목록과 2단계의 디스크 목록을 비교한다.
- 필드별로 디스크에는 있으나 DataSystem에 등록되지 않은 에셋을 식별한다.
- 발견한 미참조 에셋 목록을 사용자에게 먼저 보여주고 추가 여부를 확인받는다.

### 4단계: DataSystem 프리팹에 에셋 추가

사용자 승인 후 `execute_code` 툴로 에디터 스크립트를 실행해 **프리팹**에 미참조 에셋을 추가한다.
DataSystem 프리팹 경로: `Assets/Game/3_Prefabs/DataSystem.prefab`

```csharp
// execute_code 예시 — 실제 경로/필드/에셋은 3단계 결과로 채운다
var prefabPath = "Assets/Game/3_Prefabs/DataSystem.prefab";
var prefabAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(prefabPath);
var dataSystem = prefabAsset.GetComponent<DataSystem>();

var so = new UnityEditor.SerializedObject(dataSystem);

// 예시: cards 필드에 새 에셋 추가
var prop = so.FindProperty("cards");
int newIndex = prop.arraySize;
prop.InsertArrayElementAtIndex(newIndex);
prop.GetArrayElementAtIndex(newIndex).objectReferenceValue =
    UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.ScriptableObject>(
        "Assets/Game/1_Datas/2.Cards/새카드.asset");

so.ApplyModifiedProperties();
UnityEditor.EditorUtility.SetDirty(dataSystem);
UnityEditor.AssetDatabase.SaveAssets();
return "완료";
```

> **씬 오브젝트가 아닌 프리팹을 직접 수정**하므로 씬 저장 없이 `AssetDatabase.SaveAssets()`만으로 영구 저장된다.

### 5단계: 결과 검증

- `read_console` 툴로 에러가 없는지 확인한다.
- 추가된 에셋 수와 필드명을 최종 요약으로 사용자에게 알린다.

## 주의사항

- 에셋 추가 전 반드시 사용자에게 목록을 보여주고 확인을 받는다.
- Obstacles 폴더의 에셋은 타입 판별이 필요하므로 YAML `m_Script` 비교나 `manage_asset` 확인을 거친다.
- **씬 오브젝트(`FindFirstObjectByType`)가 아닌 프리팹(`LoadAssetAtPath`)을 사용한다.** 씬이 닫혀 있어도 동작한다.
- `InsertArrayElementAtIndex` 후 새 슬롯의 `objectReferenceValue`를 반드시 명시적으로 설정해야 한다 (Unity 기본값이 이전 요소를 복사하므로).
