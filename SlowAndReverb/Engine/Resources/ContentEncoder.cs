using System.IO;
using System.Security.Cryptography;

namespace Carpet
{
    internal static class ContentEncoder
    {
        private const long Identifier = 5739285038023271092L;

        private static readonly byte[] s_key = new byte[]
        {
            17,
            54,
            178,
            244,
            98,
            197,
            245,
            11,
            247,
            32,
            87,
            221,
            12,
            175,
            87,
            176
        };

        public static MemoryStream Encode(Stream stream)
        {
            var result = new MemoryStream();

            using (Aes aes = Aes.Create())
            {
                aes.GenerateIV();
                aes.Key = s_key;

                byte[] initializationVector = aes.IV;

                using (BinaryWriter headerWriter = new BinaryWriter(result))
                {
                    headerWriter.Write(Identifier);
                    headerWriter.Write(initializationVector.Length);
                    headerWriter.Write(initializationVector);

                    ICryptoTransform encryptor = aes.CreateEncryptor();

                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        byte[] data = reader.ReadBytes((int)stream.Length);

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (BinaryWriter dataWriter = new BinaryWriter(memoryStream))
                            {
                                dataWriter.Write(data.Length);
                                dataWriter.Write(data);
                            }

                            using (CryptoStream cryptoStream = new CryptoStream(result, encryptor, CryptoStreamMode.Write))
                            {
                                byte[] dataToEncode = memoryStream.ToArray();

                                cryptoStream.Write(dataToEncode, 0, dataToEncode.Length);
                            }
                        }
                    }
                }

                result.Position = 0;

                return result;
            }
        }

        public static MemoryStream Decode(Stream stream)
        {
            var result = new MemoryStream();

            using (BinaryReader headerReader = new BinaryReader(stream))
            {
                long identifier = headerReader.ReadInt64();

                if (identifier != Identifier)
                    return null;

                int initializationVectorLength = headerReader.ReadInt32();
                byte[] initializationVector = headerReader.ReadBytes(initializationVectorLength);

                using (Aes aes = Aes.Create())
                {
                    aes.IV = initializationVector;
                    aes.Key = s_key;

                    ICryptoTransform decryptor = aes.CreateDecryptor();

                    using (CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                    {
                        using (BinaryReader dataReader = new BinaryReader(cryptoStream))
                        {
                            int length = dataReader.ReadInt32();
                            byte[] data = dataReader.ReadBytes(length);

                            result.Write(data);
                        }
                    }
                }
            }

            result.Position = 0;

            return result;
        }

        public static MemoryStream Encode(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return Encode(stream);
        }

        public static MemoryStream Decode(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return Decode(stream);
        }
    }
}
