# FMRS :: Change Log

* 2017-1009: 1.2.6 (linuxgurugamer) for KSP 1.3.1
	+ KSP 1.3.1 update 
* 2017-0529: 1.2.5 (linuxgurugamer) for KSP 1.3.0
	+ Fixed bug in RecoveryController which caused harmless nullrefs when StageRecovery wasn't installed Updated to 1.3
* 2017-0413: 1.2.4 (linuxgurugamer) for KSP 1.2.2
	+ Added option to allow uncontrollable stages to be controlled (ie: watched) by FMRS;
		- ignored if RecoveryController is active Some fine-tuning of interactions between FMRS, StageRecovery and RecoveryController 
* 2017-0410: 1.2.3.8 (linuxgurugamer) for KSP 1.2.2
	+ Added integration with RecoveryController
	+ Added RecoveryController directory 
* 2017-0324: 1.2.3.1 (linuxgurugamer) for KSP 1.2.2
	+ Updated icons 
* 2017-0321: 1.2.3 (linuxgurugamer) for KSP 1.2.2
	+ Thanks to @https://github.com/joaofarias, this fixes the issue of clearing contracts and strategies 
* 2017-0310: 1.2.2.1 (linuxgurugamer) for KSP 1.2.2
	+ Fixed .version file to use correct record on ksp-avc site
* 2017-0310: 1.2.2 (linuxgurugamer) for KSP 1.2.2
	+ Fixed issue where contracts and strategies got canceled if Revert to Launch was done from inside FMRS 
* 2017-0226: 1.2.1 (linuxgurugamer) for KSP 1.2.2
	+ No changelog provided 

*No binaries found for the following releases*

* 1.0.01
	+ built for KSP 1.0.5
* v1.0.00
	+ compatible with KSP 1.0.2
* 0.3.02
	+ bug fix: Recovering main vessel leads into mission progress loss
* 0.3.01
	+ compiled with 0.90
	+ minor fixes to support the update
	+ bug fix: changes the default text size of the game
* v0.3.00
	+ massive code overhaul of the essential functions (ModuleManager is now required)
	+ KSP-AVC support added
	+ strategy support added (recovery factor)
	+ messaging system now can be disabled in the settings
	+ max window height reduced
	+ some minor bug fixes
	+ WIP feature ThrustLogger added
* 0.2.03
	+ serious bug fixed:	switching to spacecenter or trackingstation, while flying a dropped vessel, doesn't kicks you to your main vessel
		- also effects the recover button above the altimeter
	+ decouple detection delay increased
	+ auto thrust cut off delay decreased
	+ main menu module added (deletes wrong save values after a game crash)
* 0.2.02
	+ bug fix: starting plugin with toolbar could lead into loading an old save file
	+ bug fix: switching to tracking station, while flying dropped stage, doesn't kicks you to main vessel
	+ bug fix: the disabled texture for the stock toolbar is not loaded
	+ recovering vessel in sandbox now prints a recovery message
	+ bounce sup
	+ sion for the reset button gui added
* 0.2.01
	+ compiled with 0.25 x64
	+ stock toolbar support added
	+ "Jump back to Separation" button is now dependent to the settings in the debug toolbar (Quicksave & Quickload)
	+ all non staged vessels are all listed as separtated (no more undocked listings)
	+ recovery message now lists parts and resources
	+ after separtion the main vessel can be changed for 10 sec ("Focus Next Vessel", "Focus Prev Vessel")
	+ bug fix: kerbal in main vessel killed, while flying separated stage, leads to rep loss in main save
	+ bug fix: closing the plugin using the toolbar kicks you not to the main save
	+ bug fix: reading empty save file entrys = nullreference exception
* 0.2.00
	+ **BEFORE UPDATING FROM v0.1.xx TO v0.2.00 DELETE THE FOLDER "PluginData" INSIDE "GameData\FMRS"**
	+ settings window added ("s" button next to the "_" button)
	+ auto engine cut off for dropped vessels added
	+ (auto) recover function added (returns stored science date an funds)
	+ tracking for killed kerbals added (kill kerbal while flying dropped craft = reputation loss)
	+ recovering crafts and killing kerbals generate messages
	+ changed window style, layout and resizing
	+ text box for recovered vessels added
	+ recognizes stages with RealChute as controllable
	+ window is now clamped inside the screen
	+ "Revert To Launch" button is now dependent to the settings in the debug toolbar
	+ save file structure changed
	+ bug fix: window disappears after decoupling before launch
	+ bug fix: plugin doesn´t resets after revert to launch (to a not on launch pad launch)
	+ various smaller tweaks to support the update
* 0.1.04
	+ compiled with 0.24.2 x64
	+ reset window appearance modified (current vessel must be commendable and no flag, kerbal or debris)
	+ moved toolbar init from the Awake() into the Start() routine
	+ changed conversion of the window position values
* 0.1.03
	+ the plugin now can be reset, to pre-flight, if the controlled vessel has landed on a surface
	+ window height adapts to displayed content
	+ fixed an issue with the manual activation of decouplers by mouse click
	+ scene change (other scene than flight scene) now closes the plugin
* 0.1.02
	+ decoupling with docking ports works now as well
* 0.1.01
	+ fixed an issue, when the controlled command part is not in the same stage as the root part
* 0.1.00
	+ initial release
