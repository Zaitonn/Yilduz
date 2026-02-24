using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class TeeTests : TestBase
{
    [Fact]
    public void ShouldCreateTwoBranchesFromStream()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                    controller.close();
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            """
        );

        Assert.True(Evaluate("branch1 instanceof ReadableStream").AsBoolean());
        Assert.True(Evaluate("branch2 instanceof ReadableStream").AsBoolean());
        Assert.False(Evaluate("branch1 === branch2").AsBoolean());
    }

    [Fact]
    public void ShouldLockOriginalStreamAfterTee()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('data');
                    controller.close();
                }
            });

            const lockedBefore = sourceStream.locked;
            const [branch1, branch2] = sourceStream.tee();
            const lockedAfter = sourceStream.locked;
            """
        );

        Assert.False(Evaluate("lockedBefore").AsBoolean());
        Assert.True(Evaluate("lockedAfter").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenTeeingLockedStream()
    {
        Execute(
            """
            const sourceStream = new ReadableStream();
            const reader = sourceStream.getReader(); // Lock the stream
            """
        );

        Assert.Throws<JavaScriptException>(() => Execute("sourceStream.tee()"));
    }

    [Fact]
    public void ShouldReadSameDataFromBothBranches()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                    controller.enqueue('chunk3');
                    controller.close();
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            const reader1 = branch1.getReader();
            const reader2 = branch2.getReader();

            let data1 = [];
            let data2 = [];
            let done1 = false;
            let done2 = false;

            async function readAll() {
                while (true) {
                    const { value, done } = await reader1.read();
                    if (done) { done1 = true; break; }
                    data1.push(value);
                }
                while (true) {
                    const { value, done } = await reader2.read();
                    if (done) { done2 = true; break; }
                    data2.push(value);
                }
            }
            """
        );

        Evaluate("readAll()").UnwrapIfPromise();

        Assert.Equal(3, Evaluate("data1.length").AsNumber());
        Assert.Equal(3, Evaluate("data2.length").AsNumber());
        Assert.Equal("chunk1", Evaluate("data1[0]").AsString());
        Assert.Equal("chunk2", Evaluate("data1[1]").AsString());
        Assert.Equal("chunk3", Evaluate("data1[2]").AsString());
        Assert.Equal("chunk1", Evaluate("data2[0]").AsString());
        Assert.Equal("chunk2", Evaluate("data2[1]").AsString());
        Assert.Equal("chunk3", Evaluate("data2[2]").AsString());
        Assert.True(Evaluate("done1").AsBoolean());
        Assert.True(Evaluate("done2").AsBoolean());
    }

    [Fact]
    public void ShouldReadIndependentlyFromEachBranch()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('a');
                    controller.enqueue('b');
                    controller.close();
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            const reader1 = branch1.getReader();

            let values1 = [];
            let done1 = false;

            async function readBranch1() {
                while (true) {
                    const { value, done } = await reader1.read();
                    if (done) { done1 = true; break; }
                    values1.push(value);
                }
            }
            """
        );

        // Read only from branch1; branch2 stays untouched
        Evaluate("readBranch1()").UnwrapIfPromise();

        Assert.Equal("a", Evaluate("values1[0]").AsString());
        Assert.Equal("b", Evaluate("values1[1]").AsString());
        Assert.True(Evaluate("done1").AsBoolean());
        // branch2 has never been locked — it's an independent stream
        Assert.False(Evaluate("branch2.locked").AsBoolean());
    }

    [Fact]
    public void ShouldReadFromBranch2AfterBranch1IsFullyRead()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('x');
                    controller.enqueue('y');
                    controller.close();
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            const reader1 = branch1.getReader();
            const reader2 = branch2.getReader();

            let values1 = [];
            let values2 = [];
            let done1 = false;
            let done2 = false;

            async function readSequential() {
                // Read branch1 completely first
                while (true) {
                    const { value, done } = await reader1.read();
                    if (done) { done1 = true; break; }
                    values1.push(value);
                }
                // Then read branch2
                while (true) {
                    const { value, done } = await reader2.read();
                    if (done) { done2 = true; break; }
                    values2.push(value);
                }
            }
            """
        );

        Evaluate("readSequential()").UnwrapIfPromise();

        Assert.Equal(2, Evaluate("values1.length").AsNumber());
        Assert.Equal(2, Evaluate("values2.length").AsNumber());
        Assert.Equal("x", Evaluate("values1[0]").AsString());
        Assert.Equal("y", Evaluate("values1[1]").AsString());
        Assert.Equal("x", Evaluate("values2[0]").AsString());
        Assert.Equal("y", Evaluate("values2[1]").AsString());
        Assert.True(Evaluate("done1").AsBoolean());
        Assert.True(Evaluate("done2").AsBoolean());
    }

    [Fact]
    public void ShouldHandleErrorInOriginalStream()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.error(new Error('source error'));
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            const reader1 = branch1.getReader();
            const reader2 = branch2.getReader();

            let error1 = null;
            let error2 = null;

            async function readBranches() {
                try {
                    await reader1.read(); // reads chunk1
                    await reader1.read(); // should throw
                } catch (e) {
                    error1 = e.message;
                }
                try {
                    await reader2.read(); // reads chunk1
                    await reader2.read(); // should throw
                } catch (e) {
                    error2 = e.message;
                }
            }
            """
        );

        Evaluate("readBranches()").UnwrapIfPromise();

        Assert.Equal("source error", Evaluate("error1").AsString());
        Assert.Equal("source error", Evaluate("error2").AsString());
    }

    [Fact]
    public void ShouldHandleCloseInOriginalStream()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.close();
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            const reader1 = branch1.getReader();
            const reader2 = branch2.getReader();

            let value1 = null;
            let value2 = null;
            let done1 = false;
            let done2 = false;

            async function readChunks() {
                const r1a = await reader1.read();
                value1 = r1a.value;
                const r1b = await reader1.read();
                done1 = r1b.done;

                const r2a = await reader2.read();
                value2 = r2a.value;
                const r2b = await reader2.read();
                done2 = r2b.done;
            }
            """
        );

        Evaluate("readChunks()").UnwrapIfPromise();

        Assert.Equal("chunk1", Evaluate("value1").AsString());
        Assert.Equal("chunk1", Evaluate("value2").AsString());
        Assert.True(Evaluate("done1").AsBoolean());
        Assert.True(Evaluate("done2").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleEmptyStream()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.close();
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            const reader1 = branch1.getReader();
            const reader2 = branch2.getReader();

            let done1 = false;
            let done2 = false;

            reader1.read().then(r => { done1 = r.done; });
            reader2.read().then(r => { done2 = r.done; });
            """
        );

        await WaitForJsConditionAsync("done1 === true && done2 === true");

        Assert.True(Evaluate("done1").AsBoolean());
        Assert.True(Evaluate("done2").AsBoolean());
    }

    [Fact]
    public void ShouldNotCancelSourceWhenOnlyOneBranchIsCancelled()
    {
        Execute(
            """
            let cancelCalled = false;
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                },
                cancel(reason) {
                    cancelCalled = true;
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            branch1.cancel('branch1 cancelled');
            """
        );

        // Source should NOT be cancelled while branch2 is still open
        Assert.False(Evaluate("cancelCalled").AsBoolean());
        // branch2 should still be readable
        Assert.False(Evaluate("branch2.locked").AsBoolean());
    }

    [Fact]
    public void ShouldCancelSourceWhenBothBranchesAreCancelled()
    {
        Execute(
            """
            let cancelCalled = false;
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                },
                cancel(reason) {
                    cancelCalled = true;
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            branch1.cancel('branch1 cancelled');
            branch2.cancel('branch2 cancelled');
            """
        );

        Assert.True(Evaluate("cancelCalled").AsBoolean());
    }

    [Fact]
    public void ShouldAllowBranch2ToReadAfterBranch1IsCancelled()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('A');
                    controller.enqueue('B');
                    controller.close();
                }
            });

            const [branch1, branch2] = sourceStream.tee();
            branch1.cancel('done with branch1')
            """
        );

        Execute(
            """
            const reader2 = branch2.getReader();
            let data2 = [];
            let done2 = false;

            async function readBranch2() {
                while (true) {
                    const { value, done } = await reader2.read();
                    if (done) { done2 = true; break; }
                    data2.push(value);
                }
            }
            """
        );

        Evaluate("readBranch2()").UnwrapIfPromise();

        Assert.Equal(2, Evaluate("data2.length").AsNumber());
        Assert.Equal("A", Evaluate("data2[0]").AsString());
        Assert.Equal("B", Evaluate("data2[1]").AsString());
        Assert.True(Evaluate("done2").AsBoolean());
    }

    [Fact]
    public void ShouldMaintainStreamState()
    {
        Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('data');
                }
            });

            const [branch1, branch2] = sourceStream.tee();

            const branch1Locked = branch1.locked;
            const branch2Locked = branch2.locked;

            const reader1 = branch1.getReader();
            const branch1LockedAfter = branch1.locked;
            """
        );

        Assert.False(Evaluate("branch1Locked").AsBoolean());
        Assert.False(Evaluate("branch2Locked").AsBoolean());
        Assert.True(Evaluate("branch1LockedAfter").AsBoolean());
    }

    [Fact]
    public void ShouldProduceTwoIndependentReadableStreamInstances()
    {
        Execute(
            """
            const sourceStream = new ReadableStream();
            const branches = sourceStream.tee();
            """
        );

        // tee() returns an array of exactly 2 elements
        Assert.Equal(2, Evaluate("branches.length").AsNumber());
        Assert.True(Evaluate("branches[0] instanceof ReadableStream").AsBoolean());
        Assert.True(Evaluate("branches[1] instanceof ReadableStream").AsBoolean());
        Assert.False(Evaluate("branches[0] === branches[1]").AsBoolean());
        Assert.False(Evaluate("branches[0] === sourceStream").AsBoolean());
        Assert.False(Evaluate("branches[1] === sourceStream").AsBoolean());
    }
}
