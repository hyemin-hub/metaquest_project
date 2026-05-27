# MetaQuest Project — 성벽 부수기

Unity 6 LTS + Meta Quest 기반 VR 게임 프로젝트.

---

## 🛠 개발 환경

- **Unity 버전:** Unity 6 LTS
- **타겟 플랫폼:** Meta Quest
- **SDK:** Meta XR All-in-One SDK

---

## 🚀 처음 클론한 사람을 위한 세팅

### 1. Git LFS 설치

```bash
# Mac
brew install git-lfs

# Windows
# https://git-lfs.com 에서 다운로드 후 설치
```

설치 후 한 번만 실행:

```bash
git lfs install
```

### 2. 레포 클론

```bash
git clone https://github.com/hyemin-hub/metaquest_project.git
cd metaquest_project
```

### 3. Unity Hub에서 프로젝트 열기

`Add → Open existing project`로 클론한 폴더 선택.
Unity 6 LTS 버전으로 열어야 함.

---

## ⚙️ Unity 필수 설정 (이미 적용돼 있음)

- **Asset Serialization:** Force Text
- **Version Control:** Visible Meta Files

> 새로 합류한 팀원도 위 설정이 켜져 있는지 확인.
> `Edit → Project Settings → Editor`에서 확인 가능.

---

## 🌿 브랜치 전략

```
main
 ├ feature/wall-destruction   ← 성벽 파괴 시스템
 ├ feature/projectile         ← 발사체 / 투척 메커니즘
 ├ feature/ui                 ← UI
 └ feature/scene-setup        ← 씬 구성
```

- `main`은 항상 빌드 가능한 상태 유지
- 작업은 `feature/...` 브랜치에서
- PR 통해서 merge

---

## 📁 씬(Scene) 분리 규칙

같은 Scene을 두 명이 수정하면 머지 지옥. 그래서 Additive로 분리:

- `MainScene` — 진입점
- `GameplayScene` — 성벽 / 발사체 / 게임 로직
- `UIScene` — UI 전용
- `LightingScene` — 조명 / 환경

---

## ⚠️ 협업 규칙

1. **같은 Scene / Prefab 동시 수정 금지** — 작업 전에 슬랙/카톡으로 공지
2. **자주 pull, 자주 commit** — 오래 묵히면 충돌 폭발
3. **파일 이동은 반드시 Unity Editor 내부에서** — Finder로 옮기면 .meta 꼬임
4. **.meta 파일은 항상 같이 커밋**
5. **큰 바이너리 파일(`.fbx`, `.psd`, `.wav` 등)은 자동으로 LFS로 관리됨** — `.gitattributes` 참고

---

## 📦 LFS로 관리되는 파일

`.gitattributes`에 명시된 확장자는 자동으로 Git LFS로 업로드됨:

- 3D 모델: `.fbx`, `.obj`, `.blend`, `.glb`, `.gltf` 등
- 이미지: `.psd`, `.png`, `.jpg`, `.exr` 등
- 오디오: `.mp3`, `.wav`, `.ogg` 등
- 비디오: `.mp4`, `.mov` 등

---

## 🆘 자주 발생하는 문제

### Scene/Prefab 머지 충돌이 떴을 때

Unity Smart Merge(UnityYAMLMerge) 도구가 자동으로 시도함 (`.gitattributes`에 설정됨).
그래도 안 되면 한쪽 버전을 선택하고 나머지를 다시 적용.

### .meta 파일이 꼬였을 때

- Finder가 아니라 **Unity 내부에서만** 파일 이동
- 누락된 .meta는 Unity가 다시 생성해주지만 GUID가 바뀌어서 참조가 깨질 수 있음
