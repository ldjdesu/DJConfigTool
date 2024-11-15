//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
public struct TestDataConfig:IDataConfigLine
{
	/// <summary>
	/// 索引ID
	/// </summary>
	public UInt32 id;
	/// <summary>
	/// 字符串
	/// </summary>
	public string testString;
	/// <summary>
	/// 数组模式1
	/// </summary>
	public UInt32[] testArray1;
	/// <summary>
	/// 数组模式2
	/// </summary>
	public UInt32[] testArray2;
	/// <summary>
	/// 结构体
	/// </summary>
	public ConfigDefine.TestStruct testStruct1;
	/// <summary>
	/// 结构体数组模式
	/// </summary>
	public ConfigDefine.TestStruct[]  testArrayStruct1;
	/// <summary>
	/// 结构体嵌套
	/// </summary>
	public ConfigDefine.TestStructLoop testStructLoop;
	/// <summary>
	/// 数组结构体嵌套
	/// </summary>
	public ConfigDefine.TestStructLoop[]  testStructLoopArray;
}