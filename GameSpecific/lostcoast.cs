﻿using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class lostcoast : GameSupport
    {
        // how to match with demos:
        // start: 0.2 seconds (14 ticks before timer starts) before the blackout camera guide entity is killed
        // ending: when final trigger_once is triggered (in other words, killed)

        private bool _onceFlag = false;

        private int _black_index;
        private int _trig_index;

        public lostcoast()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "hdrtest"; //beta%
            this.LastMap = "d2_lostcoast";
            this.RequiredProperties = PlayerProperties.Position;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap || this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._black_index = state.GetEntIndexByName("blackout");
                Debug.WriteLine("blackout index is " + this._black_index);
                this._trig_index = state.GetEntIndexByPos(1109.82f, 2952f, 2521.26f);
                Debug.WriteLine("test index is " + this._trig_index);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this._black_index != -1)
            {
                var newblack = state.GetEntInfoByIndex(_black_index);

                if (newblack.EntityPtr == IntPtr.Zero)
                {
                    _black_index = -1;
                    Debug.WriteLine("lostcoast start");
                    this.StartOffsetTicks = -14;
                    // no once flag because the end wont trigger otherwise
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this._trig_index != -1)
            {
                var newtrig = state.GetEntInfoByIndex(_trig_index);

                if (newtrig.EntityPtr == IntPtr.Zero)
                {
                    _black_index = -1;
                    Debug.WriteLine("lostcoast end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}