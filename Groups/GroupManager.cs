using System.Collections.Generic;
using System.Linq;
using Rocket.API;

namespace ZaupShop.Groups
{
    public class GroupManager
    {
        public HashSet<ZaupGroup> Groups;
        public bool Whitelisting;
        public bool Blacklisting;

        public GroupManager()
        {
            LoadGroups();
            Whitelisting = ZaupShop.Instance.Configuration.Instance.EnableGroupWhitelisting;
            Blacklisting = ZaupShop.Instance.Configuration.Instance.EnableGroupBlacklisting;
        }

        private void LoadGroups()
        {
            ZaupShop.Instance.ShopDB.GetGroups();
            Groups = ZaupShop.Instance.ShopDB.GetGroups();

            if (Groups.Count <= 0) return;

            foreach (ZaupGroup group in Groups)
            {
                HashSet<ZaupGroupElement> elements = ZaupShop.Instance.ShopDB.GetGroupElements(group.Name);

                if (elements.Count == 0)
                    continue;

                group.Elements = elements;
            }
        }

        public bool IsWhitelisted(IRocketPlayer caller, ushort id, bool vehicle)
        {
            if (!Whitelisting)
                return true;

            foreach (ZaupGroup group in Groups.Where(x => x.Whitelist))
            {
                foreach (ZaupGroupElement element in group.Elements.Where(x => x.Vehicle == vehicle))
                {
                    if (id != element.ID)
                        continue;
                    
                    return caller.HasPermission($"zaupgroup.{group.Name}");
                }
            }

            return true;
        }
        
        public bool IsBlacklisted(IRocketPlayer caller, ushort id, bool vehicle)
        {
            if (!Blacklisting)
                return false;

            foreach (ZaupGroup group in Groups.Where(x => !x.Whitelist))
            {
                foreach (ZaupGroupElement element in group.Elements.Where(x => x.Vehicle == vehicle))
                {
                    if (id != element.ID)
                        continue;
                    
                    return caller.HasPermission($"zaupgroup.{group.Name}");
                }
            }

            return false;
        }
    }
}