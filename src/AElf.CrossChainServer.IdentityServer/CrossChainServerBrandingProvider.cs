using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChainServer;

[Dependency(ReplaceServices = true)]
public class CrossChainServerBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "CrossChainServer";
}
