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
    }
}
