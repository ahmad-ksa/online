# دليل إنشاء UI Scenes 🎨

## المشاهد المطلوبة

### 1️⃣ LoginScene
**المسار:** `Assets/Scenes/LoginScene.unity`

#### الخطوات:
```
1. File → New Scene → Save as "LoginScene"

2. إنشاء Canvas:
   - Right Click → UI → Panel
   - Rename: "MainCanvas"
   - Set Anchor: Stretch, Stretch
   - Color: Dark gray

3. إنشاء عناصر Login:
   - Create UI → Panel → "LoginPanel"
   - Background Image (Semi-transparent)
   - Add child elements:

     a. Title Text
        - Text: "تسجيل الدخول"
        - Font Size: 40
        - Alignment: Center
        - Pos Y: 200

     b. Email Input
        - InputField component
        - Placeholder: "البريد الإلكتروني"
        - Pos Y: 100
        - Height: 50

     c. Password Input
        - InputField component
        - Content Type: Password
        - Placeholder: "كلمة المرور"
        - Pos Y: 20
        - Height: 50

     d. Login Button
        - Button component
        - Text: "دخول"
        - Button Color: Green
        - Pos Y: -80

     e. Register Button
        - Button component
        - Text: "إنشاء حساب جديد"
        - Button Color: Blue
        - Pos Y: -140

4. إنشاء Status Panel (أعلى اليمين):
   - Panel: "StatusPanel"
   - Pos: Top Right
   - Width: 300, Height: 50
   
   a. Connection Status Text
      - Text: "جاري الاتصال..."
      - Color: Yellow

5. إنشاء Loading Panel:
   - Panel: "LoadingPanel"
   - Background: Black (Alpha: 0.5)
   - Add Spinner/Loading Animation
   - Text: "جاري التحميل..."
   - Initially: Inactive (Alpha: 0)
```

#### الـ Components:
```csharp
LoginScene Hierarchy:
├─ MainCanvas
│  ├─ LoginPanel
│  │  ├─ Title (Text)
│  │  ├─ EmailInput (InputField)
│  │  ├─ PasswordInput (InputField)
│  │  ├─ LoginButton (Button)
│  │  └─ RegisterButton (Button)
│  ├─ StatusPanel
│  │  └─ ConnectionStatusText (Text)
│  └─ LoadingPanel
│     ├─ Spinner (Image/Animation)
│     └─ LoadingText (Text)
```

#### الـ Scripts:
```
Add to MainCanvas:
- GameStarter.cs
- LoginUIManager.cs
```

---

### 2️⃣ GameScene
**المسار:** `Assets/Scenes/GameScene.unity`

#### الخطوات:
```
1. File → New Scene → Save as "GameScene"

2. إنشاء Environment:
   - Create 3D Plane (Ground)
   - Scale: (20, 1, 20)
   - Material: Grass Green
   
   - Create 3D Cubes (Buildings)
   - Position: Various positions
   - Scale: Different sizes
   - Material: Building colors
   
   - Create 3D Cubes (Walls)
   - Position: Around edges
   - Collider: Box Collider
   - Material: Wall colors

3. إنشاء Spawn Point:
   - Create Empty → Rename: "SpawnPoint"
   - Position: Center of map (0, 1, 0)
   - Add Indicator (كرة صغيرة) - للتصحيح البصري فقط

4. إنشاء Safe Zones:
   - Create 3D Cube → "SafeZone_1"
   - Position: (10, 0, 10)
   - Scale: (5, 0.1, 5)
   - Material: Green (Transparent)
   - Collider: Box Collider (Is Trigger: ✓)

5. إنشاء Managers GameObject:
   - Create Empty → "Managers"
   - Add Components:
     ✅ GameManager
     ✅ NetworkManager
     ✅ PlayerManager
     ✅ FriendsManager
     ✅ ChatManager
     ✅ VoiceChatManager
     ✅ GameUIManager
     ✅ Logger
     ✅ EventManager
     ✅ PrefabManager

6. إنشاء Main Canvas (In-Game UI):
   - Right Click → UI → Panel
   - Rename: "GameUICanvas"
   - Set Anchor: Stretch, Stretch

   a. HUD Panel (أعلى اليسار):
      - Panel: "HUDPanel"
      - Elements:
        - Player Name: "اسم اللاعب"
        - Level: "Level 1"
        - Coins: "1000"
        - Gems: "50"
        - Health Bar (Slider)

   b. Chat Panel (أسفل اليسار):
      - Panel: "ChatPanel"
      - Size: 400 x 300
      - Elements:
        - ScrollRect (Chat messages area)
        - InputField (Send message)
        - Send Button
        - Tab to switch: Global/Private/Team

   c. Friends Panel (يمين الشاشة):
      - Panel: "FriendsPanel"
      - Size: 250 x 500
      - Elements:
        - Tabs: Friends | Requests | Online
        - ScrollRect (Friends list)
        - Add Friend Button
        - Search InputField

   d. Notifications Area (أعلى اليمين):
      - Panel: "NotificationsArea"
      - Empty container (for spawning notifications)

   e. Minimap (أعلى اليمين):
      - RawImage component
      - Size: 200 x 200
      - Render Texture (from camera)

   f. Bottom Controls:
      - Button: "C" to Toggle Chat
      - Button: "F" to Toggle Friends
      - Button: "V" to Toggle Microphone
      - Button: "ESC" to Close All
```

#### الـ Hierarchy:
```csharp
GameScene Hierarchy:
├─ Main Camera (Removed - will be attached to player)
├─ Directional Light
├─ Ground (Plane)
├─ Buildings
│  ├─ Building_1 (Cube)
│  ├─ Building_2 (Cube)
│  └─ ...
├─ Walls
│  ├─ Wall_1 (Cube + Collider)
│  ├─ Wall_2 (Cube + Collider)
│  └─ ...
├─ SafeZones
│  └─ SafeZone_1 (Cube + Trigger)
├─ SpawnPoint (Empty)
├─ Managers (Empty)
│  ├─ GameManager (Script)
│  ├─ NetworkManager (Script)
│  ├─ PlayerManager (Script)
│  ├─ FriendsManager (Script)
│  ├─ ChatManager (Script)
│  ├─ VoiceChatManager (Script)
│  ├─ GameUIManager (Script)
│  ├─ Logger (Script)
│  ├─ EventManager (Script)
│  └─ PrefabManager (Script)
└─ GameUICanvas (Canvas)
   ├─ HUDPanel
   │  ├─ PlayerNameText
   │  ├─ LevelText
   │  ├─ CoinsText
   │  ├─ GemsText
   │  └─ HealthBar (Slider)
   ├─ ChatPanel
   │  ├─ ChatMessagesScroll (ScrollRect)
   │  ├─ ChatInputField (InputField)
   │  └─ SendButton (Button)
   ├─ FriendsPanel
   │  ├─ FriendsTab
   │  ├─ RequestsTab
   │  ├─ OnlineTab
   │  ├─ FriendsScroll (ScrollRect)
   │  ├─ SearchInput (InputField)
   │  └─ AddFriendButton (Button)
   ├─ NotificationsArea (Empty)
   ├─ Minimap (RawImage)
   └─ BottomControls
      ├─ ChatToggleButton
      ├─ FriendsToggleButton
      ├─ MicToggleButton
      └─ CloseAllButton
```

#### الـ Scripts:
```
Add to Managers:
- GameManager.cs
- NetworkManager.cs
- PlayerManager.cs
- FriendsManager.cs
- ChatManager.cs
- VoiceChatManager.cs
- GameUIManager.cs
- Logger.cs
- EventManager.cs
- PrefabManager.cs

Add to GameUICanvas:
- (GameUIManager سيتولى إدارتها)
```

---

## إعدادات مهمة ⚙️

### Canvas Settings:
```
Render Mode: Screen Space - Overlay
Scale Factor: 1
Reference Resolution: 1920 x 1080
```

### Camera Settings:
```
Field of View: 60
Near Clip: 0.3
Far Clip: 1000
Position: (0, 1, -10) - نسبة للاعب
```

### Lighting:
```
Directional Light Intensity: 1.5
Ambient Light: Light Gray
```

---

## الألوان الموصى بها 🎨

```
Primary (Blue): #007AFF
Secondary (Green): #34C759
Danger (Red): #FF3B30
Warning (Orange): #FF9500
Background (Dark): #1C1C1E
Text (White): #FFFFFF
Text (Secondary): #A1A1A6
```

---

## Build Settings 🔧

```
File → Build Settings

1. Add Scenes:
   - LoginScene (Index 0)
   - GameScene (Index 1)

2. Player Settings:
   - Company Name: Your Company
   - Product Name: Online Game
   - Version: 1.0.0

3. Resolution and Presentation:
   - Default Screen Width: 1920
   - Default Screen Height: 1080
   - Fullscreen Mode: Windowed
   - Run In Background: ✓

4. Quality Settings:
   - V Sync: Every V Blank
   - Anti Aliasing: 2x
   - Shadow Quality: Medium
```

---

## اختبار المشاهد ✅

```
1. Play LoginScene:
   ✓ التحقق من التوصيل بالإنترنت
   ✓ تسجيل دخول صحيح
   ✓ تحقق من رسائل الخطأ

2. Play GameScene:
   ✓ تحقق من ظهور اللاعب
   ✓ اختبر الحركة (WASD)
   ✓ افتح الدردشة (C)
   ✓ افتح الأصدقاء (F)
   ✓ شغل المايك (V)
```

---

## نصائح مهمة 💡

✅ استخدم Layouts لتنظيم الـ UI  
✅ اختبر على دقة الشاشات المختلفة  
✅ استخدم Anchors بشكل صحيح للـ Responsive UI  
✅ أضف Animations للـ UI transitions  
✅ اختبر الأداء قبل البناء  
✅ استخدم Pools للـ Prefabs كثيرة الاستخدام  

---

## المرة القادمة 🚀

بعد إنشاء المشاهد:
- ✅ **تصدير APK** (Build for Android)
- ✅ **اختبار الأونلاين** (على هاتفين)
- ✅ **تحسين الأداء** (Optimization)
