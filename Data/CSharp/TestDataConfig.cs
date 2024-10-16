//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
public struct TestDataConfig:IDataConfigLine
{
	public UInt32 id;
	public KeyCode key;
	public string testString;
	public UInt32[] testArray1;
	public UInt32[] testArray2;
	public ConfigDefine.testStruct testStruct1;
	public ConfigDefine.testStruct[]  testArrayStruct1;
	public UInt32 cc;
	public String dd;
	public ConfigDefine.EnumTest2 enumTest2;
	public ConfigDefine.EnumTest1[] enumArrayTest1;
}