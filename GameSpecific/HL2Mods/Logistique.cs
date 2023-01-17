﻿using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{      
    class Logistique : GameSupport
    {
        // start: on first map
        // ending: when the first outro credits text appear on the screen

        private MemoryWatcher<float> _creditsYPos;
        private MemoryWatcher<int> _creditsCount;
        private MemoryWatcher<int> _yResolution;

        private MemoryWatcherList _watcher = new MemoryWatcherList();

        public Logistique()
        {
            this.AddFirstMap("lg-1");
            this.AddLastMap("lg-4");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe vguimatsurface = state.GetModule("vguimatsurface.dll");

            // there are other cleaner pointers but vguimatsurface is most unlikely to change
            // i would've tried to find a sigscanned method but the pointer is extremely hard to get to
            _creditsYPos = new MemoryWatcher<float>(new DeepPointer(vguimatsurface.BaseAddress + 0x147120, 0xF00, 0x2C, 0x9D4 + 0x1BC, 0x200));
            _creditsCount = new MemoryWatcher<int>(new DeepPointer(vguimatsurface.BaseAddress + 0x147120, 0xF00, 0x2C, 0x9D4 + 0x1c8));
            _yResolution = new MemoryWatcher<int>(vguimatsurface.BaseAddress + 0x136C28);

            _watcher = new MemoryWatcherList { _creditsYPos, _creditsCount, _yResolution };
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                _watcher.UpdateAll(state.GameProcess);

                if (_creditsCount.Current >= 1 && _creditsYPos.Changed 
                    && _yResolution.Current / _creditsYPos.Current >= 1.0175f
                    && _yResolution.Current / _creditsYPos.Old < 1.0175f)
                {
                    OnceFlag = true;
                    Debug.WriteLine("logistique end");
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}
