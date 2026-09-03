# 3D Interaction Overview

새 3D 스테이지에 실제로 적용하는 방법은 [`3d-stage-setup-guide.md`](./3d-stage-setup-guide.md)를 참고합니다.

## 목적

3D 씬에서 사용하는 NPC 클릭, 플레이어 이동, 카메라 전환, 도착 후 이벤트 실행 흐름을 공통 구조로 정리했습니다.
기존 카페 씬에서는 Barista NPC와 GamePlayScene에 맞춰 상호작용 로직이 구성되어 있어 다른 3D 스테이지에서 그대로 재사용하기 어려운 부분이 있었습니다.

이번 수정에서는 클릭 가능한 대상을 `InteractionTarget`으로 분리하고, 클릭 감지와 이동 흐름은 `StageInteractionController`에서 관리하도록 변경했습니다.
새 3D 스테이지에서도 같은 방식으로 NPC나 오브젝트 상호작용을 설정할 수 있습니다.

---

## 기본 흐름

```text
NPC 또는 3D 오브젝트 클릭
→ StageInteractionController가 InteractionTarget 감지
→ InteractionTarget의 On Interaction Started 이벤트 실행
→ PlayerClickMover가 Interaction Point로 이동
→ 플레이어 도착
→ InteractionCameraController가 Camera Point로 카메라 전환
→ InteractionTarget의 On Player Arrived 이벤트 실행
→ player fade / 세션 시작 등 연결된 이벤트 실행
```

---

## 주요 컴포넌트

### 1. InteractionTarget

NPC나 상호작용 가능한 3D 오브젝트에 붙이는 컴포넌트입니다.

해당 오브젝트가 어떤 상호작용 대상인지, 플레이어가 어디로 이동해야 하는지, 상호작용 시 카메라가 어디로 이동해야 하는지를 관리합니다.
클릭 직후와 플레이어 도착 후에 실행할 이벤트도 이 컴포넌트에서 연결합니다.

#### 주요 역할

- 상호작용 대상 ID 관리
- 플레이어가 이동할 Interaction Point 지정
- 상호작용 시 사용할 Camera Point 지정
- 한 번만 상호작용할지 여부 설정
- 클릭 직후 실행할 이벤트 연결
- 플레이어 도착 후 실행할 이벤트 연결

#### 주요 필드

| 필드 | 설명 |
|---|---|
| Target Id | 상호작용 대상을 구분하기 위한 ID입니다. |
| Interaction Point | 플레이어가 이동할 위치입니다. |
| Camera Point | 상호작용 시 카메라가 이동할 위치입니다. |
| Interact Only Once | 상호작용을 한 번만 허용할지 설정합니다. |
| Target Collider | 클릭 판정에 사용할 Collider입니다. |
| On Interaction Started | 클릭 직후 실행할 이벤트입니다. |
| On Player Arrived | 플레이어가 도착한 뒤 실행할 이벤트입니다. |

`Interaction Point`가 비어 있으면 target 자신의 위치를 사용합니다.

`Target Collider`가 비어 있으면 같은 오브젝트에 붙어 있는 `Collider`를 자동으로 가져옵니다.

---

### 2. StageInteractionController

씬 전체의 3D interaction 흐름을 관리하는 컴포넌트입니다.

마우스 또는 터치 입력을 감지하고, Raycast를 통해 클릭된 오브젝트에서 `InteractionTarget`을 찾습니다.
target이 확인되면 상호작용 시작 이벤트를 실행하고, 플레이어를 `Interaction Point`로 이동시킵니다.
플레이어가 도착하면 카메라를 `Camera Point`로 전환한 뒤 도착 이벤트를 실행합니다.

#### 주요 역할

- 마우스/터치 입력 감지
- Raycast로 클릭된 `InteractionTarget` 탐색
- 상호작용 시작 이벤트 실행
- 플레이어 이동 요청
- 플레이어 도착 후 카메라 전환
- 플레이어 도착 이벤트 실행
- 상호작용 중 중복 입력 방지

#### 주요 필드

| 필드 | 설명 |
|---|---|
| Player Mover | 플레이어 이동을 담당하는 `PlayerClickMover`입니다. |
| Camera Controller | 카메라 전환을 담당하는 `InteractionCameraController`입니다. |
| Raycast Camera | 클릭 판정에 사용할 Camera입니다. |
| Disable Input While Interacting | 상호작용 중 추가 입력을 막을지 설정합니다. |

---

### 3. InteractionCameraController

카메라를 지정된 카메라 포인트로 부드럽게 이동시키는 컴포넌트입니다.

기존 `GameCameraController`는 explore view와 talk view처럼 정해진 카메라 위치를 기준으로 동작했습니다.
`InteractionCameraController`는 원하는 `Transform`을 받아 이동할 수 있도록 만들어, NPC나 오브젝트별로 다른 카메라 포인트를 사용할 수 있습니다.

#### 주요 역할

- 기본 카메라 위치 관리
- target의 Camera Point로 카메라 이동
- 카메라 위치와 회전값 보간

#### 주요 필드

| 필드 | 설명 |
|---|---|
| Default Camera Point | 기본 탐색 시점의 카메라 위치입니다. |
| Transition Duration | 카메라 이동 시간입니다. |

---

### 4. PlayerClickMover

플레이어를 지정된 위치까지 이동시키는 컴포넌트입니다.

`StageInteractionController`가 target의 `Interaction Point`를 전달하면, `PlayerClickMover`가 해당 위치까지 플레이어를 이동시킵니다.
도착하면 callback을 실행하여 다음 상호작용 흐름이 이어지도록 합니다.

#### 주요 역할

- 지정된 위치로 플레이어 이동
- 도착 거리 체크
- 도착 후 callback 실행
- 이동 중 Animator walking 상태 설정

---

### 5. StageReactionController

스테이지별 반응 연출을 담당하는 컴포넌트입니다.

현재 카페 씬에서는 guide object 숨김, player fade, NPC 반응, 보상 커피 이동 연출 등을 처리합니다.

다만 reward coffee, coffee give point, coffee move duration 같은 필드는 카페 씬에 특화된 요소입니다.
다른 스테이지로 확장할 때는 공통 reaction과 stage별 reaction을 분리하는 방향을 검토하면 좋습니다.

#### 주요 역할

- interaction 시작 시 guide object 숨김
- 대화 시작 시 player fade 처리
- 턴 성공/실패에 따른 NPC 반응 실행
- 퀘스트 성공/실패에 따른 NPC 반응 실행
- 카페 씬의 보상 커피 이동 연출 처리

---

## 기존 구조와 변경된 구조

### 기존 구조

기존 `InteractableNpc`는 한 스크립트에서 여러 역할을 함께 처리했습니다.

```text
NPC 클릭 감지
→ 플레이어 이동
→ guide 숨김
→ 카메라 전환
→ player fade
→ GamePlayManager 세션 시작
```

이 구조는 Barista NPC와 카페 씬에 강하게 연결되어 있어, 새 스테이지에서 그대로 재사용하기 어렵습니다.

### 변경된 구조

새 구조에서는 역할을 아래와 같이 나누었습니다.

| 역할 | 담당 컴포넌트 |
|---|---|
| 클릭 가능한 대상 정보 | `InteractionTarget` |
| 클릭 감지 및 이동 흐름 | `StageInteractionController` |
| 플레이어 이동 | `PlayerClickMover` |
| 카메라 전환 | `InteractionCameraController` |
| 스테이지별 반응 | `StageReactionController` |
| 게임 세션 시작 | `GamePlayManager` |

---

## 향후 개선 방향

- `PlayerClickMover`를 더 일반적인 이름인 `PlayerInteractionMover`로 변경하는 것을 검토합니다.
- `GameCameraController`와 `InteractionCameraController` 역할 통합 여부를 검토합니다.
- `StageReactionController`에서 카페 전용 보상 연출을 분리하여 stage별 reaction으로 확장하는 것을 검토합니다.
- `GamePlayManager`의 API 호출을 공통 API Provider 또는 SDK 기반 구조로 이전하는 것을 검토합니다.
- 여러 NPC가 있는 스테이지에서 target별 event 설정 방식을 검증합니다.