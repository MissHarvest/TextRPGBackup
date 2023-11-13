using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TextRPG
{
    internal class GameManager
    {
        private static GameManager _instance;
        public static GameManager Instance 
        {
            get 
            {
                if (_instance == null) new GameManager();                
                return _instance; 
            }
        }
        
        public bool isPlay = false;

        Player _player;
        public Player Player { get { return _player; } }
        
        Scene _currentScene;

        // public ?
        public Dictionary<string, Scene> Scenes = new Dictionary<string, Scene>();

        private GameManager()
        {
            _instance = this;
            // LoadPlayerData > Status , Inventory , Equip(?)
            
            _currentScene = new TitleScene();
            _player = new Player();
        }

        public void RunGame()
        {
            ChangeScene(_currentScene);
            isPlay = true;
        }

        public void GetInput(ConsoleKey key)
        {
            _currentScene.HandleInput(this, key);
        }

        public void ChangeScene(Scene scene)
        {
            Loader.Save(_player);
            _currentScene = scene;
            RefreshScene();
        }

        public void RefreshScene()
        {
            _currentScene.Update(this);
            _currentScene.DrawScene();
        }

        Player LoadPlayerData()
        {
            string[] lines = File.ReadAllLines(@"..\..\..\PlayerData.txt");
            foreach(string line in lines)
            {
                string[] data = line.Split(',');
                if(data.Length != 8)
                {
                    Console.WriteLine("옳바르지 않은 데이터 형식");
                    Environment.Exit(0);
                }
                
                int level = int.Parse(data[0]);
                string job = data[1];
                int atk = int.Parse(data[2]);
                int def = int.Parse(data[3]);
                int maxHp = int.Parse(data[4]);
                int exp = int.Parse(data[5]);
                int maxExp = int.Parse(data[6]);
                int gold = int.Parse(data[7]);

                return new Player(level, job, atk, def, maxHp, exp, maxExp, gold);
            }
            return new Player();
        }

        void SavePlayerData()
        {
            using(StreamWriter outputFile = new StreamWriter(@"..\..\..\PlayerData.txt"))
            {
                outputFile.Write($"{_player.GetData()}");
            }

            using (StreamWriter outputFile = new StreamWriter(@"..\..\..\InventoryData.txt"))
            {
                outputFile.Write($"{_player.GetData()}");
            }
        }
    }
}
