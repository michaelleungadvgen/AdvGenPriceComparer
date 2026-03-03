using System;
using AdvGenPriceComparer.WPF.Commands;
using Xunit;

namespace AdvGenPriceComparer.Tests.ViewModels;

public class RelayCommandTests
{
    [Fact]
    public void Constructor_NullExecute_ThrowsArgumentNullException()
    {
        Action execute = null!;
        Assert.Throws<ArgumentNullException>(() => new RelayCommand(execute));
    }

    [Fact]
    public void CanExecute_NoCanExecuteDelegate_ReturnsTrue()
    {
        var command = new RelayCommand(() => { });
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void CanExecute_WithCanExecuteDelegate_ReturnsDelegateResult()
    {
        bool expected = false;
        var command = new RelayCommand(() => { }, () => expected);
        Assert.False(command.CanExecute(null));

        expected = true;
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void Execute_InvokesExecuteDelegate()
    {
        bool executed = false;
        var command = new RelayCommand(() => executed = true);

        command.Execute(null);

        Assert.True(executed);
    }

    [Fact]
    public void RaiseCanExecuteChanged_InvokesCanExecuteChangedEvent()
    {
        var command = new RelayCommand(() => { });
        bool eventRaised = false;
        command.CanExecuteChanged += (s, e) => eventRaised = true;

        command.RaiseCanExecuteChanged();

        Assert.True(eventRaised);
    }
}

public class GenericRelayCommandTests
{
    [Fact]
    public void Constructor_NullExecute_ThrowsArgumentNullException()
    {
        Action<string?> execute = null!;
        Assert.Throws<ArgumentNullException>(() => new RelayCommand<string>(execute));
    }

    [Fact]
    public void CanExecute_NoCanExecuteDelegate_ReturnsTrue()
    {
        var command = new RelayCommand<string>(p => { });
        Assert.True(command.CanExecute("test"));
    }

    [Fact]
    public void CanExecute_WithValidParameter_ReturnsDelegateResult()
    {
        var command = new RelayCommand<string>(
            p => { },
            p => p == "valid");

        Assert.True(command.CanExecute("valid"));
        Assert.False(command.CanExecute("invalid"));
    }

    [Fact]
    public void CanExecute_WithWrongTypeParameter_ReturnsFalse()
    {
        var command = new RelayCommand<string>(p => { }, p => true);
        Assert.False(command.CanExecute(123)); // int instead of string
    }

    [Fact]
    public void CanExecute_WithNullParameterForReferenceType_ReturnsDelegateResult()
    {
        var command = new RelayCommand<string>(
            p => { },
            p => p == null);

        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void CanExecute_WithNullParameterForValueType_ReturnsFalse()
    {
        var command = new RelayCommand<int>(
            p => { },
            p => true);

        // int cannot be null, so CanExecute should return false for null parameter
        Assert.False(command.CanExecute(null));
    }

    [Fact]
    public void Execute_WithValidParameter_InvokesExecuteDelegate()
    {
        string? result = null;
        var command = new RelayCommand<string>(p => result = p);

        command.Execute("test");

        Assert.Equal("test", result);
    }

    [Fact]
    public void Execute_WithWrongTypeParameter_DoesNotInvokeExecuteDelegate()
    {
        bool executed = false;
        var command = new RelayCommand<string>(p => executed = true);

        command.Execute(123); // int instead of string

        Assert.False(executed);
    }

    [Fact]
    public void Execute_WithNullParameterForReferenceType_InvokesExecuteDelegate()
    {
        bool executed = false;
        var command = new RelayCommand<string>(p =>
        {
            Assert.Null(p);
            executed = true;
        });

        command.Execute(null);

        Assert.True(executed);
    }

    [Fact]
    public void Execute_WithNullParameterForValueType_DoesNotInvokeExecuteDelegate()
    {
        bool executed = false;
        var command = new RelayCommand<int>(p => executed = true);

        command.Execute(null);

        Assert.False(executed);
    }

    [Fact]
    public void RaiseCanExecuteChanged_InvokesCanExecuteChangedEvent()
    {
        var command = new RelayCommand<string>(p => { });
        bool eventRaised = false;
        command.CanExecuteChanged += (s, e) => eventRaised = true;

        command.RaiseCanExecuteChanged();

        Assert.True(eventRaised);
    }
}
