using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextRPG
{
    class Record
    {
        public int gold = 0;
        public int hp = 0;
        public int exp = 0;
        public int maxExp = 0;
        public int lv = 0;

        public void Save(Player player)
        {
            gold = player.Gold;
            maxExp = player.MaxExp;
            exp = player.Exp;
            hp = player.Hp;
            lv = player.Lv;
        }
    }

    internal class Dungeon
    {
        static Random Random;
        public Record beforeRecord;
        public Record afterRecord;

        public enum EDungunState { Continue, Clear, Fail };
        public EDungunState state;

        enum EDifficulty { Easy , Normal, Hard, Hell };
        EDifficulty _difficulty;

        string _name;
        public string Name { get { return _name; } }

        public int _recommendedDef = 1;
        int[] _goldByDiff = { 100, 300, 700, 1000 };
        int _rewardGold;
        
        Player _player;

        int _maxTryCount = 5;
        int _tryCount = 0;
        int _diffDef = 0;
        float _clearPercent = 0;
        int _exp;

        public Dungeon(string name, int difficulty, int def, int exp)
        {
            Random = new Random();
            beforeRecord = new Record();
            afterRecord = new Record();

            state = EDungunState.Continue;
            _name = name;

            _difficulty = (EDifficulty)difficulty;

            _recommendedDef = def;
            _rewardGold = _goldByDiff[(int)_difficulty];
            _exp = exp;
        }

        public void Enter(Player player)
        {
            _player = player;
            _diffDef = _recommendedDef - player.Def;
            _clearPercent = (float)player.Def / (_recommendedDef + player.Def);
            beforeRecord.Save(_player);
        }

        public EDungunState Progress()
        {
            ++_tryCount;
            if(_tryCount > _maxTryCount) 
            {
                state = EDungunState.Fail;
                SettleUp();
            }
            else if(Random.NextDouble() < _clearPercent)
            {
                state = EDungunState.Clear;
                SettleUp();
            }

            return state;
        }

        public void SettleUp()
        {
            if(state == EDungunState.Clear)
            {
                _rewardGold += (int)(_rewardGold * Random.NextDouble());
                _player.ReceiveGold(_rewardGold);
                _player.Exp += _exp;
            }

            // 체력 감소
            _player.Damaged(Random.Next(20 * ((int)_difficulty+1) + _diffDef, 25 * ((int)_difficulty + 1) + _diffDef));
            afterRecord.Save(_player);
        }
    }
}
