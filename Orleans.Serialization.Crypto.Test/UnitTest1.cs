using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Runtime;
using Orleans.Serialization.Fakes;
using System.IO;
using Microsoft.QualityTools.Testing.Fakes;

namespace Orleans.Serialization.Crypto.Test
{
    [TestClass]
    public class UnitTest1
    {
        [Serializable]
        class Foo
        {
            public string Name { get; set; } = "Foo name";
        }

        [TestMethod]
        public void TestMethod1()
        {
            var context = ShimsContext.Create();
            var serializer = new CryptoSerializer.CryptoSerializer();
            serializer.CertificateThumbprint = "‎‎‎‎B829E88A9AFB5495D2A7DAF0A329A31650AB236E";
            serializer.Initialize(new Logger1());

            MemoryStream memoryStream = new MemoryStream();
            ShimBinaryTokenStreamWriter binaryTokenStreamWriter = new ShimBinaryTokenStreamWriter() {
                CurrentOffsetGet = () => 0,
                WriteByteArray = (array) => memoryStream.Write(array, 0, array.Length),
                ReleaseBuffers = () => memoryStream = new MemoryStream(),
                ToByteArray = () => memoryStream.ToArray(),
                WriteByteArrayInt32Int32 = (array,i,j) => memoryStream.Write(array, 0, array.Length)
            };


            var serializerContext = new SerializationContext { StreamWriter = binaryTokenStreamWriter };
            serializer.Serialize(new Foo(), serializerContext, typeof(Foo));

            var deserializerContext = new DeserializationContext
            {
                CurrentObjectOffset = 0,
                
                StreamReader = new BinaryTokenStreamReader(memoryStream.ToArray())
            };

            var foo = (Foo)serializer.Deserialize(typeof(Foo), deserializerContext);
            Assert.AreEqual("Foo name", foo.Name);
            context.Dispose();
        }
    }
}
