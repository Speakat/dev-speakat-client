# 3D Stage Setup Guide

3D interaction 구조와 각 컴포넌트 역할은 [`3d-interaction-overview.md`](./3d-interaction-overview.md)를 참고합니다.

## 새 스테이지 작업 기준

새 스테이지를 만들 때는 공통 interaction 컴포넌트와 스테이지별 요소를 구분해서 작업합니다.

### 공통으로 재사용하는 요소

아래 컴포넌트는 특정 카페 씬이나 Barista NPC에 의존하지 않으므로, 다른 3D 스테이지에서도 그대로 재사용할 수 있습니다.

- `InteractionTarget`
- `StageInteractionController`
- `InteractionCameraController`
- `PlayerClickMover`

### 스테이지마다 새로 만들거나 다르게 연결하는 요소

아래 요소는 스테이지의 배경, 캐릭터, 연출 방식에 따라 달라질 수 있으므로 새 스테이지에 맞게 별도로 구성합니다.

- NPC / 오브젝트 모델
- Interaction Point 위치
- Camera Point 위치
- 스테이지별 reaction controller
- 성공/실패/보상 연출
- 배경, 조명, 소품, 프리팹 구성

처음 새 스테이지를 구성할 때는 기존 `GamePlayScene`의 공통 구조를 참고하되, 카페 전용 연출을 그대로 가져오지 않도록 주의합니다.

특히 현재 `StageReactionController`에는 reward coffee, coffee give point처럼 카페 씬에 특화된 요소가 포함되어 있습니다.

다른 스테이지에서는 별도의 reaction controller를 만들거나 필요한 기능만 분리해서 사용하는 것을 권장합니다.

---

## 카페 씬 적용 예시

현재 `GamePlayScene`에서는 Barista NPC에 공통 interaction 구조를 적용했습니다.

### 1. Main Camera 설정

Main Camera에 `InteractionCameraController`를 추가합니다.

```text
Default Camera Point = ExploreCameraPoint
```

`Default Camera Point`는 대화 전 기본 탐색 카메라 위치입니다.

---

### 2. StageInteractionController 설정

`CafeStageRoot` 또는 interaction을 관리할 root 오브젝트에 `StageInteractionController`를 추가합니다.

```text
Player Mover = PlayAgent의 PlayerClickMover
Camera Controller = Main Camera의 InteractionCameraController
Raycast Camera = Main Camera
Disable Input While Interacting = true
```

---

### 3. Barista NPC 설정

Barista NPC에 `InteractionTarget`을 추가합니다.

```text
Target Id = barista
Interaction Point = Barista 앞에 배치한 InteractionPoint
Camera Point = TalkCameraPoint
Interact Only Once = true
Target Collider = Barista NPC의 Collider
```

현재 카페 씬에서는 기존 `TalkCameraPoint`를 `Camera Point`로 사용합니다.

추후 NPC별로 다른 카메라 구도가 필요하면 `BaristaTalkCameraPoint`, `CustomerTalkCameraPoint`처럼 별도의 camera point를 만들어 연결하면 됩니다.

---

### 4. Event 연결

`InteractionTarget`의 이벤트는 아래와 같이 연결합니다.

#### On Interaction Started

NPC를 클릭한 직후 실행됩니다.

```text
StageReactionController.OnInteractionStarted()
```

현재 카페 씬에서는 guide object를 숨기는 용도로 사용합니다.

#### On Player Arrived

플레이어가 `Interaction Point`에 도착한 뒤 실행됩니다.

```text
StageReactionController.OnTalkStarted()
GamePlayManager.StartQuestSessionFromNpc()
```

현재 카페 씬에서는 도착 후 player fade를 실행하고, 게임 세션을 시작합니다.

카메라 전환은 `StageInteractionController`가 `InteractionCameraController.MoveTo(target.CameraPoint)`를 호출하여 처리합니다.

---

## 새 3D 스테이지에 적용하는 방법

새로운 3D 스테이지에서 interaction을 사용하려면 아래 순서로 설정합니다.

1. 씬에 `StageInteractionController`를 배치합니다.
2. 플레이어 오브젝트에 `PlayerClickMover`가 붙어 있는지 확인합니다.
3. Main Camera에 `InteractionCameraController`를 추가합니다.
4. 상호작용할 NPC 또는 오브젝트에 `InteractionTarget`을 추가합니다.
5. NPC 앞에 빈 오브젝트를 만들고 `Interaction Point`로 연결합니다.
6. 대화 시점 카메라 위치에 빈 오브젝트를 만들고 `Camera Point`로 연결합니다.
7. `On Interaction Started`에 클릭 직후 실행할 이벤트를 연결합니다.
8. `On Player Arrived`에 도착 후 실행할 이벤트를 연결합니다.
9. Play Mode에서 클릭 → 이동 → 도착 이벤트 흐름을 확인합니다.

처음부터 전체 게임 플로우까지 연결하기보다는, 먼저 클릭, 플레이어 이동, 카메라 전환, 도착 이벤트 실행이 정상적으로 동작하는지 확인하는 것을 권장합니다.

---

## Interaction Point / Camera Point 배치 기준

### Interaction Point

`Interaction Point`는 플레이어가 NPC 앞에 도착해야 하는 위치입니다.

아래 기준을 참고하여 배치합니다.

- NPC와 너무 가깝지 않게 배치합니다.
- Collider 내부가 아니라 NPC 앞쪽 빈 공간에 배치합니다.
- 플레이어가 도착했을 때 NPC와 자연스럽게 마주 보는 위치에 둡니다.
- 바닥 높이와 플레이어 위치가 어색하지 않은지 확인합니다.

### Camera Point

`Camera Point`는 상호작용 시 카메라가 이동할 위치와 회전값입니다.

아래 기준을 참고하여 배치합니다.

- NPC와 플레이어가 모두 보이는 구도로 배치합니다.
- 대화 UI를 가리지 않도록 여백을 둡니다.
- 카메라 위치뿐 아니라 회전값도 함께 조정합니다.
- 여러 NPC가 있다면 NPC별 Camera Point를 따로 만드는 것을 권장합니다.

---

## 테스트 방법

### 1. Interaction 단독 테스트

로그인이나 API 호출 없이 3D interaction만 확인하려면 `GamePlayManager`의 테스트 옵션을 사용합니다.

```text
Skip Session Api For Interaction Test = true
Start Session On Scene Start = false
```

이 옵션을 켜면 `GamePlayManager.StartQuestSessionFromNpc()`가 호출되어도 실제 세션 API 요청을 보내지 않습니다.

확인할 항목은 다음과 같습니다.

- NPC 클릭이 감지되는지 확인합니다.
- 플레이어가 `Interaction Point`로 이동하는지 확인합니다.
- 도착 후 이벤트가 실행되는지 확인합니다.
- 카메라가 `Camera Point`로 전환되는지 확인합니다.
- player fade가 정상 동작하는지 확인합니다.
- 세션 API 호출이 스킵되는지 확인합니다.

정상 로그 예시는 다음과 같습니다.

```text
[StageInteractionController] Target clicked: barista
[InteractionTarget] Interaction started: barista
[StageInteractionController] Player arrived at target: barista
[InteractionTarget] Player arrived: barista
[GamePlayManager] Interaction test mode: session API start skipped.
```

현재 `GamePlayScene` 단독 실행 환경에서 위 흐름이 정상적으로 동작하는 것을 확인했습니다.

테스트 결과는 다음과 같습니다.

```text
- Barista NPC 클릭 감지 성공
- 플레이어 이동 성공
- 카메라 전환 성공
- Barista NPC idle/reaction 동작 확인
- player fade 동작 확인
- 테스트 옵션에 따라 세션 API 호출 스킵 확인
```

---

### 2. 전체 게임 플로우 테스트

실제 로그인, 세션 API, 녹음, 평가 응답까지 확인하려면 로그인 후 진행해야 합니다.

```text
Login
→ StageScene
→ QuestScene
→ GamePlayScene
→ NPC 클릭
→ 세션 시작
→ 녹음
→ Speech API 제출
→ 성공/실패/완료 반응 확인
```

이때는 아래 옵션을 꺼야 합니다.

```text
Skip Session Api For Interaction Test = false
```

주의 사항은 다음과 같습니다.

- 로그인 전에는 `TokenStore`에 accessToken이 없으므로 API 호출이 실패할 수 있습니다.
- 전체 플로우 테스트는 로그인/OAuth 설정이 완료된 뒤 Android 빌드에서 확인하는 것을 권장합니다.

---

## 주의 사항

- 기존 `InteractableNpc`와 새 `InteractionTarget` / `StageInteractionController`가 동시에 동작하지 않도록 주의합니다.
- 같은 NPC에 기존 클릭 처리와 새 클릭 처리가 동시에 연결되어 있으면 이벤트가 중복 실행될 수 있습니다.
- `GamePlayScene`만 단독 실행하면 로그인 토큰이 없기 때문에 세션 API 호출이 실패할 수 있습니다.
- 3D interaction만 테스트할 때는 `Skip Session Api For Interaction Test` 옵션을 사용합니다.
- 실제 앱 동작 테스트 시에는 로그인 후 진입하고, 테스트 옵션을 꺼야 합니다.
- WebGL 환경에서는 Unity `Microphone` 사용이 제한될 수 있으므로 녹음 포함 전체 플로우는 Android 빌드에서 확인하는 것을 권장합니다.
- 실제 access token이나 debug token 값을 씬/프리팹에 저장한 상태로 커밋하지 않습니다.