using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PictureOrganizer
{
    public class Organizer
    {
        private string WorkingFolder { get; set; }
        private string SearchFolder { get; set; }
        private string SettingsFilename { get; set; }
        private Settings Settings { get; set; }
        private List<PictureFile> Files { get; set; }
        private int currentFile = -1;
        private HashSet<string> extHash = new HashSet<string> {
            "ani","anim","apng","art","bmp","bpg","bsave","cal","cin","cpc","cpt","dds","dpx","ecw","exr",
            "fits","flic","flif","fpx","gif","hdri","hevc","icer","icns","ico", "cur","ics","ilbm","jbig",
            "jbig2","jng","jpeg","jpg","kra","mng","miff","nrrd","ora","pam","pbm", "pgm", "ppm", "pnm","pcx",
            "pgf","pictor","png","psd", "psb","psp","qtvr","ras","rgbe","sgi","tga","tiff","ufo", "ufp",
            "wbmp","webp","xbm","xcf","xpm","xwd",
        };

        public Organizer()
        {
            Files = new List<PictureFile>(); 
        }

        public void Initialize(string workingFolder, string searchFolder)
        {
            SearchFolder = searchFolder;
            WorkingFolder = workingFolder;
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }

            var jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            SettingsFilename = Path.Combine(workingFolder, "settings.json");
            if (File.Exists(SettingsFilename))
            {
                var json = File.ReadAllText(SettingsFilename);
                Settings = JsonConvert.DeserializeObject<Settings>(json, jsonSettings);
            }
            else
            {
                Settings = new Settings
                {
                    ArchivePath = Path.Combine(workingFolder, "archive"),
                    DuplicatePath = Path.Combine(workingFolder, "duplicates"),
                    KeepPath = Path.Combine(workingFolder, "keep"),
                    StatusFile = Path.Combine(workingFolder, "files.json"),
                };
            }

            if (File.Exists(Settings.StatusFile))
            {
                var json = File.ReadAllText(Settings.StatusFile);
                var files = JsonConvert.DeserializeObject<List<PictureFile>>(json, jsonSettings);
                Files.Clear();
                Files.AddRange(files);
                currentFile = Files.FindIndex(f => String.Equals(f.Status, FilesStatus.Found)) - 1;
                if (currentFile < 0)
                {
                    // No files left to process
                    currentFile = Files.Count;
                }
            }
        }

        public string GetNextPictureFile()
        {
            currentFile++;

            if (currentFile < 0)
            {
                currentFile = 0;
            }

            if (currentFile >= Files.Count)
            {
                return null;
            }

            return Files[currentFile].Filename;
        }

        public void SkipPictureFile()
        {
            var f = GetCurrentPictureFile();
            if (f != null)
            {
                f.Status = FilesStatus.Skipped;
            }
        }

        public void KeepPictureFile()
        {
            var f = GetCurrentPictureFile();
            if (f != null)
            {
                MoveFile(f.Filename, Settings.KeepPath);
                f.Status = FilesStatus.Kept;
            }
        }

        public bool CheckForDuplicateFile()
        {
            var f = GetCurrentPictureFile();
            if (f != null)
            {
                string hash = GetHash(f.Filename);
                if (this.Files.Find(file => file.Hash == hash) == null)
                {
                    // this file is NOT a duplicate
                    f.Hash = hash;
                    return false;
                }

                MoveFile(f.Filename, Settings.DuplicatePath);
                f.Status = FilesStatus.Duplicate;
                f.Hash = hash;
                return true;
            }

            return false;
        }

        public void ArchivePictureFile()
        {
            var f = GetCurrentPictureFile();
            if (f != null)
            {
                MoveFile(f.Filename, Settings.ArchivePath);
                f.Status = FilesStatus.Archived;
            }
        }

        private void MoveFile(string filepath, string destinationFolder)
        {
            EnsureFolderExists(destinationFolder);

            string destFilename = Path.Combine(destinationFolder, Path.GetFileName(filepath));
            string newFilename = destFilename;
            int count = 1;
            while (File.Exists(newFilename))
            {
                newFilename = $"{destFilename} ({count++})";
            }
            File.Move(filepath, newFilename);
        }

        private PictureFile GetCurrentPictureFile()
        {
            if (currentFile >= 0 && currentFile < Files.Count)
            {
                return Files[currentFile];
            }

            return null;
        }

        private void EnsureFolderExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void Save()
        {
            var jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            File.WriteAllText(Path.Combine(WorkingFolder, "settings.json"), JsonConvert.SerializeObject(Settings, jsonSettings));
            File.WriteAllText(Path.Combine(WorkingFolder, "files.json"), JsonConvert.SerializeObject(Files, jsonSettings));
        }

        public bool FilesLoaded()
        {
            return Files.Count > 0;
        }

        public void LoadFiles()
        {
            currentFile = -1;
            Files.Clear();
            foreach (var f in Directory.EnumerateFiles(SearchFolder, "*.*", SearchOption.AllDirectories))
            {
                if (IsPictureFile(f))
                {
                    Files.Add(new PictureFile() { Filename = f, Status = FilesStatus.Found });
                }
            }
        }

        private string GetHash(string filepath)
        {
            byte[] fileContents = File.ReadAllBytes(filepath);
            byte[] hash = MD5.Create().ComputeHash(fileContents);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private bool IsPictureFile(string filepath)
        {
            string ext = Path.GetExtension(filepath).ToLowerInvariant();
            if (String.IsNullOrEmpty(ext))
            {
                return false;
            }
            return extHash.Contains(ext.Substring(1));
        }
    }
}


 
