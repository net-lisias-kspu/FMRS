# FMRS :: Changes

* 2020-1006: 1.2.9.2 (lisias) for KSP >= 1.4
	+ Added KSPe facilities for:
		- Log, UI, Instalment checks
	+ Moved the whole shebang to the `net.lisias.ksp` folder hierarchy
	+ Reworked all the merged things below to keep working from KSP 1.4 and above
		- KSP 1.3.1 and older could not be supported at this time (see [KNOWN ISSUES](./KNOWN_ISSUES.md))
			- Perhaps in the future?  
	+ Merged up latest developments from the upstream:
		- 1.2.9.1
			- Fixed initialization of vars needed due to Unity upgrade for KSP 1.8
		- 1.2.9
			- Updated for KSP 1.8
		- 1.2.8.1
		- 1.2.8
			- Version bump for 1.7.2
			- Updated AssemblyVersion.tt
			- Deleted old disabled code  
		- 1.2.7.4
* 2018-0804: 1.2.7.3 (lisias) for KSP 1.4.x
	+ Moving settings files to <KSP_ROOT>/PluginData
