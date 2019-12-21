using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace MavenSharp
{
    /// <summary>
    /// Holds the name of a Maven artifact and provides easy access to all the information it represents.
    /// 
    /// </summary>
    public class ArtifactIdentifier
    {// Docs: https://maven.apache.org/guides/mini/guide-naming-conventions.html
        #region Properties
        /// <summary>
        /// The name as specified
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Group name
        /// <para>EX: in "com.google.code.findbugs:jsr305:3.0.1" the Group is "com.google.code.findbugs"</para>
        /// </summary>
        public string Group { get; }
        /// <summary>
        /// Artifact name
        /// <para>EX: in "com.google.code.findbugs:jsr305:3.0.1" the Artifact is "jsr305"</para>
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Version number
        /// <para>EX: in "com.google.code.findbugs:jsr305:3.0.1" the Version is "3.0.1"</para>
        /// </summary>
        public ArtifactVersion Version { get; }

        /// <summary>
        /// Classifier
        /// <para>EX: in "" the Classifier is ""</para>
        /// </summary>
        public string Classifier { get; }

        /// <summary>
        /// Extension
        /// <para>EX: in "com.google.code.findbugs:jsr305:3.0.1@zip" the Extension is "@zip"</para>
        /// </summary>
        public string Extension { get; } = "jar";
        #endregion

        #region Accessors
        public bool isSnapshot => this.Version?.Qualifier?.EndsWith("-snapshot", StringComparison.OrdinalIgnoreCase) ?? false;
        #endregion

        #region Constructors
        public ArtifactIdentifier(string Descriptor)
        {
            if (string.IsNullOrEmpty(Descriptor))
            {
                throw new ArgumentNullException(nameof(Descriptor));
            }
            Contract.EndContractBlock();

            Value = Descriptor;

            // Look for the extension first
            if (Descriptor.Contains('@'))
            {
                var etoks = Descriptor.Split('@');
                if (etoks.Length > 2)
                {
                    throw new FormatException($"Invalid artifact identifier ({Descriptor}), cannot contain multiple '@' extensions");
                }

                Extension = etoks[1];
                Descriptor = etoks[0];
            }
            var toks = Descriptor.Split(':');

            if (toks.Length < 2)
            {
                throw new FormatException($"Artifact name must be in the format <group>:<name>:<version>:<?classifier>@<?extension>");
            }

            Group = toks[0];
            Name = toks[1];
            Version = null;
            Classifier = null;
            if (toks.Length >= 3)
            {
                Version = new ArtifactVersion(toks[2]);
            }

            if (toks.Length >= 4)
            {
                Classifier = toks[3];
            }

            if (toks.Length > 4)
            {
                System.Diagnostics.Debug.Print($"[{nameof(ArtifactIdentifier)}] Ignoring extra tokens in artifact name ({string.Join(':', toks.AsMemory(4))})");
            }
        }

        public ArtifactIdentifier(string group, string name, ArtifactVersion version, string classifier, string extension)
        {
            Group = group;
            Name = name;
            Version = version;
            Classifier = classifier;
            Extension = extension;
        }
        #endregion

        public ArtifactIdentifier WithVersion(ArtifactVersion version)
        {
            return new ArtifactIdentifier(Group, Name, version, Classifier, Extension);
        }

        public string Get_File()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.Append('-');
            sb.Append(Version.ToString());
            if (!string.IsNullOrEmpty(Classifier))
            {
                sb.Append('-');
                sb.Append(Classifier);
            }

            if (!string.IsNullOrEmpty(Extension))
            {
                sb.Append('.');
                sb.Append(Extension);
            }

            return sb.ToString();
        }

        public string Get_Path()
        {
            return $"{Group.Replace('.', '/')}/{Name}/{Version}/{Get_File()}";
        }
    }
}
