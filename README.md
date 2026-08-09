# لعبة متعددة اللاعبين الأونلاين 🎮

## نظرة عامة

لعبة متعددة لاعبين في الوقت الفعلي مع نظام أصدقاء كامل، دردشة، وصوت.

## المميزات ✨

### نظام اللاعب
- ✅ تحكم حركة سلس (WASD / Joystick)
- ✅ تزامن في الوقت الفعلي مع اللاعبين الآخرين
- ✅ عرض اسم اللاعب فوق الرأس
- ✅ Health Bar

### ماب المدينة
- ✅ مدينة صغيرة مع أبنية
- ✅ جدران Collision
- ✅ نقاط Spawn آمنة

### نظام الأصدقاء
- ✅ إضافة أصدقاء
- ✅ قبول/رفض الطلبات
- ✅ عرض حالة الأصدقاء (Online/Offline)
- ✅ حذف صديق

### الدردشة
- ✅ دردشة عامة (Global Chat)
- ✅ رسائل خاصة (Direct Message)
- ✅ رسائل فورية
- ✅ Timestamps

### الصوت
- ✅ Proximity Voice Chat
- ✅ تفعيل/تعطيل الميكروفون
- ✅ مؤشر نشاط الصوت
- ✅ التحكم في مستوى الصوت

### الواجهة
- ✅ HUD رئيسية
- ✅ نافذة دردشة قابلة للتوسع
- ✅ قائمة الأصدقاء
- ✅ نظام إشعارات
- ✅ Minimap

## الاختصارات 🎮

| المفتاح | الوظيفة |
|--------|--------|
| W/A/S/D | الحركة |
| C | فتح/إغلاق الدردشة |
| F | فتح/إغلاق الأصدقاء |
| V | تفعيل الميكروفون |
| ESC | إغلاق النوافذ |

## البنية 📁

```
Scripts/
├─ Player/
│  ├─ PlayerController.cs
│  └─ PlayerNetworkSync.cs
├─ Social/
│  ├─ FriendsManager.cs
│  ├─ ChatManager.cs
│  └─ VoiceChatManager.cs
└─ UI/
   └─ GameUIManager.cs
```

## البدء السريع 🚀

### 1. تحميل المشروع
```bash
git clone https://github.com/ahmad-ksa/online.git
git checkout production-ready
```

### 2. فتح في Unity
```
File > Open Project > اختر المجلد
```

### 3. تشغيل المشهد
```
Scenes/GameScene.unity > Play
```

### 4. اختبار الشبكة
- افتح نسختين من اللعبة
- تحرك في الماب
- الرسائل والصوت

## التصدير للموبايل 📱

### Android (APK)
```
File > Build Settings
1. اختر Platform: Android
2. اضغط "Build"
3. اختر مجلد الحفظ
```

### iOS
```
File > Build Settings
1. اختر Platform: iOS
2. اضغط "Build"
```

## معالجة الأخطاء 🛠️

### المشكلة: لا يوجد إنترنت
**الحل:** اللعبة تعمل في Offline Mode تلقائياً

### المشكلة: لا يسمع الصوت
**الحل:** 
- تحقق من الميكروفون في الإعدادات
- اضغط V لتفعيل الميكروفون

### المشكلة: تأخر الحركة
**الحل:** قلل جودة الرسومات في الإعدادات

## الإعدادات ⚙️

افتح `GameConfig` في Resources:

```
Nakama Host: play.nakama.dev
Nakama Port: 7349
Proximity Range: 20
Mic Volume: 1.0
```

## المتطلبات 📋

- Unity 2020.3+
- Android 6.0+ / iOS 11+
- إنترنت (للأونلاين)
- ميكروفون (للصوت)

## المطورون 👨‍💻

- ahmad-ksa

## الترخيص 📜

MIT License
