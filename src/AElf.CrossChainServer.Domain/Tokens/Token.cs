using System;
using AElf.CrossChainServer.Entities;
using JetBrains.Annotations;

namespace AElf.CrossChainServer.Tokens
{
    public class Token : MultiChainEntity<Guid>
    {
        [NotNull] 
        public virtual string Address { get; set; }

        [NotNull] 
        public virtual string Symbol { get; set; }

        public virtual int Decimals { get; set; }
    }
}