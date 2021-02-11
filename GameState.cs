﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;

namespace LiveSplit.SourceSplit
{
    // change back to struct if we ever need to give a copy of the state
    // to the ui thread
    class GameState
    {
        public const int ENT_INDEX_PLAYER = 1;

        public const float IO_EPSILON = 0.03f; // precision of about 2 ticks, could be lowered?

        public Process GameProcess;
        public GameOffsets GameOffsets;

        public HostState HostState;
        public HostState PrevHostState;

        public SignOnState SignOnState;
        public SignOnState PrevSignOnState;

        public ServerState ServerState;
        public ServerState PrevServerState;

        public string CurrentMap;
        public string GameDir;

        public float IntervalPerTick;
        public int RawTickCount;
        public int TickBase;
        public int TickCount;
        public float TickTime;

        public FL PlayerFlags;
        public FL PrevPlayerFlags;
        public Vector3f PlayerPosition;
        public Vector3f PrevPlayerPosition;
        public int PlayerViewEntityIndex;
        public int PrevPlayerViewEntityIndex;
        public int PlayerParentEntityHandle;
        public int PrevPlayerParentEntityHandle;

        public CEntInfoV2 PlayerEntInfo;
        public GameSupport GameSupport;
        public int UpdateCount;

        public GameState(Process game, GameOffsets offsets)
        {
            this.GameProcess = game;
            this.GameOffsets = offsets;
        }

        public CEntInfoV2 GetEntInfoByIndex(int index)
        {
            Debug.Assert(this.GameOffsets.EntInfoSize > 0);

            CEntInfoV2 ret;

            IntPtr addr = this.GameOffsets.GlobalEntityListPtr + ((int)this.GameOffsets.EntInfoSize * index);

            if (this.GameOffsets.EntInfoSize == CEntInfoSize.HL2)
            {
                CEntInfoV1 v1;
                this.GameProcess.ReadValue(addr, out v1);
                ret = CEntInfoV2.FromV1(v1);
            }
            else
            {
                this.GameProcess.ReadValue(addr, out ret);
            }

            return ret;
        }

        // warning: expensive -  7ms on i5
        // do not call frequently!
        public IntPtr GetEntityByName(string name)
        {
            const int MAX_ENTS = 4096; // TODO: is portal2's max higher?

            for (int i = 0; i < MAX_ENTS; i++)
            {
                CEntInfoV2 info = this.GetEntInfoByIndex(i);
                if (info.EntityPtr == IntPtr.Zero)
                    continue;
                    
                IntPtr namePtr;
                this.GameProcess.ReadPointer(info.EntityPtr + this.GameOffsets.BaseEntityTargetNameOffset, false, out namePtr);
                if (namePtr == IntPtr.Zero)
                    continue;

                string n;
                this.GameProcess.ReadString(namePtr, ReadStringType.ASCII, 32, out n);  // TODO: find real max len
                if (n == name)
                    return info.EntityPtr;
            }

            return IntPtr.Zero;
        }

        public int GetEntIndexByName(string name)
        {
            const int MAX_ENTS = 2048; // TODO: is portal2's max higher?

            for (int i = 0; i < MAX_ENTS; i++)
            {
                CEntInfoV2 info = this.GetEntInfoByIndex(i);
                if (info.EntityPtr == IntPtr.Zero)
                    continue;

                IntPtr namePtr;
                this.GameProcess.ReadPointer(info.EntityPtr + this.GameOffsets.BaseEntityTargetNameOffset, false, out namePtr);
                if (namePtr == IntPtr.Zero)
                    continue;

                string n;
                this.GameProcess.ReadString(namePtr, ReadStringType.ASCII, 32, out n);  // TODO: find real max len
                if (n == name)
                    return i;
            }

            return -1;
        }

        public int GetEntIndexByPos(float x, float y, float z, float d = 0f, bool xy = false)
        {
            Vector3f pos = new Vector3f(x, y, z);
            const int MAX_ENTS = 2048; // TODO: is portal2's max higher?

            for (int i = 0; i < MAX_ENTS; i++)
            {
                CEntInfoV2 info = this.GetEntInfoByIndex(i);
                if (info.EntityPtr == IntPtr.Zero)
                    continue;

                Vector3f newpos;
                if (!this.GameProcess.ReadValue(info.EntityPtr + this.GameOffsets.BaseEntityAbsOriginOffset, out newpos))
                    continue;

                if (d == 0f)
                {
                    if (newpos.BitEquals(pos) && i != 1) //not equal 1 becase the player might be in the same exact position
                        return i;
                }
                else // check for distance if it's a non-static entity like an npc or a prop
                {
                    if (xy) 
                    {
                        if (newpos.DistanceXY(pos) <= d && i != 1) 
                            return i;
                    }
                    else
                    {
                        if (newpos.Distance(pos) <= d && i != 1) 
                            return i;
                    }
                }
            }

            return -1;
        }

        public Vector3f GetEntityPos(int i)
        {
            Vector3f pos;
            var ent = GetEntInfoByIndex(i);
            GameProcess.ReadValue(ent.EntityPtr + this.GameOffsets.BaseEntityAbsOriginOffset, out pos);
            return pos;
        }

        // env_fades don't hold any live fade information and instead they network over fade infos to the client which add it to a list
        
        public float FindFadeEndTime(float speed)
        {
            int fadeListSize = GameProcess.ReadValue<int>(GameOffsets.FadeListPtr + 0x10);
            if (fadeListSize == 0) return 0;

            ScreenFadeInfo fadeInfo;
            uint fadeListHeader = GameProcess.ReadValue<uint>(GameOffsets.FadeListPtr + 0x4);
            for (int i = 0; i < fadeListSize; i++)
            {
                fadeInfo = GameProcess.ReadValue<ScreenFadeInfo>(GameProcess.ReadPointer((IntPtr)fadeListHeader) + 0x4 * i);
                if (fadeInfo.Speed != speed)
                    continue;
                else
                    return fadeInfo.End;
            }
            return 0;
        }

        public float FindFadeEndTime(float speed, byte r, byte g, byte b)
        {
            int fadeListSize = GameProcess.ReadValue<int>(GameOffsets.FadeListPtr + 0x10);
            if (fadeListSize == 0) return 0;

            ScreenFadeInfo fadeInfo;
            byte[] targColor = { r, g, b };
            uint fadeListHeader = GameProcess.ReadValue<uint>(GameOffsets.FadeListPtr + 0x4);
            for (int i = 0; i < fadeListSize; i++)
            {
                fadeInfo = GameProcess.ReadValue<ScreenFadeInfo>(GameProcess.ReadPointer((IntPtr)fadeListHeader) + 0x4 * i);
                byte[] color = { fadeInfo.r, fadeInfo.g, fadeInfo.b };
                if (fadeInfo.Speed != speed && !targColor.Equals(color))
                    continue;
                else
                    return fadeInfo.End;
            }
            return 0;
        }

        // ioEvents are stored in a non-contiguous list where every ioEvent contain pointers to the next or previous event 
        // todo: add more input types and combinations to ensure correct result
        public float FindOutputFireTime(string targetName, int clamp = 100)
        {
            if (GameProcess.ReadPointer(GameOffsets.EventQueuePtr) == IntPtr.Zero)
                return 0;

            EventQueuePrioritizedEvent ioEvent;
            GameProcess.ReadValue(GameProcess.ReadPointer(GameOffsets.EventQueuePtr), out ioEvent);

            // clamp the number of items to go through the list to save performance
            // the list is automatically updated once an output is fired
            for (int i = 0; i < clamp; i++)
            {
                string tempName = GameProcess.ReadString((IntPtr)ioEvent.m_iTarget, 256);
                if (tempName == targetName)
                    return ioEvent.m_flFireTime;
                else
                {
                    IntPtr nextPtr = (IntPtr)ioEvent.m_pNext;
                    if (nextPtr != IntPtr.Zero)
                    {
                        GameProcess.ReadValue(nextPtr, out ioEvent);
                        continue;
                    }
                    else return 0; // end early if we've hit the end of the list
                }
            }

            return 0;
        }

        public float FindOutputFireTime(string targetName, string command, string param, int clamp = 100)
        {
            if (GameProcess.ReadPointer(GameOffsets.EventQueuePtr) == IntPtr.Zero)
                return 0;

            EventQueuePrioritizedEvent ioEvent;
            GameProcess.ReadValue(GameProcess.ReadPointer(GameOffsets.EventQueuePtr), out ioEvent);

            for (int i = 0; i < clamp; i++) 
            {
                string tempName = GameProcess.ReadString((IntPtr)ioEvent.m_iTarget, 256);
                string tempCommand = GameProcess.ReadString((IntPtr)ioEvent.m_iTargetInput, 256);
                string tempParam = GameProcess.ReadString((IntPtr)ioEvent.v_union, 256) == null ? "" : GameProcess.ReadString((IntPtr)ioEvent.v_union, 256);

                if (tempName == targetName &&
                    tempCommand.ToLower() == command.ToLower() && 
                    tempParam.ToLower() == param.ToLower())
                    return ioEvent.m_flFireTime;
                else
                {
                    IntPtr nextPtr = (IntPtr)ioEvent.m_pNext;
                    if (nextPtr != IntPtr.Zero)
                    {
                        GameProcess.ReadValue(nextPtr, out ioEvent);
                        continue;
                    }
                    else return 0; // end early if we've hit the end of the list
                }
            }

            return 0;
        }

        // fixme: this *could* probably return true twice if the player save/loads on an exact tick
        // precision notice: will always be too early by at most 2 ticks using the standard 0.03 epsilon
        public bool CompareToInternalTimer(float splitTime, float epsilon = IO_EPSILON)
        {
            return splitTime != 0f && Math.Abs(splitTime - RawTickCount * IntervalPerTick) <= epsilon;
        }
    }

    struct GameOffsets
    {
        public IntPtr CurTimePtr;
        public IntPtr TickCountPtr => this.CurTimePtr + 12;
        public IntPtr IntervalPerTickPtr => this.TickCountPtr + 4;
        public IntPtr SignOnStatePtr;
        public IntPtr CurMapPtr;
        public IntPtr GlobalEntityListPtr;
        public IntPtr GameDirPtr;
        public IntPtr HostStatePtr;
        public IntPtr FadeListPtr;
        // note: only valid during host states: NewGame, ChangeLevelSP, ChangeLevelMP
        // note: this may not work pre-ep1 (ancient engine), HLS -7 is a good example
        public IntPtr HostStateLevelNamePtr => this.HostStatePtr + (4 * (GameMemory.IsSource2003 ? 2 : 8));
        public IntPtr ServerStatePtr;
        public IntPtr EventQueuePtr;

        public CEntInfoSize EntInfoSize;

        public int BaseEntityFlagsOffset;
        public int BaseEntityEFlagsOffset => this.BaseEntityFlagsOffset > 0 ? this.BaseEntityFlagsOffset - 4 : -1;
        public int BaseEntityAbsOriginOffset;
        public int BaseEntityTargetNameOffset;
        public int BaseEntityParentHandleOffset;
        public int BasePlayerViewEntity;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CEntInfoV1
    {
        public uint m_pEntity;
        public int m_SerialNumber;
        public int m_pPrev;
        public int m_pNext;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CEntInfoV2
    {
        public uint m_pEntity;
        public int m_SerialNumber;
        public int m_pPrev;
        public int m_pNext;
        public int m_targetname;
        public int m_classname;

        public IntPtr EntityPtr => (IntPtr)this.m_pEntity;

        public static CEntInfoV2 FromV1(CEntInfoV1 v1)
        {
            var ret = new CEntInfoV2();
            ret.m_pEntity = v1.m_pEntity;
            ret.m_SerialNumber = v1.m_SerialNumber;
            ret.m_pPrev = v1.m_pPrev;
            ret.m_pNext = v1.m_pNext;
            return ret;
        }
    }

    // taken from source sdk
    [StructLayout(LayoutKind.Sequential)]
    public struct ScreenFadeInfo
    {
        public float Speed;            // How fast to fade (tics / second) (+ fade in, - fade out)
        public float End;              // When the fading hits maximum
        public float Reset;            // When to reset to not fading (for fadeout and hold)
        public byte r, g, b, alpha;    // Fade color
        public int Flags;              // Fading flags
    };

    // todo: figure out a way to utilize ehandles
    [StructLayout(LayoutKind.Sequential)]
    public struct EventQueuePrioritizedEvent
    {
        public float m_flFireTime;
        public uint m_iTarget;
        public uint m_iTargetInput;
        public uint m_pActivator;       // EHANDLE
        public uint m_pCaller;          // EHANDLE
        public int m_iOutputID;
        public uint m_pEntTarget;       // EHANDLE
        // variant_t m_VariantValue, class, only relevant members
        // most notable is v_union which stores the parameters of the i/o event
        public uint v_union, v_eval, v_fieldtype, v_tostringfunc, v_CVariantSaveDataOpsclass;
        public uint m_pNext;
        public uint m_pPrev;

        public int GetIndexOfEHANDLE(uint EHANDLE)
        {
            // FIXME: this mask is actually version dependent, newer ones use 0x1fff!!!
            // possible sig to identify: 8b ?? ?? ?? ?? ?? 8b ?? 81 e1 ff ff 00 00
            return (EHANDLE & 0xfff) == 0xfff ? -1 : (int)(EHANDLE & 0xfff);
        }
        public int m_pActivatorIndex => GetIndexOfEHANDLE(m_pActivator);
        public int m_pCallerIndex => GetIndexOfEHANDLE(m_pCaller);
        public int m_pEntTargetIndex => GetIndexOfEHANDLE(m_pEntTarget);
    };
}
