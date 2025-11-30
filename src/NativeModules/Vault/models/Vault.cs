using Essentials.Core;
using System;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;


namespace Essentials.NativeModules.Vault.Models
{
    public class Vault
    {
        [XmlAttribute] public string Name = string.Empty;
        [XmlAttribute] public string Permission = string.Empty;
        [XmlAttribute] public byte Width;
        [XmlAttribute] public byte Height;
        [XmlAttribute] public string Color = string.Empty;

        public Vault()
        {
        }

        public Vault(string name, string permission, byte width, byte height, string color)
        {
            Name = name;
            Permission = permission;
            Width = width;
            Height = height;
            Color = color;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Vault vault)
                return false;

            return vault.Name == Name;
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static Vault Parse(string vaultName)
        {

            return EssCore.Instance.Config.Vaultconfig.Vault.FirstOrDefault(virtualLocker =>
                string.Equals(virtualLocker.Name, vaultName, StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool TryParse(string vaultName, out Vault vault)
        {
            vault = null;
            foreach (var virtualLocker in EssCore.Instance.Config.Vaultconfig.Vault.Where(virtualLocker =>
                string.Equals(virtualLocker.Name, vaultName, StringComparison.CurrentCultureIgnoreCase)))
            {
                vault = virtualLocker;
                return true;
            }

            return false;
        }
    }
}