namespace WorkflowEngine.Catalog;

/// <summary>Durchsuchbarer Katalog aller im Editor darstellbaren Bausteine.</summary>
public interface IBlockCatalog
{
  IReadOnlyList<BlockDescriptor> Blocks { get; }

  bool TryGet(string alias, out BlockDescriptor descriptor);
}
