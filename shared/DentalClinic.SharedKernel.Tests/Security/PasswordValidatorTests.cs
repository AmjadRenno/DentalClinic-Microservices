using DentalClinic.SharedKernel.Security;
using FluentAssertions;
using Xunit;

namespace DentalClinic.SharedKernel.Tests.Security;

public class PasswordValidatorTests
{
    [Fact]
    public void Validate_EmptyPassword_ShouldReturnError()
    {
        // Arrange
        var validator = new PasswordValidator();
        
        // Act
        var result = validator.Validate("");
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password cannot be empty");
    }
    
    [Fact]
    public void Validate_NullPassword_ShouldReturnError()
    {
        // Arrange
        var validator = new PasswordValidator();
        
        // Act
        var result = validator.Validate(null!);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password cannot be empty");
    }
    
    [Theory]
    [InlineData("Short1!")] // Too short (7 chars, min 8)
    [InlineData("1234567")] // Too short (7 chars)
    public void Validate_PasswordTooShort_ShouldReturnError(string password)
    {
        // Arrange
        var validator = new PasswordValidator(PasswordPolicy.Default);
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least 8 characters"));
    }
    
    [Fact]
    public void Validate_StrictPolicy_PasswordTooShort_ShouldReturnError()
    {
        // Arrange
        var validator = new PasswordValidator(PasswordPolicy.Strict);
        
        // Act
        var result = validator.Validate("Short123!");
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least 12 characters"));
    }
    
    [Theory]
    [InlineData("alllowercase123!")] // Missing uppercase
    [InlineData("password123!")]
    public void Validate_MissingUppercase_ShouldReturnError(string password)
    {
        // Arrange
        var validator = new PasswordValidator();
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("uppercase letter"));
    }
    
    [Theory]
    [InlineData("ALLUPPERCASE123!")] // Missing lowercase
    [InlineData("PASSWORD123!")]
    public void Validate_MissingLowercase_ShouldReturnError(string password)
    {
        // Arrange
        var validator = new PasswordValidator();
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("lowercase letter"));
    }
    
    [Theory]
    [InlineData("NoDigitsHere!")] // Missing digit
    [InlineData("PasswordOnly!")]
    public void Validate_MissingDigit_ShouldReturnError(string password)
    {
        // Arrange
        var validator = new PasswordValidator();
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("digit"));
    }
    
    [Theory]
    [InlineData("NoSpecialChar123")] // Missing special character
    [InlineData("Password1234")]
    public void Validate_MissingSpecialCharacter_ShouldReturnError(string password)
    {
        // Arrange
        var validator = new PasswordValidator();
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("special character"));
    }
    
    [Theory]
    [InlineData("AAAA11!!")]  // 8 chars with only 3 unique (A, 1, !) - minimum is 4
    public void Validate_NotEnoughUniqueCharacters_ShouldReturnError(string password)
    {
        // Arrange
        var validator = new PasswordValidator();
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("unique characters"));
    }
    
    [Theory]
    [InlineData("password")] // Common password
    [InlineData("123456")]
    [InlineData("qwerty")]
    [InlineData("admin")]
    [InlineData("password1")] // password is too common (without special char)
    public void Validate_CommonPassword_ShouldReturnError(string password)
    {
        // Arrange
        var validator = new PasswordValidator();
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("too common"));
    }
    
    [Theory]
    [InlineData("MyStr0ng!Pass")] // Valid password
    [InlineData("C0mpl3x!Password")]
    [InlineData("S3cur3#P@ssw0rd")]
    [InlineData("V@lid123Pass")]
    public void Validate_ValidPassword_DefaultPolicy_ShouldSucceed(string password)
    {
        // Arrange
        var validator = new PasswordValidator(PasswordPolicy.Default);
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData("MyVeryStr0ng!Password")] // Valid strict password (12+ chars)
    [InlineData("S3cur3P@ssw0rdHere")]
    [InlineData("Th1s!sV3ryS3cur3")]
    public void Validate_ValidPassword_StrictPolicy_ShouldSucceed(string password)
    {
        // Arrange
        var validator = new PasswordValidator(PasswordPolicy.Strict);
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void Validate_StrictPolicy_ValidPasswordWithSixUniqueChars_ShouldSucceed()
    {
        // Arrange
        var validator = new PasswordValidator(PasswordPolicy.Strict);
        var password = "AbCd1234!@#$"; // Has A, b, C, d, 1-4, !, @, #, $ = 10+ unique chars
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void Validate_MultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var validator = new PasswordValidator();
        var password = "short"; // Too short, no uppercase, no digit, no special char
        
        // Act
        var result = validator.Validate(password);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);
        result.Errors.Should().Contain(e => e.Contains("at least 8 characters"));
        result.Errors.Should().Contain(e => e.Contains("uppercase letter"));
        result.Errors.Should().Contain(e => e.Contains("digit"));
        result.Errors.Should().Contain(e => e.Contains("special character"));
    }
    
    [Fact]
    public void GenerateStrongPassword_ShouldMeetAllRequirements()
    {
        // Arrange
        var validator = new PasswordValidator(PasswordPolicy.Strict);
        
        // Act
        var password = PasswordValidator.GenerateStrongPassword(16);
        var result = validator.Validate(password);
        
        // Assert
        password.Should().HaveLength(16);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void GenerateStrongPassword_MultipleCalls_ShouldGenerateDifferentPasswords()
    {
        // Act
        var password1 = PasswordValidator.GenerateStrongPassword(16);
        var password2 = PasswordValidator.GenerateStrongPassword(16);
        var password3 = PasswordValidator.GenerateStrongPassword(16);
        
        // Assert
        password1.Should().NotBe(password2);
        password2.Should().NotBe(password3);
        password1.Should().NotBe(password3);
    }
    
    [Theory]
    [InlineData(12)]
    [InlineData(16)]
    [InlineData(20)]
    [InlineData(32)]
    public void GenerateStrongPassword_WithDifferentLengths_ShouldGenerateCorrectLength(int length)
    {
        // Act
        var password = PasswordValidator.GenerateStrongPassword(length);
        
        // Assert
        password.Should().HaveLength(length);
        password.Any(char.IsUpper).Should().BeTrue();
        password.Any(char.IsLower).Should().BeTrue();
        password.Any(char.IsDigit).Should().BeTrue();
        password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)).Should().BeTrue();
    }
    
    [Fact]
    public void PasswordPolicy_Default_ShouldHaveExpectedValues()
    {
        // Act
        var policy = PasswordPolicy.Default;
        
        // Assert
        policy.MinimumLength.Should().Be(8);
        policy.RequireUppercase.Should().BeTrue();
        policy.RequireLowercase.Should().BeTrue();
        policy.RequireDigit.Should().BeTrue();
        policy.RequireSpecialCharacter.Should().BeTrue();
        policy.MinimumUniqueCharacters.Should().Be(4);
    }
    
    [Fact]
    public void PasswordPolicy_Strict_ShouldHaveExpectedValues()
    {
        // Act
        var policy = PasswordPolicy.Strict;
        
        // Assert
        policy.MinimumLength.Should().Be(12);
        policy.RequireUppercase.Should().BeTrue();
        policy.RequireLowercase.Should().BeTrue();
        policy.RequireDigit.Should().BeTrue();
        policy.RequireSpecialCharacter.Should().BeTrue();
        policy.MinimumUniqueCharacters.Should().Be(6);
    }
    
    [Fact]
    public void PasswordPolicy_CustomPolicy_ShouldBeRespected()
    {
        // Arrange
        var customPolicy = new PasswordPolicy
        {
            MinimumLength = 10,
            RequireUppercase = false,
            RequireDigit = true,
            RequireSpecialCharacter = false,
            MinimumUniqueCharacters = 5
        };
        var validator = new PasswordValidator(customPolicy);
        
        // Act
        var result = validator.Validate("lowercase123456"); // 15 chars, no uppercase, no special
        
        // Assert
        result.IsValid.Should().BeTrue(); // Should pass because custom policy doesn't require uppercase/special
    }
}
