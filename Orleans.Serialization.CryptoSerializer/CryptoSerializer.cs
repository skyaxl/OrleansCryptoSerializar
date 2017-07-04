using System;
using Orleans.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System.Configuration;
using System.Text;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Orleans.Concurrency;

namespace Orleans.Serialization.CryptoSerializer
{
    
    public class CryptoSerializer : IExternalSerializer
    {

        private Logger _logger;


        private LocalStorageCertificateProvider _certificateProvider;

        public string CertificateThumbprint { private get; set; }


        public CryptoSerializer()
        {
            RSACryptoServiceProvider.UseMachineKeyStore = true;
            CertificateThumbprint = ConfigurationManager.AppSettings["CertificateThumbprint"];
        }

        public object DeepCopy(object source, ICopyContext context)
        {

            if (source == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter
            {
                Context = new StreamingContext(StreamingContextStates.All, context)
            };
            object ret = null;
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, source);
                memoryStream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                formatter.Binder = DynamicBinder.Instance;
                ret = formatter.Deserialize(memoryStream);
            }

            return ret;
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {

            var reader = context.StreamReader;
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            var n = reader.ReadInt();
            var originalBytes = reader.ReadBytes(n);
            var bytes = originalBytes;
            try
            {

                 bytes = DecryptDataOaepSha1(_certificateProvider.Certificate, bytes);
            }
            catch(Exception ex)
            {
                bytes = originalBytes;
                _logger.Warn(0, $"{BitConverter.ToString(bytes)} cannot by decrypted",ex);
            }
            var formatter = new BinaryFormatter
            {
                Context = new StreamingContext(StreamingContextStates.All, context),
                 
            };

            object retVal = null;
            try
            {

                using (var memoryStream = new MemoryStream(bytes))
                {
                    retVal = formatter.Deserialize(memoryStream);
                }
            }
            catch (Exception)
            {
                retVal = Activator.CreateInstance(expectedType);
            }

            return retVal;
        }


        private byte[] EncryptDataOaepSha1(X509Certificate2 cert, byte[] data)
        {
            // GetRSAPublicKey returns an object with an independent lifetime, so it should be
            // handled via a using statement.
            var result = new byte[0];
            using (RSA rsa = cert.GetRSAPublicKey())
            {
                result = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            }
            return result;

        }

        private byte[] DecryptDataOaepSha1(X509Certificate2 cert, byte[] data)
        {
            // GetRSAPrivateKey returns an object with an independent lifetime, so it should be
            // handled via a using statement.s
            using (RSA rsa = cert.GetRSAPrivateKey())
            {
               return rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            }
        }

        public void Initialize(Logger logger)
        {
            _logger = logger;
            _certificateProvider = new LocalStorageCertificateProvider(CertificateThumbprint);
            _certificateProvider.Initialize(logger);
        }



        public bool IsSupportedType(Type itemType)
        {
            return itemType.GetTypeInfo().IsSerializable &&
                itemType.CustomAttributes.Any( x=> typeof(Cryptographable).IsAssignableFrom(x.AttributeType));

        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {

            var writer = context.StreamWriter;
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (item == null)
            {
                writer.WriteNull();
                return;
            }

            var formatter = new BinaryFormatter
            {
                Context = new StreamingContext(StreamingContextStates.All, context)
            };
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, item);
                memoryStream.Flush();
                bytes = memoryStream.ToArray();
            }
            bytes = EncryptDataOaepSha1(_certificateProvider.Certificate, bytes);
            writer.Write(bytes.Length);
            writer.Write(bytes);


        }

    }

    class DynamicBinder : SerializationBinder
    {
        public static readonly SerializationBinder Instance = new DynamicBinder();

        private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        public override Type BindToType(string assemblyName, string typeName)
        {
            lock (this.assemblies)
            {
                Assembly result;
                if (!this.assemblies.TryGetValue(assemblyName, out result))
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        this.assemblies[assembly.GetName().FullName] = assembly;

                    // in some cases we have to explicitly load the assembly even though it seems to be already loaded but for some reason it's not listed in AppDomain.CurrentDomain.GetAssemblies()
                    if (!this.assemblies.TryGetValue(assemblyName, out result))
                        this.assemblies[assemblyName] = Assembly.Load(new AssemblyName(assemblyName));

                    result = this.assemblies[assemblyName];
                }

                return result.GetType(typeName);
            }
        }

    }
}
