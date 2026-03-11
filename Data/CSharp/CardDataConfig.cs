//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
public struct CardDataConfig:IDataConfigLine
{
	/// <summary>
	/// 索引ID
	/// </summary>
	public UInt32 id;
	/// <summary>
	/// 名称
	/// </summary>
	public string name;
	/// <summary>
	/// 等级
	/// </summary>
	public int level;
	/// <summary>
	/// 能否叠加
	/// </summary>
	public int isStacking;
	public ConfigDefine.CardClass cardClass;
	public ConfigDefine.CardCamp cardCamp;
	/// <summary>
	/// 描述
	/// </summary>
	public string des;
	/// <summary>
	/// 技能描述
	/// </summary>
	public string skillDes;
	/// <summary>
	/// 信仰值
	/// </summary>
	public int faith;
	/// <summary>
	/// 费用（65535为耗尽所有费用）
	/// </summary>
	public int cost;
	/// <summary>
	/// 攻击
	/// </summary>
	public int atk;
	/// <summary>
	/// 血量
	/// </summary>
	public int hp;
	/// <summary>
	/// 防御
	/// </summary>
	public int def;
	/// <summary>
	/// 速度
	/// </summary>
	public int spd;
	/// <summary>
	/// 攻击距离
	/// </summary>
	public int range;
	/// <summary>
	/// 卡牌标签
	/// </summary>
	public UInt32[] lableId;
	/// <summary>
	/// 【弃用】资源名（CardSmall   CardMid  CardLarge）
	/// </summary>
	public string path;
	/// <summary>
	/// 原始ID(如果是变体)
	/// </summary>
	public UInt32 originId;
	public bool IsNull ()
	{
		return id == 0;
}
}