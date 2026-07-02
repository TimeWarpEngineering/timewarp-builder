#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for ScopeExtensions.Let: transforms the object to a different type or value.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ScopeExtensions_
{
  [TestTag("ScopeExtensions")]
  public sealed class Let_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Let_Given_>();

    public static async Task String_Should_TransformToDifferentType()
    {
      int length = "hello".Let(s => s.Length);

      length.ShouldBe(5);
      await Task.CompletedTask;
    }

    public static async Task ValueType_Should_TransformValue()
    {
      int doubled = 21.Let(x => x * 2);

      doubled.ShouldBe(42);
      await Task.CompletedTask;
    }

    public static async Task ChainedTransforms_Should_ComposeLeftToRight()
    {
      string result = 7
        .Let(x => x * 10)
        .Let(x => $"value-{x}");

      result.ShouldBe("value-70");
      await Task.CompletedTask;
    }

    public static async Task NullTransform_Should_ThrowArgumentNullException()
    {
      Should.Throw<ArgumentNullException>(() => "subject".Let((Func<string, int>)null!));
      await Task.CompletedTask;
    }
  }
}
