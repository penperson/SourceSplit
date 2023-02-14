﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using LiveSplit.SourceSplit.Utilities;
using System.Xml;
using System.Drawing;
using static LiveSplit.SourceSplit.ComponentHandling.SourceSplitSettingsHelp;
using LiveSplit.SourceSplit.ComponentHandling;
using System.Security;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    public partial class SourceSplitSettings : UserControl
    {
        private void SetSettingDescriptions()
        {
            const string EXPLAIN_GENERIC_LOAD = "New Game map loads are map changes which are not triggered by a changelevel trigger, or through using the changelevel command. Instead, it is typically triggered using the 'map' command. If triggered through the console, a dedicated loading screen with a progress bar will apprear. If triggered by the map using entities, a black screen is shown.";

            const string EXPLAIN_CHANGELEVEL = "Changelevels are map changes which are triggered by a changelevel trigger, or through using the changelevel command. When a changelevel is triggered, the game will freeze the screen, and display a box with 'LOADING' written on it.";

            const string EXPLAIN_SESSION = "A session is the time between 2 loads.";

            _help.SetName(labVersion, "Version text");
            _help.SetDescription(labVersion,
                @"This is the version of this build of SourceSplit. The build date is parenthesized and appended to it.");

            _help.SetName(tlpAutoStartEndReset, "Main Functions");

            _help.SetDescription(gAutoSplit, 
                "Settings related to Automatic Splitting.");
            
            _help.SetDescription(chkAutoSplitEnabled,
                "This option enables or disables Automatic Splitting.");

            _help.SetName(gMapTransitions, "Split on Map Transitions");
            _help.SetDescription(gMapTransitions,
                @"This option includes options for configuring how SourceSplit should treat types of map transitions, and whether to split upon detecting one.");

            _help.SetName(panSplitInterval, "Every ? transition");
            _help.SetDescription(panSplitInterval,
                @"This option decides whether SourceSplit should skip a number of considered map transitions before Auto-Splitting on one.

If set to 1, SourceSplit will not skip any transitions; if set to 2, SourceSplit will skip 1 transition, before splitting on the next one; and etc....

This mechanism resets when the timer is reset.");

            _help.SetDescription(chkSplitGenericMap,
                @"This option decides whether SourceSplit should consider New Game map loads as map transitions, or ignore them.

" + EXPLAIN_GENERIC_LOAD);

            _help.SetName(dgvMapTransitions, "Map Transition List");
            _help.SetDescription(dgvMapTransitions,
                @"This option is the list of map transitions which is to be compared to one for filtering to decide whether SourceSplit should consider it.

The left-hand side of each line is the source map, and the corresponding right-hand side of each line is the destination map in a map transition.

Select and type in the bottom-most row to add a new entry. Select an entire line and hit Delete to remove that line. 
Double click on a cell to begin editing it. Select one or more cells and hit backspace to clear them.

Enter a single asterisk (*) to signify any map.");

            _help.SetName(cmbMTLMode, "Map Transition Filtering Type");
            _help.SetDescription(cmbMTLMode,
                @"This option decides what SourceSplit should do if a map transition is included in the list.

If set to 'Allow', SourceSplit will consider this map transition. If set to 'Disallow', SourceSplit will ignore this map transition.");

            _help.SetName(chkUseMTL, "Only ? Map Transitions");
            _help.SetDescription(chkUseMTL,
                @"This option decides whether to enable or disable filtering map transitions to decide which ones SourceSplit should consider.");

            _help.SetName(gMTL, "Only ? Map Transitions");
            _help.SetDescription(gMTL,
                @"This option contains settings of the filtering applied onto detected map transitions to decide whether SourceSplit should consider them");

            _help.SetDescription(chkSplitSpecial,
                @"This option decides whether SourceSplit should Auto-Split upon detecting a game/mod-specific pre-defined event such as completing objectives, reaching specific destinations, etc..");

            _help.SetName(gAdditionalAutoStart, "Also Auto-Start when");
            _help.SetDescription(gbAutoStartEndReset,
                @"This option contains settings for Automatic Starting, Stopping and Resetting");

            _help.SetDescription(chkAutoStart,
                @"This option enables or disables Automatic Starting");

            _help.SetDescription(chkAutoStop,
                @"This option enables or disables Automatic Stop");

            _help.SetDescription(chkAutoReset,
                @"This option enables or disables Automatic Resetting");

            _help.SetDescription(chkAllowAddAutoStart,
                @"This option enables or disables an additional Auto-Start condition.");

            _help.SetName(gAdditionalAutoStart, "Also Auto-Start when...");
            _help.SetDescription(gAdditionalAutoStart,
                @"This option contains settings of the Additional Auto-Start, which will be triggered along with the normal Auto-Start.");

            _help.SetName(cmbAddAutoStartMode, "Additional Auto-Start condition");
            _help.SetDescription(cmbAddAutoStartMode,
                $@"This option decides the condition of the additional Auto-Start

If the condition is 'Starting a New Game on this map', when a New Game map load is detected, the game will compare the destination map to the input string. 
If the condition is 'Transitioning to this map', when a changelevel map transition is detected, the game will compare the destination map to the input string.
If the condition is 'Loading a save with this name', when a save is loaded, the game will compare the name of the save to the input string.

The input string does not need to include the file extension, if there is one.

{EXPLAIN_CHANGELEVEL}

{EXPLAIN_GENERIC_LOAD}
");

            _help.SetName(boxAddAutoStartName, "Additional Auto-Start input string");
            _help.SetDescription(boxAddAutoStartName,
                @"This option defines the input string that the specified additional Auto-Start condition should use");

            _help.SetDescription(gbTiming, 
                @"This option contain settings for configuring how SourceSplit should count Game Time");

            _help.SetDescription(gTimingMethods,
                @"This option contains settings for choosing what types of time SourceSplit should consider.");

            _help.SetDescription(chkCountEngineTicks,
                @"This setting decides whether SourceSplit should count Engine Ticks into Game Time. 

Engine Ticks are ticks when the game is active (i.e. a map is loaded), and when physics is being simulated (i.e. when the game isn't paused)");

            _help.SetDescription(chkCountPauses,
                @"This setting decides whether SourceSplit should count Pauses into Game Time. 

These moments are when the game is displaying its load screen; or displaying the 'LOADING' plaque, if the pause was triggered using the 'pause' command.");

            _help.SetDescription(chkCountDisconnect,
                @"This setting decides whether SourceSplit should count Disconnects into Game Time.

These moments are when the game is not active (i.e. when no map is loaded, and the game is sitting idle in the menus, with a picture for its background).");

            _help.SetDescription(chkNoGamePresent,
                @"This setting decides whether SourceSplit should count No Game time into Game Time.

These moments are when the game is not open. This does seem weird, yes, but it's there, because...");

            _help.SetDescription(chkDemoInterop,
                @"This setting decides whether SourceSplit should perform interoperation with the game/mod's Demo Recording.

If enabled, SourceSplit will incorporate Game Time measured in demos which are being recorded, or have just finished recording while the game/mod is running.

In some games/mods, this setting is forced on.");

            _help.SetDescription(chkAutomatic,
                @"This setting decides whether SourceSplit should automatically decide what the best Timing Method for the game/mod.

Enabling this will lock the controls in Timing Method.");

            _help.SetDescription(gbAdditionalTimer,
                @"This setting contains options for enabling or disabling Additional Timers.

These timers are displayed on LiveSplit like a Text Component. Their position is dependent on SourceSplit's position in the list in LiveSplit's Layout Editor.

These timers will only work if SourceSplit is loaded in the Layout through the Layout Editor.");

            _help.SetName(gHigherPrecision, "Show Higher Precision Time");
            _help.SetDescription(gHigherPrecision,
                @"This option contain settings for enabling, disabling, and configuring the Higher Precision Timer.

This timer's precision can be configured, and can be instructed to show the alternate timing method compared to the current one. (If comparing to Game Time, then it will read Real Time, and vice versa).");

            _help.SetDescription(chkShowGameTime,
                @"This option enables or disables the Higher Precision Timer.");

            string decimalPlacesHelp = @"This option decides the precision for the Higher Precision Timer.

The maximum value is 7.";
            _help.SetName(nudDecimalPlaces, "Decimal Places");
            _help.SetDescription(nudDecimalPlaces, decimalPlacesHelp);
            _help.SetDescription(label3, decimalPlacesHelp);

            _help.SetDescription(chkShowAlt,
                @"This option decides whether the Higher Precision Timer will show the alternate timing method compared to the current one. 

If comparing to Game Time, then it will read Real Time, and vice versa.");

            _help.SetDescription(chkShowTickCount,
                $@"This option enables or disables the Game Time Tick Count.

This is an Additional Timer which will show both the current Game Time as ticks, and the current session's tick count.

{EXPLAIN_SESSION}");

            _help.SetDescription(chkShowCurDemo,
                @"This option decide whether SourceSplit should show the information of the currently-recorded demo.

This Additional Timer will show the name and the current recording time of the demo which is being recorded. If no demo is being recorded, the information of the previously recorded demo will be shown.");

            _help.SetDescription(gbMapTimes,
                $@"This option shows the Session and Map times window. 

This window shows the time of the recorded sessions, and tallies the time and displays them by map.

{EXPLAIN_SESSION}
");

            _help.SetName(gPrintDemoInfo, "Print info of Demos after recording");
            _help.SetDescription(gPrintDemoInfo,
                @"This option contain settings for printing information of demos which have just finished recording into the game/mod's console.

A custom external Demo Parser can be specified to be used instead of SourceSplit's internal one.");

            _help.SetName(boxDemoParserPath, "Path of external Demo Parser");
            _help.SetDescription(boxDemoParserPath,
                @"This option decides the path of the external Demo Parser, for use to print the information of the last recorded demo.

This Demo Parser must automatically print the demo's information when given the double-quoted path to it as a single standalone parameter.");

            _help.SetDescription(chkPrintDemoInfo,
                @"This option enables or disables printing the information of demos which have just finished recording into the game/mod's console.

Note: this may not work for all games/mods.");

            _help.SetDescription(gTimerBehavior,
                @"This option contains settings for changing timer's behavior upon reacting to various events.");

            _help.SetDescription(chkHoldUntilPause,
                @"This option decides whether the timer should pause when it is started while the game is paused, and then immediately resume upon the very next moment the game is not paused.

If 'Pauses' is not enabled in 'Timing Options', and this is enabled, the timer will still behave as described.");

            _help.SetDescription(chkRTAStartOffset,
                @"This option decides if the built-in Game Time offset for Auto-Start should be applied onto Real Time.

Some games may have hardcoded Auto-Start time offsets, such as Portal when loading vault save. When an Auto-Start is triggered, that time offset is then applied onto Game Time, but not Real Time. This setting decides whether that offset should also be applied onto Real Time.");

            _help.SetDescription(groupBox5,
                @"This option contain settings for handling Saving and Loading.");

            _help.SetDescription(chkServerInitialTicks,
                @"This option enables or disables including server initializtion ticks when counting Game Time.

These ticks happen before the game is fully loaded, and before demo recording can usually begin.");

            string slPenaltyHelp = @"This option decides the number of ticks which should be added to Game Time when the game finishes loading something.

Because this is only added when the game finishes loading, rapidly Save/Load buffering will not rapidly add this number of ticks to the timer.

In some games/mods, this setting is forced to 1.";
            _help.SetName(nudSLPenalty, "Ticks to add to IGT per finished load");
            _help.SetDescription(nudSLPenalty, slPenaltyHelp);
            _help.SetDescription(label4, slPenaltyHelp);

            _help.SetDescription(boxSplitInstead,
                @"This option decides whether an Auto-Split should be triggered when an Auto-Reset does.

If Auto-Resetting is disabled, an Auto-Split will not be triggered.");

            _help.SetDescription(chkResetMapTransitions,
                @"This option enables or disables optimizations for multi-run speedruns.

Multi-run speedruns are runs which repeat a category more than once, resulting in Auto-Reset and Auto-Start being triggered more than once. This setting changes some internal logic to accommodate such runs.");

            _help.SetDescription(chkFirstMapReset,
                $@"This option decides whether an Auto-Reset should be triggered when a New Game map load to the pre-defined first map of the game/mod is triggered.

{EXPLAIN_GENERIC_LOAD}
");

            _help.SetName(labCurrentGame, "Detected game/mod");
            _help.SetDescription(labCurrentGame,
                @"The game/mod that is identified by SourceSplit");

            _help.SetName(labRunningFor, "Running for");
            _help.SetDescription(labRunningFor,
                @"The amount of time that this instance of SourceSplit has been running for.");

            _help.SetDescription(butGRepo,
                @"This button opens the Github repository of this SourceSplit fork.");

            _help.SetDescription(butReport,
                @"This button opens the Github Issues page of this SourceSplit fork, where you can report an issue that you've encountered while using the tool.");

            _help.SetDescription(butReleases,
                @"This button opens the Github Releases page of this SourceSplit fork, where you can find the latest and previous releases of this SourceSplit fork.");

            _help.SetDescription(butSetup,
                @"This button opens the Configuring and Setup guide on this SourceSplit fork's Github page, where you can find detailed guides on installing and using this tool.");

            _help.SetDescription(groupBox6,
                @"The main people behind SourceSplit.

Fatalis is the creator of the original versions of SourceSplit.
2838 is the creator of this fork of SourceSplit, and is also currently its maintainer.");

            _help.SetDescription(groupBox7,
                @"The people who tested v3.3.0.

Version 3.3.0 represented a massive change of this fork of SourceSplit, introducing many new features and changes to code. This required extensive testing, to ensure timing accuracy, and tool stability.");
        }
    }
}
