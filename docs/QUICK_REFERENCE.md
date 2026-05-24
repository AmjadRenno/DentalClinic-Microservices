# Quick Reference Guide

## 🧪 Testing

### Running Tests
```bash
# All tests
dotnet test

# Specific project
dotnet test services/Booking/BookingService.Domain.Tests

# With coverage
dotnet test /p:CollectCoverage=true
```

### Writing Tests
```csharp
using FluentAssertions;
using Xunit;
using BookingService.Domain.Tests.TestHelpers;

public class MyTests
{
    [Fact]
    public void Test_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Theory]
    [InlineData(1, 60)]
    [InlineData(2, 120)]
    public void Test_WithParameters(int hours, int expectedMinutes)
    {
        // Arrange & Act & Assert
        var duration = TimeSpan.FromHours(hours);
        duration.TotalMinutes.Should().Be(expectedMinutes);
    }
}
```

---

## ⚠️ Error Handling

### Throwing Exceptions
```csharp
using DentalClinic.SharedKernel.Exceptions;

// Not Found (404)
throw new NotFoundException("Appointment", appointmentId);

// Validation (400)
throw new ValidationException("Field", "Error message");
throw new ValidationException(new Dictionary<string, string[]> 
{
    ["PatientId"] = new[] { "Required" },
    ["Date"] = new[] { "Must be in future" }
});

// Business Rule (422)
throw new BusinessRuleException("Cannot cancel completed appointments");

// Conflict (409)
throw new ConflictException("Appointment", id, "Already confirmed");
```

### Expected Response
```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not Found",
  "status": 404,
  "detail": "Appointment with id '123' was not found.",
  "instance": "/api/appointments/123",
  "errorCode": "ENTITY_NOT_FOUND",
  "timestamp": "2026-02-12T10:30:00Z",
  "traceId": "0HMVFE42A5V7E:00000001"
}
```

---

## ✅ Validation

### Creating Validators
```csharp
using FluentValidation;

public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be positive.")
            .LessThanOrEqualTo(10000)
            .WithMessage("Amount too large.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid email required.");

        RuleFor(x => x)
            .Must(x => x.End > x.Start)
            .WithMessage("End must be after start.");
    }
}
```

### Using in Service
```csharp
public class MyService
{
    private readonly IValidator<MyCommand> _validator;

    public async Task Handle(MyCommand command)
    {
        // Validate
        var result = await _validator.ValidateAsync(command);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new ValidationException(errors);
        }

        // Process...
    }
}
```

---

## 🏗️ Test Helpers

### Builder Pattern
```csharp
public static class AppointmentBuilder
{
    public static Appointment Create()
    {
        return new Appointment(
            Guid.NewGuid(),
            new PatientId(Guid.NewGuid()),
            new DentistId(Guid.NewGuid()),
            CreateValidTimeSlot());
    }

    public static Appointment CreateConfirmed()
    {
        var appointment = Create();
        appointment.Confirm();
        return appointment;
    }

    public static TimeSlot CreateValidTimeSlot(int daysFromNow = 1)
    {
        var start = DateTime.UtcNow.AddDays(daysFromNow);
        var end = start.AddHours(1);
        return new TimeSlot(start, end);
    }
}
```

### Usage
```csharp
// Simple
var appointment = AppointmentBuilder.Create();

// Confirmed state
var confirmedAppointment = AppointmentBuilder.CreateConfirmed();

// Custom time
var slot = AppointmentBuilder.CreateValidTimeSlot(daysFromNow: 7);
var appointment = AppointmentBuilder.CreateWithTimeSlot(slot);
```

---

## 📊 FluentAssertions Cheat Sheet

```csharp
// Equality
result.Should().Be(expected);
result.Should().NotBe(unexpected);

// Null checks
result.Should().BeNull();
result.Should().NotBeNull();

// Booleans
flag.Should().BeTrue();
flag.Should().BeFalse();

// Collections
list.Should().HaveCount(5);
list.Should().Contain(item);
list.Should().NotContain(item);
list.Should().BeEmpty();
list.Should().NotBeEmpty();

// Strings
text.Should().Be("expected");
text.Should().StartWith("prefix");
text.Should().EndWith("suffix");
text.Should().Contain("substring");

// Numbers
number.Should().BeGreaterThan(5);
number.Should().BeLessThan(10);
number.Should().BeInRange(1, 100);

// Exceptions
Action act = () => method();
act.Should().Throw<InvalidOperationException>()
    .WithMessage("Error message")
    .WithParameterName("paramName");

// Types
obj.Should().BeOfType<MyType>();
obj.Should().BeAssignableTo<IMyInterface>();

// Dates
date.Should().BeAfter(otherDate);
date.Should().BeBefore(otherDate);
date.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
```

---

## 🔍 Logging

### Structured Logging
```csharp
_logger.LogInformation(
    "Processing appointment {AppointmentId} for patient {PatientId}",
    appointmentId,
    patientId);

_logger.LogWarning(
    "Failed to process {Count} items",
    failedCount);

_logger.LogError(
    exception,
    "Critical error in {MethodName}",
    nameof(HandlePayment));
```

### Log Levels
- **Trace**: Very detailed, development only
- **Debug**: Debugging information
- **Information**: General flow
- **Warning**: Abnormal but expected
- **Error**: Errors and exceptions
- **Critical**: System failures

---

## 🎯 Error Codes

| Code | HTTP | Description |
|------|------|-------------|
| `ENTITY_NOT_FOUND` | 404 | Resource not found |
| `VALIDATION_ERROR` | 400 | Input validation failed |
| `BUSINESS_RULE_VIOLATION` | 422 | Business logic violation |
| `CANNOT_CONFIRM_APPOINTMENT` | 422 | Invalid state transition |
| `CANNOT_CANCEL_APPOINTMENT` | 422 | Cannot cancel in current state |
| `CANNOT_AUTHORIZE_PAYMENT` | 422 | Payment not in correct state |
| `CONFLICT` | 409 | Resource conflict |
| `INTERNAL_ERROR` | 500 | Unexpected server error |

---

## 📦 Package Versions

```xml
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
<PackageReference Include="FluentAssertions" Version="7.0.0" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="xUnit" Version="2.9.3" />
<PackageReference Include="coverlet.collector" Version="6.0.4" />
```

---

## 🚀 Project Setup Checklist

When adding a new service:

- [ ] Add `SharedKernel` project reference
- [ ] Add `FluentValidation` packages
- [ ] Register validators: `builder.Services.AddValidatorsFromAssembly(...)`
- [ ] Add global exception handler: `app.UseGlobalExceptionHandler()`
- [ ] Create validators for all commands
- [ ] Add logging to service methods
- [ ] Create test project with proper structure
- [ ] Add test helpers/builders
- [ ] Write comprehensive unit tests

---

**Last Updated**: February 12, 2026
