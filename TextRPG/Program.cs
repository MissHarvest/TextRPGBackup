using System.Xml.Linq;

namespace TextRPG
{
    internal class Program
    {
        public delegate void KeyInputHandler(ConsoleKey key);

        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;      

            Console.Clear();
            Screen.SetSize(80, 40);
            

            KeyInputHandler keyInputHandle = GameManager.Instance.GetInput;
            GameManager.Instance.RunGame();

            while (GameManager.Instance.isPlay)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    keyInputHandle(key);
                }
                Thread.Sleep(100);
            }
        }
    }
}