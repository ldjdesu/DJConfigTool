//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
namespace ConfigDefine
{
	public enum EnumTest1
	{
		 Test0 = 0,
		 Test1 = 1,
		 Test2 = 2,
	}
	public enum CurrencyType
	{
		 Soul = 0,
		 San = 1,
		 Money = 2,
	}
	public enum SetAccountType
	{
		 Time = 0,
		 Add = 1,
		 Remove = 2,
	}
	public enum MirrorActorAttr
	{
		 Mind = 0,
		 Ideology = 1,
		 Economy = 2,
		 Strength = 3,
		 Intelligence = 4,
	}
	public struct TestStruct
	{
		public UInt32 aa;
		public string bb;
	}
	public struct Currency
	{
		public CurrencyType currencyType;
		public int value;
	}
	public struct SetAccount
	{
		public SetAccountType setAccountType;
		public int value;
	}
}