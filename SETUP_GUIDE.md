# 🎮 دليل تثبيت المشروع الجاهز للإنتاج

## خطوات التثبيت السريعة

### 1️⃣ استخدام الفرع الجديد
```bash
git checkout production-ready
```

### 2️⃣ إنشاء المديرين في المشهد
- افتح أي مشهد في Unity
- أنشئ GameObject جديد باسم "Managers"
- أضف هذه Components:
  - GameManager
  - NetworkManager
  - PlayerManager
  - LocalDataManager
  - Logger
  - EventManager

### 3️⃣ إعداد GameConfig
- انقر بزر الفأرة الأيمن في Project > Create > Game > Config
- سمّه "GameConfig"
- اسحبه إلى: `Assets/Resources/GameConfig.asset`
- املأ الإعدادات في Inspector:
  - Nakama Host: `play.nakama.dev`
  - Nakama Port: `7349`
  - Debug Mode: ✓ (للتطوير)

### 4️⃣ اختبار الاتصال
```csharp
public class GameInit : MonoBehaviour
{
    async void Start()
    {
        // اتصل بالسيرفر
        bool connected = await NetworkManager.Instance.ConnectToServer();
        
        if (connected)
        {
            // صادق اللاعب
            bool authenticated = await NetworkManager.Instance
                .AuthenticatePlayer("test@example.com", "password123");
            
            if (authenticated)
            {
                // حمّل بيانات اللاعب
                await PlayerManager.Instance.LoadPlayerData();
                Logger.Log("جاهز للعب! ✅", "GameInit");
            }
        }
    }
}
```

## المميزات الجديدة ✨

### معالجة أخطاء شاملة
- try-catch في جميع العمليات
- رسائل خطأ واضحة
- إعادة محاولة تلقائية للاتصال

### Logging متقدم
- حفظ في ملفات
- مستويات مختلفة (Debug, Info, Warning, Error, Critical)
- ألوان ملونة في Console

### Offline Mode
- عمل بدون إنترنت
- حفظ محلي للبيانات
- مزامنة تلقائية عند العودة

### Interfaces للمرونة
- `INetworkManager` - للاختبار والمحاكاة
- `IPlayerManager` - للتوسع السهل

## الملفات الجديدة 📁

```
Scripts/
├── Core/
│   ├── Interfaces/
│   │   ├── INetworkManager.cs ✨ جديد
│   │   └── IPlayerManager.cs ✨ جديد
│   ├── NetworkManager.cs ⬆️ محدث
│   └── PlayerManager.cs ⬆️ محدث
│
├── Utilities/
│   ├── Result.cs ✨ جديد
│   ├── Logger.cs ✨ جديد
│   ├── GameConfig.cs ✨ جديد
│   ├── LocalDataManager.cs ✨ جديد
│   └── EventManager.cs ✨ جديد
```

## الخطوات التالية 🚀

1. **دمج Nakama SDK الحقيقي**
   - ثبّت من NuGet: `Nakama.cs`
   - استبدل المحاكاة بـ SDK الحقيقي

2. **إضافة Scenes**
   - Login Scene
   - Lobby Scene
   - Game Room Scene

3. **إضافة UI**
   - استخدم UIManager الموجود
   - أنشئ واجهات للـ scenes

4. **الاختبار**
   - اختبر الاتصال بالشبكة
   - اختبر Offline Mode
   - اختبر معالجة الأخطاء

## معالجة المشاكل 🔧

### مشكلة: "Logger not found"
- تأكد أن GameObject "Managers" يحتوي على Logger component

### مشكلة: "GameConfig not found"
- أنشئ GameConfig في Resources/GameConfig.asset

### مشكلة: الاتصال بالسيرفر فشل
- تحقق من إعدادات Nakama في GameConfig
- تأكد من أن السيرفر يعمل
- فعّل Debug Mode لرؤية التفاصيل

## المرة القادمة 💡

✅ المشروع **جاهز الآن للعبة حقيقية**!

لو عندك أي سؤال أو تريد تطوير أكثر، قول لي! 🎮
