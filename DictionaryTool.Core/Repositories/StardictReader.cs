using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DictionaryTool.Core.Repositories
{
    /// <summary>
    /// Đọc bộ 3 file Stardict (.idx + .dict.dz, bỏ qua .ifo vì chỉ cần
    /// sametypesequence=m, nghĩa là mỗi entry là 1 khối text thuần, không có
    /// type-byte). .idx không nén: mỗi entry là "word\0" + offset (4 byte,
    /// big-endian) + length (4 byte, big-endian) trỏ vào dữ liệu đã giải nén
    /// của .dict.dz (dictzip tương thích gzip thường, GZipStream đọc được).
    /// </summary>
    public static class StardictReader
    {
        public static IEnumerable<KeyValuePair<string, string>> ReadEntries(string idxPath, string dictDzPath)
        {
            var dictData = DecompressDict(dictDzPath);

            using (var idxStream = File.OpenRead(idxPath))
            {
                var idxBytes = ReadAllBytes(idxStream);
                int pos = 0;

                while (pos < idxBytes.Length)
                {
                    int wordStart = pos;
                    while (idxBytes[pos] != 0)
                    {
                        pos++;
                    }
                    var word = Encoding.UTF8.GetString(idxBytes, wordStart, pos - wordStart);
                    pos++; // bỏ qua byte 0 kết thúc chuỗi

                    uint offset = ReadUInt32BigEndian(idxBytes, pos);
                    pos += 4;
                    uint length = ReadUInt32BigEndian(idxBytes, pos);
                    pos += 4;

                    var definition = Encoding.UTF8.GetString(dictData, (int)offset, (int)length);
                    yield return new KeyValuePair<string, string>(word, definition);
                }
            }
        }

        private static byte[] DecompressDict(string dictDzPath)
        {
            using (var fileStream = File.OpenRead(dictDzPath))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (var ms = new MemoryStream())
            {
                gzipStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static byte[] ReadAllBytes(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static uint ReadUInt32BigEndian(byte[] buffer, int startIndex)
        {
            return (uint)((buffer[startIndex] << 24)
                         | (buffer[startIndex + 1] << 16)
                         | (buffer[startIndex + 2] << 8)
                         | buffer[startIndex + 3]);
        }
    }
}
