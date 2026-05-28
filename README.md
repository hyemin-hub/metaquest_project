# MetaQuest Project — 성벽 부수기 🏰

Unity 6 LTS + Meta Quest 기반 VR 게임 프로젝트.

---

## 📌 개발 환경

| 항목 | 버전 |
|------|------|
| Unity | **Unity 6 LTS** |
| 타겟 플랫폼 | Meta Quest |
| SDK | Meta XR All-in-One SDK |

> ⚠️ Unity 버전이 다르면 프로젝트가 안 열리거나 오류가 날 수 있어. 반드시 **Unity 6 LTS**로 맞춰줘.

---

## 🚀 처음 시작하는 사람용 세팅 가이드

> Mac과 Windows 명령어가 다른 부분이 있어서 OS별로 나눠뒀어. 본인 OS에 맞는 거 따라하면 돼.

### Step 1. Unity 6 LTS 설치

1. [Unity Hub 다운로드](https://unity.com/download)
2. Unity Hub 실행 → **Installs** 탭 → **Install Editor**
3. **Unity 6 LTS** 선택
4. 모듈 선택할 때 반드시 체크:
   - ✅ **Android Build Support**
   - ✅ **Android SDK & NDK Tools**
   - ✅ **OpenJDK**

> Meta Quest는 Android 기반이라서 Android Build Support 꼭 필요.

---

### Step 2. Git 설치 확인

터미널(Mac) 또는 명령 프롬프트/PowerShell(Windows)에서:

```bash
git --version
```

버전이 나오면 OK. `command not found` 같은 에러가 나면 설치 필요:

- **Mac:** 위 명령어 치면 자동으로 설치 팝업이 떠. 따라가면 됨.
- **Windows:** [Git for Windows](https://git-scm.com/download/win) 다운로드 후 설치 (기본 옵션 그대로 OK)

---

### Step 3. Git LFS 설치 ⚠️ **꼭 필요**

LFS 없으면 `.fbx`, `.psd`, `.wav` 같은 큰 파일이 **깨진 채로 받아져**. 반드시 설치하고 시작.

**Mac:**
```bash
# Homebrew 없으면 먼저 설치
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Git LFS 설치
brew install git-lfs
```

**Windows:**
- [git-lfs.com](https://git-lfs.com) 들어가서 다운로드 → 설치 (기본 옵션 그대로)

**Mac/Windows 공통 — 설치 후 한 번만 실행:**
```bash
git lfs install
```

`Git LFS initialized.` 메시지 뜨면 성공.

---

### Step 4. GitHub 권한 받기

레포에 push하려면 **collaborator 권한**이 필요해. 아직 못 받았으면 hyemin한테 본인 GitHub 아이디 알려주고 추가해달라고 요청해.

추가되면 이메일로 초대 메일 와. **수락**하면 끝.

---

### Step 5. 레포 클론

원하는 위치에서:

```bash
git clone https://github.com/hyemin-hub/metaquest_project.git
cd metaquest_project
```

> 클론할 때 Git이 자동으로 LFS 파일도 받아와. 시간이 좀 걸릴 수 있어.

---

### Step 6. Unity Hub에서 프로젝트 열기

1. Unity Hub 실행 → **Projects** 탭 → **Add** 버튼
2. 방금 클론한 `metaquest_project` 폴더 선택
3. 프로젝트 목록에 추가되면 클릭해서 열기

> 처음 열 때는 패키지 설치하느라 시간 좀 걸려 (5~10분). 기다려.

---

### Step 7. Unity 설정 확인

`Edit → Project Settings → Editor`에서 아래 두 개 확인:

| 항목 | 값 |
|------|------|
| **Asset Serialization → Mode** | Force Text |
| **Version Control → Mode** | Visible Meta Files |

이미 설정돼 있을 거야. 만약 다르면 위 값으로 바꿔.

---

### Step 8. UnityYAMLMerge (Smart Merge) 등록 ⚠️ **꼭 필요**

`.gitattributes`에 Smart Merge 설정은 잡혀있는데, **각자 로컬에서 한 번** mergetool 경로를 git에 알려줘야 실제로 동작해. 안 해두면 씬/프리팹 충돌 시 자동 머지 안 되고 그냥 텍스트 충돌 뜸.

**Mac:**
```bash
# Unity 버전은 본인이 설치한 버전에 맞게 수정 (예: 6000.0.30f1)
git config --global merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)"
git config --global merge.unityyamlmerge.driver '/Applications/Unity/Hub/Editor/<Unity버전>/Unity.app/Contents/Tools/UnityYAMLMerge merge -p %O %B %A %A'
git config --global merge.unityyamlmerge.recursive binary
```

**Windows (PowerShell):**
```powershell
git config --global merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)"
git config --global merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/<Unity버전>/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %B %A %A'
git config --global merge.unityyamlmerge.recursive binary
```

> `<Unity버전>` 자리는 본인이 깐 Unity 6 LTS 정확한 버전 이름으로 바꿔야 함. (예: `6000.0.30f1`) Unity Hub의 Installs 탭에서 확인.

---

### Step 9. Meta XR SDK 설치 (B 담당이면 필수, 나머지는 권장)

Meta XR All-in-One SDK는 라이선스 동의가 필요해서 `manifest.json`에 미리 못 박아둘 수 없음. 각자 Unity Package Manager에서 직접 설치:

1. Unity Editor 열기
2. `Window → Package Manager`
3. 좌상단 `+` 버튼 → `Add package by name...`
4. 입력: `com.meta.xr.sdk.all` → Add
5. (또는) Unity Asset Store에서 "Meta XR All-in-One SDK" 검색 → Import

> 기본 Unity XR Interaction Toolkit / Oculus XR Plugin은 이미 `manifest.json`에 들어가 있어서 자동 설치돼. Meta XR SDK는 컨트롤러 햅틱, 손 추적 같은 고급 기능 쓸 때 필요.

---

## 👥 팀원별 작업 분담

| 팀원 | 담당 영역 | 만드는 파일 | 테스트 씬 |
|------|------|------|------|
| **A** | 물리 / 성벽 | `WallBlock.cs`, 성벽 Prefab | `Test_A_Scene` |
| **B** | VR 인터랙션 / 투사체 | `Projectile.cs`, `ExplosiveProjectile.cs`, 공 Prefab | `Test_B_Scene` |
| **C** | 게임 매니저 / 가챠 | `GameManager.cs`, `GachaManager.cs` | `Test_C_Scene` |
| **D** | UI / 최종 통합 | `UIManager.cs`, MainScene 관리 | `MainScene` |

> A·B·C는 본인 Test 씬에서 작업하고 **스크립트 + 프리팹만** push. D가 MainScene에서 모두 통합.

---

## 🌿 브랜치 전략

```
main
 ├ feature/A-wall-physics       ← 팀원 A: 성벽 물리 시스템
 ├ feature/B-projectile         ← 팀원 B: VR 인터랙션 / 투사체
 ├ feature/C-managers           ← 팀원 C: GameManager / GachaManager
 └ feature/D-ui-integration     ← 팀원 D: UI / 최종 통합
```

**규칙:**
- `main`은 항상 빌드 가능한 상태로 유지 (직접 push 금지, PR만)
- 작업은 반드시 `feature/...` 브랜치에서
- 완료되면 PR(Pull Request) 올려서 코드 리뷰 후 merge
- 작업 단위가 크면 `feature/A-wall-physics-hp-system` 처럼 세부 이름 붙여도 OK

**브랜치 만들기:**
```bash
git checkout -b feature/A-내작업이름
```

---

## 📁 씬(Scene) 분리 규칙

같은 Scene을 두 명이 동시에 수정하면 **머지 지옥** 시작됨. 그래서 1인 1씬으로 분리:

| 씬 | 담당 | 용도 |
|------|------|------|
| `MainScene` | **D 전용** | 최종 통합. A·B·C는 절대 건드리지 말 것 |
| `Test_A_Scene` | A | 성벽 물리 테스트 |
| `Test_B_Scene` | B | VR 투사체 / 인터랙션 테스트 |
| `Test_C_Scene` | C | GameManager / Gacha 로직 테스트 |

> ⚠️ MainScene은 `CODEOWNERS`로 D 승인 없이는 머지 못 하게 보호돼 있음. 실수로라도 수정하지 말기.

---

## 📂 폴더 구조

```
Assets/
├── Scenes/         (MainScene + Test_A/B/C_Scene)
├── Scripts/
│   ├── Physics/    (WallBlock.cs 등 — A)
│   ├── Projectiles/(Projectile.cs 등 — B)
│   ├── Managers/   (GameManager.cs, GachaManager.cs — C)
│   └── UI/         (UIManager.cs — D)
├── Prefabs/
│   ├── Walls/      (성벽 블록 — A)
│   └── Projectiles/(공/폭탄 등 — B)
├── Materials/
│   └── Physics/    (나무/돌 Physic Material — A)
├── Audio/
│   ├── SFX/        (효과음 — 3번 담당이 채움)
│   └── BGM/        (배경음 — 3번 담당이 채움)
└── Models/         (Meshy로 뽑은 3D 모델)
```

> 빈 폴더에 `.gitkeep`이 들어있는데, 실제 파일이 들어가면 지워도 됨.

---

## ⚠️ 협업 5계명

1. **같은 Scene / Prefab 동시 수정 금지** — 작업 전에 슬랙/카톡으로 "나 ○○ 씬 수정한다" 공지
2. **자주 pull, 자주 commit** — 하루치 작업을 한꺼번에 올리면 충돌 폭발
3. **파일 이동은 반드시 Unity Editor 내부에서** — Finder/탐색기로 옮기면 `.meta` 꼬여서 참조 다 깨짐
4. **.meta 파일은 항상 같이 커밋** — 절대 빼먹지 말기
5. **큰 바이너리 파일은 신경 안 써도 됨** — `.gitattributes`가 자동으로 LFS로 보내줌

---

## 📦 작업 흐름 (매번 반복)

```bash
# 1. 작업 시작 전: 최신 상태로 받기
git checkout main
git pull origin main

# 2. 새 브랜치 만들기
git checkout -b feature/내작업이름

# 3. Unity에서 작업

# 4. 작업 끝나면 커밋
git add .
git commit -m "feat: 내가 한 작업 설명"

# 5. 원격에 올리기
git push -u origin feature/내작업이름

# 6. GitHub에서 Pull Request 생성 → 팀원에게 리뷰 요청
```

---

## 💬 커밋 메시지 컨벤션

| 접두어 | 용도 | 예시 |
|------|------|------|
| `feat:` | 새 기능 | `feat: 활 발사 시스템 추가` |
| `fix:` | 버그 수정 | `fix: 화살 충돌 판정 오류 수정` |
| `chore:` | 설정/잡일 | `chore: gitignore 업데이트` |
| `docs:` | 문서 | `docs: README 업데이트` |
| `refactor:` | 코드 정리 | `refactor: 발사체 코드 구조 변경` |

---

## 🆘 자주 발생하는 문제 & 해결법

### ❓ `git push`가 `permission denied`로 거부됨
→ Collaborator 권한 못 받은 상태. hyemin한테 요청.

### ❓ 클론했는데 `.fbx`, `.psd` 파일이 깨진 텍스트로 보임
→ Git LFS 설치 안 함. **Step 3** 다시 확인하고:
```bash
git lfs install
git lfs pull
```

### ❓ Scene 또는 Prefab 머지 충돌 떴을 때
1. 일단 패닉 금지
2. 슬랙/카톡에 충돌 났다고 공지
3. 가능하면 한쪽 버전을 살리고 나머지 작업은 다시 수동 적용
4. 정 안 되면 Unity의 **Smart Merge** (UnityYAMLMerge) 활용 — `.gitattributes`에 이미 설정돼 있음

### ❓ `.meta` 파일이 꼬였을 때
- 파일 이동을 Finder/탐색기로 한 경우 자주 발생
- **앞으로는 무조건 Unity Editor 안에서만 파일 이동**
- 이미 꼬였으면 → Unity가 재생성하긴 하지만 GUID가 바뀌어서 참조 깨질 수 있음. 슬랙/카톡에 공유하고 같이 복구

### ❓ Unity가 갑자기 느려지거나 이상해짐
가장 흔한 해결법:
1. Unity 종료
2. 프로젝트 폴더의 `Library/` 폴더 통째로 삭제 (걱정 마, Unity가 다시 만들어줌)
3. Unity 다시 열기 (재빌드 시간이 좀 걸려)

---

## 📚 추가 자료

- [Unity 공식 매뉴얼](https://docs.unity3d.com/Manual/index.html)
- [Meta XR SDK 문서](https://developer.oculus.com/documentation/unity/)
- [Git LFS 공식](https://git-lfs.com)

---

## 👥 팀원 명단

| 이름 | GitHub 아이디 | 담당 |
|------|------|------|
| (이름) | @아이디 | A — 물리 / 성벽 |
| (이름) | @아이디 | B — VR 인터랙션 / 투사체 |
| (이름) | @아이디 | C — 게임 매니저 / 가챠 |
| (이름) | @아이디 | D — UI / 최종 통합 |
| hyemin | @hyemin-hub | 협업 인프라 (GitHub 세팅) |

> 채우고 나면 `.github/CODEOWNERS`에 있는 `@팀원X-깃허브아이디` 자리도 실제 아이디로 바꿔주기.

---

막히면 슬랙/카톡에 바로 물어보기. 혼자 끙끙대지 말기 🙏
