﻿using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DayHard : GameSupport
    {
        // start: when player view entity changes from start camera to the player
        // ending: when breen is killed

        private int _camIndex;
        private int _propIndex;

        public DayHard()
        {
            this.AddFirstMap("dayhardpart1");
            this.AddLastMap("breencave");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._camIndex = state.GameEngine.GetEntIndexByName("cutscene3");
               // Debug.WriteLine("_camIndex index is " + this._camIndex);
            }

            if (this.IsLastMap)
            {
                this._propIndex = state.GameEngine.GetEntIndexByName("Patch3");
                //Debug.WriteLine("_propIndex index is " + this._propIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Current == 1 &&
                    state.PlayerViewEntityIndex.Old == _camIndex)
                {
                    Debug.WriteLine("DayHard start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }

            }
            else if (this.IsLastMap && _propIndex != -1)
            {
                var newProp = state.GameEngine.GetEntInfoByIndex(_propIndex);

                if (newProp.EntityPtr == IntPtr.Zero)
                {
                    Debug.WriteLine("DayHard end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
