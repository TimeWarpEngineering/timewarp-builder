#region Purpose
// Contract for nested builders that finish via Done() and return control to a parent builder.
#endregion

#region Design
// Done() bundles three steps: build the child, hand the result to the parent, return the
// parent — enabling deep fluent chains without the caller juggling intermediate results.
// TParent is covariant (out) and constrained to class; nested builders typically wrap a
// standalone IBuilder<TBuilt> internally (see TimeWarp.Nuru's NestedCompiledRouteBuilder).
#endregion

namespace TimeWarp.Builder;

/// <summary>
/// Interface for nested builders that return to a parent context via <see cref="Done"/>.
/// </summary>
/// <typeparam name="TParent">The parent builder type to return to.</typeparam>
/// <remarks>
/// <para>
/// This interface enables fluent API patterns where child builders can return to the parent
/// for continued chaining. Nested builders typically wrap a standalone <see cref="IBuilder{TBuilt}"/>
/// internally and delegate building to it.
/// </para>
/// <para>
/// The <see cref="Done"/> method performs: Build() + pass result to parent + return parent.
/// </para>
/// <code>
/// // Nested builder returns to parent after building
/// app.Map(r => r                                    // NestedCompiledRouteBuilder&lt;EndpointBuilder&gt;
///     .WithLiteral("deploy")
///     .WithParameter("env")
///     .Done())                                      // Builds route, returns EndpointBuilder
///     .WithHandler(handler)
///     .Done();                                      // Returns to app builder
/// </code>
/// </remarks>
public interface INestedBuilder<out TParent> where TParent : class
{
  /// <summary>
  /// Completes the nested builder configuration and returns to the parent builder.
  /// </summary>
  /// <returns>The parent builder for continued chaining.</returns>
  TParent Done();
}
