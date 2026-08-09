# دليل إنشاء Prefabs 🎮

## Prefabs المطلوبة

### 1️⃣ Player Prefab

**المسار:** `Assets/Resources/Prefabs/Player.prefab`

**الخطوات:**
```
1. Create → 3D Object → Capsule
2. أعد تسميته: Player
3. أضف Components:
   ✅ PlayerController
   ✅ PlayerNetworkSync
   ✅ Rigidbody (Body Type: Dynamic, Gravity: ✓)
   ✅ Capsule Collider (Is Trigger: ✗)
4. أنشئ فوق الرأس:
   - Create Empty → اسمه "NameDisplay"
   - أضف Text Mesh 3D
   - أضف Canvas (Screen Space - World)
5. أضف AudioSource (للصوت)
6. حفظ كـ Prefab: اسحبه في Prefabs/Player
```

### 2️⃣ ChatMessage Prefab

**المسار:** `Assets/Resources/Prefabs/ChatMessage.prefab`

**الخطوات:**
```
1. Create UI → Panel
2. أعد تسميته: ChatMessage
3. أضف Components:
   ✅ ChatMessageUI (Script)
4. ابنيتها:
   - Background Image (Semi-transparent)
   - Sender Name (Text)
   - Message Content (Text - multiline)
   - Timestamp (Text - صغير جداً)
5. ضبط الأحجام
6. حفظ كـ Prefab
```

### 3️⃣ FriendItem Prefab

**المسار:** `Assets/Resources/Prefabs/FriendItem.prefab`

**الخطوات:**
```
1. Create UI → Button
2. أعد تسميته: FriendItem
3. أضف Components:
   ✅ FriendItemUI (Script)
4. ابنيتها:
   - Avatar Image
   - Friend Name (Text)
   - Status Indicator (Image - دائرة خضراء/رمادية)
   - Message Button
   - Invite Button
5. حفظ كـ Prefab
```

### 4️⃣ Notification Prefab

**المسار:** `Assets/Resources/Prefabs/Notification.prefab`

**الخطوات:**
```
1. Create UI → Panel
2. أعد تسميته: Notification
3. أضف Components:
   ✅ NotificationUI (Script)
4. ابنيتها:
   - Background Image (ملون)
   - Message Text (Text)
   - Close Button (X)
5. حفظ كـ Prefab
```

### 5️⃣ InviteNotification Prefab

**المسار:** `Assets/Resources/Prefabs/InviteNotification.prefab`

**الخطوات:**
```
1. Create UI → Panel
2. أعد تسميته: InviteNotification
3. أضف Components:
   ✅ InviteNotificationUI (Script)
4. ابنيتها:
   - Avatar Image
   - Invite Text ("أحمد يريد إضافتك صديق!")
   - Accept Button (أخضر)
   - Decline Button (أحمر)
5. حفظ كـ Prefab
```

---

## استخدام PrefabManager

```csharp
// إنشاء لاعب
GameObject player = PrefabManager.Instance.CreatePlayer(
    new Vector3(0, 0, 0), 
    Quaternion.identity
);

// إنشاء رسالة دردشة
GameObject msg = PrefabManager.Instance.CreateChatMessage(
    "Ahmed", 
    "مرحبا بالجميع!", 
    chatContentArea
);

// إنشاء عنصر صديق
GameObject friend = PrefabManager.Instance.CreateFriendItem(
    "محمد", 
    true, 
    friendsListArea
);

// إنشاء إشعار
GameObject notif = PrefabManager.Instance.CreateNotification(
    "لاعب جديد انضم!", 
    notificationsArea
);

// إنشاء إشعار دعوة
GameObject invite = PrefabManager.Instance.CreateInviteNotification(
    "فاطمة", 
    invitesArea
);
```

---

## ملاحظات مهمة ⚠️

✅ جميع الـ Prefabs يجب أن تكون في `Assets/Resources/Prefabs/`
✅ أضف الـ Scripts المناسبة لكل Prefab
✅ اختبر كل Prefab قبل الحفظ
✅ استخدم PrefabManager للإنشاء الآمن
✅ تأكد من إضافة CanvasGroup للـ UI للـ Fade animations

---

## هيكل المجلد

```
Assets/
├─ Resources/
│  └─ Prefabs/
│     ├─ Player.prefab
│     ├─ ChatMessage.prefab
│     ├─ FriendItem.prefab
│     ├─ Notification.prefab
│     └─ InviteNotification.prefab
├─ Scenes/
├─ Scripts/
└─ UI/
```
