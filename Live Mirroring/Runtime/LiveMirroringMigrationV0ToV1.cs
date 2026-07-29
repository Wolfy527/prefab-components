#if UNITY_EDITOR
namespace Wolfy.PropTools.Customer.LiveMirroring
{
using Wolfy.PropTools.Customer.Authoring;

internal sealed class LiveMirroringMigrationV0ToV1 :
    IVersionedDataMigration<LiveMirroringSystem>
{
    public int FromVersion => 0;
    public int ToVersion => 1;

    public void Apply(LiveMirroringSystem system)
    {
        if (system.pairs == null)
            return;

        for (int i = 0; i < system.pairs.Length; i++)
        {
            LiveMirroringSystem.MirrorPair pair = system.pairs[i];

            if (pair == null)
                continue;

            if (string.IsNullOrWhiteSpace(pair.pairName))
                pair.pairName = $"Mirror Pair {i + 1}";
        }
    }
}
}
#endif
