using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClickOnce.Packager
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var platform = args.Length > 0 ? args[0] : "Windows";

            var directory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            var contentDirectory = directory.GetDirectories().FirstOrDefault(x => x.Name == "Content");

            Console.WriteLine("Starting creation of bundle...");

            Environment.CurrentDirectory = directory.FullName;

            var bundlePath = Path.Combine(directory.FullName, "../../../../../Bundle." + platform + ".zip");

            if (File.Exists(bundlePath))
            {
                File.Delete(bundlePath);
            }

            Console.WriteLine("Opening ZIP file...");
            using (var zipFile = new FileStream(bundlePath, FileMode.Create, FileAccess.Write))
            {
                using (var zip = new ZipArchive(zipFile, ZipArchiveMode.Create, true))
                {
                    var date = DateTime.UtcNow;
                    var major = date.ToString("yy");
                    var minor = date.ToString("MMdd");
                    var patch = date.ToString("HHmmss");
                    var build = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "0";

                    var manifest = new Manifest();
                    manifest.version = major + "." + minor + "." + patch + "." + build;
                    manifest.files = new List<ManifestFile>();

                    foreach (var file in directory.GetFiles())
                    {
                        string version = null;
                        string publicKeyToken = null;

                        if (!(file.Extension == ".dll" || file.Extension == ".exe"))
                        {
                            continue;
                        }

                        if (file.Name == "ClickOnce.Packager.exe")
                        {
                            continue;
                        }

                        Console.WriteLine("Processing " + file.Name + "...");

                        try
                        {
                            var asm = Assembly.LoadFile(file.FullName);
                            version = asm.GetName().Version.ToString();
                            publicKeyToken = BitConverter.ToString(asm.GetName().GetPublicKeyToken()).ToUpperInvariant().Replace("-", "");
                        }
                        catch { }

                        manifest.files.Add(new ManifestFile
                        {
                            path = manifest.version + "/" + file.Name + ".deploy",
                            name = file.Name,
                            version = version,
                            publicKeyToken = string.IsNullOrWhiteSpace(publicKeyToken) ? null : publicKeyToken,
                            digestMethod = "sha256",
                            digestValue = GetSha256DigestValueForFile(file),
                            size = file.Length
                        });

                        var entry = zip.CreateEntry(manifest.version + "/" + file.Name + ".deploy");
                        using (var target = entry.Open())
                        {
                            using (var reader = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                            {
                                reader.CopyTo(target);
                            }
                        }
                    }

                    foreach (var file in contentDirectory.GetFiles())
                    {
                        manifest.files.Add(new ManifestFile
                        {
                            path = manifest.version + "/Content/" + file.Name,
                            name = "Content/" + file.Name,
                            version = null,
                            publicKeyToken = null,
                            digestMethod = "sha256",
                            digestValue = GetSha256DigestValueForFile(file),
                            size = file.Length
                        });

                        Console.WriteLine("Processing Content/" + file.Name + "...");

                        var entry = zip.CreateEntry(manifest.version + "/Content/" + file.Name);
                        using (var target = entry.Open())
                        {
                            using (var reader = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                            {
                                reader.CopyTo(target);
                            }
                        }
                    }

                    var entryManifest = zip.CreateEntry("manifest.json");
                    var targetManifest = entryManifest.Open();
                    using (var writer = new StreamWriter(targetManifest))
                    {
                        var manifestJson = JsonConvert.SerializeObject(manifest, Formatting.Indented);

                        writer.Write(manifestJson);

                        Console.WriteLine("Created manifest:");
                        Console.WriteLine(manifestJson);
                    }
                }
            }
        }

        private static string GetSha256DigestValueForFile(FileInfo file)
        {
            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sha = new SHA256Managed())
                {
                    var bytes = sha.ComputeHash(stream);
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        private class Manifest
        {
            public string version { get; set; }

            public List<ManifestFile> files { get; set; }
        }

        private class ManifestFile
        {
            public string path { get; set; }

            public string name { get; set; }

            public string version { get; set; }

            public string publicKeyToken { get; set; }

            public string digestMethod { get; set; }

            public string digestValue { get; set; }

            public long size { get; set; }
        }
    }
}
