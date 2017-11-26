using I18N.West;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Rocket.API;
using System;
using fr34kyn01535.Uconomy;

namespace ZaupShop
{
    public class DatabaseMgr
    {
        // The base code for this class comes from Uconomy itself.

        internal DatabaseMgr()
        {
            CP1250 cP1250 = new CP1250();
            this.CheckSchema();
        }

        internal void CheckSchema()
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show tables like '", ZaupShop.Instance.Configuration.Instance.ItemShopTableName, "'");
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("CREATE TABLE `", ZaupShop.Instance.Configuration.Instance.ItemShopTableName, "` (`id` int(6) NOT NULL,`itemname` varchar(50) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '20.00',`buyback` decimal(15,2) NOT NULL DEFAULT '0.00',PRIMARY KEY (`id`)) ");
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show tables like '", ZaupShop.Instance.Configuration.Instance.VehicleShopTableName, "'");
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("CREATE TABLE `", ZaupShop.Instance.Configuration.Instance.VehicleShopTableName, "` (`id` int(6) NOT NULL,`vehiclename` varchar(50) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '100.00',PRIMARY KEY (`id`)) ");
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show columns from `", ZaupShop.Instance.Configuration.Instance.ItemShopTableName, "` like 'buyback'");
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("ALTER TABLE `", ZaupShop.Instance.Configuration.Instance.ItemShopTableName, "` ADD `buyback` decimal(15,2) NOT NULL DEFAULT '0.00'");
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
        }

        private MySqlConnection createConnection()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                if (Uconomy.Instance.Configuration.Instance.DatabasePort == 0)
                {
                    Uconomy.Instance.Configuration.Instance.DatabasePort = 3306;
                }
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] { 
                    Uconomy.Instance.Configuration.Instance.DatabaseAddress, 
                    Uconomy.Instance.Configuration.Instance.DatabaseName, 
                    Uconomy.Instance.Configuration.Instance.DatabaseUsername, 
                    Uconomy.Instance.Configuration.Instance.DatabasePassword,
                    Uconomy.Instance.Configuration.Instance.DatabasePort}));
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return mySqlConnection;
        }

        public bool AddItem(int id, string name, decimal cost, bool change)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                if (!change)
                {
                    mySqlCommand.CommandText = string.Concat(
                        new string[] { 
                            "Insert into `", 
                            ZaupShop.Instance.Configuration.Instance.ItemShopTableName, 
                            "` (`id`, `itemname`, `cost`) VALUES ('",
                            id.ToString(),
                            "', '",
                            name,
                            "', '",
                            cost.ToString(),
                            "');" 
                        });
                }
                else
                {
                    mySqlCommand.CommandText = string.Concat(
                        new string[] { 
                            "update `",
                            ZaupShop.Instance.Configuration.Instance.ItemShopTableName,
                            "` set itemname='",
                            name,
                            "', cost='",
                            cost.ToString(),
                            "' where id='",
                            id.ToString(),
                            "';" 
                        });
                }
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }

        public bool AddVehicle(int id, string name, decimal cost, bool change)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                if (!change)
                {
                    mySqlCommand.CommandText = string.Concat(
                        new string[] { 
                            "Insert into `",
                            ZaupShop.Instance.Configuration.Instance.VehicleShopTableName,
                            "` (`id`, `vehiclename`, `cost`) VALUES ('",
                            id.ToString(),
                            "', '",
                            name,
                            "', '",
                            cost.ToString(),
                            "');" 
                        });
                }
                else
                {
                    mySqlCommand.CommandText = string.Concat(
                        new string[] { 
                            "update `",
                            ZaupShop.Instance.Configuration.Instance.VehicleShopTableName,
                            "` set vehiclename='",
                            name,
                            "', cost='",
                            cost.ToString(),
                            "' where id='",
                            id.ToString(),
                            "';" 
                        });
                }
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }

        public decimal GetItemCost(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(new string[] { 
                    "select `cost` from `",
                    ZaupShop.Instance.Configuration.Instance.ItemShopTableName,
                    "` where `id` = '",
                    id.ToString(),
                    "';" 
                });
                mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return num;
        }

        public decimal GetVehicleCost(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(new string[] { 
                    "select `cost` from `",
                    ZaupShop.Instance.Configuration.Instance.VehicleShopTableName, 
                    "` where `id` = '", 
                    id.ToString(), 
                    "';" 
                });
                mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return num;
        }

        public bool DeleteItem(int id)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(
                    new string[] { 
                        "delete from `",
                        ZaupShop.Instance.Configuration.Instance.ItemShopTableName, 
                        "` where id='", 
                        id.ToString(), 
                        "';" 
                    });
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }

        public bool DeleteVehicle(int id)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(
                    new string[] { 
                        "delete from `", 
                        ZaupShop.Instance.Configuration.Instance.VehicleShopTableName,
                        "` where id='", 
                        id.ToString(), 
                        "';" 
                    });
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }

        public bool SetBuyPrice(int id, decimal cost)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(
                    new string[] { 
                        "update `", 
                        ZaupShop.Instance.Configuration.Instance.ItemShopTableName,
                        "` set `buyback`='",
                        cost.ToString(), 
                        "' where id='", 
                        id.ToString(), 
                        "';" 
                    });
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }

        public decimal GetItemBuyPrice(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(new string[] { 
                    "select `buyback` from `",
                    ZaupShop.Instance.Configuration.Instance.ItemShopTableName,
                    "` where `id` = '",
                    id.ToString(),
                    "';" 
                });
                mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return num;
        }
    }
}