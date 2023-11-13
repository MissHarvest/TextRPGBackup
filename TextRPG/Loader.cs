using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TextRPG
{
    public static class Loader
    {
        public static JObject LoadPlayerData()
        {
            using (StreamReader file = File.OpenText(@"..\..\..\Player.json"))
            {
                using(JsonTextReader reader = new JsonTextReader(file))
                {
                    return (JObject)JToken.ReadFrom(reader);
                }
            }
        }

        public static JObject LoadEquipment()
        {
            using (StreamReader file = File.OpenText(@"..\..\..\Equipment.json"))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    return (JObject)JToken.ReadFrom(reader);
                }
            }
        }

        public static void Save(Player player)
        {
            string path = @"..\..\..\Player.json";
            JObject configData = new JObject(
                new JProperty("Lv", player.Lv),
                new JProperty("Class", player.Class),
                new JProperty("Atk", player.BaseAtk),
                new JProperty("Def", player.BaseDef),
                new JProperty("MaxHP", player.MaxHp),
                new JProperty("Exp", player.Exp),
                new JProperty("MaxExp", player.MaxExp),
                new JProperty("Gold", player.Gold)
                );
            
            Item[] inventory = player.Inventory.ToArray();
            configData.Add("Inventory", JArray.FromObject(inventory));

            File.WriteAllText(path, configData.ToString());
            
            Save(player.Equipment);
        }

        public static void Save(Item[] items)
        {
            string path = @"..\..\..\Equipment.json";
            JObject configData = new JObject();
            configData.Add("Equip", JArray.FromObject(items));

            File.WriteAllText(path, configData.ToString());
        }
    }
}
