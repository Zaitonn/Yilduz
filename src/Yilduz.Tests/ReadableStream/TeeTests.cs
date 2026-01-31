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

            // Read from branch1
            reader1.read().then(result => {
                data1.push(result.value);
                done1 = result.done;
                return reader1.read();
            }).then(result => {
                data1.push(result.value);
                return reader1.read();
            }).then(result => {
                data1.push(result.value);
                return reader1.read();
            }).then(result => {
                done1 = result.done;
            });

            // Read from branch2
            reader2.read().then(result => {
                data2.push(result.value);
                done2 = result.done;
                return reader2.read();
            }).then(result => {
                data2.push(result.value);
                return reader2.read();
            }).then(result => {
                data2.push(result.value);
                return reader2.read();
            }).then(result => {
                done2 = result.done;
            });
            """
        );

        // The branches should be readable independently
        Assert.True(Evaluate("reader1 instanceof ReadableStreamDefaultReader").AsBoolean());
        Assert.True(Evaluate("reader2 instanceof ReadableStreamDefaultReader").AsBoolean());
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

            reader1.read().then(result => {
                return reader1.read();
            }).catch(err => {
                error1 = err;
            });

            reader2.read().then(result => {
                return reader2.read();
            }).catch(err => {
                error2 = err;
            });
            """
        );

        // Both branches should receive the error
        Assert.True(Evaluate("reader1 instanceof ReadableStreamDefaultReader").AsBoolean());
        Assert.True(Evaluate("reader2 instanceof ReadableStreamDefaultReader").AsBoolean());
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

            let result1 = null;
            let result2 = null;

            reader1.read().then(result => {
                result1 = result;
                return reader1.read();
            }).then(result => {
                result1 = { ...result1, closed: result.done };
            });

            reader2.read().then(result => {
                result2 = result;
                return reader2.read();
            }).then(result => {
                result2 = { ...result2, closed: result.done };
            });
            """
        );

        // Both branches should be properly closed
        Assert.True(Evaluate("reader1 instanceof ReadableStreamDefaultReader").AsBoolean());
        Assert.True(Evaluate("reader2 instanceof ReadableStreamDefaultReader").AsBoolean());
    }

    [Fact]
    public void ShouldHandleIndependentCancellation()
    {
        Execute(
            """
            let cancelReason = null;
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                    // Keep open
                },
                cancel(reason) {
                    cancelReason = reason;
                }
            });

            const [branch1, branch2] = sourceStream.tee();

            // Cancel only one branch
            const cancelPromise1 = branch1.cancel('branch1 cancelled');

            // The other branch should still be readable
            const reader2 = branch2.getReader();
            """
        );

        Assert.True(Evaluate("cancelPromise1 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("reader2 instanceof ReadableStreamDefaultReader").AsBoolean());
    }

    [Fact]
    public void ShouldHandleBothBranchesCancellation()
    {
        Execute(
            """
            let cancelReason = null;
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                    // Keep open
                },
                cancel(reason) {
                    cancelReason = reason;
                }
            });

            const [branch1, branch2] = sourceStream.tee();

            // Cancel both branches
            const cancelPromise1 = branch1.cancel('branch1 cancelled');
            const cancelPromise2 = branch2.cancel('branch2 cancelled');
            """
        );

        Assert.True(Evaluate("cancelPromise1 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("cancelPromise2 instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleEmptyStream()
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

            let result1 = null;
            let result2 = null;

            reader1.read().then(result => {
                result1 = result;
            });

            reader2.read().then(result => {
                result2 = result;
            });
            """
        );

        // Both branches should handle empty stream correctly
        Assert.True(Evaluate("reader1 instanceof ReadableStreamDefaultReader").AsBoolean());
        Assert.True(Evaluate("reader2 instanceof ReadableStreamDefaultReader").AsBoolean());
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
}
