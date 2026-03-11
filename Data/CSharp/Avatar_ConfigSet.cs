//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
using System.Collections.Generic;
public struct Avatar_ConfigSet:IDataConfig
{
	Dictionary<string, Dictionary<string,int>> configDic;
	List<string[]> data;
	AvatarDataConfig cache;
	Dictionary<string, int> dicCache;
	Dictionary<string, List<int>> dicListCache;
	List<int> indexListCache;
	List<AvatarDataConfig> configLineListCache;
	public AvatarDataConfig GetConfigByKey(string keyName, string value)
	{
		if (configDic.TryGetValue(keyName, out dicCache))
		{
			int index;
			dicCache.TryGetValue(value, out index);
			return DeserializeByIndex(index);
		}
		else
		{
			return new AvatarDataConfig();
		}
	}
	public List<IDataConfigLine> GetConfigsByKey(string keyName, string value)
	{
		List<IDataConfigLine> configLineList; 
		configLineList = new List<IDataConfigLine>(1);
		configLineList.Add(GetConfigByKey(keyName, value));
		return configLineList;
	}
	private AvatarDataConfig DeserializeByIndex(int i)
	{
		cache.id = data[i][0].ParseUInt32();
		cache.name = data[i][1];
		cache.desc = data[i][2];
		cache.storyId = data[i][3].ParseUInt32();
		return cache;
	}
	public void Init(List<string[]> data)
	{
		this.data = data;
		cache = new AvatarDataConfig();
		configDic = new Dictionary<string, Dictionary<string, int>>();
		Dictionary<string,int> idDic = new Dictionary<string,int>();
		for (int i = 0; i < data.Count; i++)
		{
			idDic.Add(data[i][0], i);
		}
		configDic.Add("id",idDic);
	}
	public List<AvatarDataConfig> GetAllConfig()
	{
		List<AvatarDataConfig> temp = new List<AvatarDataConfig>(data.Count);
		for (int i = 0; i < data.Count; i++)
		{
			temp.Add(DeserializeByIndex(i));
		}
		return temp;
	}
}
