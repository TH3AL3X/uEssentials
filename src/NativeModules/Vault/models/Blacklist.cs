using System.Collections.Generic;
using System.Xml.Serialization;

namespace Essentials.NativeModules.Vault.Models
{
    public class Blacklist
    {
        [XmlAttribute]
        public string BypassPermission;
        [XmlArrayItem("Id")]
        public List<ushort> Items;

        public Blacklist()
        {

        }
        public Blacklist(string bypassPermission, List<ushort> items)
        {
            BypassPermission = bypassPermission;
            Items = items;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Blacklist blacklist)
                return false;

            return blacklist.BypassPermission == BypassPermission;
        }

        public override int GetHashCode()
        {
            return BypassPermission.GetHashCode();
        }
    }
}