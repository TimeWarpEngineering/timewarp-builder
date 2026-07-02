#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for ScopeExtensions.Run: executes a terminal action on the object with no return value.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ScopeExtensions_
{
  [TestTag("ScopeExtensions")]
  public sealed class Run_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Run_Given_>();

    public static async Task Object_Should_ExecuteActionWithReceiver()
    {
      string? observed = null;

      "subject".Run(s => observed = s);

      observed.ShouldBe("subject");
      await Task.CompletedTask;
    }

    public static async Task EndOfChain_Should_ActAsTerminalOperation()
    {
      List<string> log = [];

      "subject"
        .Also(_ => log.Add("first"))
        .Run(_ => log.Add("terminal"));

      log.ShouldBe(new[] { "first", "terminal" });
      await Task.CompletedTask;
    }

    public static async Task NullAction_Should_ThrowArgumentNullException()
    {
      Should.Throw<ArgumentNullException>(() => "subject".Run(null!));
      await Task.CompletedTask;
    }
  }
}
