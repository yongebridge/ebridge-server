using Nest;
using Volo.Abp.Domain.Entities;

namespace AElf.CrossChainServer.Entities
{
    public class MultiChainEntity<TKey> : CrossChainServerEntity<TKey>, IMultiChain
    {
        [Keyword]
        public virtual string ChainId { get; set; }
    }
}