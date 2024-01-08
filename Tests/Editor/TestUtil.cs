using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Majiang.Test
{
    public static class TestUtil
    {
        public static List<T> LoadDataFromJsonl<T>(string filePath)
        {
            List<T> data = new List<T>(1024 * 10);

            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var entity = JsonConvert.DeserializeObject<T>(line);
                    data.Add(entity);
                }
            }

            return data;
        }

        public static void ProcessInParallel<T>(List<T> data, Action<T> action, int numSlices = 4)
        {
            int sliceSize = data.Count / numSlices;

            Parallel.For(0, numSlices, i =>
            {
                int start = i * sliceSize;
                int end = (i == numSlices - 1) ? data.Count : start + sliceSize;

                for (int j = start; j < end; j++)
                {
                    action(data[j]);
                }
            });
        }
    }
}
