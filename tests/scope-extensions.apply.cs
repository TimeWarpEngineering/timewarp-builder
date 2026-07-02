#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for ScopeExtensions.Apply: configures the object and returns the original object.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ScopeExtensions_
{
  [TestTag("ScopeExtensions")]
  public sealed class Apply_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Apply_Given_>();

    public static async Task MutableObject_Should_ConfigureAndReturnSameInstance()
    {
      Options subject = new();

      Options result = subject.Apply(options => options.Name = "configured");

      result.ShouldBeSameAs(subject);
      subject.Name.ShouldBe("configured");
      await Task.CompletedTask;
    }

    public static async Task MidChainConfiguration_Should_PreserveFluentFlow()
    {
      List<string> log = [];

      Options result = new Options()
        .Apply(options => options.Name = "first")
        .Also(options => log.Add(options.Name))
        .Apply(options => options.Name = "second");

      result.Name.ShouldBe("second");
      log.ShouldBe(new[] { "first" });
      await Task.CompletedTask;
    }

    public static async Task NullAction_Should_ThrowArgumentNullException()
    {
      Should.Throw<ArgumentNullException>(() => new Options().Apply(null!));
      await Task.CompletedTask;
    }
  }

  internal sealed class Options
  {
    public string Name { get; set; } = "";
  }
}
