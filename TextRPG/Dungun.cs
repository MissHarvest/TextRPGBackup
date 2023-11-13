using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextRPG
{
    class DungeonResult
    {
        int _beforeGold;
        int _beforeHp;
        int _afterGold;
        int _afterHp;
        int _beforeExp;
        int _afterExp;
        int _beforeLv;
        int _afterLv;

        public void RecordBefore(Player player)
        {
            _beforeGold = player.Gold;
            _beforeExp = player.Exp;
            _beforeLv = player.Lv;
            _beforeHp = player.Hp;
        }

        public void RecordAfter(Player player)
        {
            _afterGold = player.Gold;
            _afterExp = player.Exp;
            _afterLv = player.Lv;
            _afterHp = player.Hp;
        }

        public string[] GetRecord()
        {
            string[] msg = new string[]
            {
                "[탐험 결과]",
                $"체력 : {_beforeHp} -> {_afterHp}",
                $"Gold : {_beforeGold} -> {_afterGold}",
                $"Lv : {_beforeLv} ({_beforeExp}) -> {_afterLv} ({_afterExp})"
            };
            return msg;
        }
    }

    internal class Dungun
    {
        static Random Random;
        DungeonResult result;

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

        public Dungun(string name, int difficulty, int def, int exp)
        {
            Random = new Random();
            result = new DungeonResult();
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
            result.RecordBefore(_player);
        }

        public EDungunState Progress()
        {
            ++_tryCount;
            if(_tryCount > _maxTryCount) 
            {
                state = EDungunState.Fail;
            }
            else if(Random.NextDouble() < _clearPercent)
            {
                state = EDungunState.Clear;
            }

            return state;
        }

        public string[] SettleUp()
        {
            if(state == EDungunState.Clear)
            {
                _rewardGold += (int)(_rewardGold * Random.NextDouble());
                _player.ReceiveGold(_rewardGold);
                _player.Exp += _exp;
            }

            // 체력 감소            
            _player.Damaged(Random.Next(20 * ((int)_difficulty+1) + _diffDef, 25 * ((int)_difficulty + 1) + _diffDef));
            result.RecordAfter(_player);
            return result.GetRecord();
        }
    }
}
