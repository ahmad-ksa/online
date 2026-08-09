# دليل ربط Nakama Server 🌐

## المتطلبات:

✅ **Nakama Server** (محلي أو سحابي)
✅ **Server IP**: 45.76.130.15 (أو عنوان خادمك)
✅ **Server Port**: 7349 (الافتراضي)
✅ **إنترنت**

---

## الخطوة 1️⃣: إنشاء NakamaConfig

### في Unity:
```
Project Window → Right Click
→ Create → Game → Nakama Config
```

### في Inspector:
```
Server Host: 45.76.130.15 (أو عنوان خادمك)
Server Port: 7349
Use SSL: false (تغيره إلى true إذا كان الخادم يستخدم HTTPS)
Server Key: defaultkey (أو مفتاحك الخاص)
Connection Timeout: 30
Max Retries: 5
Debug Mode: ✓ (للتطوير)
```

---

## الخطوة 2️⃣: إضافة NakamaClient إلى المشهد

### في GameScene:
```
1. Select Managers GameObject
2. Add Component → NakamaClient
3. في Inspector:
   - Drag NakamaConfig asset إلى Nakama Config field
```

---

## الخطوة 3️⃣: إضافة RealTimeMatchManager

### في GameScene:
```
1. Select Managers GameObject
2. Add Component → RealTimeMatchManager
```

---

## الخطوة 4️⃣: تحديث NetworkManager

### في NetworkManager.cs:
```csharp
private NakamaClient nakamaClient;

private void Start()
{
    nakamaClient = NakamaClient.Instance;
    
    // الآن يستخدم إعدادات NakamaConfig
    // عندما تغير الإعدادات في Inspector،
    // سيتم الاتصال تلقائياً بالخادم الجديد!
}
```

---

## الخطوة 5️⃣: اختبار الاتصال

### في Unity:
```
1. Play Game
2. شاهد Console (Ctrl+Shift+C)
3. تحقق من الرسائل:
   ✓ "Connected to Nakama Server!"
   ✓ "Authentication successful!"
```

---

## تغيير الخادم من Inspector 🎮

### بسهولة جداً:

```
1. اختر Assets/Resources/NakamaConfig.asset
2. عدّل في Inspector:
   - Server Host: غيّره من 45.76.130.15 إلى أي عنوان آخر
   - Server Port: غيّره من 7349 إلى أي منفذ آخر
   - Use SSL: فعّل/عطّل
3. Play
4. Done! اللعبة ستتصل بالخادم الجديد تلقائياً
```

---

## إعداد خادم Nakama الفعلي 🚀

### على Vultr Server (45.76.130.15):

#### 1️⃣ الاتصال بالخادم:
```bash
ssh root@45.76.130.15
# أدخل كلمة المرور
```

#### 2️⃣ تثبيت Docker:
```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
```

#### 3️⃣ تشغيل Nakama:
```bash
docker run -d \
  -p 7349:7349 \
  -p 7350:7350 \
  -e NAKAMA_DATASTORE=postgres \
  -e NAKAMA_DATABASE="postgres://nakama:nakama@localhost/nakama" \
  heroiclabs/nakama:latest
```

#### 4️⃣ تشغيل PostgreSQL:
```bash
docker run -d \
  -e POSTGRES_PASSWORD=nakama \
  -e POSTGRES_USER=nakama \
  -e POSTGRES_DB=nakama \
  -p 5432:5432 \
  postgres:13
```

#### 5️⃣ فتح الـ Firewall:
```bash
sudo ufw allow 7349
sudo ufw allow 7350
sudo ufw enable
```

---

## إعادة تعيين الخادم بدون إعادة تشغيل 🔄

### الطريقة الأولى (الموصى بها):
```
1. افتح NakamaConfig.asset
2. غيّر Server Host و Port
3. اضغط Save (Ctrl+S)
4. في اللعبة، ستتصل تلقائياً بالخادم الجديد
```

### الطريقة الثانية (RuntimeConfig):
```csharp
// في أي script
var nakamaConfig = Resources.Load<NakamaConfig>("NakamaConfig");
// سيستخدم الإعدادات الحالية تلقائياً
```

---

## معالجة الأخطاء 🛡️

### إذا فشل الاتصال:

```
❌ "Connection refused"
→ تأكد أن الخادم يعمل: ssh root@45.76.130.15
→ تحقق من Port: netstat -tulpn | grep 7349

❌ "Connection timeout"
→ أطل Connection Timeout في NakamaConfig
→ تحقق من الـ Firewall

❌ "Authentication failed"
→ تأكد من Server Key صحيح
→ تحقق من قاعدة البيانات
```

---

## إعدادات موصى بها 💡

### للتطوير (Development):
```
Server Host: 45.76.130.15 (أو localhost:7350)
Debug Mode: ✓
Log Network Messages: ✓
Max Retries: 3
```

### للإنتاج (Production):
```
Server Host: خادمك الفعلي
Use SSL: ✓ (HTTPS)
Debug Mode: ✗
Log Network Messages: ✗
Max Retries: 5
```

---

## الملفات المتعلقة 📁

```
Scripts/Network/
├─ NakamaConfig.cs (الإعدادات - قابل للتعديل)
├─ NakamaClient.cs (الاتصال الحقيقي)
└─ RealTimeMatchManager.cs (تزامن المباريات)

Resources/
└─ NakamaConfig.asset (الإعدادات الفعلية)
```

---

## الخطوة التالية 🚀

الآن اللعبة:
✅ متصلة بالخادم الحقيقي
✅ قابلة لتغيير الخادم من Inspector
✅ تزامن فعلي للاعبين
✅ معالجة أخطاء شاملة
✅ جاهزة للاختبار!

**الآن يمكنك:
1. تصدير APK
2. اختبار على هاتفين
3. التحقق من التزامن الفعلي**

---

## نصائح مهمة ⚡

✓ جرّب الخادم المحلي أولاً (localhost:7350)
✓ استخدم Debug Mode أثناء التطوير
✓ تأكد من فتح Ports في Firewall
✓ احفظ logs لمعرفة الأخطاء
✓ اختبر الاتصال قبل البناء

---

**الآن اللعبة أونلاين كاملة! 🎮**
