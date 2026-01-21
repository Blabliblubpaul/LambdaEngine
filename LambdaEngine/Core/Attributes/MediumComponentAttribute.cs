namespace LambdaEngine.Core.Attributes;

/// <summary>
/// Marks a component with a size of 16-32 bytes and suppresses respective warnings.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public class MediumComponentAttribute : Attribute { }