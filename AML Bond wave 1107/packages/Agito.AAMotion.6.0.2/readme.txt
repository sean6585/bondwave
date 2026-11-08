//	Revision History
//  (25/09/2018)
//
//	Template:
//  (Release Date)
//
// The versioning is according to "Semantic Versioning 2.0.0":			
// Given a version number MAJOR.MINOR.PATCH, increments mean:           
//                                                                      
// 1. MAJOR version: incompatible API changes.                          
// 2. MINOR version: add functionality in a backwards compatible manner.
// 3. PATCH version: backwards compatible bug fixes.                    
//
// (16/06/24) Daniel
// v6.0.2:	[Bug Fix] .NET 4.0 YamlDotNet reference 4.2.2
//
// (31/05/24) Alvin
// v6.0.1:	[Bug Fix] Set motion mode when MoveAbs is called.
//
// (08/05/24) Daniel
// v6.0.0:	[Feature] Reference AAComm 7.0.0
//
// (20/12/23) Alvin
// v5.13.0: [Feature] Added support for interpreting user program variable names via AdaptUserProgMessage
//
// (30/11/23) Alvin
// v5.12.3: [Bug Fix] AllStatInterpreter now supports 12 axes to compliment changes in 5.10.0
//			[Bug Fix] VectorMotion Pause Continue Fix, Added Enum for Vector Motion
//
// (27/11/23) Daniel
// v5.12.2: [Maintenance] AACommServer: Allow "retrying" socket init. upon process start
//
// (22/11/23) Alvin
// v5.12.1: [Maintenance] Added support for on-the-fly target/speed/accel change for MoveAbs()
//
// (31/10/23) Daniel
// v5.12.0: [Maintenance]:Add .net 6.0 and 7.0 targets. (nuget 5.11.0 is wrongly built and deprecated)
//
// (08/09/23) LYY
// v5.11.0: [Bug Fix]:Fix the errcode of  'FailAboutData' when connecting.
//
// (04/08/23) Alvin
// v5.10.0: [Bug Fix]: AllStatUpdate to query for all 12 axes (instead of 8) such that the auxiliary ports will be updated too.
//
// (3/08/23) Alvin
// v5.9.0: [Bug Fix]: AllStatUpdate to include AGD155_EC_ID as well.
//
// (13/04/23) Daniel
// v5.8.0: [Bug Fix]: CRITICAL! AAComm RecUpload and "continuous" messages related bug.
//
// (13/04/23) Daniel
// v5.7.0: [Bug Fix]: Add YamlDotNet nuget dependency.
//
// (13/04/23) LYY
// v5.6.10: [Maintenance]: Add vector motion methods.
//
// (12/01/23) LYY
//	v5.6.9: [Maintenance]: Added gcode analysis methods.
//
// (12/01/23) LYY
//	v5.6.8: [Maintenance]: Added methods to cigroup-axis class to do 6-axes linear motion and write doutport value related to axis to buffer.
//
// (12/01/23) LYY
//	v5.6.6: [Maintenance]: Move params and methods of event class to single axis class.
//
// (12/01/23) LYY
//	v5.6.5: [Maintenance]: Added params of gantry.
//
// (12/01/23) LYY
//	v5.6.4: [Maintenance]: Added params and methods of error map.
//
// (12/01/23) LYY
//	v5.6.3: [Maintenance]: Added params of userpwm.
//
// (12/01/23) LYY
//	v5.6.2: [Maintenance]: Added params and methods of group9(userparam).
//
// (11/01/23) LYY
//	v5.6.1: [Maintenance]: Added params and methods of position lock.
//
// (03/01/23) Alvin
//	v5.6.0: [Feature]: Added override for Home method to do homing based on sequence stored in memory.
//
// (17/08/22) CJ
//	v5.5.0: [Maintenance]: Communicator interface, AllStat Instantiation is cleaned to used feature_1 integer directly
//
// (03/08/22) CJ
//	v5.5.0: [Feature]: All the Move commands in Single axis motion is now updated to include speed, accel, decel parameters
//
// (03/08/22) CJ
//	v5.4.13: [Maintenance]: MotionStat is added to the end of each Move Command. This should eliminate the timing issue that we experienced with the Taiwan customer
//
// (03/08/22) CJ
//	v5.4.12: [Bug Fix]: Removing MotorCurrent, Fixing MotorCurr and CurrRef Statuses in ISingleAxis
//
// (04/10/21)
//	v5.4.11: [Maintenance] 	Modify all Properties related to Force/Current/Position Mode Switch feature to be based on InternalProperies.
//
// (13/09/21)
//	v5.4.10: [Maintenance] 	if packet number in a SlowLogger UpLoad is zero, jump to next UpLoad without processing 
//
// (10/09/21)
//	v5.4.9: [Maintenance] 	Slow logger API StopSlowLogData: check the ID of the packet that was last logged ,
//							Remove unwanted packet ID and contents from the main log list
//
// (09/09/21)
//	v5.4.8: [Feature] 	Slow logger API functions: StartSLowLogProcess and StopSlowLogData
//
// (07/09/21)
//	v5.4.7: [Bug Fix]	BuffTime and BuffPos command format must be array type
//
// (06/09/21)
//	v5.4.6: [Feature]	Add properties and methods for Spline buffer motion in CiSingleAxis
//
// (04/08/21)
//	v5.4.5: [Bug Fix]	HomeToLimitThenIndex, Jog to index - 4, the first parameter is speed , argu 1
//	
// (30/06/21)
//	v5.4.4: [Maintenance]	Daniel: Turn on AGMotion 
//			[Maintenance]	Open the CloseAACommServer API to the user
//
// (08/06/21)
//	v5.4.3: [Maintenance]	Daniel: Hide AGMotion for interim version release
//
// (24/05/21)
//	v5.4.3: [Maintenance]	Do some Clearance works
//
// (23/05/21)
//	v5.4.3: [Feature]		Add Force/Current/Position Mode Switch feature
//
// (20/05/21)
//	v5.4.2: [Feature]		Add new AGMotion feature
//			[Maintenance]	Eliminate all compilation Warnings
//			[Maintenance]	AGMotion as an individual namespace
//
// (12/05/21)
//	v5.4.1: [Bug Fix]	Li Yue: Default set time out as 2 seconds when send out packet
//
// (16/04/21)
//	v5.4.0: [Feature]	Li Yue: Add an option to set time out when send out packet, convenient for the user to debug for breakpoint
//
// (24/03/21)
//	v5.3.2: [Bug Fix]	Alvin: Corrected <param> tag for CNC Arc Motion
//
// (04/03/21)
//	v5.3.1: [Bug Fix]	Li Yue: Fix the bug on AIO Output Readback value incorrect
//
// (17/02/21)
//	v5.2.10: [Maintenance]	Li Yue: Move VBus,VLogic,PwrTemp,MotorTemp into IMotionController to be used for all controller class
//
// (17/02/21)
//	v5.2.9: [Maintenance]	Li Yue: Extend AGM400 Axes, IO, GroupAxis, PositionEventEx, full range from A to F
//									Extend AGM800 Axes, IO, GroupAxis, PositionEventEx, full range from A to L
//
// (29/10/20)
//	v5.2.8: [Maintenance]	Li Yue: AllStat for different controllers
//
// (29/09/20)
//	v5.2.7: [Maintenance]	Daniel: change 4.6.2 target frameworks to 4.8 (remove redundancy)
//
// (29/09/20)
//	v5.2.6: [Bug Fix]		Alvin: Updated calls to GetAllAxisStat within Group.cs
//
// (19/09/20)
//	v5.2.5: [Feature]		Daniel: and multiple target frameworks support: .net 4, 4.6, 4.6.2
//
// (18/09/20)
//	v5.1.5: [Maintenance]	Alvin: Updated definitions and bug fixes for CNC Motion
//
// (18/09/20)
//	v5.1.4: [Maintenance]	call GetAllAxisStat using a new parameter to decide if AllStatAxisData group 10 is included
//			[Maintenance]	Add DInPortHigh in AllStatAxisData group 10	
//			[Maintenance]	Add BoardTemp in AllStatAxisData group 10	 
//			[Maintenance]	Clearance in IOMoudle.cs	
//
// (17/09/20)
//	v5.1.3: [Maintenance]	Array Type parameter index start from 1 for AAMotion user
//			[Maintenance]	rename and add elements in enum ControllerType	
//			[Bug Fix]		Instantiate eEventSetting dictionary for read
//
// (16/09/20)
//	v5.1.2: [Feature]		Add BiDirConfig feature 
//	
//
// (15/09/20)
//	v5.1.1: [Maintenance]	Rename Class AG100 to AGD155
//			[Maintenance]	Rename Class MAS02 to AGM800
//			[Maintenance]	Rename Class MAS01 to AGM400
//			[Maintenance]	Rename Class AG300 to AGC300
//			[Maintenance]	Rename Class AGD301 to AGC301
//			[Feature]		Add DInLogHigh feature 
//
//
// (10/09/20)
//	v5.1.0: [Feature]		Add AGD301 class
//			[Maintenance]	Change the commands of GetAllSettingsDIOAG300, GetAllSettingsAIOAG300 to dynamic contents, initialize them in the CommandList and rebuild later dynamically. 
//			[Bug Fix]		Dynamic Command fix the AGD301 system down after connection	
//			[Feature]		Add TimerHandler in TimerEvent for internal debug from AAMotion user source code program
//			[Bug Fix]		Add eIOSetting.DInMode in UpdatePropertyListIOSettingArray
//			[Bug Fix]		NumOfAOut for output array-type keywords
//
// (9/09/20)
//	v5.0.1: [Bug Fix]		FIX nuget package
//
// (9/09/20)
//	v5.0.0: [Maintenance]	Merge from 4.21.8
//			[Maintenance]	ParseDIOSettings, ParseAIOSettings don't trigger event to write into Array-Type IO variables 
//
// (4/09/20)
//	v4.21.8: [Maintenance]	Replace PosEvent with PosEvent_A,PosEvent_B,PosEvent_C in AG300 class
//
// (4/09/20)
//	v4.21.7: [Maintenance]	FIX: Take CiStatus from  AllStat returning data structure
//
// (31/08/20)
//	v4.21.6: [Bug Fix]	FIX: make AllStat "Continuous" message type
//
// (27/08/20)
//	v4.21.5: [Bug Fix]	Increase Axis to L
//						
// 
//  (27/08/20)
//	v4.21.4: [Feature]	Add CiStatus feature ( Modified )
//						Temporarily use .PropertyList[Axis].CiSlaveSettingsArray[eCiSlaveSetting.CiStatus][1] to replace .PropertyList[Axis].Statuses.CiStatus_1
//
//  (27/08/20)
//	v4.21.3: [Feature]	Add CiStatus feature
//						Temporarily disable CiSlaveRefreshRequired since AllStat[255] cannot work set
//						increase the AxisRef to Axis K
//						define GetAllCiAxisStat to replace GetAllAxisStatExtended1
//						add CiIdentity, CiStatus into eCiSlaveSetting
//
//  (06/08/20)
//	v4.21.2: [Feature]	Optimize Array Properties Access
//						Add new command GetAllSettingDIOCiMaster, GetAllSettingAIOCiMaster
//
//  (24/07/20)
//	v4.21.1: [Feature]	Add MAS02 (AGM800) class, initial version
//
//  (30/06/20)
//	v4.20.1: [Feature]	Add AG100 class, initial version
//
//  (30/06/20)
//	v4.19.2:[Maintenance] Rename the property arrayStorage
//			[Bug Fix] Refresh takes after the communication OK with the controller
//			[Bug Fix] VBus, VLogic, PwrTemp,MotorTemp updated valid only after the communication OK with the controller
//
//  (26/06/20)
//	v4.19.1:[Maintenance] Set a flag called "AxisInternalPropertyUpdatedFromController" to true after the connection is OK, which prevent 
//					intermittently read  PropertyList after connection but InternalProperty have not been updated.
//					
//  (23/06/20)
//	v4.19.0:[Maintenance] Optimize the IO aray properties update event handler
//			[Maintenance] Property read must occur after connected, otherwise default as "unreasonable" values
//
//  (19/06/20)
//	v4.18.0: [Feature] Replace the Set methods on "Array-Like" IO Features with direct "Array" format Read/Write Properties. 
//			These are DInMode,DOutMode,DOutSelect,AInFilt,AInOffSet,AInDB,AInGain,AInMode,AOutOffset,AOutShifts,AOutMode,AOutPort
//
//  (17/06/20)
//	v4.17.0: [Feature] Add dynamic allocation array size of Digital and Analgue input/output Mode, Filt,Offset,DB,Gain
//
//  (12/06/20)
//	v4.16.0: [Maintenance] Upgraded AAComm DLL is upgraded from v3.0.0.2 to v3.0.0.18
//
//  (30/04/20)
//  v4.15.0: [Feature] AOutPort has additional option to change to openloop mode 
//			 [Maintenance] Clean up namespaces to use only one AAMotion namespace 
//
//  v4.14.0: ..... 
//
//  (10/01/19)
//	v4.13.1: [Bug Fix] Put back bulk command method which was removed in v4.12.0
//
//  (07/01/19)
//	v4.13.0: [Feature] MultiController.MoveAbs/MoveRel : Basic (Cross-controller) Multi-axis Synchronized PTP Method
//
//  (6/1/2019)
//  V4.12.1: [Maintenance] Changed [UpdateInternalStatus()] to type boolean
//
//	(30/12/2019)
//	v4.12.0: [Feature] New method [UpdateInternalStatus()] on all MotionControllers which allow user to explicitly update internal status.
//
//	(17/10/2019)
//	v4.11.0: [Feature] New Homing method, which can take in sequence file .hseq file which is created from PCSuite
//
//	(13/10/2019)
//	v4.10.2: [Bug Fix] Edited summary of SendBulkCommandString method
//
//	(10/10/2019)
//	v4.10.1: [Bug Fix] Differentiated between internal and external bulk messaging
//	v4.10.0: [Feature] Support for bulk messaging
//
//	(26/08/2019)
//  v4.9.0: [Feature] Support PositionEvents from AG300 Controller
//
//	v4.8.0: [Feature] LinearAbsolute : Generic CNC Method
//					  MultiSegmentOn : Multi-segment Option
//
//  v4.7.0: [Features]
//			Added InTargetTol, Added InTargetTime Keywords
//			Added Arc functions in Group Axis to include Additional Cycles Input Parameter
//			[Bug Fixes]
//			Arc functions in Group axis was missing 1 parameter
//
//
//
//	v4.6.0: AAComm is now taking from nuget package
//
//  v4.5.0: [Feature] AllStat: Extension to be backward compatible and for supported MotorTemp
//
//  v4.4.0: [Feature] Controller has new constructor overload to allow configurable internal update rate. 
//  
//	v4.3.0: Whenever motion method is called from either SingleAxis or GroupAxis, AllStat command is called immediately afterwards to update the statuses of the internal variables.  
//
//
//	Planned features for next release:
//  -AOutPort - OpenLoop mode need to support changing to different Axis (or maybe not, maybe we will modify infrastructure so there will be no
//  more need to support this feature in the future). 
// -QC all the documentation
// -Change Id and Iq according to the change in firmware version v1.3.0
// -WPF support to all the properties to provide data binding functionality
// -Updating status for all axes independent of what controller type is instantiated. 
// -New options to homing method to home to hardstop. 
// -Make it possible to load homing sequence file from PCSuite directly into AAMotion so that homing will be able to perform the exact steps defined by homing sequence file. 
// Update IGroup status property to link to eCncStatusParams enum in ParamCommon.cs 
// -Clean up code, clean references and namespaces (position events etc)
// -Deprecate PositionEvent
//