# MiniGameProject — Claude Code 안내

모바일(터치, 세로/가로 미정) 2D Unity 게임. 4~5초짜리 미니게임을 연속으로 하는 메이드 인 와리오 형식이다.
학부 졸업작품 팀(기획 1, 프로그래머 1, 아트 6)이 만들고 있고, 이 저장소에서 미니게임 코드를 Claude Code로 구현한다.

## 지금 만드는 범위 (2026-11-25)

- **우선제작 30개는 모두 구현되어 있다.** 문서(`Docs/Design/`)는 구현 기준으로 맞춰져 있고, 구글 시트의 우선제작·전체 탭에도 반영되어 있다 (2026-11-25).
- **우선제작 미니게임 30개(스테이지별 5개)만 만든다.** 목록과 구현용 컨텍스트: `Docs/Design/Priority/`
- 나머지 90개는 후순위라 **만들지 않는다.** `minigames.json`에서 `status: "deferred"`로 표시되어 있고, 스프라이트 폴더 README에도 "후순위"라고 적혀 있다. 기획 참고용으로만 남겨 둔다.

## 먼저 읽을 것

| 문서 | 내용 |
|---|---|
| `Docs/Design/README.md` | 기획 문서 목차 |
| `Docs/Design/Priority/README.md` | **우선제작 30개 목록, 템플릿 묶음, 확인 필요 항목** |
| `Docs/Design/Priority/{ID}.md` | **미니게임 하나의 구현용 컨텍스트** (성공·실패, 조정 수치 기본값, 에셋 자리, 확인 필요) |
| `Docs/Design/01_게임개요와_규칙.md` | 게임 규칙, 현재 코드 구조와 기획의 차이 |
| `Docs/Design/02_플레이방식_템플릿.md` | 조작 기호 A~J와 공통 템플릿 |
| `Docs/Design/03_에셋_규칙.md` | 스프라이트 폴더·파일 이름 규칙, 임시 이미지 사용법 |
| `Docs/Design/04_미결정사항.md` | 아직 정해지지 않은 것. 여기 있는 내용은 임의로 결정하지 않는다 |
| `Docs/Design/MiniGames/{테마}.md` | 테마별 전체 기획안 20개 (우선제작/후순위 표시, 참고용) |
| `Docs/Design/minigames.json` | 미니게임 120개 전체 데이터 (`status`: priority / deferred, 원본은 구글 시트) |

미니게임 하나를 만들 때는 **`Docs/Design/Priority/{ID}.md`를 따른다.** 그 문서의 "확인 필요" 항목은 적힌 가정으로 구현하고, 작업을 마칠 때 기획자에게 다시 알린다.

## 코드 구조 (이미 있는 것)

- `Assets/_Game/Script/MiniGame/MiniGame.cs`: 모든 미니게임의 기본 클래스
  - `OnBegin()`을 구현한다. 결과가 정해지면 `Finish(true/false)`를 호출한다 (처음 한 번만 반영).
  - 제한 시간은 `StageFlowController`가 재고, 시간이 끝나면 `TimeUp()`이 불린다.
  - **버티기형**은 프리팹에서 `SucceedOnTimeUp = true` (끝까지 버티면 성공), **빨리깨기형**은 `false` (시간 초과면 실패).
  - `Title` 필드에는 명세의 **지시문**을 넣는다 (예: "썰어라!").
- `Assets/_Game/Script/MiniGame/Common/TouchMiniGame.cs`: 터치 미니게임의 공통 부모. 놀이 영역(800 × 500), 터치 전달, 에셋 자리(`Slots`), 제한시간에 따른 속도 배율(`SpeedScale`)
- `Assets/_Game/Script/Input/TouchInput.cs`: 터치 입력 창구 (에디터에서는 마우스 좌클릭 = 손가락 하나). 새 입력은 여기를 쓴다
- `Assets/_Game/Script/Editor/MiniGameSpriteLinker.cs`: 메뉴 MiniGame > 스프라이트 자동 연결
- `Assets/Scenes/Test/MiniGameTest.unity`: 미니게임을 하나씩 골라 테스트하는 씬
- `Assets/_Game/Script/MiniGame/TestButtonMiniGame.cs`: 테스트용 예시
- `Assets/_Game/Script/Stage/StageFlowController.cs`: 스테이지 진행. `NormalGames` 배열에 미니게임 프리팹을 등록하면 랜덤으로 나온다 (직전과 같은 미니게임은 연속으로 안 나옴).
- 스테이지 씬: `Assets/Scenes/Stage/{Desert,Jungle,Cave,Precipice,Forest,Laboratory}.unity`

## 미니게임을 만들 때 규칙

1. **명세를 그대로 구현한다.** 명세와 다르게 해야 할 이유가 있으면 바꾸지 말고 먼저 알린다.
2. **공통 템플릿을 먼저 만들고 재사용한다.** 프로그래머가 1명이라 미니게임마다 새 로직을 짜지 않는다. 우선제작 30개는 20개 방식으로 구현되어 있다 (`02_플레이방식_템플릿.md`, `Priority/README.md`의 "템플릿별 묶음"). 여러 개가 쓰는 템플릿부터 만든다.
3. **수치는 인스펙터에 노출한다.** 대상 개수, 연타 횟수, 속도 기준 등은 `[SerializeField]`로 빼고, 명세에 적힌 값을 기본값으로 쓴다. 4초 안에 끝나는 분량이어야 한다.
4. **후순위 미니게임은 만들지 않는다.** 템플릿을 만들 때 후순위 미니게임을 위해 기능을 미리 넣지 않는다.
5. **아트는 만들지 않는다.** 이미지는 아트팀이 `Assets/_Game/Sprite/{Theme}/{ID}/`에 넣는다. 코드는 스프라이트를 `[SerializeField] Sprite` 필드로 받고, 그림이 없는 동안은 `Assets/_Game/Art/Placeholder/`(Circle, Square, GridTile)를 색만 바꿔 쓴다. 각 폴더의 `README.md`에 필요한 파일 목록이 있다.
6. **파일 위치**
   - 스크립트: `Assets/_Game/Script/MiniGame/{Theme}/` — 클래스 이름은 영문 PascalCase + `MiniGame` (예: `PineappleSliceMiniGame`), 파일 맨 위 주석에 ID와 한글 이름을 적는다.
   - 공통 템플릿 스크립트: `Assets/_Game/Script/MiniGame/Common/`
   - 프리팹: `Assets/_Game/Prefabs/MiniGame/{Theme}/MiniGame_{ID}.prefab` (예: `MiniGame_JUN_10.prefab`)
7. **입력은 터치 기준**으로 만든다 (에디터에서는 마우스로 테스트). 프로젝트에 있는 Input System 설정을 따른다.
8. **기억해서 따라 하는 유형은 만들지 않는다.** 순서 정보가 필요한 미니게임도 정답 순서는 화면에 계속 보여 준다.

## 하지 말 것

- `StageFlowController`의 라운드·보스·제한 시간 로직은 기획자가 요청하기 전까지 바꾸지 않는다. 바꿀 내용은 `01_게임개요와_규칙.md`의 "현재 코드에서 바꿔야 할 것"에 정리되어 있다 (보스 삭제, 10·20·30… 라운드 시작 때 체력회복 찬스 연출, 그 라운드가 끝나면 스피드업 연출 후 −0.5초, 20라운드를 마치면 클리어, 무한 모드는 등록된 미니게임 전체 랜덤).
- 지시문을 보여 주는 별도 화면을 만들지 않는다. 지시문은 미니게임 화면 안에 계속 표시한다 (만져라 메이드 인 와리오 스타일).
- `04_미결정사항.md`에 있는 항목(화면 방향, 테마 진행 방식, 점수 등)을 임의로 정하지 않는다. 필요하면 질문한다.
- 기획 문서(`Docs/Design/`)를 코드 작업 중에 고치지 않는다. 기획 원본은 구글 시트이며, 기획자가 갱신한다.
