# Flight Manager for Reusable Stages [FMRS] - (Archive)

FMRS lets you jump back and forth in time. Unofficial fork by Lisias.


## In a Hurry

* [Releases](./Archive)
	* [Latest Release](https://github.com/net-lisias-kspu/FMRS/releases)
* [Source](https://github.com/net-lisias-kspu/FMRS)
* [Change Log](./CHANGE_LOG.md)
 

## Description

FMRS lets you jump back and forth in time.

It generates save points after separating controllable probes, ship, landers, ...

So you can launch your mission into space, then jump back in time to the separation and have full control over your dropped vessels.

You can do SpaceX´s Falcon 9 style launches and fly your first stage back to the launch site.

Or launch a space plane on the back of a normal plane. Bring the space plan into space and then fly the launch plane back to the runway.

Drop scientific probes out of flying planes. Let them descend to the ground and do research.

The separated vessels will be added to your main save after landing, or automatically recovered.

### Instruction

Every vessel which should be listed needs to have a probe core, command pod or a RealChute on it.

Go to the launch pad or runway and arm FMRS.

Launch you mission as usual.

Separate your boosters, probes, ...

Every separated vessel which is controllable or has a RealChute will be listed in the window of the plugin.

Now you can jump back to the time of separation and have full control over these vessels.

After you have landed, jump to other dropped crafts or go back to your main vessel and close the plugin.

### Features

* Creates save points after separation of controllable vessels and lets you jump back to the separation and control these vessels.
	+ After you have landed, these vessels will be added to your main save or recovered.
* Recover function.
	+ You can recover the landed vessel by clicking the in game recover button or use the auto recover function.
	+ The costs of the vessel will be refunded including a calculated recovery factor.
	+ All stored scientific data will be added to your main save.
	+ You will get a message which lists all recovered values of your craft as soon as you jump back to your main vessel.
* Automatic recover of separated and landed vessels (can be enabled in the settings).
	+ Recovers the landed vessels automatically, before jumping to other crafts
* Automatic engine cut off of separated crafts (can be enabled in the settings).
	+ FRMS will cut of the engines of separated crafts immediately after separation.
* Killed Kerbal tracking.
	+ If you kill a Kerbal during flying a dropped craft, you will lose reputation in your main save.
* Toolbar support.
	+ FMRS supports the stock toolbar and [Blizzy78's Toolbar Plugin](http://forum.kerbalspaceprogram.com/threads/60863-0-23-5-Toolbar-1-7-1-Common-API-for-draggable-resizable-buttons-toolbar).

### Installation

Copy the FMRS Folder into the GameData Folder of you KSP install.

### Known issues

* Transmitting science during flying a dropped vessel won't return you all science points.
* Contracts can´t be completed while controlling a dropped vessel.
* If you encounter any bug, please contact me on the forums.
* Check the "write debug messages to log file" option in the settings, recreate the bug and send me the log file.
* KSP Bugs
	+ Loading nearby vessels in atmosphere.
	+ http://bugs.kerbalspaceprogram.com/issues/2429

### Licence

This plugin is licensed under the MIT license.


## UPSTREAM

* [linuxgurugamer](https://forum.kerbalspaceprogram.com/index.php?/profile/129964-linuxgurugamer/) CURRENT MAINTAINER
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/157214-141_-flight-manager-for-reusable-stages-fmrs-now-with-recoverycontroller-integration/)
	+ [SpaceDock](https://spacedock.info/mod/1251/%20Flight%20Manager%20for%20Reusable%20Stages%20(FMRS)%20Continued)
	+ [GitHub](https://github.com/linuxgurugamer/FMRS)
* [Omegano](https://forum.kerbalspaceprogram.com/index.php?/profile/172838-omegano/) PARENT
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/72605-110-flight-manager-for-reusable-stages-fmrs-x110-experimental/) 
	+ [GitHub](https://github.com/Omegano/FMRS)
* [SIT89](https://forum.kerbalspaceprogram.com/index.php?/profile/110467-sit89/) ROOT
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/72605-110-flight-manager-for-reusable-stages-fmrs-x110-experimental/&)
	+ [GitHub](https://github.com/SIT89/FMRS)
