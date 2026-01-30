# GoldenAge Prototype

1920년대 뉴욕을 배경으로 한 과학-마법 융합 액션 RPG 프로토타입

## 프로젝트 개요

- **장르**: 3인칭 액션 RPG
- **배경**: 1920년대 뉴욕 로어 맨해튼
- **플레이 시간**: 15~30분 (프로토타입)
- **엔진**: Unity 2022.3 LTS (URP)

## 핵심 특징

- **과학-마법 융합 전투**: 테슬라 충격기 + 에테르 파동 = 차원 전격 콤보
- **1920년대 분위기**: 재즈 에이지, 스피크이지, 아르데코 스타일
- **미스터리 퀘스트**: 차원 균열과 관련된 기이한 사건 조사

## 폴더 구조

```
golden_age/
├── docs/                           # 기획 문서
│   ├── 골든에이지_TDD_프로토타입.md
│   ├── 골든에이지_시스템명세_프로토타입.md
│   ├── 골든에이지_레벨디자인_프로토타입.md
│   ├── 골든에이지_애셋리스트_프로토타입.md
│   └── 에셋_수집_가이드.md
└── unity/                          # Unity 프로젝트
    └── Assets/_Project/
        ├── Scripts/                # C# 스크립트
        ├── Prefabs/               # 프리팹
        ├── Scenes/                # 씬
        ├── Data/                  # ScriptableObject
        └── ...
```

## 시작하기

### 요구 사항

- Unity 2022.3 LTS 이상
- Universal Render Pipeline (URP)
- Input System Package

### 설치

1. 이 저장소를 클론합니다:
   ```bash
   git clone https://github.com/yonghwan1106/goldenage-prototype.git
   ```

2. Unity Hub에서 `unity` 폴더를 프로젝트로 추가합니다.

3. Unity 2022.3 LTS 버전으로 프로젝트를 엽니다.

4. Package Manager에서 필요한 패키지를 설치합니다:
   - Input System
   - Cinemachine
   - TextMeshPro

## 조작법

| 키 | 동작 |
|----|------|
| WASD | 이동 |
| Shift | 달리기 |
| 좌클릭 | 근접 공격 |
| Q | 테슬라 충격기 |
| R | 에테르 파동 |
| E | 상호작용 |
| ESC | 일시정지 |

## 개발 진행 상황

- [x] 기획 문서 작성
- [x] 프로젝트 구조 설정
- [x] 핵심 스크립트 작성
- [ ] 에셋 수집 및 적용
- [ ] 레벨 제작
- [ ] 밸런싱 및 테스트

## 기술 스택

- **엔진**: Unity 2022.3 LTS
- **렌더링**: Universal Render Pipeline (URP)
- **언어**: C# (.NET Standard 2.1)
- **입력**: New Input System

## 문서

- [기술 설계 문서 (TDD)](docs/골든에이지_TDD_프로토타입.md)
- [시스템 명세서](docs/골든에이지_시스템명세_프로토타입.md)
- [레벨 디자인 문서](docs/골든에이지_레벨디자인_프로토타입.md)
- [애셋 리스트](docs/골든에이지_애셋리스트_프로토타입.md)

## 라이선스

이 프로젝트는 개인 학습 및 포트폴리오 목적으로 제작되었습니다.

---

🤖 Generated with [Claude Code](https://claude.ai/claude-code)
