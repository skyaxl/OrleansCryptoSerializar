using System.Threading.Tasks;
using Orleans;
using Orleans.Serialization.CryptoSerializer;
using System;

namespace Orleans.CryptoGrain.Interfaces
{

    [Cryptographable]
    [Serializable]
    public class Message
    {
        public string Data { get; set; }
        public Message()
        {

        }
        public Message(string data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Grain interface IGrain1
    /// </summary>
    public interface ICryptoGrain : IGrainWithGuidKey
    {
        Task<Message> Hello(Message message);
    }
}
