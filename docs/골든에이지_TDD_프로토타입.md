# GoldenAge 프로토타입 - 기술 설계 문서 (TDD)

> **문서 버전**: 1.1
> **작성일**: 2026년 1월
> **대상 독자**: 초보 게임 개발자 (1인 개발)
> **프로토타입 범위**: 15~30분 플레이 가능한 데모

---

## 1. 프로젝트 개요

### 1.1 프로토타입 목표
- **핵심 검증 사항**: 과학-마법 융합 전투 시스템의 재미
- **플레이 시간**: 15~30분
- **맵 범위**: 1920년대 로어 맨해튼 1블록
- **구현 시스템**: 이동 + 전투 + 대화 (3개 핵심 시스템)

### 1.2 기술 스택 요약

| 항목 | 선택 | 버전 |
|------|------|------|
| **게임 엔진** | Unity | 2022.3 LTS |
| **렌더 파이프라인** | URP (Universal Render Pipeline) | - |
| **스크립팅 언어** | C# | .NET Standard 2.1 |
| **IDE** | Visual Studio 2022 또는 JetBrains Rider | - |
| **버전 관리** | Git + GitHub | - |
| **아트 스타일** | 스타일라이즈드 (Stylized) | - |

---

## 2. Unity 프로젝트 설정

### 2.1 새 프로젝트 생성

```
1. Unity Hub 실행
2. New Project 클릭
3. 템플릿: "3D (URP)" 선택
4. 프로젝트 이름: "GoldenAge_Prototype"
5. 저장 위치: C:\Users\user\projects\2026_active\golden_age\unity
```

### 2.2 프로젝트 설정 (Project Settings)

#### Player Settings
```
Company Name: YourStudioName
Product Name: GoldenAge Prototype
Version: 0.1.0

Resolution:
- Default Screen Width: 1920
- Default Screen Height: 1080
- Fullscreen Mode: Fullscreen Window

Scripting Backend: IL2CPP (빌드 시)
API Compatibility Level: .NET Standard 2.1
```

#### Quality Settings
```
프로토타입용 프리셋:
- Pixel Light Count: 4
- Texture Quality: Full Res
- Anisotropic Textures: Per Texture
- Anti Aliasing: 4x MSAA
- Soft Particles: Yes
- Shadows: Hard and Soft
- Shadow Resolution: High
```

#### Physics Settings
```
Gravity: (0, -9.81, 0)
Default Solver Iterations: 6
Default Solver Velocity Iterations: 1
Sleep Threshold: 0.005
Bounce Threshold: 2
```

### 2.3 URP 설정

#### URP Asset 설정
```
Assets/Settings/URP-HighFidelity.asset:

Rendering:
- Depth Texture: On
- Opaque Texture: On
- Opaque Downsampling: 2x Bilinear

Quality:
- HDR: On
- MSAA: 4x
- Render Scale: 1.0

Lighting:
- Main Light: Per Pixel
- Cast Shadows: On
- Shadow Resolution: 2048

Shadows:
- Max Distance: 50
- Cascade Count: 4
```

---

## 3. 폴더 구조

### 3.1 권장 폴더 구조

```
Assets/
├── _Project/                    # 프로젝트 전용 에셋
│   ├── Animations/             # 애니메이션 클립 및 컨트롤러
│   │   ├── Player/
│   │   ├── NPC/
│   │   └── Enemy/
│   ├── Audio/                  # 사운드 파일
│   │   ├── BGM/
│   │   ├── SFX/
│   │   └── Voice/
│   ├── Materials/              # 머티리얼
│   │   ├── Characters/
│   │   ├── Environment/
│   │   └── VFX/
│   ├── Models/                 # 3D 모델
│   │   ├── Characters/
│   │   ├── Props/
│   │   └── Environment/
│   ├── Prefabs/                # 프리팹
│   │   ├── Characters/
│   │   ├── Items/
│   │   ├── UI/
│   │   └── VFX/
│   ├── Scenes/                 # 씬 파일
│   │   ├── MainMenu.unity
│   │   ├── Gameplay.unity
│   │   └── Test/               # 테스트용 씬
│   ├── Scripts/                # C# 스크립트
│   │   ├── Core/               # 핵심 시스템
│   │   ├── Player/             # 플레이어 관련
│   │   ├── Combat/             # 전투 시스템
│   │   ├── Dialogue/           # 대화 시스템
│   │   ├── Quest/              # 퀘스트 시스템
│   │   ├── UI/                 # UI 관련
│   │   └── Utilities/          # 유틸리티
│   ├── Textures/               # 텍스처 파일
│   ├── UI/                     # UI 에셋
│   │   ├── Sprites/
│   │   └── Fonts/
│   └── Data/                   # 데이터 파일 (ScriptableObject)
│       ├── Items/
│       ├── Dialogues/
│       └── Quests/
├── Plugins/                    # 외부 플러그인
├── Settings/                   # URP, Input System 설정
└── StreamingAssets/            # 런타임 로드 파일
```

### 3.2 네이밍 컨벤션

| 유형 | 규칙 | 예시 |
|------|------|------|
| **폴더** | PascalCase | `Characters`, `Environment` |
| **씬** | PascalCase | `MainMenu.unity`, `Gameplay.unity` |
| **스크립트** | PascalCase | `PlayerController.cs`, `DialogueManager.cs` |
| **프리팹** | PascalCase + 접두사 | `Pref_Player.prefab`, `Pref_Enemy_Gangster.prefab` |
| **머티리얼** | PascalCase + 접두사 | `Mat_Brick_Red.mat` |
| **텍스처** | PascalCase + 접미사 | `Brick_Red_Diffuse.png`, `Brick_Red_Normal.png` |
| **애니메이션** | PascalCase | `Player_Idle.anim`, `Player_Run.anim` |
| **오디오** | PascalCase | `SFX_Gunshot.wav`, `BGM_Jazz_01.mp3` |

---

## 4. 아키텍처 설계

### 4.1 씬 구조

```
[씬 구성도]

MainMenu (씬)
    └── 메인 메뉴 UI, 설정, 게임 시작

Gameplay (씬) ← 프로토타입 메인 씬
    ├── Managers (빈 오브젝트)
    │   ├── GameManager
    │   ├── InputManager
    │   ├── DialogueManager
    │   ├── QuestManager
    │   ├── CombatManager
    │   └── AudioManager
    ├── Environment
    │   ├── Terrain
    │   ├── Buildings
    │   └── Props
    ├── Characters
    │   ├── Player
    │   ├── NPCs
    │   └── Enemies
    ├── UI
    │   ├── HUD Canvas
    │   ├── Dialogue Canvas
    │   └── Pause Menu Canvas
    └── Lighting
        ├── Directional Light
        └── Point/Spot Lights
```

### 4.2 매니저 시스템 (싱글톤 패턴)

프로토타입에서는 간단한 싱글톤 패턴을 사용합니다.

```csharp
// 기본 싱글톤 템플릿
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
```

### 4.3 매니저 역할 정의

| 매니저 | 역할 | 주요 메서드 |
|--------|------|------------|
| **GameManager** | 게임 상태 관리, 일시정지 | `PauseGame()`, `ResumeGame()`, `SaveGame()` |
| **InputManager** | 입력 처리 (New Input System) | `GetMovementInput()`, `GetActionInput()` |
| **DialogueManager** | 대화 시스템 제어 | `StartDialogue()`, `NextLine()`, `EndDialogue()` |
| **QuestManager** | 퀘스트 진행 관리 | `StartQuest()`, `UpdateQuest()`, `CompleteQuest()` |
| **CombatManager** | 전투 상태 및 데미지 계산 | `EnterCombat()`, `CalculateDamage()`, `ExitCombat()` |
| **AudioManager** | BGM, SFX 재생 | `PlayBGM()`, `PlaySFX()`, `StopAll()` |

### 4.4 게임 상태 머신

```csharp
public enum GameState
{
    MainMenu,       // 메인 메뉴
    Exploration,    // 자유 탐색
    Dialogue,       // 대화 중
    Combat,         // 전투 중
    Paused,         // 일시정지
    Cutscene        // 컷씬 재생
}

// GameManager에서 상태 관리
public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }

    public event System.Action<GameState> OnStateChanged;

    public void ChangeState(GameState newState)
    {
        if (CurrentState != newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
```

---

## 5. 핵심 기술 스택 상세

### 5.1 입력 시스템 (New Input System)

Unity의 New Input System 패키지를 사용합니다.

#### 설치 방법
```
1. Window > Package Manager
2. Unity Registry에서 "Input System" 검색
3. Install 클릭
4. Project Settings > Player > Active Input Handling을 "Both" 또는 "Input System Package" 선택
```

#### Input Actions 설정

```
Assets/Settings/PlayerInputActions.inputactions

Action Map: Player
├── Move (Value, Vector2)
│   └── WASD, Arrow Keys, Left Stick
├── Look (Value, Vector2)
│   └── Mouse Delta, Right Stick
├── Sprint (Button)
│   └── Left Shift, Left Trigger
├── Attack (Button)
│   └── Left Mouse, Right Trigger
├── Interact (Button)
│   └── E, A Button
├── Skill1 (Button) - 테슬라 충격기
│   └── Q, Left Bumper
├── Skill2 (Button) - 에테르 파동
│   └── R, Right Bumper
└── Pause (Button)
    └── Escape, Start Button
```

### 5.2 물리 시스템

#### 레이어 구성
```
Layer 0: Default
Layer 3: Player
Layer 6: Enemy
Layer 7: NPC
Layer 8: Interactable
Layer 9: Ground
Layer 10: Obstacle
Layer 11: Projectile
```

#### 충돌 매트릭스 (Physics > Layer Collision Matrix)
```
           Player  Enemy  NPC  Interactable  Ground  Obstacle  Projectile
Player       -      O      -       O           O        O          -
Enemy        O      -      -       -           O        O          O
NPC          -      -      -       -           O        O          -
Projectile   -      O      -       -           -        O          -
```

### 5.3 캐릭터 컨트롤러

프로토타입에서는 CharacterController 컴포넌트를 사용합니다.

```csharp
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // 구현 내용은 시스템 명세서 참조
}
```

### 5.4 카메라 시스템

Cinemachine 패키지를 사용하여 3인칭 카메라를 구현합니다.

#### 설치 방법
```
1. Window > Package Manager
2. Unity Registry에서 "Cinemachine" 검색
3. Install 클릭
```

#### 카메라 구성
```
Main Camera
└── CinemachineBrain (컴포넌트)

CM vcam_FreeLook (Cinemachine Free Look)
├── Follow: Player Transform
├── Look At: Player (Head 또는 Chest)
├── Top Rig: Height 4, Radius 5
├── Middle Rig: Height 2, Radius 7
└── Bottom Rig: Height 0.5, Radius 5

CM vcam_Dialogue (Cinemachine Virtual Camera)
├── Follow: Dialogue Target
└── Look At: NPC
```

---

## 6. 데이터 구조 설계

### 6.1 ScriptableObject 활용

데이터 주도 설계(Data-Driven Design)를 위해 ScriptableObject를 적극 활용합니다.

#### 캐릭터 데이터
```csharp
[CreateAssetMenu(fileName = "NewCharacter", menuName = "GoldenAge/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    public string characterName;
    public string description;
    public Sprite portrait;

    [Header("스탯")]
    public int maxHealth = 100;
    public int attackPower = 10;
    public int defense = 5;
    public float moveSpeed = 4f;

    [Header("전투")]
    public AttackData basicAttack;
    public SkillData[] skills;
}
```

#### 아이템 데이터
```csharp
[CreateAssetMenu(fileName = "NewItem", menuName = "GoldenAge/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public int maxStack = 1;
    public bool isConsumable;

    [Header("효과 (소모품인 경우)")]
    public int healAmount;
    public int energyRestore;
}

public enum ItemType
{
    Weapon,
    Consumable,
    Quest,
    Key
}
```

#### 대화 데이터
```csharp
[CreateAssetMenu(fileName = "NewDialogue", menuName = "GoldenAge/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string dialogueID;
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public Sprite speakerPortrait;
    [TextArea(3, 5)]
    public string text;
    public DialogueChoice[] choices; // null이면 자동 진행
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public DialogueData nextDialogue;
    public string questToStart;     // 선택 시 시작할 퀘스트 ID
    public string flagToSet;        // 선택 시 설정할 플래그
}
```

#### 퀘스트 데이터
```csharp
[CreateAssetMenu(fileName = "NewQuest", menuName = "GoldenAge/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea(3, 5)]
    public string description;

    public QuestObjective[] objectives;

    [Header("보상")]
    public int expReward;
    public ItemData[] itemRewards;
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveID;
    public string description;
    public ObjectiveType type;
    public string targetID;         // 대상 ID (적, NPC, 아이템 등)
    public int requiredCount = 1;
}

public enum ObjectiveType
{
    KillEnemy,      // 적 처치
    TalkToNPC,      // NPC와 대화
    CollectItem,    // 아이템 수집
    ReachLocation   // 위치 도달
}
```

### 6.2 런타임 데이터 구조

#### 플레이어 저장 데이터
```csharp
[System.Serializable]
public class PlayerSaveData
{
    // 기본 정보
    public string playerName;
    public int currentHealth;
    public int maxHealth;
    public int level;
    public int experience;

    // 위치
    public Vector3Serializable position;
    public Vector3Serializable rotation;

    // 인벤토리
    public List<InventorySlotData> inventory;

    // 퀘스트 진행
    public List<QuestProgressData> questProgress;

    // 게임 플래그
    public List<string> unlockedFlags;
}

[System.Serializable]
public struct Vector3Serializable
{
    public float x, y, z;

    public Vector3Serializable(Vector3 v)
    {
        x = v.x; y = v.y; z = v.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}
```

---

## 7. VFX/파티클 시스템

### 7.1 Unity Particle System 기본 설정

프로토타입에서 사용할 파티클 시스템의 기본 구조입니다.

#### Particle System 컴포넌트 구조
```
[Particle System 계층 구조]

ParticleSystem (Root)
├── Main Module         → 기본 설정 (수명, 속도, 크기)
├── Emission Module     → 방출량 설정
├── Shape Module        → 방출 형태 (구, 원뿔, 박스)
├── Velocity Module     → 속도/방향 제어
├── Color Module        → 색상 변화
├── Size Module         → 크기 변화
├── Rotation Module     → 회전
├── Noise Module        → 불규칙한 움직임
├── Collision Module    → 충돌 처리
├── Sub Emitters        → 하위 이미터
├── Texture Sheet       → 스프라이트 애니메이션
├── Lights Module       → 파티클 조명
├── Trails Module       → 궤적 효과
└── Renderer            → 렌더링 설정
```

### 7.2 핵심 이펙트 설정 가이드

#### 테슬라 충격기 이펙트
```csharp
// VFX_Tesla_Shock 프리팹 설정
// 위치: Assets/_Project/Prefabs/VFX/VFX_Tesla_Shock.prefab

[파티클 시스템 설정]

Duration: 1.0
Looping: false
Start Lifetime: 0.3 ~ 0.5
Start Speed: 5 ~ 15
Start Size: 0.1 ~ 0.3
Start Color: #00BFFF (Electric Blue)
Gravity Modifier: 0
Simulation Space: World

[Emission]
Rate over Time: 0
Bursts:
  - Time: 0, Count: 50, Cycles: 1

[Shape]
Shape: Sphere
Radius: 0.5
Emit from: Volume

[Color over Lifetime]
Gradient: White → Electric Blue → Fade

[Size over Lifetime]
Curve: 1.0 → 0.0 (shrink)

[Renderer]
Render Mode: Billboard
Material: Particles/Additive
Sort Mode: By Distance
```

#### 에테르 파동 이펙트
```csharp
// VFX_Ether_Wave 프리팹 설정

[Main]
Duration: 0.8
Start Lifetime: 0.5
Start Speed: 20
Start Size: 0.5
Start Color: #8B008B (Purple)

[Shape]
Shape: Cone
Angle: 15
Radius: 0.3

[Color over Lifetime]
Gradient: Purple → Pink → White → Fade

[Trails]
Enabled: true
Ratio: 1.0
Lifetime: 0.3
Width over Trail: 1.0 → 0.0
Color over Trail: Match particle
```

#### 융합 콤보 이펙트 (차원 전격)
```csharp
// VFX_Fusion_Blast 프리팹 설정 (복합 구조)

[Parent: VFX_Fusion_Blast]
├── Core (중심 폭발)
│   Duration: 0.3
│   Emission: Burst 200
│   Shape: Sphere (radius 1)
│   Color: Gold (#FFD700)
│
├── Shockwave (충격파 링)
│   Mesh: Torus
│   Scale over Lifetime: 0 → 16
│   Color: Gold → Transparent
│
├── Sparks (스파크)
│   Duration: 1.0
│   Emission: Burst 100
│   Shape: Sphere (radius 8)
│   Velocity: Outward 10
│   Gravity: 2
│
└── Distortion (왜곡) [선택]
    Shader: Screen distortion
    Scale: Pulse effect
```

### 7.3 VFX 코드 통합

```csharp
// VFXManager.cs - 이펙트 스폰 및 관리

using UnityEngine;
using System.Collections.Generic;

public class VFXManager : Singleton<VFXManager>
{
    [Header("이펙트 프리팹")]
    [SerializeField] private GameObject teslaShockVFX;
    [SerializeField] private GameObject etherWaveVFX;
    [SerializeField] private GameObject fusionBlastVFX;
    [SerializeField] private GameObject hitVFX;

    // 오브젝트 풀링 (성능 최적화)
    private Dictionary<string, Queue<ParticleSystem>> vfxPools = new();

    public void SpawnVFX(string vfxName, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = GetPrefab(vfxName);
        if (prefab == null) return;

        // 풀에서 가져오기 또는 새로 생성
        ParticleSystem ps = GetFromPool(vfxName);
        if (ps == null)
        {
            GameObject obj = Instantiate(prefab, position, rotation);
            ps = obj.GetComponent<ParticleSystem>();
        }
        else
        {
            ps.transform.position = position;
            ps.transform.rotation = rotation;
            ps.gameObject.SetActive(true);
        }

        ps.Play();

        // 재생 완료 후 풀로 반환
        StartCoroutine(ReturnToPool(ps, vfxName, ps.main.duration + 0.5f));
    }

    private GameObject GetPrefab(string name)
    {
        return name switch
        {
            "TeslaShock" => teslaShockVFX,
            "EtherWave" => etherWaveVFX,
            "FusionBlast" => fusionBlastVFX,
            "Hit" => hitVFX,
            _ => null
        };
    }

    private ParticleSystem GetFromPool(string name)
    {
        if (vfxPools.TryGetValue(name, out var pool) && pool.Count > 0)
        {
            return pool.Dequeue();
        }
        return null;
    }

    private System.Collections.IEnumerator ReturnToPool(ParticleSystem ps, string name, float delay)
    {
        yield return new WaitForSeconds(delay);

        ps.Stop();
        ps.gameObject.SetActive(false);

        if (!vfxPools.ContainsKey(name))
            vfxPools[name] = new Queue<ParticleSystem>();

        vfxPools[name].Enqueue(ps);
    }
}
```

### 7.4 셰이더 노트

```
[권장 셰이더]

1. 파티클 기본: Universal Render Pipeline/Particles/Unlit
   - Additive: 밝은 이펙트 (전기, 폭발)
   - Alpha Blended: 연기, 안개

2. 왜곡 효과: Custom Distortion Shader
   - 융합 콤보 충격파에 사용
   - Screen Space Distortion

3. 차원 균열: Custom Shader
   - Dissolve + Noise
   - 보라색 이미션 + 검은 코어
```

---

## 8. 테스트 전략

### 8.1 테스트 유형

```
[테스트 피라미드]

        △ E2E 테스트 (수동 플레이 테스트)
       ╱ ╲
      ╱   ╲ 통합 테스트 (시스템 간 연동)
     ╱     ╲
    ╱       ╲ 단위 테스트 (개별 컴포넌트)
   ──────────
```

### 8.2 단위 테스트 가이드

Unity Test Framework를 사용합니다.

#### 설치
```
1. Window > Package Manager
2. Unity Registry에서 "Test Framework" 검색
3. Install 클릭
```

#### 테스트 폴더 구조
```
Assets/_Project/Tests/
├── EditMode/           → Editor에서 실행 (빠름)
│   ├── PlayerStatsTests.cs
│   ├── DamageCalculationTests.cs
│   └── QuestProgressTests.cs
└── PlayMode/           → 런타임에서 실행 (느림)
    ├── PlayerMovementTests.cs
    ├── CombatSystemTests.cs
    └── DialogueSystemTests.cs
```

#### 단위 테스트 예시

```csharp
// Tests/EditMode/DamageCalculationTests.cs
using NUnit.Framework;

public class DamageCalculationTests
{
    [Test]
    public void CalculateDamage_BasicAttack_ReturnsCorrectValue()
    {
        // Arrange
        int baseDamage = 10;
        int attackerLevel = 1;
        int defenderDefense = 3;

        // Act
        int result = DamageCalculator.Calculate(baseDamage, attackerLevel, defenderDefense);

        // Assert
        Assert.AreEqual(7, result); // 10 - 3 = 7
    }

    [Test]
    public void CalculateDamage_HighDefense_ReturnsMinimumDamage()
    {
        // Arrange
        int baseDamage = 5;
        int attackerLevel = 1;
        int defenderDefense = 10;

        // Act
        int result = DamageCalculator.Calculate(baseDamage, attackerLevel, defenderDefense);

        // Assert
        Assert.AreEqual(1, result); // 최소 데미지는 1
    }

    [Test]
    public void CalculateDamage_LevelBonus_AppliesCorrectly()
    {
        // Arrange
        int baseDamage = 10;
        int attackerLevel = 3; // +20% 보너스
        int defenderDefense = 0;

        // Act
        int result = DamageCalculator.Calculate(baseDamage, attackerLevel, defenderDefense);

        // Assert
        Assert.AreEqual(12, result); // 10 * 1.2 = 12
    }
}
```

```csharp
// Tests/PlayMode/PlayerMovementTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerMovementTests
{
    private GameObject player;
    private PlayerMovement movement;

    [SetUp]
    public void SetUp()
    {
        // 테스트용 플레이어 생성
        player = new GameObject("TestPlayer");
        player.AddComponent<CharacterController>();
        movement = player.AddComponent<PlayerMovement>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(player);
    }

    [UnityTest]
    public IEnumerator PlayerMovement_IsMoving_ReturnsFalseWhenIdle()
    {
        yield return null; // 1프레임 대기

        Assert.IsFalse(movement.IsMoving());
    }

    [UnityTest]
    public IEnumerator PlayerMovement_GetCurrentSpeed_ReturnsZeroWhenIdle()
    {
        yield return null;

        Assert.AreEqual(0f, movement.GetCurrentSpeed());
    }
}
```

### 8.3 플레이 테스트 체크리스트

#### 핵심 기능 테스트

```
[이동 시스템]
□ WASD 이동이 부드럽게 작동하는가?
□ Shift 달리기가 정상 작동하는가?
□ 카메라가 캐릭터를 부드럽게 따라가는가?
□ 경사면/계단에서 이동이 자연스러운가?
□ 벽/장애물 충돌이 정상 작동하는가?

[전투 시스템]
□ 좌클릭 근접 공격이 발동되는가?
□ Q키 테슬라 충격기가 발동되는가?
□ R키 에테르 파동이 발동되는가?
□ 융합 콤보가 조건 충족 시 발동되는가?
□ 쿨다운이 정상 작동하는가?
□ 적이 데미지를 받고 사망하는가?
□ 플레이어가 데미지를 받는가?

[대화 시스템]
□ NPC에게 E키로 대화를 시작할 수 있는가?
□ 대화 텍스트가 정상 표시되는가?
□ 선택지가 정상 작동하는가?
□ 대화 종료가 정상 작동하는가?

[퀘스트 시스템]
□ 퀘스트가 정상적으로 시작되는가?
□ 목표 진행이 추적되는가?
□ 퀘스트 완료 시 보상이 지급되는가?
□ 퀘스트 UI가 정상 표시되는가?

[인벤토리]
□ 아이템을 획득할 수 있는가?
□ 인벤토리 UI가 정상 표시되는가?
□ 소모품을 사용할 수 있는가?

[저장/불러오기]
□ 게임 저장이 정상 작동하는가?
□ 저장된 게임을 불러올 수 있는가?
□ 불러온 데이터가 정확한가?
```

#### 버그 리포트 템플릿

```
[버그 제목]: 간단한 설명

[재현 단계]:
1. ...
2. ...
3. ...

[예상 결과]: 어떻게 작동해야 하는가

[실제 결과]: 실제로 무슨 일이 일어났는가

[발생 빈도]: 항상 / 가끔 / 드물게

[환경]:
- Unity 버전:
- 플랫폼:
- 빌드 번호:

[스크린샷/영상]: (있다면 첨부)
```

---

## 9. 디버그 도구

### 9.1 Unity Console 활용

#### Debug 클래스 활용
```csharp
// 기본 로그
Debug.Log("일반 메시지");
Debug.LogWarning("경고 메시지");
Debug.LogError("에러 메시지");

// 컨텍스트 로그 (오브젝트 클릭 시 하이라이트)
Debug.Log("플레이어 상태", gameObject);

// 조건부 로그
Debug.Assert(health > 0, "체력이 0 이하입니다!");

// 포맷 로그
Debug.LogFormat("플레이어 위치: {0}, 체력: {1}", transform.position, health);
```

#### 커스텀 로그 시스템
```csharp
// DebugLogger.cs
public static class DebugLogger
{
    public static bool EnableCombatLog = true;
    public static bool EnableMovementLog = false;
    public static bool EnableQuestLog = true;

    public static void LogCombat(string message)
    {
        if (EnableCombatLog)
            Debug.Log($"[전투] {message}");
    }

    public static void LogMovement(string message)
    {
        if (EnableMovementLog)
            Debug.Log($"[이동] {message}");
    }

    public static void LogQuest(string message)
    {
        if (EnableQuestLog)
            Debug.Log($"[퀘스트] {message}");
    }
}

// 사용 예시
DebugLogger.LogCombat($"데미지 {damage} 적용, 남은 HP: {currentHealth}");
```

### 9.2 Debug.DrawRay / DrawLine 활용

```csharp
// 시각적 디버깅 (Scene 뷰에서만 표시)

// Ray 그리기 (공격 방향 확인)
Debug.DrawRay(transform.position, transform.forward * attackRange, Color.red, 1f);

// Line 그리기 (경로 확인)
Debug.DrawLine(startPoint, endPoint, Color.green, 2f);

// 실전 예시: PlayerCombat에서 공격 범위 시각화
private void Update()
{
    #if UNITY_EDITOR
    // 근접 공격 범위 표시
    Debug.DrawRay(attackPoint.position, transform.forward * meleeRange, Color.red);

    // AOE 범위 표시 (원형)
    for (int i = 0; i < 36; i++)
    {
        float angle = i * 10 * Mathf.Deg2Rad;
        float nextAngle = (i + 1) * 10 * Mathf.Deg2Rad;

        Vector3 start = attackPoint.position + new Vector3(
            Mathf.Cos(angle) * aoeRadius, 0, Mathf.Sin(angle) * aoeRadius);
        Vector3 end = attackPoint.position + new Vector3(
            Mathf.Cos(nextAngle) * aoeRadius, 0, Mathf.Sin(nextAngle) * aoeRadius);

        Debug.DrawLine(start, end, Color.yellow);
    }
    #endif
}
```

### 9.3 Gizmos 활용 (에디터 전용)

```csharp
// 에디터에서 시각적 도우미 그리기
private void OnDrawGizmos()
{
    // 기본 (항상 표시)
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRange);
}

private void OnDrawGizmosSelected()
{
    // 선택 시에만 표시
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(attackPoint.position, attackRange);

    // 공격 방향
    Gizmos.color = Color.blue;
    Gizmos.DrawLine(attackPoint.position, attackPoint.position + transform.forward * 5f);
}
```

### 9.4 디버그 UI (인게임)

```csharp
// DebugUI.cs - 런타임 디버그 정보 표시
public class DebugUI : MonoBehaviour
{
    [SerializeField] private bool showDebugInfo = true;

    private PlayerStats playerStats;
    private PlayerMovement playerMovement;

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        GUILayout.Box("=== Debug Info ===");

        // 플레이어 정보
        if (playerStats != null)
        {
            GUILayout.Label($"HP: {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
            GUILayout.Label($"Level: {playerStats.Level}");
            GUILayout.Label($"EXP: {playerStats.Experience}/{playerStats.ExperienceToNextLevel}");
        }

        // 이동 정보
        if (playerMovement != null)
        {
            GUILayout.Label($"Speed: {playerMovement.GetCurrentSpeed():F1} m/s");
            GUILayout.Label($"Moving: {playerMovement.IsMoving()}");
            GUILayout.Label($"Running: {playerMovement.IsRunning()}");
        }

        // 전투 정보
        GUILayout.Label($"In Combat: {CombatManager.Instance?.IsInCombat}");
        GUILayout.Label($"Active Enemies: {CombatManager.Instance?.ActiveEnemyCount}");

        // FPS
        GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F0}");

        GUILayout.EndArea();
    }

    // F1 키로 토글
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            showDebugInfo = !showDebugInfo;
    }
}
```

### 9.5 디버그 단축키

```
[권장 디버그 단축키]

F1: 디버그 UI 토글
F2: 무적 모드 토글
F3: 시간 배속 (1x → 2x → 0.5x)
F4: 적 즉사 (마우스 위치)
F5: 빠른 저장
F9: 빠른 불러오기
~: 콘솔 열기 (구현 시)
```

```csharp
// DebugCommands.cs
public class DebugCommands : MonoBehaviour
{
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool godMode = false;

    private void Update()
    {
        // F2: 무적 모드
        if (Input.GetKeyDown(KeyCode.F2))
        {
            godMode = !godMode;
            Debug.Log($"무적 모드: {godMode}");
        }

        // F3: 시간 배속
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Time.timeScale = Time.timeScale switch
            {
                1f => 2f,
                2f => 0.5f,
                _ => 1f
            };
            Debug.Log($"시간 배속: {Time.timeScale}x");
        }

        // F4: 마우스 위치 적 즉사
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider.TryGetComponent<EnemyAI>(out var enemy))
                {
                    enemy.TakeDamage(9999, DamageType.Physical);
                    Debug.Log("적 즉사!");
                }
            }
        }
    }
    #endif
}
```

---

## 10. 성능 목표 및 최적화 전략

### 7.1 타겟 성능

| 항목 | 최소 목표 | 권장 목표 |
|------|---------|---------|
| **프레임레이트** | 30 FPS | 60 FPS |
| **해상도** | 1280x720 | 1920x1080 |
| **로딩 시간** | 10초 이내 | 5초 이내 |
| **메모리 사용** | 4GB 이하 | 2GB 이하 |

### 7.2 타겟 하드웨어

```
최소 사양:
- OS: Windows 10
- CPU: Intel Core i5-6600 / AMD Ryzen 3 1200
- RAM: 8GB
- GPU: NVIDIA GTX 960 / AMD RX 470
- Storage: 5GB

권장 사양:
- OS: Windows 10/11
- CPU: Intel Core i7-9700 / AMD Ryzen 5 3600
- RAM: 16GB
- GPU: NVIDIA GTX 1660 / AMD RX 5600 XT
- Storage: 5GB SSD
```

### 7.3 최적화 전략

#### 드로우콜 최적화
```
1. Static Batching
   - 움직이지 않는 오브젝트에 Static 플래그 설정

2. GPU Instancing
   - 반복되는 오브젝트 (가로등, 벤치 등)에 GPU Instancing 활성화

3. LOD (Level of Detail)
   - 건물, 소품에 LOD Group 설정
   - LOD0: ~10m, LOD1: 10-30m, LOD2: 30m+
```

#### 텍스처 최적화
```
Import Settings:
- Max Size: 2048 (캐릭터), 1024 (소품), 512 (원거리 배경)
- Compression: Normal (BC7), DXT1/DXT5
- Generate Mip Maps: Yes
- Aniso Level: 4-8
```

#### 오디오 최적화
```
Import Settings:
- BGM: Streaming, Vorbis, Quality 70%
- SFX (긴 것): Compressed In Memory, Vorbis
- SFX (짧은 것): Decompress On Load, ADPCM
```

### 7.4 프로파일링 도구

```
1. Unity Profiler (Window > Analysis > Profiler)
   - CPU, GPU, Memory 사용량 모니터링

2. Frame Debugger (Window > Analysis > Frame Debugger)
   - 드로우콜 분석

3. Memory Profiler (Package Manager에서 설치)
   - 상세 메모리 분석
```

---

## 8. 개발 환경 설정

### 8.1 Git 설정

#### .gitignore 파일
```
# Unity generated
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Mm]emoryCaptures/
[Rr]ecordings/

# IDE
.vs/
.idea/
*.csproj
*.unityproj
*.sln
*.suo
*.user
*.userprefs
*.pidb
*.booproj

# OS generated
.DS_Store
.DS_Store?
Thumbs.db

# Builds
*.apk
*.aab
*.exe

# Crashlytics
crashlytics-build.properties
```

#### .gitattributes 파일
```
# Unity YAML
*.unity text
*.prefab text
*.asset text
*.meta text
*.mat text
*.anim text
*.controller text

# Binary files
*.png binary
*.jpg binary
*.fbx binary
*.wav binary
*.mp3 binary
```

### 8.2 Visual Studio 설정

#### 권장 확장
```
1. Unity Snippets - Unity 코드 스니펫
2. C# Extensions - 리팩토링 도구
3. GitLens - Git 히스토리 시각화
```

#### 코드 스타일 (.editorconfig)
```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# Naming conventions
dotnet_naming_rule.private_fields_should_be_camel_case.severity = suggestion
dotnet_naming_rule.private_fields_should_be_camel_case.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case.style = camel_case_style

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.camel_case_style.capitalization = camel_case
```

---

## 9. 빌드 및 배포

### 9.1 빌드 설정

```
File > Build Settings

Platform: PC, Mac & Linux Standalone
Target Platform: Windows
Architecture: x86_64

Player Settings:
- Scripting Backend: IL2CPP (릴리즈), Mono (개발/테스트)
- Api Compatibility Level: .NET Standard 2.1
- Managed Stripping Level: Medium (릴리즈)
```

### 9.2 빌드 자동화 (선택사항)

```csharp
// Editor/BuildScript.cs
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        string[] scenes = { "Assets/_Project/Scenes/MainMenu.unity",
                           "Assets/_Project/Scenes/Gameplay.unity" };

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Windows/GoldenAge.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        // 결과 로깅
    }
}
```

---

## 10. 다음 단계

본 TDD 문서 작성이 완료되면 다음 문서로 진행합니다:

1. **시스템 명세서**: 이동, 전투, 대화 시스템의 상세 구현
2. **레벨 디자인 문서**: 1블록 맵 설계
3. **애셋 리스트**: 필요한 3D 모델, 텍스처, 사운드 목록

---

## 부록: 유용한 리소스

### 공식 문서
- [Unity Manual](https://docs.unity3d.com/Manual/index.html)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/index.html)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/index.html)

### 학습 자료
- [Unity Learn](https://learn.unity.com/) - 공식 무료 강좌
- [Brackeys YouTube](https://www.youtube.com/c/Brackeys) - 초보자 친화적 튜토리얼
- [Sebastian Lague](https://www.youtube.com/c/SebastianLague) - 고급 프로그래밍

### 커뮤니티
- [Unity Forums](https://forum.unity.com/)
- [r/Unity3D](https://www.reddit.com/r/Unity3D/)
- [인디 게임 개발자 한국 커뮤니티](https://cafe.naver.com/indiedevkr)

---

*본 문서는 GoldenAge 프로토타입 개발을 위한 기술 설계 문서입니다.*
