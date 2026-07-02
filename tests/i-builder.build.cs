#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for IBuilder<TBuilt>: standalone builders produce their configured object via Build().
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace IBuilder_
{
  [TestTag("Interfaces")]
  public sealed class Build_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Build_Given_>();

    public static async Task ConcreteBuilder_Should_ReturnConfiguredObject()
    {
      Widget widget = new WidgetBuilder()
        .WithName("gizmo")
        .WithSize(10)
        .Build();

      widget.Name.ShouldBe("gizmo");
      widget.Size.ShouldBe(10);
      await Task.CompletedTask;
    }

    public static async Task CovariantTBuilt_Should_AssignToBaseTypedInterface()
    {
      // IBuilder<out TBuilt> covariance: a Widget builder is usable where an object builder is expected
      IBuilder<object> builder = new WidgetBuilder().WithName("gizmo");

      builder.Build().ShouldBeOfType<Widget>();
      await Task.CompletedTask;
    }

    public static async Task RepeatedBuild_Should_ProduceIndependentObjects()
    {
      WidgetBuilder builder = new WidgetBuilder().WithName("gizmo");

      Widget first = builder.Build();
      Widget second = builder.Build();

      first.ShouldNotBeSameAs(second);
      first.ShouldBe(second);
      await Task.CompletedTask;
    }
  }

  internal sealed record Widget(string Name, int Size);

  internal sealed class WidgetBuilder : IBuilder<Widget>
  {
    private string Name = "";
    private int Size;

    public WidgetBuilder WithName(string name)
    {
      Name = name;
      return this;
    }

    public WidgetBuilder WithSize(int size)
    {
      Size = size;
      return this;
    }

    public Widget Build() => new(Name, Size);
  }
}
