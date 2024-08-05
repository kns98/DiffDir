using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Program <directory1> <directory2>");
            return;
        }

        string dir1 = args[0];
        string dir2 = args[1];

        if (!Directory.Exists(dir1) || !Directory.Exists(dir2))
        {
            Console.WriteLine("Both directories must exist.");
            return;
        }

        var dir1Files = Directory.GetFiles(dir1, "*", SearchOption.AllDirectories);
        var dir2Files = Directory.GetFiles(dir2, "*", SearchOption.AllDirectories);

        var dir1Hashes = new ConcurrentDictionary<string, string>();
        var dir2Hashes = new ConcurrentDictionary<string, string>();

        // Compute hashes in parallel
        Parallel.Invoke(
            () => ComputeHashes(dir1Files, dir1Hashes),
            () => ComputeHashes(dir2Files, dir2Hashes)
        );

        // Calculate sets
        var s1 = dir1Hashes.Values.Except(dir2Hashes.Values).ToList();
        var s2 = dir2Hashes.Values.Except(dir1Hashes.Values).ToList();
        var s3 = dir1Hashes.Values.Intersect(dir2Hashes.Values).ToList();

        // Write results to files
        File.WriteAllLines("S1.txt", s1);
        File.WriteAllLines("S2.txt", s2);

        Console.WriteLine("Files unique to the first directory (S1):");
        s1.ForEach(Console.WriteLine);

        Console.WriteLine("\nFiles unique to the second directory (S2):");
        s2.ForEach(Console.WriteLine);

        Console.WriteLine("\nFiles common to both directories (S3):");
        s3.ForEach(Console.WriteLine);
    }

    static void ComputeHashes(string[] files, ConcurrentDictionary<string, string> hashDictionary)
    {
        foreach (var file in files)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var hash = md5.ComputeHash(stream);
                    var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    hashDictionary.TryAdd(hashString, file);
                }
            }
        }
    }
}
