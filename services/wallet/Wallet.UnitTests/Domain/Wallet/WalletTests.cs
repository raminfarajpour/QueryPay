using FluentAssertions;
using Wallet.Domain.WalletAggregate;
using Xunit;

namespace Wallet.UnitTests.Domain.Wallet;

public class WalletTests
{
    [Fact]
    public void InsufficientBalanceException_Should_Inherit_From_Exception()
    {
        // Act
        var exception = new InsufficientBalanceException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Money_OperatorTests_Should_Handle_Null_Scenarios()
    {
        // Arrange
        Money nullMoney = null;
        Money nonNullMoney = new Money(10);

        // Act & Assert
        (nullMoney < nonNullMoney).Should().BeTrue();
        (nullMoney > nonNullMoney).Should().BeFalse();
        (nullMoney + nonNullMoney).Amount.Should().Be(10);
        (nonNullMoney - nullMoney).Amount.Should().Be(10);
    }
    

    [Fact]
    public void Owner_Should_Contain_Correct_Values()
    {
        // Arrange
        var owner = new Owner(1, "1234567890");

        // Act & Assert
        owner.UserId.Should().Be(1);
        owner.Mobile.Should().Be("1234567890");
    }

    [Fact]
    public void Transaction_Should_Set_Properties_Correctly()
    {
        // Arrange
        var transactionInfo = new TransactionInfo("TID", "RID", "Test");
        var amount = new Money(100);
        var balanceBefore = new Money(200);
        var balanceAfter = new Money(300);

        // Act
        var transaction = new Transaction(transactionInfo, amount, balanceBefore, balanceAfter, TransactionDirection.Increase);

        // Assert
        transaction.TransactionInfo.Should().Be(transactionInfo);
        transaction.Amount.Should().Be(amount);
        transaction.BalanceBefore.Should().Be(balanceBefore);
        transaction.BalanceAfter.Should().Be(balanceAfter);
        transaction.Direction.Should().Be(TransactionDirection.Increase);
    }

    [Fact]
    public void Wallet_Should_Throw_Exception_On_Insufficient_Balance()
    {
        // Arrange
        var wallet = new global::Wallet.Domain.WalletAggregate.Wallet();
        wallet.Create(new Money(100), new Money(50), new Owner(1, "1234567890"));
        var transactionInfo = new TransactionInfo("TID", "RID", "Withdrawal");

        // Act
        Action action = () => wallet.Withdraw(new Money(200), transactionInfo);

        // Assert
        action.Should().Throw<InsufficientBalanceException>();
    }

    [Fact]
    public void Wallet_Should_Add_Transaction_On_Deposit()
    {
        // Arrange
        var wallet = new global::Wallet.Domain.WalletAggregate.Wallet();
        wallet.Create(new Money(100), new Money(50), new Owner(1, "1234567890"));
        var transactionInfo = new TransactionInfo("TID", "RID", "Deposit");

        // Act
        wallet.Deposit(new Money(50), transactionInfo);

        // Assert
        wallet.Transactions.Should().HaveCount(2);
        wallet.Transactions.Should().ContainSingle(t => t.Amount.Amount == 50 && t.Direction == TransactionDirection.Increase);
    }

    [Fact]
    public void Wallet_Should_Handle_Initial_Balance_Creation()
    {
        // Act
        var wallet = new global::Wallet.Domain.WalletAggregate.Wallet();
        wallet.Create(new Money(100), new Money(50), new Owner(1, "1234567890"));

        // Assert
        wallet.Balance.Amount.Should().Be(100);
        wallet.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public void Wallet_ClearTransaction_Should_Remove_All_Transactions()
    {
        // Arrange
        var wallet = new global::Wallet.Domain.WalletAggregate.Wallet();
        wallet.Create(new Money(100), new Money(50), new Owner(1, "1234567890"));
        var transactionInfo = new TransactionInfo("TID", "RID", "Deposit");
        wallet.Deposit(new Money(50), transactionInfo);

        // Act
        wallet.ClearTransaction();

        // Assert
        wallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void TransactionInfo_Should_Contain_Correct_Values()
    {
        // Arrange
        var transactionInfo = new TransactionInfo("TID", "RID", "Description");

        // Assert
        transactionInfo.TransactionId.Should().Be("TID");
        transactionInfo.ReferenceId.Should().Be("RID");
        transactionInfo.Description.Should().Be("Description");
    }
}