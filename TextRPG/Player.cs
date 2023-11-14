using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TextRPG
{
    class EquipManager
    {
        Item[] _equips;
        public Item[] EquipItems { get { return _equips; } }

        // _equips[(int)Item.EType]
        public EquipManager()
        {
            //JObject save = Loader.LoadEquipment();
            //_equips = save["Equip"].ToObject<Item[]>();
            _equips = new Item[2];

            for(int i = 0; i < _equips.Length; ++i)
            {
                _equips[i] = Item.NULL;
            }
        }

        public void Wear(Item item)
        {
            int parts = (int)item.type;
            if (_equips[parts] == item) // 동일한 아이템일 경우
            {
                _equips[parts].bEquip = false;
                _equips[parts] = Item.NULL;
            }
            else if(_equips[parts] == Item.NULL) // 비어있는 경우
            {
                _equips[parts] = item;
                item.bEquip = true;
            }
            else // 비어있지 않은 경우
            {
                _equips[parts].bEquip = false;
                _equips[parts] = item;
                item.bEquip = true;
            }            
        }
    }

    public class Player
    {
        int lv = 1;
        public int Lv { get { return lv; } }
        
        string job = "전사";
        public string Class { get { return job; } }

        int atk = 10;
        public int BaseAtk { get { return atk; } }
        int _deltaAtk = 0;
        public int Atk { get { return atk + _deltaAtk; } }

        int def = 1;
        public int BaseDef { get { return def; } }
        int _deltaDef = 0;
        public int Def { get { return def + _deltaDef; } }
        
        int hp;
        public int Hp 
        {
            get 
            {
                return hp; 
            } 
            set
            {
                hp = value;
                if(hp < 0)
                {
                    hp = 0;

                    // 골드 감소
                    _gold -= (int)(_gold * 0.3f);
                }
            }
        }

        int maxHp = 100;
        int _deltaHp = 0;
        public int MaxHp { get { return maxHp + _deltaHp; } }

        int _exp = 0;
        int _maxExp = 10;
        public int MaxExp { get { return _maxExp; } }
        int[] _expByLevel = { 0, 10, 20, 30, 40, 50, 70, 95, 120, 200 };

        public int Exp 
        {
            get { return _exp; }
            set 
            {
                _exp = value; 
                if(_exp >= _maxExp)
                {
                    _exp -= _maxExp;                    
                    ++lv;
                    _maxExp = _expByLevel[lv];
                    atk += 2;
                    def += 1;
                }
            } 
        }

        int _gold = 0;
        public int Gold { get { return _gold; } }

        List<Item> _inventory = new List<Item>();
        public List<Item> Inventory { get { return _inventory; } }

        EquipManager _equipManager;
        public Item[] Equipment { get { return _equipManager.EquipItems; } }
        public Player() // 초기값 설정
        {
            JObject save = Loader.LoadPlayerData();

            lv = (int)save["Lv"];
            job = save["Class"].ToString();
            atk = (int)save["Atk"];
            def = (int)save["Def"];
            maxHp = (int)save["MaxHP"];
            hp = maxHp;
            _maxExp = (int)save["MaxExp"];
            Exp = (int)save["Exp"];            
            _gold = (int)save["Gold"];

            _inventory = save["Inventory"].ToObject<List<Item>>();

            _equipManager = new EquipManager();

            foreach(var item in _inventory)
            {
                if (item.bEquip)
                    _equipManager.Wear(item);
            }
        }

        public Player(int lv, string job, int atk, int def, int maxHp, int exp, int maxExp, int gold)
        {
            this.lv = lv;
            this.job = job;
            this.atk = atk;
            
            this.def = def;
            
            this.maxHp = maxHp;
            hp = maxHp;

            _maxExp = maxExp;
            Exp = exp;            
             
            _gold = gold;            
        }

        public void EquipItem(int index)
        {
            _equipManager.Wear(_inventory[index]); // 장착 교환 탈착 의 형태로 리턴?

            _deltaHp = _deltaDef = _deltaAtk = 0;

            foreach(Item item in _equipManager.EquipItems)
            {
                if (item == null) continue;
                switch(item.Status)
                {
                    case "체력":
                        _deltaHp += item.Value;
                        break;

                    case "공격력":
                        _deltaAtk += item.Value;
                        break;

                    case "방어력":
                        _deltaDef += item.Value;
                        break;
                }
            }
        }

        public void SortInventory()
        {
            _inventory.Sort((x, y) =>
            {
                if (x.Name.Length > y.Name.Length) return -1;
                else if (x.Name.Length == y.Name.Length) return 0;
                else return 1;
            });
        }

        public void Buy(Item item)
        {
            if (_gold < item.Price)
            {
                throw new GoldShortageException();
            }
            else if (_inventory.Count == 5)// _inventory.Max)
            {
                throw new IndexOutOfRangeException();
            }
            else
            {
                _inventory.Add(item);
            }
        }

        public void Sell(int index)
        {
            if (_inventory[index].bEquip)
            {
                throw new EquippedItemException();
            }
            else
            {
                _gold += _inventory[index].Price;
                _inventory.RemoveAt(index);
            }
        }

        public void Damaged(int dmg)
        {
            Hp -= dmg;
        }

        public void ReceiveGold(int gold)
        {
            _gold += gold;
        }

        public bool Rest()
        {
            if (_gold >= 300)
            {
                hp = MaxHp; 
                _gold -= 300;
                return true;
            }
            return false;   
            
        }

        public string GetData()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}",
                lv.ToString(),
                job,
                atk.ToString(),
                def.ToString(),
                maxHp.ToString(),
                Exp.ToString(),
                _maxExp.ToString(),
                Gold.ToString()
                );
        }
    }
}
