using System;
using System.Collections.Generic;

using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Permissions;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Steamworks;
using Rocket.API.Serialisation;

namespace ZaupShop
{
    public class CommandShop : IRocketCommand
    {

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }
        public string Name
        {
            get
            {
                return "shop";
            }
        }
        public string Help
        {
            get
            {
                return "Allows admins to change, add, or remove items/vehicles from the shop.";
            }
        }
        public string Syntax
        {
            get
            {
                return "<add | rem | chng | sell> [v.]<itemid> <cost>";
            }
        }
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }
        public List<string> Permissions
        {
            get { return new List<string>() { "shop.*", "shop.add", "shop.rem", "shop.chng", "shop.sell" }; }
        }
        public void Execute(IRocketPlayer caller, string[] msg)
        {
            bool console = (caller is ConsolePlayer);
            string[] permnames = { "shop.*", "shop.add", "shop.rem", "shop.chng", "shop.sell" };
            bool[] perms = { false, false, false, false, false };
            bool anyuse = false;
            string message;
            foreach (Permission s in caller.GetPermissions())
            {
                switch (s.Name)
                {
                    case "shop.*":
                        perms[0] = true;
                        anyuse = true;
                        break;
                    case "shop.add":
                        perms[1] = true;
                        anyuse = true;
                        break;
                    case "shop.rem":
                        perms[2] = true;
                        anyuse = true;
                        break;
                    case "shop.chng":
                        perms[3] = true;
                        anyuse = true;
                        break;
                    case "shop.sell":
                        perms[4] = true;
                        anyuse = true;
                        break;
                    case "*":
                        perms[0] = true;
                        perms[1] = true;
                        perms[2] = true;
                        perms[3] = true;
                        perms[4] = true;
                        anyuse = true;
                        break;
                }
            }
            if (console || ((UnturnedPlayer)caller).IsAdmin)
            {
                perms[0] = true;
                perms[1] = true;
                perms[2] = true;
                perms[3] = true;
                perms[4] = true;
                anyuse = true;
            }
            if (!anyuse)
            {
                // Assume this is a player
                UnturnedChat.Say(caller, "You don't have permission to use the /shop command.");
                return;
            }
            if (msg.Length == 0)
            {
                message = ZaupShop.Instance.Translate("shop_command_usage", new object[] {});
                // We are going to print how to use
                this.sendMessage(caller, message, console);
                return;
            }
            if (msg.Length < 2)
            {
                message = ZaupShop.Instance.Translate("no_itemid_given", new object[] {});
                this.sendMessage(caller, message, console);
                return;
            }
            if (msg.Length == 2 && msg[0] != "rem")
            {
                message = ZaupShop.Instance.Translate("no_cost_given", new object[] { });
                this.sendMessage(caller, message, console);
                return;
            }
            else if (msg.Length >= 2)
            {
                string[] type = Parser.getComponentsFromSerial(msg[1], '.');
                if (type.Length > 1 && type[0] != "v")
                {
                    message = ZaupShop.Instance.Translate("v_not_provided", new object[] { });
                    this.sendMessage(caller, message, console);
                    return;
                }
                ushort id;
                if (type.Length > 1)
                {
                    if (!ushort.TryParse(type[1], out id)) {
                        message = ZaupShop.Instance.Translate("invalid_id_given", new object[] { });
                        this.sendMessage(caller, message, console);
                        return;
                    }
                } else {
                    if (!ushort.TryParse(type[0], out id))
                    {
                        message = ZaupShop.Instance.Translate("invalid_id_given", new object[] { });
                        this.sendMessage(caller, message, console);
                        return;
                    }
                }
                // All basic checks complete.  Let's get down to business.
                bool success = false;
                bool change = false;
                bool pass = false;
                switch (msg[0])
                {
                    case "chng":
                        if (!perms[3] && !perms[0])
                        {
                            message = ZaupShop.Instance.Translate("no_permission_shop_chng", new object[] { });
                            this.sendMessage(caller, message, console);
                            return;
                        }
                        change = true;
                        pass = true;
                        goto case "add";
                    case "add":
                        if (!pass)
                        {
                            if (!perms[1] && !perms[0])
                            {
                                message = ZaupShop.Instance.Translate("no_permission_shop_add", new object[] { });
                                this.sendMessage(caller, message, console);
                                return;
                            }
                        }
                        string ac = (pass) ? ZaupShop.Instance.Translate("changed", new object[] { }) : ZaupShop.Instance.Translate("added", new object[] { });
                        switch (type[0])
                        {
                            case "v":
                                if (!this.IsAsset(id, "v"))
                                {
                                    message = ZaupShop.Instance.Translate("invalid_id_given", new object[] { });
                                    this.sendMessage(caller, message, console);
                                    return;
                                }
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                                message = ZaupShop.Instance.Translate("changed_or_added_to_shop", new object[] { 
                                    ac,
                                    va.vehicleName,
                                    msg[2]
                                });
                                success = ZaupShop.Instance.ShopDB.AddVehicle((int)id, va.vehicleName, decimal.Parse(msg[2]), change);
                                if (!success)
                                {
                                    message = ZaupShop.Instance.Translate("error_adding_or_changing", new object[] { va.vehicleName });
                                }
                                this.sendMessage(caller, message, console);
                                break;
                            default:
                                if (!this.IsAsset(id, "i"))
                                {
                                    message = ZaupShop.Instance.Translate("invalid_id_given", new object[] { });
                                    this.sendMessage(caller, message, console);
                                    return;
                                }
                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                                message = ZaupShop.Instance.Translate("changed_or_added_to_shop", new object[] { 
                                    ac,
                                    ia.itemName,
                                    msg[2]
                                });
                                success = ZaupShop.Instance.ShopDB.AddItem((int)id, ia.itemName, decimal.Parse(msg[2]), change);
                                if (!success)
                                {
                                    message = ZaupShop.Instance.Translate("error_adding_or_changing", new object[] { ia.itemName });
                                }
                                this.sendMessage(caller, message, console);
                                break;
                        }
                        break;
                    case "rem":
                        if (!perms[2] && !perms[0])
                        {
                            message = ZaupShop.Instance.Translate("no_permission_shop_rem", new object[] { });
                            this.sendMessage(caller, message, console);
                            return;
                        }
                        switch (type[0])
                        {
                            case "v":
                                if (!this.IsAsset(id, "v"))
                                {
                                    message = ZaupShop.Instance.Translate("invalid_id_given", new object[] { });
                                    this.sendMessage(caller, message, console);
                                    return;
                                }
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                                message = ZaupShop.Instance.Translate("removed_from_shop", new object[] { va.vehicleName });
                                success = ZaupShop.Instance.ShopDB.DeleteVehicle((int)id);
                                if (!success)
                                {
                                    message = ZaupShop.Instance.Translate("not_in_shop_to_remove", new object[] { va.vehicleName });
                                }
                                this.sendMessage(caller, message, console);
                                break;
                            default:
                                if (!this.IsAsset(id, "i"))
                                {
                                    message = ZaupShop.Instance.Translate("invalid_id_given", new object[] { });
                                    this.sendMessage(caller, message, console);
                                    return;
                                }
                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                                message = ZaupShop.Instance.Translate("removed_from_shop", new object[] { ia.itemName });
                                success = ZaupShop.Instance.ShopDB.DeleteItem((int)id);
                                if (!success)
                                {
                                    message = ZaupShop.Instance.Translate("not_in_shop_to_remove", new object[] { ia.itemName });
                                }
                                this.sendMessage(caller, message, console);
                                break;
                        }
                        break;
                    case "sell":
                        if (!perms[4] && !perms[0])
                        {
                            message = ZaupShop.Instance.Translate("no_permission_shop_buy", new object[] { });
                            this.sendMessage(caller, message, console);
                            return;
                        }
                        if (!this.IsAsset(id, "i"))
                        {
                            message = ZaupShop.Instance.Translate("invalid_id_given", new object[] { });
                            this.sendMessage(caller, message, console);
                            return;
                        }
                        ItemAsset iab = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                        decimal buyb;
                        decimal.TryParse(msg[2], out buyb);
                        message = ZaupShop.Instance.Translate("set_buyback_price", new object[] {
                            iab.itemName,
                            buyb.ToString()
                        });
                        success = ZaupShop.Instance.ShopDB.SetBuyPrice((int)id, buyb);
                        if (!success)
                        {
                            message = ZaupShop.Instance.Translate("not_in_shop_to_buyback", new object[] { iab.itemName });
                        }
                        this.sendMessage(caller, message, console);
                        break;
                    default:
                        // We shouldn't get this, but if we do send an error.
                        message = ZaupShop.Instance.Translate("not_in_shop_to_remove", new object[] { });;
                        this.sendMessage(caller, message, console);
                        return;
                }
            }
        }
        private bool IsAsset(ushort id, string type)
        {
            // Check for valid Item/Vehicle Id.
            switch (type)
            {
                case "i":
                    if (Assets.find(EAssetType.ITEM, id) != null)
                    {
                        return true;
                    }
                    return false;
                case "v":
                    if (Assets.find(EAssetType.VEHICLE, id) != null)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
        private void sendMessage(IRocketPlayer caller, string message, bool console)
        {
            if (console)
            {
                Rocket.Core.Logging.Logger.Log(message);
            }
            else
            {
                UnturnedChat.Say(caller, message);
            }
        }
    }
}
