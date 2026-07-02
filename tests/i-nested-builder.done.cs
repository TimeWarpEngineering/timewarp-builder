#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for INestedBuilder<TParent>: nested builders build their child, hand it to the
// parent, and return the parent via Done() for continued fluent chaining.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace INestedBuilder_
{
  [TestTag("Interfaces")]
  public sealed class Done_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Done_Given_>();

    public static async Task NestedBuilder_Should_ReturnParentInstance()
    {
      RobotBuilder parent = new();

      RobotBuilder returned = parent.AddArm(arm => arm.WithLength(2).Done());

      returned.ShouldBeSameAs(parent);
      await Task.CompletedTask;
    }

    public static async Task Done_Should_PassBuiltChildToParent()
    {
      Robot robot = new RobotBuilder()
        .AddArm(arm => arm.WithLength(2).Done())
        .AddArm(arm => arm.WithLength(3).Done())
        .Build();

      robot.Arms.Count.ShouldBe(2);
      robot.Arms[0].Length.ShouldBe(2);
      robot.Arms[1].Length.ShouldBe(3);
      await Task.CompletedTask;
    }

    public static async Task CovariantTParent_Should_AssignToBaseTypedInterface()
    {
      // INestedBuilder<out TParent> covariance: a RobotBuilder-parented nested builder is
      // usable where an object-parented one is expected
      RobotBuilder parent = new();
      INestedBuilder<object> nested = new NestedArmBuilder<RobotBuilder>(parent, _ => { });

      nested.Done().ShouldBeSameAs(parent);
      await Task.CompletedTask;
    }

    public static async Task FluentChain_Should_ComposeWithScopeExtensions()
    {
      List<int> observedLengths = [];

      Robot robot = new RobotBuilder()
        .AddArm(arm => arm.WithLength(5).Done())
        .Also(builder => builder.AddArm(arm => arm.WithLength(7).Done()))
        .Build()
        .Apply(built => observedLengths.AddRange(built.Arms.Select(a => a.Length)));

      robot.Arms.Count.ShouldBe(2);
      observedLengths.ShouldBe(new[] { 5, 7 });
      await Task.CompletedTask;
    }
  }

  internal sealed record Arm(int Length);

  internal sealed record Robot(IReadOnlyList<Arm> Arms);

  // Mirrors the pattern used by TimeWarp.Nuru's NestedCompiledRouteBuilder<TParent>:
  // wraps the child state, and Done() = build child + hand to parent + return parent.
  internal sealed class NestedArmBuilder<TParent> : INestedBuilder<TParent> where TParent : class
  {
    private readonly TParent Parent;
    private readonly Action<Arm> OnBuilt;
    private int Length;

    public NestedArmBuilder(TParent parent, Action<Arm> onBuilt)
    {
      Parent = parent;
      OnBuilt = onBuilt;
    }

    public NestedArmBuilder<TParent> WithLength(int length)
    {
      Length = length;
      return this;
    }

    public TParent Done()
    {
      OnBuilt(new Arm(Length));
      return Parent;
    }
  }

  internal sealed class RobotBuilder : IBuilder<Robot>
  {
    private readonly List<Arm> Arms = [];

    public RobotBuilder AddArm(Func<NestedArmBuilder<RobotBuilder>, RobotBuilder> configure)
    {
      ArgumentNullException.ThrowIfNull(configure);
      return configure(new NestedArmBuilder<RobotBuilder>(this, Arms.Add));
    }

    public Robot Build() => new([.. Arms]);
  }
}
