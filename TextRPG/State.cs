using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using static System.Reflection.Metadata.BlobBuilder;

namespace TextRPG
{
    class Scene
    {
        protected Dictionary<string, Scene> _next = new Dictionary<string, Scene>();
        
        protected Scene _prev;
        public Scene Prev { get { return _prev; } set { _prev = value; } }

        protected string _name = "";
        public string Name { get { return _name; } }

        protected string _comment = "";

        protected string[] _choices = { };
        public string[] Option { get { return _choices; } }

        protected string[] _display = { };
        public string[] Display { get { return _display; } }

        static MsgWidget _board = new MsgWidget(40, 3);

        static TextBlock _goldText = new TextBlock(15, 3);

        protected void ThrowMessage(string msg)
        {
            int xOffset = 33;
            int yOfsset = 28;
            _board.text = msg;

            _board.Draw(xOffset, yOfsset);
            Thread.Sleep(1000);
            GameManager.Instance.RefreshScene();
        }

        protected void ShowGold()
        {
            int xOffset = 63;
            int yOffset = 20;
            string playerGold = GameManager.Instance.Player.Gold.ToString();
            _goldText.text = new string[] { $"{playerGold, 10} G"};
            _goldText.Draw(xOffset, yOffset);
        }

        virtual public void HandleInput(GameManager game, ConsoleKey key) { }

        virtual public void Update(GameManager game) { }

        virtual public void DrawScene() 
        {
            Screen.ShowMapName(_name, _comment);
            Screen.DrawBotScreen(Option, 3, true);
        }// 강제하고 싶은데
    }

    class TitleScene : Scene
    {
        ItemSlot _itemSlot;
        public TitleScene()
        {            
            _name = "타이틀";
            _display = File.ReadAllLines(@"..\..\..\Title.txt");
            _next.Add("Town", new TownScene(this));
        }

        override public void HandleInput(GameManager game, ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.Enter:
                    game.ChangeScene(_next["Town"]);
                    break;
            }
        }

        public override void DrawScene()
        {
            Screen.SetSize(80, 40);
            Screen.DrawScreen(Display, 5, 0);  
        }
    }
    
    class TownScene : Scene
    {
        public TownScene(Scene parent)
        {
            _name = "마을";
            _comment = "거래, 휴식 등을 할 수 있습니다.";
            _prev = parent;

            _choices = new string[] { "상태보기", "인벤토리", "상점", "던전 입구" , "신전" };

            _next.Add("Status", new StatusScene(this));
            _next.Add("Inventory", new InventoryScene(this));
            _next.Add("Shop", new ShopScene(this));
            _next.Add("DungunEntrance", new DungunEntranceScene(this));
            _next.Add("Temple", new TempleScene(this));

            SetDisplay();
        }

        override public void HandleInput(GameManager game, ConsoleKey key)
        {
            // 맵이 늘어날 때 마다 스위치 추가하는거 불편한데 . . . 
            // Dictionary 말고 List 로 할까 
            switch(key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;

                case ConsoleKey.D1:
                    game.ChangeScene(_next["Status"]);
                    break;

                case ConsoleKey.D2:
                    game.ChangeScene(_next["Inventory"]);
                    break;

                case ConsoleKey.D3:
                    game.ChangeScene(_next["Shop"]);
                    break;

                case ConsoleKey.D4:
                    game.ChangeScene(_next["DungunEntrance"]);
                    break;

                case ConsoleKey.D5:
                    game.ChangeScene(_next["Temple"]);
                    break;
            }
        }

        void SetDisplay()
        {
            _display = File.ReadAllLines(@"..\..\..\Town.txt");
        }

        public override void DrawScene()
        {
            base.DrawScene();            
            Screen.Split();
            Screen.DrawTopScreen(Display, 2);            
        }        
    }
    
    class StatusScene : Scene
    {
        Widget _statusWidget;
        public StatusScene(Scene parent)
        {
            _name = "능력치";
            _comment = "플레이어의 능력치를 확인합니다.";
            _prev = parent;
            _statusWidget = new StatusWidget(35, 18);
        }

        override public void HandleInput(GameManager game, ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;
            }
        }

        public override void Update(GameManager game)
        {
            base.Update(game);
            Player player = game.Player;
            ((StatusWidget)_statusWidget).Player = player;
        }

        public override void DrawScene()
        {
            base.DrawScene();
            Screen.DrawTopScreen(Display);
            _statusWidget.Draw(6, 2);
        }
    }

    class InventoryScene : Scene
    {
        List<ItemSlot> textBlocks = new List<ItemSlot>();

        public InventoryScene(Scene parent)
        {
            _name = "인벤토리";
            _comment = "플레이어의 인벤토리를 확인합니다.";
            _prev = parent;
            _choices = new string[] { "장착관리", "아이템 정렬" };
            _next.Add("Equip", new EquipScene(this));
        }

        public override void HandleInput(GameManager game, ConsoleKey key)
        {
            base.HandleInput(game, key);

            switch(key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;

                case ConsoleKey.D1:
                    game.ChangeScene(_next["Equip"]);
                    break;

                case ConsoleKey.D2:
                    game.Player.SortInventory();
                    game.RefreshScene();
                    break;
            }
        }

        public override void Update(GameManager game)
        {
            base.Update(game);
            Player player = game.Player;

            textBlocks.Clear();

            for (int i = 0; i < player.Inventory.Count; ++i)
            {
                ItemSlot slot = new ItemSlot(player.Inventory[i]);
                slot.index = i;
                textBlocks.Add(slot);
            }
        }

        public override void DrawScene()
        {
            base.DrawScene();
            Screen.DrawTopScreen(Display);

            int xOffset = 2;
            int yOffset = 1;

            for(int i = 0; i < textBlocks.Count; ++i)
            {
                textBlocks[i].Draw(xOffset, yOffset);
                yOffset += textBlocks[i].Height;
            }
            ShowGold();
        }
    }

    class EquipScene : Scene
    {
        List<ItemSlot> slots = new List<ItemSlot>();

        public EquipScene(Scene parent)
        {
            _name = "장착관리";
            _comment = "플레이어의 착용 장비를 관리합니다.";
            _prev = parent;
        }

        public override void HandleInput(GameManager game, ConsoleKey key)
        {
            if (key < ConsoleKey.D0 || key >= ConsoleKey.D1 + _choices.Length) return;
            
            switch(key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;

                default:
                    game.Player.EquipItem((int)key - 49);
                    game.RefreshScene();
                    break;
            }
        }

        public override void Update(GameManager game)
        {
            base.Update(game);
            Player player = game.Player;
            // Set Choice
            SetOption(player);

            // Set Display
            // 일단 인벤토리 목록을 보여준다. > 부위 별로 착용한 아이템을 보여준다?
            SetDisplay(player);
        }

        void SetDisplay(Player player)
        {
            slots.Clear();

            for (int i = 0; i < player.Inventory.Count; ++i)
            {
                ItemSlot slot = new ItemSlot(player.Inventory[i]);
                slot.index = i;
                slots.Add(slot);
            }
        }

        void SetOption(Player player)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < player.Inventory.Count; ++i)
            {
                Item item = player.Inventory[i];
                string line = $"{item.Name}";
                if (item.bEquip) line = line.Insert(0, "[E]");
                lines.Add(line);
            }
            _choices = lines.ToArray();
        }

        public override void DrawScene()
        {
            base.DrawScene();
            int xOffset = 2;
            int yOffset = 1; 
            for(int i = 0; i < slots.Count; ++i)
            {
                slots[i].Draw(xOffset, yOffset);
                yOffset += slots[i].Height;
            }
        }
    }

    class ShopScene : Scene
    {
        public Shop shop;
        Widget _widget;

        public ShopScene(Scene parent)
        {
            _name = "상점";
            _comment = "아이템을 구입 또는 판매합니다.";
            _prev = parent;
            _widget = new TextBlock(40, 9);
            ((TextBlock)_widget).text = new string[] { "어서오세요.", "[일반 상점] 입니다.",  "","" ,"무엇을 도와드릴까요?" , "1. 구입", "2. 판매" };
            _choices = new string[] { "구입", "판매" };
            shop = new Shop();
            
            _display = File.ReadAllLines(@"..\..\..\npc.txt");

            _next.Add("Buy", new BuyScene(this));
            _next.Add("Sell", new SellScene(this));
        }

        public override void HandleInput(GameManager game, ConsoleKey key)
        {
            base.HandleInput(game, key);

            // Set Display
            switch (key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;

                case ConsoleKey.D1:
                    game.ChangeScene(_next["Buy"]);
                    break;

                case ConsoleKey.D2:
                    game.ChangeScene(_next["Sell"]);
                    break;
            }
        }

        public override void DrawScene()
        {
            base.DrawScene();
            Screen.DrawTopScreen(Display);
            _widget.Draw(35, 3);
            ShowGold();
        }
    }

    class BuyScene : Scene
    {
        Shop _shop;

        List<ItemSlot> slots = new List<ItemSlot>();

        public BuyScene(Scene parent)
        {
            _name = "구입";
            _comment = "아이템을 구입합니다.";
            _prev = parent;
            _shop = ((ShopScene)Prev).shop;

            SetDisplay();
            SetOption();
        }

        public override void HandleInput(GameManager game, ConsoleKey key)
        {
            base.HandleInput(game, key);
            if (key < ConsoleKey.D0 || key >= ConsoleKey.D1 + _choices.Length) return;

            switch (key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;

                default:
                    Item item = _shop.Goods[(int)key - 49];
                    if(game.Player.Buy(item) == false) // 인벤토리가 가득차는 경우도 고려해봐야함.
                    {
                        ThrowMessage("골드가 부족합니다.");                        
                    }
                    else
                    {
                        ThrowMessage($"{item.Name} 을 구입했습니다.");
                    }
                    break;
            }
        }

        void SetDisplay()
        {
            for(int i = 0; i <_shop.Goods.Count; ++i)
            {
                ItemSlot slot = new ItemSlot(_shop.Goods[i]);
                slot.index = i;
                slots.Add(slot);
            }         
        }

        void SetOption()
        {
            List<string> lines = new List<string>();
            foreach (Item item in _shop.Goods)
            {
                string line = $"{item.Name}";
                lines.Add(line);
            }
            _choices = lines.ToArray();
        }

        public override void DrawScene()
        {
            base.DrawScene();
            Screen.DrawTopScreen(Display);
            int xOffset = 2;
            int yOffset = 1;

            for(int i = 0; i < slots.Count; ++i)
            {
                slots[i].Draw(xOffset, yOffset);
                yOffset += slots[i].Height;
            }

            ShowGold();
        }
    }
    
    class SellScene : Scene
    {
        List<ItemSlot> slots = new List<ItemSlot>();
        public SellScene(Scene parent)
        {
            _name = "판매";
            _comment = "아이템을 판매합니다.";
            _prev = parent;
        }

        public override void HandleInput(GameManager game, ConsoleKey key)
        {
            base.HandleInput(game, key);
            if (key < ConsoleKey.D0 || key >= ConsoleKey.D1 + _choices.Length) return;

            switch (key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;

                default:
                    string ItemName = game.Player.Inventory[(int)key - 49].Name;
                    if(game.Player.Sell((int)key - 49))
                    {
                        ThrowMessage($"{ItemName} 을 판매했습니다.");
                    }
                    else
                    {
                        ThrowMessage("장비를 해제 후 판매해주세요.");
                    }
                    
                    break;
            }
        }

        public override void Update(GameManager game)
        {
            base.Update(game);
            Player player = game.Player;

            SetDisplay(player);
            SetOption(player);
        }

        void SetDisplay(Player player)
        {
            slots.Clear();

            for(int i = 0; i <player.Inventory.Count; ++i)
            {
                ItemSlot slot = new ItemSlot(player.Inventory[i]);
                slot.index = i;
                slots.Add(slot);
            }
        }

        void SetOption(Player player)
        {
            List<string> lines = new List<string> ();
            foreach(Item item in player.Inventory)
            {
                string line = $"{item.Name}";
                lines.Add(line);
            }
            _choices = lines.ToArray();
        }

        public override void DrawScene()
        {
            base.DrawScene();
            Screen.DrawTopScreen(Display);

            int xOffset = 2;
            int yOffset = 1;

            for (int i = 0; i < slots.Count; ++i)
            {
                slots[i].Draw(xOffset, yOffset);
                yOffset += slots[i].Height;
            }

            ShowGold();
        }
    }

    class TempleScene : Scene
    {
        public TempleScene(Scene parent)
        {
            _name = "신전";
            _comment = "체력을 회복할 수 있습니다.";
            _prev = parent;
            SetDisplay();
            _choices = new string[] { "회복하기 ( 300 G )" };
        }

        public override void HandleInput(GameManager game, ConsoleKey key)
        {
            switch(key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;

                case ConsoleKey.D1:
                    if (game.Player.Rest() == false)
                    {
                        ThrowMessage("골드가 부족합니다.");
                    }
                    else
                    {
                        ThrowMessage("체력을 회복했습니다.");
                    }
                    break;
            }
        }

        void SetDisplay()
        {
            _display = File.ReadAllLines(@"..\..\..\Church.txt");
        }

        public override void DrawScene()
        {
            base.DrawScene();
            Screen.DrawTopScreen(Display, 5);

            ShowGold();
        }
    }

    class DungunEntranceScene : Scene
    {
        string[] _recommendDef;
        List<Widget> _widgets;
        public DungunEntranceScene(Scene parent)
        {
            _name = "던전 입구";
            _comment = "입장할 던전을 선택합니다.";
            _prev = parent;

            _choices = new string[] { "쉬운 던전", "일반 던전", "어려운 던전" };
            _recommendDef = new string[] { "1 ~ 3", "5 ~ 10", "10 ~ 20" };

            _widgets = new List<Widget>();

            for (int i = 0; i < 3; ++i)
            {
                TextBlock textBlock = new TextBlock(50, 3);
                string dungeon = Utility.MatchCharacterLength(_choices[i], 20);
                textBlock.text = new string[] { $"{i + 1}. {dungeon} | 권장 방어력 {_recommendDef[i]}" };
                _widgets.Add(textBlock);
            }
        }

        public override void HandleInput(GameManager game, ConsoleKey key)
        {
            base.HandleInput(game, key);
            if(game.Player.Hp <= 0)
            {
                return;
            }

            switch(key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;

                case ConsoleKey.D1:
                    game.ChangeScene(new EasyDungunScene(this));
                    break;

                case ConsoleKey.D2:
                    game.ChangeScene(new NormalDungunScene(this));
                    break;

                case ConsoleKey.D3:
                    game.ChangeScene(new HardDungunScene(this));
                    break;
            }
        }

        public override void DrawScene()
        {
            base.DrawScene();
            Screen.DrawTopScreen(Display);

            int xOffset = 2;
            int yOffset = 1;

            for(int i = 0; i < _widgets.Count; ++i)
            {
                _widgets[i].Draw(xOffset, yOffset);
                yOffset += _widgets[i].Height;
            }
        }
    }

    class BaseDungunScene : Scene
    {
        protected Dungun _dungeon;
        
        int _yLine = 4;
                
        string[] msg;

        public BaseDungunScene()
        {
            msg = new string[] { "공략 중 . . .", "공략 성공!!", "공략 실패 ㅠㅁㅠ" };
        }

        public override void HandleInput(GameManager game, ConsoleKey key)
        {
            if (_dungeon.state == Dungun.EDungunState.Continue) return;
            
            switch(key)
            {
                case ConsoleKey.D0:
                    game.ChangeScene(_prev);
                    break;
            }
        }

        public override void Update(GameManager game)
        {   
            _dungeon.Enter(game.Player);      
        }

        public override void DrawScene()
        {
            Screen.Clear();
            Screen.Split();

            Screen.ShowMapName(_name);

            do
            {
                int result = (int)_dungeon.Progress();
                Screen.PrintLine(5, _yLine, msg[result]);
                _yLine += 2;
                Thread.Sleep(1000);

            } while (_dungeon.state == Dungun.EDungunState.Continue);

            Screen.DrawTopScreen(_dungeon.SettleUp(), 3, true);

            Screen.DrawBotScreen(Option);
        }
    }

    class EasyDungunScene : BaseDungunScene
    {
        public EasyDungunScene(Scene parent)
        {
            _dungeon = new Dungun("마을 근처", 0, 2, 2);
            _name = _dungeon.Name;  
            _prev = parent;
        }
    }

    class NormalDungunScene : BaseDungunScene
    {
        public NormalDungunScene(Scene parent)
        {
            _dungeon = new Dungun("성벽 외곽", 1, 7, 7);
            _name = _dungeon.Name;
            _prev = parent;
        }
    }

    class HardDungunScene : BaseDungunScene
    {
        public HardDungunScene(Scene parent)
        {
            _dungeon = new Dungun("지하 미궁", 2, 20, 14);
            _name = _dungeon.Name;
            _prev = parent;
        }
    }
}
