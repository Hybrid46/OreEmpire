#pragma instancing_options procedural:setup

Out = In;

InstanceID = 0;
#if UNITY_ANY_INSTANCING_ENABLED
	InstanceID = unity_InstanceID;
#endif