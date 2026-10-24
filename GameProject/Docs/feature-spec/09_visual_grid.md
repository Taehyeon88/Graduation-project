# VisualGrid 아키텍처

> 타일 위에 겹쳐 그리는 하이라이트(이동 범위·배치 구역·선택/호버 표시) 시스템. 이 문서는 구조 요약과 **현재 모든 VG의 레이어 순서표**를 기록한다. 스프라이트·색 같은 아트 수치는 범위 밖.

## 핵심 파일

| 경로 | 역할 |
|---|---|
| `Assets/Game/0_Scripts/5.Data/VisualGridData.cs` | `VisualGridData` SO(타입 목록) + 중첩 `VisualGridType`(`FillType`/`BorderType`/`SymbolType`) |
| `Assets/Game/0_Scripts/4.Creators/VisualGridCreator.cs` | `Singleton` — **이름**으로 VG를 조회해 타일의 SpriteRenderer 슬롯에 적용/제거 |
| `Assets/Game/1_Datas/7.VisualGrids/` | VG 에셋 6개 (아래 순서표) |
| `Assets/Game/3_Prefabs/Isometrics/TileVisual.prefab` | 타일 1개 = SpriteRenderer 슬롯 6개(`VG_None` 태그), Sorting Layer 2 |

## 1. 구조 요약

- `VisualGridData`는 `List<VisualGridType>`을 가진다. 한 VG가 Fill+Border처럼 여러 레이어를 가질 수 있다.
- 각 `VisualGridType`은 `sortingOrder`(int)·색·스프라이트를 가진다. **Fill/Border/Symbol 구분은 순서와 무관**하다 — 화면 순서는 오직 `sortingOrder` 수치(클수록 위)로 정해진다.
- `CreateVisualGrid`는 `VG_None` 태그 슬롯을 하나씩 가져다 `sr.sortingOrder = vgType.sortingOrder`를 덮어쓴다. 슬롯 인덱스는 순서와 무관하다.
- 이전 방식(타입별 +10/+20/+30 오프셋, `layerDepth`, z값)은 폐기됐다. z값(`GetVGTypeTransZ`)은 폐기 전에도 호출되지 않았다.

## 2. API (`VisualGridCreator`)

`tokenId`는 호출한 쪽의 `gameObject.GetInstanceID()` 또는 토큰 ID — 같은 (tokenId, VG 이름) 쌍으로 묶어 제거한다.

| 메서드 | 동작 |
|---|---|
| `CreateVisualGrid(tokenId, pos, vgName)` | 해당 타일에 VG 표시 |
| `RemoveVisualGrid(tokenId, vgName)` | (tokenId, 이름)으로 만든 VG 전부 제거 |
| `RemoveVisualGridById(tokenId)` | tokenId로 만든 VG 전부 제거 |
| `ChangeVisualGrid(pos, tokenId, before, after)` | 이름 교체 |
| `ChangeVisualGridPosition(tokenId, pos, vgName)` | 제거 후 새 위치에 생성(이동) |
| `ContainVisualGrid(tokenId, vgName)` | 표시 중인지 조회 |
| `SetHideVisualGrid(tokenId)` / `SetReverseVisualGrid(tokenId)` | tokenId의 VG 숨김 / 복원 |

## 3. ★ 현재 VG 레이어 순서표

order가 클수록 위에 그려진다. 값은 `1_Datas/7.VisualGrids/*.asset`의 `sortingOrder` 실측치.

| order | VG | 타입 | 사용처 | 용도 |
|---|---|---|---|---|
| **33** | `Hero_Move` | Symbol | `MoveSystem` | 영웅 이동 가능 칸 |
| **30** | `Selector_Select` | Symbol | `GridSelector` | 선택된 영웅 위치 |
| **30** | `Selector_Hover` | Symbol | `GridSelector` | 마우스 호버 칸 |
| **30** | `Zone_Hero` | Symbol | `MatchSetupSystem` | 아군 배치 구역(전투 내내 유지) |
| **30** | `Zone_Enemy` | Symbol | `MatchSetupSystem` | 적 배치 구역(전투 내내 유지) |
| **20** | `Hero_SetUp_Highlight` | Border | `CardSystem` | 카드 배치 가능 칸 위 마우스 호버 |

새 VG에 쓸 수 있는 빈 값: 1~19, 21~29, 31~32, 34 이상.

## 4. 주의 / 알려진 한계

- **order 30 동점 4개** (`Selector_Select`·`Selector_Hover`·`Zone_Hero`·`Zone_Enemy`): 같은 타일에 겹치면 위아래가 코드로 정해지지 않는다. 순서 변경 전에도 전부 동점(layerDepth 0 + Symbol 30)이었으므로 새로 생긴 문제는 아니다. 겹침 우선순위가 필요하면 에셋의 `sortingOrder`를 다르게 줘야 한다 — **원하는 우선순위는 미정(확인 필요)**.
- **`Hero_SetUp_Highlight`(20)는 Symbol(30) 아래**다. 배치 구역(`Zone_Hero`) 타일 위에서는 하이라이트 테두리가 Zone 심볼에 가려질 수 있다.
- **이름 조회 실패는 조용히 무시된다.** `CreateVisualGrid`는 `visualGridDatas`에서 `v.name == vgName`을 못 찾으면 에러 없이 반환한다. 씬의 `VisualGridCreator.visualGridDatas`에 등록돼 있어야 동작한다 (`GameDemoScene` 6개 / `MapTestScene`은 `Hero_Move`·`Hero_SetUp_Highlight` 2개만 등록 — 이 씬에서는 Zone·Selector가 안 보인다).
- **타일당 슬롯 6개.** 한 타일에 VG 레이어가 6개를 넘으면 초과분은 표시되지 않는다.
- `VisualGridData`의 `GetVGType*()`는 SO 안의 로직이라 [convention.md](../convention.md) 11절 위반 목록에 이미 기록돼 있다.

## 5. 새 VG 추가 절차

1. `Create > Data > VisualGrid`로 SO를 만들고 `7.VisualGrids/`에 둔다. 타입(Fill/Border/Symbol)별 스프라이트·색·`sortingOrder`를 채운다.
2. 위 순서표에서 겹치지 않는 `sortingOrder`를 고른다.
3. (에디터) 사용할 씬의 `VisualGridCreator.visualGridDatas`에 등록한다.
4. 호출 코드에서 SO **파일 이름과 같은 문자열**로 `CreateVisualGrid`를 호출한다.
5. 이 문서의 순서표를 갱신한다.
