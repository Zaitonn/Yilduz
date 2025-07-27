using System;
using Jint;
using Xunit;
using Yilduz.Storages.Storage;

namespace Yilduz.Tests.Storages;

public sealed class EventTests : TestBase
{
    private StorageInstance _localStorage = null!;
    private StorageInstance _sessionStorage = null!;
    private bool _eventTriggered;

    private StorageInstance GetActiveStorage(string type)
    {
        return type switch
        {
            "localStorage" => _localStorage,
            "sessionStorage" => _sessionStorage,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    protected override Options GetOptions()
    {
        return new()
        {
            CancellationToken = Token,
            Storage =
            {
                LocalStorageConfigurator = storage => _localStorage = storage,
                SessionStorageConfigurator = storage => _sessionStorage = storage,
            },
        };
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldTriggerEventOnRemoveItem(string type)
    {
        var storage = GetActiveStorage(type);

        Engine.Execute($"{type}.setItem('testKey', 'testValue');");

        storage.Updated += (_, args) =>
        {
            _eventTriggered = true;
            Assert.Equal("testKey", args.Key);
            Assert.Null(args.NewValue);
            Assert.Equal("testValue", args.OldValue);
        };

        Engine.Execute($"{type}.removeItem('testKey');");
        Assert.True(_eventTriggered);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldTriggerEventOnClear(string type)
    {
        var storage = type switch
        {
            "localStorage" => _localStorage,
            "sessionStorage" => _sessionStorage,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        Engine.Execute(
            $"""
            {type}.setItem('key1', 'value1');
            {type}.setItem('key2', 'value2');
            """
        );

        storage.Updated += (_, args) =>
        {
            _eventTriggered = true;
            Assert.Null(args.Key);
            Assert.Null(args.NewValue);
            Assert.Null(args.OldValue);
        };

        Engine.Execute($"{type}.clear();");
        Assert.True(_eventTriggered);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldTriggerEventOnItemUpdate(string type)
    {
        var eventCount = 0;
        var storage = GetActiveStorage(type);
        storage.Updated += (_, args) =>
        {
            eventCount++;
            if (eventCount == 1)
            {
                // First set
                Assert.Equal("updateKey", args.Key);
                Assert.Equal("initialValue", args.NewValue);
                Assert.Null(args.OldValue);
            }
            else if (eventCount == 2)
            {
                // Update
                Assert.Equal("updateKey", args.Key);
                Assert.Equal("updatedValue", args.NewValue);
                Assert.Equal("initialValue", args.OldValue);
            }
        };

        Engine.Execute(
            $"""
                {type}.setItem('updateKey', 'initialValue');
                {type}.setItem('updateKey', 'updatedValue');
            """
        );

        Assert.Equal(2, eventCount);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldNotTriggerEventOnNonExistentRemove(string type)
    {
        var storage = GetActiveStorage(type);
        storage.Updated += (_, _) => _eventTriggered = true;

        Engine.Execute($"{type}.removeItem('nonExistentKey');");
        Assert.False(_eventTriggered);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldTriggerEventWithCorrectStorageArea(string type)
    {
        var storage = GetActiveStorage(type);

        StorageInstance? eventStorageArea = null;
        storage.Updated += (_, args) => eventStorageArea = args.StorageArea;

        Engine.Execute($"{type}.setItem('areaKey', 'areaValue');");

        Assert.NotNull(eventStorageArea);
        Assert.Same(storage, eventStorageArea);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleSpecialCharactersInKeysAndValues(string type)
    {
        var storage = GetActiveStorage(type);
        storage.Updated += (_, args) =>
        {
            _eventTriggered = true;
            Assert.Equal("special_key@#$%", args.Key);
            Assert.Equal("special_value!@#$%^&*()", args.NewValue);
        };

        Engine.Execute($"{type}.setItem('special_key@#$%', 'special_value!@#$%^&*()');");
        Assert.True(_eventTriggered);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleNullAndUndefinedValues(string type)
    {
        var eventCount = 0;
        var storage = GetActiveStorage(type);

        storage.Updated += (_, args) =>
        {
            eventCount++;
            Assert.Equal("nullKey", args.Key);
            // Storage should convert null/undefined to string
            Assert.NotNull(args.NewValue);
        };

        Engine.Execute(
            $"""
                {type}.setItem('nullKey', null);
                {type}.setItem('nullKey', undefined);
            """
        );

        Assert.Equal(2, eventCount);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleLongKeysAndValues(string type)
    {
        var longKey = new string('k', 1000);
        var longValue = new string('v', 5000);
        var storage = GetActiveStorage(type);

        storage.Updated += (_, args) =>
        {
            _eventTriggered = true;
            Assert.Equal(longKey, args.Key);
            Assert.Equal(longValue, args.NewValue);
        };

        Engine.Execute($"{type}.setItem('{longKey}', '{longValue}');");
        Assert.True(_eventTriggered);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleNumericKeys(string type)
    {
        var storage = GetActiveStorage(type);
        storage.Updated += (_, args) =>
        {
            _eventTriggered = true;
            Assert.Equal("123", args.Key);
            Assert.Equal("numericValue", args.NewValue);
        };

        Engine.Execute($"{type}.setItem(123, 'numericValue');");
        Assert.True(_eventTriggered);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleEmptyKeysAndValues(string type)
    {
        var storage = GetActiveStorage(type);
        storage.Updated += (_, args) =>
        {
            _eventTriggered = true;
            Assert.Equal("", args.Key);
            Assert.Equal("", args.NewValue);
        };

        Engine.Execute($"{type}.setItem('', '');");
        Assert.True(_eventTriggered);
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldTriggerEventOnBracketNotationAssignment(string type)
    {
        var storage = GetActiveStorage(type);

        storage.Updated += (_, args) =>
        {
            _eventTriggered = true;
            Assert.Equal("bracketKey", args.Key);
            Assert.Equal("bracketValue", args.NewValue);
        };

        Engine.Execute($"{type}['bracketKey'] = 'bracketValue';");
        Assert.True(_eventTriggered);
    }
}
