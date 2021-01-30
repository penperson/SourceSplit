﻿// what is this?
// to cut down on the number of files included in this project, this file was created
// to contain code for mods with similar auto-start and stop behavior or didn't require much programming

// mods included: think tank, gnome, hl2 backwards mod, hl2 reject, trapville, rtslville, hl abridged, episode one

using LiveSplit.ComponentUtil;
using System.Diagnostics;


namespace LiveSplit.SourceSplit.GameSpecific
{

    class HL2Mods_ThinkTank : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final output is fired

        private bool _onceFlag;

        public HL2Mods_ThinkTank()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ml04_ascend";
            this.LastMap = "ml04_crown_bonus";
            this.StartOnFirstMapLoad = true;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("servercommand", 3);
                if (state.CheckOutputSplitTime(splitTime))
                {
                    Debug.WriteLine("think tank end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_Gnome : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final output is fired

        private bool _onceFlag;

        public HL2Mods_Gnome()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "at03_findthegnome";
            this.LastMap = "at03_nev_no_gnomes_land";
            this.StartOnFirstMapLoad = true;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("cmd_end", 2);
                if (state.CheckOutputSplitTime(splitTime))
                {
                    Debug.WriteLine("gnome end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_BackwardsMod : GameSupport
    {
        // start: on first map
        public HL2Mods_BackwardsMod()
        {
            this.StartOnFirstMapLoad = true;
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "backward_d3_breen_01";
            this.StartOnFirstMapLoad = true;
        }
    }

    class HL2Mods_Reject : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_Reject()
        { 
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "reject";
            this.StartOnFirstMapLoad = true;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        private bool _onceFlag;

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            float splitTime = state.FindOutputFireTime("komenda", 3);
            if (state.CheckOutputSplitTime(splitTime))
            {
                Debug.WriteLine("hl2 reject end");
                _onceFlag = true;
                return GameSupportResult.PlayerLostControl;
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_TrapVille : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_TrapVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "aquickdrivethrough_thc16c4";
            this.LastMap = "makeearthgreatagain_thc16c4";
            this.StartOnFirstMapLoad = true;
        }

        private bool _onceFlag = false;

        private Vector3f _endSector = new Vector3f(7953f, -11413f, 2515f);

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            // todo: probably should use the helicopter's position?
            if (IsLastMap && state.PlayerPosition.Distance(_endSector) <= 300f)
            {
                float splitTime = state.FindOutputFireTime("game_end", 10);
                if (state.CheckOutputSplitTime(splitTime))
                {
                    Debug.WriteLine("trapville end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_RTSLVille : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_RTSLVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "from_ashes_map1_rtslv";
            this.LastMap = "terminal_rtslv";
            this.StartOnFirstMapLoad = true;
        }

        private bool _onceFlag = false;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap && state.PlayerViewEntityIndex != GameState.ENT_INDEX_PLAYER)
            {
                float splitTime = state.FindOutputFireTime("clientcommand", 8);
                if (state.CheckOutputSplitTime(splitTime))
                {
                    Debug.WriteLine("rtslville end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_Abridged : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_Abridged()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ml05_training_facilitea";
            this.LastMap = "ml05_shortcut17";
            this.StartOnFirstMapLoad = true;
        }

        private bool _onceFlag = false;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("end_disconnect", "command", "disconnect; map_background background_ml05", 6);
                if (state.CheckOutputSplitTime(splitTime))
                {
                    Debug.WriteLine("hl abridged end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_EpisodeOne : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_EpisodeOne()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "direwolf";
            this.LastMap = "outland_resistance";
            this.StartOnFirstMapLoad = true;
        }

        private bool _onceFlag = false;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("point_clientcommand2", 4);
                if (state.CheckOutputSplitTime(splitTime))
                {
                    Debug.WriteLine("episode one end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}