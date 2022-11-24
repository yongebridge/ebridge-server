using Volo.Abp.Settings;

namespace AElf.CrossChainServer.Settings;

public class CrossChainServerSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(CrossChainServerSettings.MySetting1));
    }
}
