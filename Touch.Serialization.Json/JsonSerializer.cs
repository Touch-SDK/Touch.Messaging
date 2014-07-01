using System.IO;
using System.Text;
using Newtonsoft.Json;
using System;

namespace Touch.Serialization
{
    /// <summary>
    /// JSON serializer.
    /// </summary>
    sealed public class JsonSerializer : ISerializer
    {
        #region ISerializer implementation
        public void Serialize<T>(T obj, Stream output)
            where T : class, new()
        {
            var data = Serialize(obj);
            var bytes = Encoding.UTF8.GetBytes(data);

            var length = bytes.Length;
            var i = 0;

            while (i < length)
            {
                var count = Math.Min(bytes.Length - i, 32);
                output.Write(bytes, i, count);
                i += 32;
            }

            output.Flush();
        }

        public T Deserialize<T>(Stream stream)
            where T : class, new()
        {
            var bytes = new byte[stream.Length];
            var i = 0;

            while (i < bytes.Length)
            {
                var count = Math.Min(bytes.Length - i, 32);
                i += stream.Read(bytes, i, count);
            }

            var data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            return Deserialize<T>(data);
        }

        public string Serialize<T>(T obj)
            where T : class, new()
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string data)
            where T : class, new()
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
        #endregion
    }
}
