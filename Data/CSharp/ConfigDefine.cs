//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
namespace ConfigDefine
{
	/// <summary>
	/// 测试枚举
	/// </summary>
	public enum EnumTest1
	{
	/// <summary>
	/// 测试0
	/// </summary>
		 Test0 = 0,
	/// <summary>
	/// 测试1
	/// </summary>
		 Test1 = 1,
	/// <summary>
	/// 测试2
	/// </summary>
		 Test2 = 2,
	}
	/// <summary>
	/// 收益类型
	/// </summary>
	public enum CurrencyType
	{
	/// <summary>
	/// 影响力
	/// </summary>
		 Soul = 0,
	/// <summary>
	/// 精神力
	/// </summary>
		 San = 1,
	/// <summary>
	/// 钱
	/// </summary>
		 Money = 2,
	}
	/// <summary>
	/// 结算类型
	/// </summary>
	public enum SetAccountType
	{
	/// <summary>
	/// 时间结算
	/// </summary>
		 Time = 0,
	/// <summary>
	/// 添加时结算
	/// </summary>
		 Add = 1,
	/// <summary>
	/// 移除时结算
	/// </summary>
		 Remove = 2,
	}
	/// <summary>
	/// 镜中人属性
	/// </summary>
	public enum MirrorActorAttr
	{
	/// <summary>
	/// 精神力
	/// </summary>
		 Mind = 0,
	/// <summary>
	/// 思潮值
	/// </summary>
		 Ideology = 1,
	/// <summary>
	/// 经济水平
	/// </summary>
		 Economy = 2,
	/// <summary>
	/// 力量
	/// </summary>
		 Strength = 3,
	/// <summary>
	/// 智力
	/// </summary>
		 Intelligence = 4,
	}
	/// <summary>
	/// 思潮
	/// </summary>
	public enum IdeologicalTrend
	{
	/// <summary>
	/// 无
	/// </summary>
		 None = 0,
	/// <summary>
	/// 未开化
	/// </summary>
		 Uncivilized = 1,
	/// <summary>
	/// 左上
	/// </summary>
		 LeftUp = 2,
	/// <summary>
	/// 左下
	/// </summary>
		 LeftDown = 3,
	/// <summary>
	/// 右上
	/// </summary>
		 RightUp = 4,
	/// <summary>
	/// 右下
	/// </summary>
		 RightDown = 5,
	}
	/// <summary>
	/// 测试结构体
	/// </summary>
	public struct TestStruct
	{
	/// <summary>
	/// 注释
	/// </summary>
		public UInt32 aa;
	/// <summary>
	/// 注释
	/// </summary>
		public string bb;
	}
	/// <summary>
	/// 测试结构体嵌套
	/// </summary>
	public struct TestStructLoop
	{
	/// <summary>
	/// 注释
	/// </summary>
		public UInt32 aa;
	/// <summary>
	/// 测试结构体
	/// </summary>
		public TestStruct TestStruct1;
	/// <summary>
	/// 测试结构体
	/// </summary>
		public TestStruct TestStruct2;
	}
	/// <summary>
	/// 收益
	/// </summary>
	public struct Currency
	{
	/// <summary>
	/// 收益类型
	/// </summary>
		public CurrencyType currencyType;
	/// <summary>
	/// 内容
	/// </summary>
		public int value;
	}
	/// <summary>
	/// 结算
	/// </summary>
	public struct SetAccount
	{
	/// <summary>
	/// 结算类型
	/// </summary>
		public SetAccountType setAccountType;
		public int value;
	}
}