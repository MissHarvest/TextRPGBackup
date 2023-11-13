using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TextRPG
{
    class Point
    {
        int x = 0;
        int y = 0;
        string str = "";

        public Point(int x, int y, string str)
        {
            this.x = x;
            this.y = y;
            this.str = str;
        }

        public void Draw()
        {
            Console.SetCursorPosition(x, y);
            Console.Write(str);
            Console.SetCursorPosition(52, 25);
        }

        public void Draw(string msg)
        {
            str = msg;
            Console.SetCursorPosition(x, y);
            Console.Write(msg);
            Console.SetCursorPosition(52, 25);
        }

        public void Clear()
        {
            byte[] buffer = Encoding.Default.GetBytes(str);
            int length = buffer.Length;

            for(int i = 0; i < length; ++i)
            {
                Console.SetCursorPosition(x + i, y);
                Console.Write(" ");
            }
        }

        public void Delete()
        {
            byte[] buffer = Encoding.Default.GetBytes(str);
            int length = buffer.Length;

            for (int i = 0; i < length; ++i)
            {
                Console.SetCursorPosition(x + i, y);
                Console.Write(" ");
            }
            str = "";
        }
    }

    class Widget
    {
        // 생성할 때, 가로 및 세로 길이 설정 
        // 이 때, 세로는 최소 3 이상이어야 한다.
        // text 변수를 가지게 만들고 싶다.
        // 그래서 해당 text 를 자신 안에 추가한다.
        // 그리는 위치가 필요. 좌측 상단을 시작으로.
        // new TextBlock(10, 3)
        // text = "아이템";
        // 좌 , 우 , 가운데 정렬이 되면 좋겠다.
        // Screen.Draw(TextBlock)
        protected int _width;
        protected int _height;
        public int Height { get { return _height; } }

        public Widget(int width, int heigt)
        {
            _width = width;
            _height = heigt;
        }

        virtual public void Draw(int xPos, int yPos)
        {
            for (int i = xPos + 1; i < xPos + _width - 1; ++i)
            {
                Screen.SetCursorPosition(i, yPos);
                Console.Write("━");

                Screen.SetCursorPosition(i, yPos + _height - 1);
                Console.Write("━");
            }

            for (int i = yPos + 1; i < yPos + _height; ++i)
            {
                Screen.SetCursorPosition(xPos, i);
                Console.Write("┃");

                Screen.SetCursorPosition(xPos + _width - 1, i);
                Console.Write("┃");
            }

            Screen.SetCursorPosition(xPos, yPos);
            Console.Write("┏");

            Screen.SetCursorPosition(xPos + _width - 1, yPos);
            Console.Write("┓");

            Screen.SetCursorPosition(xPos, yPos + _height - 1);
            Console.Write("┗");

            Screen.SetCursorPosition(xPos + _width - 1, yPos + _height - 1);
            Console.Write("┛");
        }
    }

    class TextBlock : Widget
    {
        // text 삽입 위치 
        int _x = 2;
        int _y = 1;

        string[] lines;
        public string[] text { set { lines = value; } }

        public TextBlock(int width, int height) : base (width, height) 
        {
            
        }

        public override void Draw(int xPos, int yPos)
        {
            base.Draw(xPos, yPos);

            for(int i = 0; i < lines.Length; ++i)
            {
                Screen.SetCursorPosition(_x + xPos, _y + yPos);
                Console.Write($"{lines[i]}");
                ++_y;
            }
            _y = 1;
        }
    }

    class ItemSlot : Widget
    {
        // text 삽입 위치 
        int _x = 2;
        int _y = 1;

        Item _item;

        public int index = 0;

        public ItemSlot(Item item) : base(50, 5)
        {
            _item = item;
        }

        public override void Draw(int xPos, int yPos)
        {
            base.Draw(xPos, yPos);

            // offset 할 때, 한글이 아닌 다른 문자 확인 과정 필요
            string itemName = _item.bEquip ? _item.Name.Insert(0, "[E]") : _item.Name;
            string firstLine = Utility.MatchCharacterLength(itemName, 15);

            firstLine = $"{index + 1}. " + firstLine;
            firstLine += $"|{_item.Status} + {_item.Value,3} |";
            Screen.SetCursorPosition(_x + xPos, _y + yPos);
            Console.Write($"{firstLine}");

            _y += 2;
            Screen.SetCursorPosition(_x + xPos, _y + yPos);
            string secondLine = Utility.MatchCharacterLength(_item.Description, 40);
            secondLine += $"{_item.Price,4} G";
            Console.Write($"{secondLine}");

            _y = 1;
        }
    }

    class StatusWidget :Widget
    {
        // text 삽입 위치 
        int _x = 2;
        int _y = 1;

        Player? _player;
        public Player Player { set { _player = value; } }

        public StatusWidget(int width, int height) : base(width, height)
        {
            _player = null;
        }

        public override void Draw(int xPos, int yPos)
        {
            base.Draw(xPos, yPos);
            DrawOutLine(xPos, yPos);

            if (_player == null) return;

            Screen.SetCursorPosition(xPos + _x, yPos + _y);
            Console.Write($" Lv. {_player.Lv,2} ( {_player.Exp} / {_player.MaxExp} )");
            
            _y += 2;
            Screen.SetCursorPosition(xPos + _x, yPos + _y);
            Console.Write($" Chad ( {_player.Class} )");

            _y += 2;
            Screen.SetCursorPosition(xPos + _x, yPos + _y);
            Console.Write($" 공격력 : {_player.Atk}");

            _y += 2;
            Screen.SetCursorPosition(xPos + _x, yPos + _y);
            Console.Write($" 방어력 : {_player.Def}");

            _y += 2;
            Screen.SetCursorPosition(xPos + _x, yPos + _y);
            Console.Write($"  체력  : {_player.Hp} / {_player.MaxHp}");

            _y += 2;
            Screen.SetCursorPosition(xPos + _x, yPos + _y);
            Console.Write($"  Gold  : {_player.Gold,4} G");

            _y = 1;
        }

        void DrawOutLine(int xPos, int yPos)
        {
            for (int i = xPos - 1; i < xPos + _width + 1; ++i)
            {
                Screen.SetCursorPosition(i, yPos - 1);
                Console.Write("━");

                Screen.SetCursorPosition(i, yPos + _height);
                Console.Write("━");
            }

            for (int i = yPos; i < yPos + _height; ++i)
            {
                Screen.SetCursorPosition(xPos - 2, i);
                Console.Write("┃");

                Screen.SetCursorPosition(xPos + _width + 1, i);
                Console.Write("┃");
            }

            Screen.SetCursorPosition(xPos - 2, yPos - 1);
            Console.Write("┏");

            Screen.SetCursorPosition(xPos + _width + 1, yPos - 1);
            Console.Write("┓");

            Screen.SetCursorPosition(xPos - 2, yPos + _height);
            Console.Write("┗");

            Screen.SetCursorPosition(xPos + _width + 1, yPos + _height);
            Console.Write("┛");
        }
    }

    class MsgWidget : Widget
    {
        // text 삽입 위치 
        int _x = 2;
        int _y = 1;

        public string text;
        public MsgWidget(int width, int height) : base(width, height) 
        {

        }

        public override void Draw(int xPos, int yPos)
        {
            base.Draw(xPos, yPos);
            DrawOutLine(xPos, yPos);

            Screen.SetCursorPosition(xPos + _x, yPos + _y);
            Console.Write($"{text}");            
        }

        void DrawOutLine(int xPos, int yPos)
        {
            for (int i = xPos - 1; i < xPos + _width + 1; ++i)
            {
                Screen.SetCursorPosition(i, yPos - 1);
                Console.Write("━");

                Screen.SetCursorPosition(i, yPos + _height);
                Console.Write("━");
            }

            for (int i = yPos; i < yPos + _height; ++i)
            {
                Screen.SetCursorPosition(xPos - 2, i);
                Console.Write("┃");

                Screen.SetCursorPosition(xPos + _width + 1, i);
                Console.Write("┃");
            }

            Screen.SetCursorPosition(xPos - 2, yPos - 1);
            Console.Write("┏");

            Screen.SetCursorPosition(xPos + _width + 1, yPos - 1);
            Console.Write("┓");

            Screen.SetCursorPosition(xPos - 2, yPos + _height);
            Console.Write("┗");

            Screen.SetCursorPosition(xPos + _width + 1, yPos + _height);
            Console.Write("┛");
        }
    }

    public static class Screen
    {
        static int Width;
        static int Height;
        static int Left;
        static int Right;
        static int Boundary = 25;

        public static void SetSize(int width, int height)
        {
            Width = width; 
            Height = height;
            Left = 4;
            Right = Left + Width;
            Console.SetWindowSize(Width + 10, Height + 10);
            
            DrawBoundary();

        }

        public static void Clear()
        {
            Console.Clear();
            DrawBoundary();
        }

        static void DrawBoundary()
        {
            for (int i = Left + 1 ; i < Right - 1; ++i)
            {
                Console.SetCursorPosition(i, 0);
                Console.Write("━");

                Console.SetCursorPosition(i, Height);
                Console.Write("━");
            }

            for(int i = 1; i < Height; ++i)
            {
                Console.SetCursorPosition(Left, i);
                Console.Write("┃");

                Console.SetCursorPosition(Right, i);
                Console.Write("┃");
            }

            Console.SetCursorPosition(Left, 0);
            Console.Write("┏");

            Console.SetCursorPosition(Right, 0);
            Console.Write("┓");

            Console.SetCursorPosition(Left, Height);
            Console.Write("┗");

            Console.SetCursorPosition(Right, Height);
            Console.Write("┛");
        }

        public static void Split(int yOffset = 0)
        {
            int line = Boundary + yOffset;

            for(int i = Left + 1; i < Right - 1; ++i)
            {
                Console.SetCursorPosition(i, line);
                Console.Write("━");
            }

            Console.SetCursorPosition(Left, line);
            Console.Write("┠");

            Console.SetCursorPosition(Right, line);
            Console.Write("┫");
        }

        public static void ShowMapName(string name, string comment = "") // 색상 설정?
        {
            DeleteLine(1);

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append($"{name}");
            sb.Append("]");
            sb.Append($" {comment}");

            Console.SetCursorPosition(Left + 3, 1);
            Console.Write(sb.ToString());
        }

        static void DeleteLine(int line)
        {
            for(int i = Left + 1; i < Right; ++i)
            {
                Console.SetCursorPosition(i, line);
                Console.Write(' ');
            }
        }

        public static void DrawScreen(string[] contents, int xOffset, int yOffset)
        {
            Console.Clear();
            DrawBoundary();
            int y = yOffset;
            for (int i = 0; i < contents.Length; ++i)
            {
                Console.SetCursorPosition(Left + xOffset, y);
                Console.Write(contents[i]);
                ++y;
            }
        }

        public static void DrawTopScreen(string[] contents, int xOffset = 3, bool space = false)
        {
            ClearTopScreen();
            int y = 2;

            for (int i = 0; i < contents.Length; ++i)
            {
                if (space) y++;
                Console.SetCursorPosition(Left + xOffset, y++);
                Console.Write(contents[i]);
            }
        }

        public static void DrawBotScreen(string[] contents, int xOffset = 3, bool space = false)
        {
            ClearBotScreen();
            int y = Boundary + 1;

            for (int i = 0; i < contents.Length; ++i)
            {
                if (space) y++;
                Console.SetCursorPosition(Left + xOffset, y++);
                Console.Write($"{i+1}. {contents[i]}");
            }

            Console.SetCursorPosition(Left + xOffset, Height - 2);
            Console.Write("0. 뒤로가기 / 나가기");
        }

        static void ClearTopScreen()
        {
            for(int i = 2; i < Boundary; ++i)
            {
                DeleteLine(i);
            }
        }

        static void ClearBotScreen()
        {
            for (int i = Boundary + 1; i < Height ; ++i)
            {
                DeleteLine(i);
            }
        }

        public static void PrintLine(int xOffset, int y, string line)
        {
            Console.SetCursorPosition(Left + xOffset, y);
            Console.Write(line);
        }

        public static void SetCursorPosition(int x, int y)
        {
            Console.SetCursorPosition(Left + 1 + x, y + 2);
        }
    }
}
