namespace CICD.Demo.Api.Tests;

public class UnitTest1
{
    [Fact]
    public void Add_TwoNumbers_ReturnsCorrectResult()
    {
        // Arrange
        int firstNumber = 2;
        int secondNumber = 3;

        // Act
        int result = firstNumber + secondNumber;

        // Assert
        Assert.Equal(5, result);
    }
}
