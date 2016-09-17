using LeagueLib.Files;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LeagueSandbox.ContentSerializer
{
    public class BaseContentConfiguration
    {
        public virtual Regex[] ResourcePatterns { get; }
        public virtual string OutputDirectory { get; }
        public virtual string ConversionMapPath { get; }

        public FontConfigFile Localization { get; private set; }
        public InibinConverter Converter { get; private set; }

        protected BaseContentConfiguration(FontConfigFile localization)
        {
            Localization = localization;
            Converter = new InibinConverter(ConversionMap.Load(ConversionMapPath));
        }

        public virtual object FindId(string archiveFilePath)
        {
            return archiveFilePath.Split('/').Last().Replace(".inibin", "");
        }

        public virtual string FindName(ContentFile content)
        {
            var name = content.Id.ToString();

            if (!content.Values.ContainsKey("Data")) return name;
            if (!content.Values["Data"].ContainsKey("DisplayName")) return name;

            var nameKey = (string)content.Values["Data"]["DisplayName"];
            if (string.IsNullOrEmpty(nameKey)) return name;
            if (!Localization.Content.ContainsKey(nameKey)) return name;

            return Localization.Content[nameKey];
        }
    }

    public abstract class ContentConfiguration : BaseContentConfiguration
    {
        protected ContentConfiguration(FontConfigFile localization) : base(localization) { }
        public override abstract string ConversionMapPath { get; }
        public override abstract Regex[] ResourcePatterns { get; }
        public override abstract string OutputDirectory { get; }

        public class Spell : ContentConfiguration
        {
            public override string ConversionMapPath { get { return "ConversionMaps/SpellConversionMap.json"; } }
            public override string OutputDirectory { get { return "Spells"; } }
            public override Regex[] ResourcePatterns
            {
                get
                {
                    return new Regex[] {
                        new Regex(@"^DATA\/Spells\/[^\/]*\.inibin\z"),
                        new Regex(@"^DATA\/Shared\/Spells\/[^\/]*\.inibin\z"),
                        new Regex(@"^DATA\/Characters\/.*\/Spells\/[^\/]*\.inibin\z")
                    };
                }
            }

            public Spell(FontConfigFile localization) : base(localization) { }
        }

        public class Item : ContentConfiguration
        {
            public override string ConversionMapPath { get { return "ConversionMaps/ItemConversionMap.json"; } }
            public override string OutputDirectory { get { return "Items"; } }
            public override Regex[] ResourcePatterns
            {
                get
                {
                    return new Regex[] {
                        new Regex(@"^DATA\/Items\/[^\/]*\.inibin\z")
                    };
                }
            }

            public Item(FontConfigFile localization) : base(localization) { }

            public override object FindId(string archiveFilePath)
            {
                archiveFilePath = archiveFilePath.Split('/').Last().Replace(".inibin", "");
                if (!archiveFilePath.All(char.IsDigit))
                    throw new Exception("Could not extract ID from given path");
                return Convert.ToInt32(archiveFilePath);
            }
        }
    }
}
