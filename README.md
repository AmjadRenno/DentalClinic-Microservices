# DentalClinic

نظام إدارة عيادة أسنان متكامل مبني باستخدام بنية Microservices.

## 📋 نظرة عامة

DentalClinic هو نظام شامل لإدارة عيادات الأسنان يوفر:
- إدارة المواعيد والحجوزات
- إدارة المرضى وبياناتهم
- إدارة الأطباء والموظفين
- نظام المدفوعات
- نظام الإشعارات
- واجهة مستخدم متقدمة مع Umbraco CMS

## 🏗️ البنية المعمارية

المشروع مبني على بنية Microservices مع الخدمات التالية:

### الخدمات الرئيسية (Services)

- **AuthService.API**: خدمة المصادقة والتفويض
- **BookingService**: إدارة المواعيد والحجوزات (DDD Implementation)
- **DentistService.API**: إدارة بيانات الأطباء
- **PatientService.API**: إدارة بيانات المرضى
- **PaymentService**: معالجة الدفعات (DDD Implementation)
- **NotificationService.API**: إرسال الإشعارات
- **GatewayService**: API Gateway للتوجيه
- **OrchestratorService**: تنسيق العمليات بين الخدمات

### الواجهات الأمامية

- **DentalClinic.Cms**: واجهة إدارة المحتوى باستخدام Umbraco
- **Frontend**: تطبيق Blazor للمستخدمين

### المكونات المشتركة

- **DentalClinic.SharedKernel**: Domain Events والمكونات المشتركة
- **DentalClinic.ServiceDefaults**: إعدادات مشتركة للخدمات
- **DentalClinic.AppHost**: Aspire AppHost لإدارة التطبيق

## 🛠️ التقنيات المستخدمة

- **.NET 8.0**: إطار العمل الرئيسي
- **ASP.NET Core**: بناء APIs
- **Entity Framework Core**: ORM للتعامل مع قواعد البيانات
- **SQLite**: قاعدة البيانات
- **Umbraco CMS**: إدارة المحتوى
- **Blazor**: واجهة المستخدم التفاعلية
- **Domain-Driven Design (DDD)**: في خدمات Booking و Payment
- **.NET Aspire**: لإدارة وتشغيل الخدمات

## 📦 البنية التفصيلية

### Booking Service (DDD)
```
BookingService.Domain      - Entities, Value Objects, Domain Logic
BookingService.Application - Commands, Queries, Use Cases
BookingService.Infrastructure - Data Access, Repositories
BookingService.API         - REST API Endpoints
```

### Payment Service (DDD)
```
PaymentService.Domain      - Entities, Value Objects, Domain Logic
PaymentService.Application - Business Logic
PaymentService.Infrastructure - Data Access, Repositories
PaymentService.API         - REST API Endpoints
```

## 🚀 البدء

### المتطلبات

- .NET 8.0 SDK
- Visual Studio 2022 أو VS Code
- SQLite

### التشغيل

1. استنساخ المشروع:
```bash
git clone https://github.com/AmjadRn/DentalClinic.git
cd DentalClinic
```

2. تشغيل المشروع باستخدام .NET Aspire:
```bash
dotnet run --project DentalClinic.AppHost
```

أو تشغيل كل خدمة بشكل منفصل:
```bash
dotnet run --project AuthService.API
dotnet run --project BookingService/BookingService.API
dotnet run --project DentistService.API
# ... إلخ
```

## 📝 الميزات

- ✅ نظام مصادقة وتفويض آمن
- ✅ حجز المواعيد مع الأطباء
- ✅ إدارة ملفات المرضى
- ✅ معالجة المدفوعات
- ✅ إشعارات تلقائية
- ✅ واجهة مستخدم سهلة الاستخدام
- ✅ لوحة تحكم إدارية

## 🧪 الاختبارات

المشروع يحتوي على اختبارات وحدة (Unit Tests):
- `BookingService.Domain.Tests`
- `PaymentService.Domain.Tests`

تشغيل الاختبارات:
```bash
dotnet test
```

## 📄 الترخيص

هذا المشروع مفتوح المصدر.

## 👨‍💻 المطور

تم تطويره بواسطة Amjad

## 📞 التواصل

لأي استفسارات أو مشاكل، يرجى فتح Issue على GitHub.
