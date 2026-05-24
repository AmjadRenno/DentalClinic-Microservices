# DentalClinic Security Implementation

## Overview
This document describes the comprehensive security measures implemented across the DentalClinic microservices platform.

## Table of Contents
1. [Rate Limiting](#rate-limiting)
2. [CORS Policies](#cors-policies)
3. [Input Sanitization](#input-sanitization)
4. [Password Policy](#password-policy)
5. [Account Lockout](#account-lockout)
6. [Authentication](#authentication)
7. [Security Headers](#security-headers)
8. [Secrets Management](#secrets-management)
9. [Security Best Practices](#security-best-practices)

---

## 1. Rate Limiting

### Implementation
- **Location**: API Gateway (`GatewayService`)
- **Package**: `AspNetCoreRateLimit 5.0.0`
- **Type**: IP-based rate limiting

### Configuration
File: `GatewayService/ratelimitsettings.json`

**General Rules:**
- 60 requests per minute
- 200 requests per 15 minutes
- 500 requests per hour

**Endpoint-Specific Rules:**
- **Login** (`*/auth/login`): 5 requests/minute
- **Register** (`*/auth/register`): 3 requests/hour
- **Payments** (`*/payments/*`): 10 requests/minute

**Localhost Exemption:**
- Development: 1000 requests/minute

### Usage
```csharp
// Automatically applied in GatewayService middleware pipeline
app.UseIpRateLimiting(); // Before authentication
```

---

## 2. CORS Policies

### Environment-Specific Configuration

**Development Environment:**
```csharp
policy.WithOrigins(allowedOrigins)
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

**Production Environment:**
```csharp
policy.WithOrigins(allowedOrigins)
      .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
      .WithMethods("GET", "POST", "PUT", "DELETE")
      .AllowCredentials();
```

### Allowed Origins
Configure in `GatewayService/appsettings.json`:
```json
{
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173",
    "http://localhost:7000"
  ]
}
```

### Best Practices
- **Never use** `AllowAnyOrigin()` in production
- Explicitly list allowed origins
- Restrict headers and methods in production
- Always validate origin against configuration

---

## 3. Input Sanitization

### Implementation
**Location**: `DentalClinic.SharedKernel/Security/InputSanitizer.cs`

### Available Methods

#### RemoveHtml(string input)
Removes HTML tags and script content
```csharp
var clean = InputSanitizer.RemoveHtml("<script>alert('xss')</script>Hello");
// Result: "Hello"
```

#### SanitizeSqlInput(string input)
Removes SQL keywords and dangerous characters
```csharp
var clean = InputSanitizer.SanitizeSqlInput("'; DROP TABLE Users--");
// Result: "''   --"
```

#### SanitizeText(string input, int maxLength)
General text cleaning with length limit
```csharp
var clean = InputSanitizer.SanitizeText("User  Name\n\nWith  Spaces", 50);
// Result: "User Name With Spaces"
```

#### SanitizeEmail(string email)
Email validation and normalization
```csharp
var clean = InputSanitizer.SanitizeEmail("  USER@Example.COM  ");
// Result: "user@example.com"
```

#### SanitizePhoneNumber(string phone)
Extracts and validates phone digits (10-15 characters)
```csharp
var clean = InputSanitizer.SanitizePhoneNumber("+1 (555) 123-4567");
// Result: "15551234567"
```

#### SanitizeSearchQuery(string query, int maxLength)
Search input protection
```csharp
var clean = InputSanitizer.SanitizeSearchQuery("dental  clinic", 100);
// Result: "dental clinic"
```

### Important Notes
- Input sanitization is **defense-in-depth**, not primary defense
- Always use **parameterized queries** for database operations
- Always use **output encoding** when rendering HTML
- Sanitize at **service boundaries** (API controllers, gRPC handlers)

---

## 4. Password Policy

### Implementation
**Location**: `DentalClinic.SharedKernel/Security/PasswordValidator.cs`

### Default Policy
```csharp
var policy = PasswordPolicy.Default;
// MinimumLength: 8
// RequireUppercase: true
// RequireLowercase: true
// RequireDigit: true
// RequireSpecialCharacter: true
// MinimumUniqueCharacters: 4
```

### Strict Policy (Currently Used)
```csharp
var policy = PasswordPolicy.Strict;
// MinimumLength: 12
// RequireUppercase: true
// RequireLowercase: true
// RequireDigit: true
// RequireSpecialCharacter: true
// MinimumUniqueCharacters: 6
```

### Password Requirements (Strict)
- ✅ Minimum 12 characters
- ✅ Maximum 128 characters
- ✅ At least one uppercase letter (A-Z)
- ✅ At least one lowercase letter (a-z)
- ✅ At least one digit (0-9)
- ✅ At least one special character (!@#$%^&*)
- ✅ At least 6 unique characters
- ✅ Not in common password list

### Common Password Protection
Blocks these weak passwords:
```
password, 123456, 12345678, qwerty, abc123, 
password1, 111111, 123123, admin, letmein, 
welcome, monkey, dragon, master, sunshine
```

### Usage in AuthService
```csharp
// Automatically validates on registration
var (success, errorMessage) = await _userService.RegisterUserAsync(
    fullName, email, password
);

if (!success)
{
    // errorMessage contains specific validation failures
    return BadRequest(new { message = errorMessage });
}
```

### Password Validation Errors
Examples:
- "Password must be at least 12 characters long"
- "Password must contain at least one uppercase letter"
- "Password must contain at least one special character"
- "Password is too common. Please choose a stronger password"

---

## 5. Account Lockout

### Implementation
**Location**: `DentalClinic.SharedKernel/Security/`
- `DaprAccountLockoutService.cs` - **Production (Distributed)**
- `AccountLockoutService.cs` - Development (In-Memory)

### Distributed Architecture (Production)
```
[AuthService Instance 1] ↘
                           → [Dapr State Store] → [Redis]
[AuthService Instance 2] ↗
```

**Benefits:**
- ✅ State shared across multiple instances
- ✅ Scales horizontally
- ✅ Persistent lockout state
- ✅ Automatic failover with Redis cluster

### Configuration (AuthService)
```csharp
// Dapr-based distributed lockout (Production)
builder.Services.AddDaprClient();
builder.Services.AddSingleton<IAccountLockoutService>(sp => 
{
    var daprClient = sp.GetRequiredService<DaprClient>();
    return new DaprAccountLockoutService(
        daprClient,
        stateStoreName: "lockout-statestore",
        maxFailedAttempts: 5,
        lockoutDuration: TimeSpan.FromMinutes(15),
        failedAttemptWindow: TimeSpan.FromMinutes(10)
    );
});
```

### Dapr State Store Components

**Development (In-Memory):**
```yaml
# components/lockout-statestore-memory.yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: lockout-statestore
spec:
  type: state.in-memory
  version: v1
```

**Production (Redis):**
```yaml
# components/lockout-statestore.yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: lockout-statestore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
```

### Behavior

**Successful Login:**
- Resets failed attempt counter
- Clears any lockout status

**Failed Login:**
- Increments failed attempt counter
- Shows remaining attempts to user
- Example: "Invalid credentials. 3 attempts remaining before lockout."

**After 5 Failed Attempts:**
- Account is locked for 15 minutes
- User receives lockout message
- Example: "Account is locked due to multiple failed login attempts. Try again in 14 minutes."

**User Enumeration Protection:**
- Failed attempts recorded even if user doesn't exist
- Prevents attackers from discovering valid email addresses
- Generic "Invalid credentials" message for non-existent users

### Storage
- **Development**: In-memory storage (suitable for single instance)
- **Production**: Should use Redis or database-backed implementation
  ```csharp
  // TODO: Implement distributed lockout service for production
  // services.AddSingleton<IAccountLockoutService, RedisAccountLockoutService>();
  ```

### Manual Reset
If needed, implement admin endpoint:
```csharp
await _lockoutService.ResetFailedAttemptsAsync(email);
```

---

## 6. Authentication

### JWT Configuration
**Location**: All microservices (`appsettings.json`)

```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "DentalClinic.AuthService",
    "Audience": "DentalClinic.Services"
  }
}
```

### Token Lifetime
- **Expiration**: 2 hours
- **Clock Skew**: 1 minute (tolerance for time differences)

### Claims Included
```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
new Claim(ClaimTypes.Name, user.FullName),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Role, user.Role ?? "Patient")
```

### Password Hashing
- **Algorithm**: BCrypt
- **Work Factor**: 12 (configurable, higher = more secure but slower)
- **Salt**: Automatically generated per password
```csharp
BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
```

---

## 7. Security Headers

### Implementation
**Location**: `DentalClinic.SharedKernel/Middleware/SecurityHeadersMiddleware.cs`

Applied in **API Gateway** (GatewayService) to protect all downstream services.

### Headers Applied

#### X-Content-Type-Options: nosniff
Prevents browsers from MIME-type sniffing, forcing them to respect the declared Content-Type.

**Protection:** Prevents attackers from uploading a file with one extension but different content type.

#### X-Frame-Options: DENY
Prevents the application from being embedded in an iframe.

**Protection:** Clickjacking attacks where malicious sites overlay transparent frames.

#### X-XSS-Protection: 1; mode=block
Enables browser's built-in XSS filter (legacy support for older browsers).

**Protection:** Cross-Site Scripting attacks (modern apps use CSP instead).

#### Strict-Transport-Security (HSTS)
Forces HTTPS for all future requests (1 year duration).

**Configuration:**
```csharp
// Only applied in production (not localhost)
if (!context.Request.Host.Host.Contains("localhost"))
{
    context.Response.Headers["Strict-Transport-Security"] = 
        "max-age=31536000; includeSubDomains";
}
```

**Protection:** Man-in-the-middle attacks by preventing HTTP connections.

#### Content-Security-Policy (CSP)
Restricts resources the browser can load.

**Policy:**
```csharp
"default-src 'self'; " +
"script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
"style-src 'self' 'unsafe-inline'; " +
"img-src 'self' data: https:; " +
"font-src 'self' data:; " +
"connect-src 'self'; " +
"frame-ancestors 'none';"
```

**R9les:**
- `default-src 'self'`: Only load resources from same origin
- `script-src 'self' 'unsafe-inline' 'unsafe-eval'`: Allow scripts from same origin and inline scripts (for Swagger/SPAs)
- `img-src 'self' data: https:`: Allow images from same origin, data URIs, and HTTPS
- `frame-ancestors 'none'`: Prevent embedding in iframes (same as X-Frame-Options)

**Protection:** XSS attacks by preventing execution of unauthorized scripts.
✅ Security headers applied  

### Authentication Service
✅ Password strength validation  
✅ Account lockout protection (Distributed via Dapr)
**Value:** `strict-origin-when-cross-origin`
- Same-origin: Send full URL
- Cross-origin HTTPS→HTTPS: Send origin only
- Cross-origin HTTPS→HTTP: Send nothing

**Protection:** Information leakage through referrer headers.

#### Permissions-Policy
Disables browser features not needed by the application.

**Disabled Features:**
```
geolocation, microphone, camera, payment, usb, 
magnetometer, gyroscope, accelerometer
```

**Protection:** Prevents malicious scripts from accessing device hardware.

### Usage in Gateway

**Program.cs:**
```csharpProduction)
⚠️ Adjust Content-Security-Policy for your frontend  
⚠️ Enable CSP reporting endpoint  
⚠️ Test with security scanners  
⚠️ Monitor CSP violations  
### Customization

To customize headers for specific services, create a custom middleware:

```csharp
public class CustomSecurityHeadersMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Add custom headers
        context.Response.Headers["Custom-Header"] = "value";
        
        // Or override default CSP for specific paths
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; script-src 'self'; ...";
        }
        
        await _next(context);
    }
}
```

### Testing Security Headers

**Chrome DevTools:**
1. Open DevTools (F12)
2. Network tab
3. Click any request
4. Headers tab → Response Headers
5. Verify security headers present

**Command Line:**
```bash
curl -I http://localhost:5073/api/appointments
```

**Expected Output:**
```
HTTP/1.1 200 OK
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'; ...
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=(), microphone=(), ...
```

### Security Headers Checklist

✅ **Applied in Gateway** (protects all services)  
✅ **X-Content-Type-Options**: nosniff  
✅ **X-Frame-Options**: DENY  
✅ **X-XSS-Protection**: 1; mode=block  
✅ **Strict-Transport-Security**: max-age=31536000 (production only)  
✅ **Content-Security-Policy**: Restrictive policy  
✅ **Referrer-Policy**: strict-origin-when-cross-origin  
✅ **Permissions-Policy**: Disables unnecessary features  

### Production Recommendations

**1. Adjust CSP for Your Frontend**
```csharp
// If using specific CDNs for libraries
"script-src 'self' https://cdn.jsdelivr.net https://unpkg.com; " +
"style-src 'self' https://fonts.googleapis.com; " +
"font-src 'self' https://fonts.gstatic.com;"
```

**2. Enable CSP Reporting**
```csharp
context.Response.Headers["Content-Security-Policy"] = 
    "default-src 'self'; ... report-uri https://your-csp-report-endpoint.com/report";
```

**3. Test with Security Scanners**
- [Mozilla Observatory](https://observatory.mozilla.org/)
- [Security Headers](https://securityheaders.com/)
- [OWASP ZAP](https://www.zaproxy.org/)

**4. Monitor CSP Violations**
Set up endpoint to receive CSP violation reports:
```csharp
[HttpPost("api/csp-report")]
public IActionResult CspReport([FromBody] CspViolationReport report)
{
    _logger.LogWarning("CSP Violation: {Report}", report);
    return Ok();
}
```

---

## 8. Secrets Management

### Development
Currently using `appsettings.json` and `appsettings.Development.json`

**⚠️ WARNING**: Never commit secrets to version control!

### Production Recommendations

#### Option 1: Azure Key Vault
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

Access secrets:
```csharp
var jwtKey = builder.Configuration["Jwt:Key"]; // From Key Vault
```

#### Option 2: AWS Secrets Manager
```csharp
builder.Configuration.AddSecretsManager();
```

#### Option 3: Environment Variables
```csharp
var jwtKey = builder.Configuration["Jwt__Key"]; // From env var
```

Set in deployment:
```bash
# Linux/Docker
export Jwt__Key="production-secret-key"

# Windows
setx Jwt__Key "production-secret-key"

# Kubernetes Secret
kubectl create secret generic jwt-config --from-literal=Jwt__Key=production-key
```

### Secrets Checklist
- [ ] JWT signing key (minimum 32 characters)
- [ ] Database connection strings
- [ ] Third-party API keys
- [ ] Email service credentials
- [ ] Payment gateway credentials

---

## 8. Security Best Practices

### API Gateway
✅ Rate limiting enabled  
✅ Strict CORS policies  
✅ HTTPS enforced (production)  
✅ JWT validation before routing  

### Authentication Service
✅ Password strength validation  
✅ Account lockout protection  
✅ BCrypt password hashing  
✅ User enumeration prevention  

### Input Validation
✅ Sanitization utilities available  
✅ FluentValidation for DTOs  
✅ Parameterized database queries  
✅ Output encoding for HTML  

### Error Handling
✅ Global exception handler  
✅ No sensitive data in errors  
✅ RFC 7807 problem details  
✅ Structured logging  

### Deployment
⚠️ Move secrets to Key Vault  
⚠️ Enable HTTPS redirect  
⚠️ Implement distributed lockout  
⚠️ Configure security headers  

### Security Headers (TODO)
Add these in production:
```csharp
app.Use(async (context, next) =>
{Configure Redis for distributed lockout  
⚠️ Adjust CSP for production frontend"X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    await next();
});
```

---

## Testing Security Features

### Test Password Validation
```bash
# Should fail - too short
POST /auth/register
{ "password": "Short1!" }

# Should fail - no special character
POST /auth/register
{ "password": "LongPassword123" }

# Should fail - common password
POST /auth/register
{ "password": "Password123!" }

# Should succeed
POST /auth/register
{ "password": "MyStr0ng!Passw0rd" }
```

### Test Account Lockout
```bash
# 1st failed attempt
POST /auth/login
{ "username": "test@test.com", "password": "wrong" }
# Response: "Invalid credentials. 4 attempts remaining before lockout."

# 2nd-4th attempts
# ... same response with decreasing count

# 5th failed attempt
POST /auth/login
{ "username": "test@test.com", "password": "wrong" }
# Response: "Invalid credentials. Account has been locked."

# 6th attempt
POST /auth/login
{ "username": "test@test.com", "password": "wrong" }
# Response: "Account is locked due to multiple failed login attempts. Try again in 14 minutes."
```

### Test Rate Limiting
```bash
# Send 6 requests to login in quick succession
for i in {1..6}; do
  curl -X POST http://localhost:5000/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"test","password":"test"}'
done

# 6th request should return HTTP 429 Too Many Requests
```

---

## Security Incident Response

### In Case of Suspected Breach

1. **Immediate Actions**
   - Rotate JWT signing key
   - Invalidate all active tokens
   - Reset all user passwords
   - Enable maintenance mode

2. **Investigation**
   - Review application logs
   - Check for unusual login patterns
   - Analyze rate limiting logs
   - Examine database for SQL injection

3. **Communication**
   - Notify affected users
   - Document incident timeline
   - Report to compliance team

4. **Remediation**
   - Apply security patches
   - Update password policies
   - Increase lockout sensitivity
   - Review code for vulnerabilities

---

## Security Audit Log

| Date       | Change                     | By   | Reason                      |
|------------|----------------------------|------|-----------------------------|
| 2024-01-XX | Rate limiting implemented  | Team | Prevent DDoS attacks        |
| 2024-01-XX | Password policy (strict)   | Team | OWASP recommendations       |
| 2024-01-XX | Account lockout (5 attempts)| Team | Prevent brute force         |
| 2024-01-XX | CORS policies restricted   | Team | Prevent unauthorized access |
| 2024-02-12 | Distributed lockout (Dapr) | Team | Multi-instance support      |
| 2024-02-12 | Security headers           | Team | OWASP recommendations       |

---

## References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [Microsoft Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [BCrypt Documentation](https://github.com/BcryptNet/bcrypt.net)
- [AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit)

---

**Last Updated**: 2024  
**Maintained By**: DentalClinic Development Team
