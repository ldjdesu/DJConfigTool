//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
public struct AvatarDataConfig:IDataConfigLine
{
	/// <summary>
	/// 索引ID
	/// </summary>
	public UInt32 id;
	/// <summary>
	/// 角色名
	/// </summary>
	public string name;
	/// <summary>
	/// 内容
	/// </summary>
	public string desc;
	/// <summary>
	/// 剧本id
	/// </summary>
	public UInt32 storyId;
	public bool IsNull ()
	{
		return id == 0;
}
}