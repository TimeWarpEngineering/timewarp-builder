#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for ScopeExtensions.Also: executes a side effect and returns the original object.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ScopeExtensions_
{
  [TestTag("ScopeExtensions")]
  public sealed class Also_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Also_Given_>();

    public static async Task ReferenceType_Should_ExecuteActionAndReturnSameInstance()
    {
      List<string> subject = [];

      List<string> result = subject.Also(list => list.Add("side effect"));

      result.ShouldBeSameAs(subject);
      subject.ShouldContain("side effect");
      await Task.CompletedTask;
    }

    public static async Task ChainedCalls_Should_ExecuteActionsInOrder()
    {
      List<int> executionOrder = [];

      string result = "subject"
        .Also(_ => executionOrder.Add(1))
        .Also(_ => executionOrder.Add(2));

      result.ShouldBe("subject");
      executionOrder.ShouldBe(new[] { 1, 2 });
      await Task.CompletedTask;
    }

    public static async Task ValueType_Should_ReturnOriginalValue()
    {
      int result = 42.Also(_ => { });

      result.ShouldBe(42);
      await Task.CompletedTask;
    }

    public static async Task NullAction_Should_ThrowArgumentNullException()
    {
      Should.Throw<ArgumentNullException>(() => "subject".Also(null!));
      await Task.CompletedTask;
    }
  }
}
