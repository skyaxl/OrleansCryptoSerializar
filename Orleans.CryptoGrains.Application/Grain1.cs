using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.CryptoGrain.Interfaces;
using Orleans.Serialization.CryptoSerializer;

namespace Orleans.CryptoGrains.Application
{
    

    /// <summary>
    /// Grain implementation class Grain1.
    /// </summary>
    public class CryptoGrain : Grain, ICryptoGrain
    {
        public Task<Message> Hello(Message message)
        {
            return Task.FromResult(new Message($"Hello {message.Data}"));
        }
    }
}
