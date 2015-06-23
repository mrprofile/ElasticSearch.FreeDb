﻿using System;
using System.Diagnostics;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using Nest;

namespace XmcdParser
{
    class Program
    {
        private static Uri _node;
        private static ConnectionSettings _settings;
        private static ElasticClient _client;

        static void Main()
        {
            InitializeConnection();
            InitializeIndex();
            ParseDisks();
        }

        private static void InitializeConnection()
        {
            _node = new Uri("http://10.128.36.197:9200");
            _settings = new ConnectionSettings(_node, defaultIndex: "disk");
            _client = new ElasticClient(_settings);
        }

        private static void InitializeIndex()
        {
            var indexSettings = new IndexSettings { NumberOfReplicas = 1, NumberOfShards = 1 };

            _client.CreateIndex(c => c
                .Index("disk")
                .InitializeUsing(indexSettings)
                .AddMapping<Disk>(m => m.MapFromAttributes()));
        }

        private static void ParseDisks()
        {
            int i = 0;
            var parser = new Parser();
            var buffer = new byte[1024 * 1024];
            var descriptor = new BulkDescriptor();
            using (var bz2 = new BZip2InputStream(File.Open(@"C:\Users\501936093\Documents\Visual Studio 2013\Projects\freedb-complete-20141201.tar.bz2", FileMode.Open)))
            using (var tar = new TarInputStream(bz2))
            {
                var batchCount = 0;
                const int numOfBatch = 128;
                TarEntry entry;
                
                while ((entry = tar.GetNextEntry()) != null)
                {
                    
                    if (entry.Size == 0 || entry.Name == "README" || entry.Name == "COPYING")
                        continue;
                    var readSoFar = 0;
                    while (true)
                    {
                        var read = tar.Read(buffer, readSoFar, ((int)entry.Size) - readSoFar);
                        if (read == 0)
                            break;

                        readSoFar += read;
                    }
                    
                    var fileText = new StreamReader(new MemoryStream(buffer, 0, readSoFar)).ReadToEnd();
                    
                    try
                    {
                        batchCount++;
                        var disk = parser.Parse(fileText);
                        descriptor.Index<Disk>(op => op.Document(disk));

                        if (batchCount % numOfBatch == 0)
                        {
                            var sp = Stopwatch.StartNew();
                            batchCount = 0;
                            _client.Bulk(descriptor);
                            Console.WriteLine("Took {0} seconds to perform 128 record batch insert ...", sp.Elapsed);
                            sp.Reset();
                            descriptor = new BulkDescriptor();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.WriteLine(entry.Name);
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}