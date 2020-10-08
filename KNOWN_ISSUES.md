# FMRS :: Known Issues

* There's a nasty problem (probably a stack overflow on OnAwake) affecting Unity 5 and 2017 that played havoc with the KSPe logging.
	+ Heavy disk I/O may aggravate the problem.
	+ This was the real issue when I first realised the problem on [this code](https://github.com/net-lisias-kspu/FMRS/blob/e6008f51fc60dc2608de78b11fb44cd2daa85aae/Source/FMRS/FMRS_Util.cs#L206).
	+ This can affect also [1.4.0 \<= KSP \<= 1.7.3], so Unity 2017 also have the problem
	+ Unity 2019 (KSP \>= 1.8) **is not affected**.
	+ Complete history on [KSPe's Issue #7](https://github.com/net-lisias-ksp/KSPAPIExtensions/issues/7)
