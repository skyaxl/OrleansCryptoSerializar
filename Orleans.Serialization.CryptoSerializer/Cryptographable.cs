using System;

namespace Orleans.Serialization.CryptoSerializer
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class Cryptographable : Attribute
    {

    }
}
