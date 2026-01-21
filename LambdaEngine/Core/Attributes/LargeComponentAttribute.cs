namespace LambdaEngine.Core.Attributes;

/// <summary>
/// Marks a component with a size of 32-64 bytes and suppresses respective warnings.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public class LargeComponentAttribute : Attribute { }