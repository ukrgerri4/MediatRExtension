using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extension.Core.Models
{
    public class FileNameInfo
    {
        public FileNameInfo(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName) || !Regex.Match(fullName, @".*[.].*", RegexOptions.IgnoreCase).Success)
            {
                throw new ArgumentException("File name not valid.");
            }

            var splittedFullname = fullName.Split('.');

            Extension = splittedFullname.Last();
            Name = string.Join(".", splittedFullname.Take(splittedFullname.Length - 1));
        }


        public FileNameInfo(string name, string extension)
        {
            Extension = !string.IsNullOrWhiteSpace(extension) ? extension.Trim('.') : throw new ArgumentNullException(nameof(extension));
            Name = !string.IsNullOrWhiteSpace(name) ? name : string.Empty;
        }

        public string Extension { get; set; }
        public string Name { get; set; }

        public string FullName => $"{Name}.{Extension}";
    }
}
