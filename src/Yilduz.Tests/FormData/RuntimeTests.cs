using System.Linq;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.FormData;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void AppendShouldWork()
    {
        Engine.Execute("const formData = new FormData();");
        Engine.Execute("formData.append('key', 'value');");
        var hasKey = Engine.Evaluate("formData.has('key')").AsBoolean();
        var getValue = Engine.Evaluate("formData.get('key')").AsString();

        Assert.True(hasKey);
        Assert.Equal("value", getValue);
    }

    [Fact]
    public void SetShouldWork()
    {
        Engine.Execute("const formData = new FormData();");
        Engine.Execute("formData.append('key', 'value1');");
        Engine.Execute("formData.append('key', 'value2');");
        Engine.Execute("formData.set('key', 'newValue');");

        var getValue = Engine.Evaluate("formData.get('key')").AsString();
        var allValues = Engine.Evaluate("formData.getAll('key')");

        Assert.Equal("newValue", getValue);
        Assert.Single(allValues.AsArray());
    }

    [Fact]
    public void DeleteShouldWork()
    {
        Engine.Execute("const formData = new FormData();");
        Engine.Execute("formData.append('key', 'value');");
        Engine.Execute("formData.delete('key');");
        var hasKey = Engine.Evaluate("formData.has('key')").AsBoolean();

        Assert.False(hasKey);
    }

    [Fact]
    public void ForEachShouldWork()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key1', 'value1');
            formData.append('key2', 'value2');

            const results = [];
            formData.forEach((value, key) => {
                results.push({key, value});
            });
            """
        );

        var results = Engine.Evaluate("results").AsArray();
        Assert.Equal<uint>(2, results.Length);
    }

    [Fact]
    public void EntriesShouldWork()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key1', 'value1');
            formData.append('key2', 'value2');

            const entries = Array.from(formData.entries());
            """
        );

        var entries = Engine.Evaluate("entries").AsArray();
        Assert.Equal<uint>(2, entries.Length);
    }

    [Fact]
    public void KeysShouldWork()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key1', 'value1');
            formData.append('key2', 'value2');

            const keys = Array.from(formData.keys());
            """
        );

        var keys = Engine.Evaluate("keys").AsArray();
        Assert.Equal<uint>(2, keys.Length);
        Assert.Equal("key1", keys[0].AsString());
        Assert.Equal("key2", keys[1].AsString());
    }

    [Fact]
    public void ValuesShouldWork()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key1', 'value1');
            formData.append('key2', 'value2');

            const values = Array.from(formData.values());
            """
        );

        var values = Engine.Evaluate("values").AsArray();
        Assert.Equal<uint>(2, values.Length);
        Assert.Equal("value1", values[0].AsString());
        Assert.Equal("value2", values[1].AsString());
    }

    [Fact]
    public void GetAllShouldReturnAllValuesForKey()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key', 'value1');
            formData.append('key', 'value2');
            formData.append('key', 'value3');
            formData.append('other', 'otherValue');

            const allValues = formData.getAll('key');
            const otherValues = formData.getAll('other');
            const nonExistentValues = formData.getAll('nonExistent');
            """
        );

        var allValues = Engine.Evaluate("allValues").AsArray();
        var otherValues = Engine.Evaluate("otherValues").AsArray();
        var nonExistentValues = Engine.Evaluate("nonExistentValues").AsArray();

        Assert.Equal<uint>(3, allValues.Length);
        Assert.Equal("value1", allValues[0].AsString());
        Assert.Equal("value2", allValues[1].AsString());
        Assert.Equal("value3", allValues[2].AsString());

        Assert.Single(otherValues);
        Assert.Equal("otherValue", otherValues[0].AsString());

        Assert.Empty(nonExistentValues);
    }

    [Fact]
    public void GetShouldReturnFirstValueOrNull()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key', 'value1');
            formData.append('key', 'value2');

            const firstValue = formData.get('key');
            const nonExistentValue = formData.get('nonExistent');
            """
        );

        var firstValue = Engine.Evaluate("firstValue").AsString();
        var nonExistentValue = Engine.Evaluate("nonExistentValue");

        Assert.Equal("value1", firstValue);
        Assert.True(nonExistentValue.IsNull());
    }

    [Fact]
    public void SetShouldReplaceAllExistingValues()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key', 'value1');
            formData.append('key', 'value2');
            formData.append('key', 'value3');

            const beforeSet = formData.getAll('key');
            formData.set('key', 'newValue');
            const afterSet = formData.getAll('key');
            """
        );

        var beforeSet = Engine.Evaluate("beforeSet").AsArray();
        var afterSet = Engine.Evaluate("afterSet").AsArray();

        Assert.Equal<uint>(3, beforeSet.Length);
        Assert.Single(afterSet);
        Assert.Equal("newValue", afterSet[0].AsString());
    }

    [Fact]
    public void HasShouldReturnCorrectBoolean()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('existing', 'value');

            const hasExisting = formData.has('existing');
            const hasNonExistent = formData.has('nonExistent');
            """
        );

        var hasExisting = Engine.Evaluate("hasExisting").AsBoolean();
        var hasNonExistent = Engine.Evaluate("hasNonExistent").AsBoolean();

        Assert.True(hasExisting);
        Assert.False(hasNonExistent);
    }

    [Fact]
    public void DeleteShouldRemoveAllValuesForKey()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key', 'value1');
            formData.append('key', 'value2');
            formData.append('other', 'otherValue');

            const beforeDelete = formData.has('key');
            formData.delete('key');
            const afterDelete = formData.has('key');
            const otherExists = formData.has('other');
            """
        );

        var beforeDelete = Engine.Evaluate("beforeDelete").AsBoolean();
        var afterDelete = Engine.Evaluate("afterDelete").AsBoolean();
        var otherExists = Engine.Evaluate("otherExists").AsBoolean();

        Assert.True(beforeDelete);
        Assert.False(afterDelete);
        Assert.True(otherExists);
    }

    [Fact]
    public void ForEachShouldPassCorrectParameters()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('name', 'John');
            formData.append('age', '30');

            const results = [];
            formData.forEach(function(value, key, formDataRef) {
                results.push({
                    value: value,
                    key: key,
                    isFormData: formDataRef === formData
                });
            });
            """
        );

        var results = Engine.Evaluate("results").AsArray();
        Assert.Equal<uint>(2, results.Length);

        var firstResult = results[0].AsObject();
        Assert.Equal("John", firstResult.Get("value").AsString());
        Assert.Equal("name", firstResult.Get("key").AsString());
        Assert.True(firstResult.Get("isFormData").AsBoolean());

        var secondResult = results[1].AsObject();
        Assert.Equal("30", secondResult.Get("value").AsString());
        Assert.Equal("age", secondResult.Get("key").AsString());
        Assert.True(secondResult.Get("isFormData").AsBoolean());
    }

    [Fact]
    public void ForEachShouldSupportThisArg()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key', 'value');

            const thisArg = { prefix: 'PREFIX: ' };
            let capturedThis = null;

            formData.forEach(function(value, key) {
                capturedThis = this;
            }, thisArg);
            """
        );

        var capturedThis = Engine.Evaluate("capturedThis").AsObject();
        Assert.Equal("PREFIX: ", capturedThis.Get("prefix").AsString());
    }

    [Fact]
    public void IteratorsShouldWorkCorrectly()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('name', 'John');
            formData.append('age', '30');

            // Test entries iterator
            const entriesArray = [];
            for (const entry of formData.entries()) {
                entriesArray.push(entry);
            }

            // Test keys iterator
            const keysArray = [];
            for (const key of formData.keys()) {
                keysArray.push(key);
            }

            // Test values iterator
            const valuesArray = [];
            for (const value of formData.values()) {
                valuesArray.push(value);
            }

            // Test default iterator (should be same as entries)
            const defaultArray = [];
            for (const entry of formData) {
                defaultArray.push(entry);
            }
            """
        );

        var entriesArray = Engine.Evaluate("entriesArray").AsArray();
        var keysArray = Engine.Evaluate("keysArray").AsArray();
        var valuesArray = Engine.Evaluate("valuesArray").AsArray();
        var defaultArray = Engine.Evaluate("defaultArray").AsArray();

        Assert.Equal<uint>(2, entriesArray.Length);
        Assert.Equal<uint>(2, keysArray.Length);
        Assert.Equal<uint>(2, valuesArray.Length);
        Assert.Equal<uint>(2, defaultArray.Length);

        // Check entries format
        var firstEntry = entriesArray[0].AsArray();
        Assert.Equal("name", firstEntry[0].AsString());
        Assert.Equal("John", firstEntry[1].AsString());

        // Check keys
        Assert.Equal("name", keysArray[0].AsString());
        Assert.Equal("age", keysArray[1].AsString());

        // Check values
        Assert.Equal("John", valuesArray[0].AsString());
        Assert.Equal("30", valuesArray[1].AsString());

        // Check default iterator is same as entries
        var firstDefaultEntry = defaultArray[0].AsArray();
        Assert.Equal("name", firstDefaultEntry[0].AsString());
        Assert.Equal("John", firstDefaultEntry[1].AsString());
    }

    [Fact]
    public void ShouldHandleEmptyFormData()
    {
        Engine.Execute(
            """
            const formData = new FormData();

            const hasAny = formData.has('anything');
            const getValue = formData.get('anything');
            const getAllValues = formData.getAll('anything');
            const keys = Array.from(formData.keys());
            const values = Array.from(formData.values());
            const entries = Array.from(formData.entries());

            let forEachCalled = false;
            formData.forEach(() => { forEachCalled = true; });
            """
        );

        var hasAny = Engine.Evaluate("hasAny").AsBoolean();
        var getValue = Engine.Evaluate("getValue");
        var getAllValues = Engine.Evaluate("getAllValues").AsArray();
        var keys = Engine.Evaluate("keys").AsArray();
        var values = Engine.Evaluate("values").AsArray();
        var entries = Engine.Evaluate("entries").AsArray();
        var forEachCalled = Engine.Evaluate("forEachCalled").AsBoolean();

        Assert.False(hasAny);
        Assert.True(getValue.IsNull());
        Assert.Empty(getAllValues);
        Assert.Empty(keys);
        Assert.Empty(values);
        Assert.Empty(entries);
        Assert.False(forEachCalled);
    }

    [Fact]
    public void ShouldHandleSpecialCharacters()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('key with spaces', 'value with spaces');
            formData.append('key!@#$%^&*()', 'value!@#$%^&*()');
            formData.append('unicode🌟', 'unicode🌟value');
            formData.append('empty', '');

            const normalValue = formData.get('key with spaces');
            const specialValue = formData.get('key!@#$%^&*()');
            const unicodeValue = formData.get('unicode🌟');
            const emptyValue = formData.get('empty');
            """
        );

        var normalValue = Engine.Evaluate("normalValue").AsString();
        var specialValue = Engine.Evaluate("specialValue").AsString();
        var unicodeValue = Engine.Evaluate("unicodeValue").AsString();
        var emptyValue = Engine.Evaluate("emptyValue").AsString();

        Assert.Equal("value with spaces", normalValue);
        Assert.Equal("value!@#$%^&*()", specialValue);
        Assert.Equal("unicode🌟value", unicodeValue);
        Assert.Equal("", emptyValue);
    }

    [Fact]
    public void ShouldThrowErrorForInvalidArguments()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("const formData = new FormData(); formData.append('key');")
        );
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("const formData = new FormData(); formData.get();")
        );
    }

    [Fact]
    public void ShouldMaintainInsertionOrder()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('third', '3');
            formData.append('first', '1');
            formData.append('second', '2');
            formData.append('first', '1.1');

            const keys = Array.from(formData.keys());
            const values = Array.from(formData.values());
            """
        );

        var keys = Engine.Evaluate("keys").AsArray();
        var values = Engine.Evaluate("values").AsArray();

        Assert.Equal<uint>(4, keys.Length);
        Assert.Equal("third", keys[0].AsString());
        Assert.Equal("first", keys[1].AsString());
        Assert.Equal("second", keys[2].AsString());
        Assert.Equal("first", keys[3].AsString());

        Assert.Equal("3", values[0].AsString());
        Assert.Equal("1", values[1].AsString());
        Assert.Equal("2", values[2].AsString());
        Assert.Equal("1.1", values[3].AsString());
    }

    [Fact]
    public void ShouldCreateEmptyFormData()
    {
        Engine.Execute("const formData = new FormData();");

        var formData = Engine.Evaluate("formData");
        Assert.NotNull(formData);
        Assert.False(formData.IsNull());
        Assert.False(formData.IsUndefined());
    }

    [Fact]
    public void ShouldHandleTypeConversions()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('number', 42);
            formData.append('boolean', true);
            formData.append('null', null);
            formData.append('undefined', undefined);
            formData.append('object', {toString: () => 'object-string'});

            const numberValue = formData.get('number');
            const booleanValue = formData.get('boolean');
            const nullValue = formData.get('null');
            const undefinedValue = formData.get('undefined');
            const objectValue = formData.get('object');
            """
        );

        var numberValue = Engine.Evaluate("numberValue").AsString();
        var booleanValue = Engine.Evaluate("booleanValue").AsString();
        var nullValue = Engine.Evaluate("nullValue").AsString();
        var undefinedValue = Engine.Evaluate("undefinedValue").AsString();
        var objectValue = Engine.Evaluate("objectValue").AsString();

        Assert.Equal("42", numberValue);
        Assert.Equal("true", booleanValue);
        Assert.Equal("null", nullValue);
        Assert.Equal("undefined", undefinedValue);
        Assert.Equal("object-string", objectValue);
    }

    [Fact]
    public void ShouldHandleLargeDataSets()
    {
        Engine.Execute(
            """
            const formData = new FormData();

            // Add 1000 entries
            for (let i = 0; i < 1000; i++) {
                formData.append('key' + i, 'value' + i);
            }

            const firstValue = formData.get('key0');
            const lastValue = formData.get('key999');
            const middleValue = formData.get('key500');
            const allKeys = Array.from(formData.keys());
            """
        );

        var firstValue = Engine.Evaluate("firstValue").AsString();
        var lastValue = Engine.Evaluate("lastValue").AsString();
        var middleValue = Engine.Evaluate("middleValue").AsString();
        var allKeys = Engine.Evaluate("allKeys").AsArray();

        Assert.Equal("value0", firstValue);
        Assert.Equal("value999", lastValue);
        Assert.Equal("value500", middleValue);
        Assert.Equal<uint>(1000, allKeys.Length);
    }

    [Fact]
    public void ShouldHandleMethodChaining()
    {
        Engine.Execute(
            """
            const formData = new FormData();

            // These methods should return undefined and be chainable conceptually
            const appendResult = formData.append('key1', 'value1');
            const setResult = formData.set('key2', 'value2');
            const deleteResult = formData.delete('nonexistent');

            const hasKey1 = formData.has('key1');
            const hasKey2 = formData.has('key2');
            """
        );

        var appendResult = Engine.Evaluate("appendResult");
        var setResult = Engine.Evaluate("setResult");
        var deleteResult = Engine.Evaluate("deleteResult");
        var hasKey1 = Engine.Evaluate("hasKey1").AsBoolean();
        var hasKey2 = Engine.Evaluate("hasKey2").AsBoolean();

        Assert.True(appendResult.IsUndefined());
        Assert.True(setResult.IsUndefined());
        Assert.True(deleteResult.IsUndefined());
        Assert.True(hasKey1);
        Assert.True(hasKey2);
    }

    [Fact]
    public void ShouldHandleIteratorProtocol()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('a', '1');
            formData.append('b', '2');

            // Test that iterators have correct protocol
            const entriesIterator = formData.entries();
            const keysIterator = formData.keys();
            const valuesIterator = formData.values();

            const hasNextMethod = typeof entriesIterator.next === 'function';
            const hasIteratorSymbol = typeof entriesIterator[Symbol.iterator] === 'function';
            const iteratorReturnsItself = entriesIterator[Symbol.iterator]() === entriesIterator;

            // Test manual iteration
            const firstEntry = entriesIterator.next();
            const secondEntry = entriesIterator.next();
            const thirdEntry = entriesIterator.next();
            """
        );

        var hasNextMethod = Engine.Evaluate("hasNextMethod").AsBoolean();
        var hasIteratorSymbol = Engine.Evaluate("hasIteratorSymbol").AsBoolean();
        var iteratorReturnsItself = Engine.Evaluate("iteratorReturnsItself").AsBoolean();

        var firstEntry = Engine.Evaluate("firstEntry").AsObject();
        var secondEntry = Engine.Evaluate("secondEntry").AsObject();
        var thirdEntry = Engine.Evaluate("thirdEntry").AsObject();

        Assert.True(hasNextMethod);
        Assert.True(hasIteratorSymbol);
        Assert.True(iteratorReturnsItself);

        // Check iterator results
        Assert.False(firstEntry.Get("done").AsBoolean());
        Assert.False(secondEntry.Get("done").AsBoolean());
        Assert.True(thirdEntry.Get("done").AsBoolean());

        var firstValue = firstEntry.Get("value").AsArray();
        Assert.Equal("a", firstValue[0].AsString());
        Assert.Equal("1", firstValue[1].AsString());
    }

    [Fact]
    public void ShouldHandleConcurrentModification()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('a', '1');
            formData.append('b', '2');
            formData.append('c', '3');

            const results = [];
            let count = 0;

            formData.forEach((value, key) => {
                results.push({key, value});
                count++;
                
                // Modify during iteration
                if (count === 1) {
                    formData.append('d', '4');
                }
                if (count === 2) {
                    formData.delete('c');
                }
            });
            """
        );

        var results = Engine.Evaluate("results").AsArray();
        var count = Engine.Evaluate("count").AsNumber();

        // Should still iterate over the original entries
        Assert.Equal<uint>(3, results.Length);
        Assert.Equal(3, count);
    }

    [Fact]
    public void ShouldWorkWithSpread()
    {
        Engine.Execute(
            """
            const formData = new FormData();
            formData.append('name', 'John');
            formData.append('age', '30');

            const entriesArray = [...formData];
            const keysArray = [...formData.keys()];
            const valuesArray = [...formData.values()];
            """
        );

        var entriesArray = Engine.Evaluate("entriesArray").AsArray();
        var keysArray = Engine.Evaluate("keysArray").AsArray();
        var valuesArray = Engine.Evaluate("valuesArray").AsArray();

        Assert.Equal<uint>(2, entriesArray.Length);
        Assert.Equal<uint>(2, keysArray.Length);
        Assert.Equal<uint>(2, valuesArray.Length);

        var firstEntry = entriesArray[0].AsArray();
        Assert.Equal("name", firstEntry[0].AsString());
        Assert.Equal("John", firstEntry[1].AsString());
    }

    [Fact]
    public void ShouldHandleExtremeCases()
    {
        Engine.Execute(
            """
            const formData = new FormData();

            // Very long strings
            const longKey = 'x'.repeat(10000);
            const longValue = 'y'.repeat(10000);
            formData.append(longKey, longValue);

            const retrievedValue = formData.get(longKey);

            // Many duplicate keys
            for (let i = 0; i < 100; i++) {
                formData.append('duplicate', 'value' + i);
            }

            const allDuplicates = formData.getAll('duplicate');
            """
        );

        var retrievedValue = Engine.Evaluate("retrievedValue").AsString();
        var allDuplicates = Engine.Evaluate("allDuplicates").AsArray();

        Assert.Equal(10000, retrievedValue.Length);
        Assert.True(retrievedValue.All(c => c == 'y'));
        Assert.Equal<uint>(100, allDuplicates.Length);
    }

    [Fact]
    public void ShouldMaintainReferenceIntegrity()
    {
        Engine.Execute(
            """
            const formData1 = new FormData();
            const formData2 = new FormData();

            formData1.append('key', 'value1');
            formData2.append('key', 'value2');

            const value1 = formData1.get('key');
            const value2 = formData2.get('key');

            // Should not interfere with each other
            const has1 = formData1.has('key');
            const has2 = formData2.has('key');
            """
        );

        var value1 = Engine.Evaluate("value1").AsString();
        var value2 = Engine.Evaluate("value2").AsString();
        var has1 = Engine.Evaluate("has1").AsBoolean();
        var has2 = Engine.Evaluate("has2").AsBoolean();

        Assert.Equal("value1", value1);
        Assert.Equal("value2", value2);
        Assert.True(has1);
        Assert.True(has2);
    }
}
