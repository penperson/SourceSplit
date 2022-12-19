﻿using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_StillAlive : PortalBase
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on game disconnect

        private MemoryWatcher<Vector3f> _elevatorPos;
        private float _splitTime;

        public PortalMods_StillAlive() : base()
        {
            this.AddFirstMap("stillalive_1");
            this.AddLastMap("stillalive_14");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
        {
            if (state.HostState.Current == HostState.GameShutdown)
                this.OnUpdate(state, actions);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap)
                _elevatorPos = new MemoryWatcher<Vector3f>(state.GameEngine.GetEntityByName("a10_a11_elevator_body") + state.GameEngine.BaseEntityAbsOriginOffset);

            _splitTime = 0f;
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            float splitTime = 0f;
            if (this.IsLastMap)
            {
                _elevatorPos.Update(state.GameProcess);
                if (_elevatorPos.Current.Z >= 3760)
                    splitTime = state.GameEngine.GetOutputFireTime("client_command", 10);
            }
            else
                splitTime = state.GameEngine.GetOutputFireTime("*command*", "Command", "*map *", 10);

            if (splitTime != 0f)
                _splitTime = splitTime;

            if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
            {
                _splitTime = 0f;
                Debug.WriteLine("portal still alive " + (!this.IsLastMap ? "split" : "end"));
                OnceFlag = true;

                state.QueueOnNextSessionEnd = this.IsLastMap ?
                    () => actions.End() :
                    () => actions.Split();
            }

            return;
        }

    }
}
