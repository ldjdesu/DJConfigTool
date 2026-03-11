//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
using System.Collections.Generic;
public struct Card_ConfigSet:IDataConfig
{
	Dictionary<string, Dictionary<string,int>> configDic;
	List<string[]> data;
	CardDataConfig cache;
	Dictionary<string, int> dicCache;
	Dictionary<string, List<int>> dicListCache;
	List<int> indexListCache;
	List<CardDataConfig> configLineListCache;
	public CardDataConfig GetConfigByKey(string keyName, string value)
	{
		if (configDic.TryGetValue(keyName, out dicCache))
		{
			int index;
			dicCache.TryGetValue(value, out index);
			return DeserializeByIndex(index);
		}
		else
		{
			return new CardDataConfig();
		}
	}
	public List<IDataConfigLine> GetConfigsByKey(string keyName, string value)
	{
		List<IDataConfigLine> configLineList; 
		configLineList = new List<IDataConfigLine>(1);
		configLineList.Add(GetConfigByKey(keyName, value));
		return configLineList;
	}
	private CardDataConfig DeserializeByIndex(int i)
	{
		cache.id = data[i][0].ParseUInt32();
		cache.name = data[i][1];
		cache.level = data[i][2].ParseInt32();
		cache.isStacking = data[i][3].ParseInt32();
		cache.cardClass = (ConfigDefine.CardClass)data[i][4].ParseUInt32();
		cache.cardCamp = (ConfigDefine.CardCamp)data[i][5].ParseUInt32();
		cache.des = data[i][6];
		cache.skillDes = data[i][7];
		cache.faith = data[i][8].ParseInt32();
		cache.cost = data[i][9].ParseInt32();
		cache.atk = data[i][10].ParseInt32();
		cache.hp = data[i][11].ParseInt32();
		cache.def = data[i][12].ParseInt32();
		cache.spd = data[i][13].ParseInt32();
		cache.range = data[i][14].ParseInt32();
		for (int temp2 = 16; temp2 < 16 + 5; temp2++)
		{
			temp1[temp2 - 16] = data[i][temp2].ParseUInt32();
		}
		cache.lableId = temp1;
		cache.path = data[i][21];
		cache.originId = data[i][22].ParseUInt32();
		return cache;
	}
	UInt32[]temp1;
	public void Init(List<string[]> data)
	{
		this.data = data;
		cache = new CardDataConfig();
		configDic = new Dictionary<string, Dictionary<string, int>>();
		Dictionary<string,int> idDic = new Dictionary<string,int>();
		temp1 = new UInt32[5];
		for (int i = 0; i < data.Count; i++)
		{
			idDic.Add(data[i][0], i);
		}
		configDic.Add("id",idDic);
	}
	public List<CardDataConfig> GetAllConfig()
	{
		List<CardDataConfig> temp = new List<CardDataConfig>(data.Count);
		for (int i = 0; i < data.Count; i++)
		{
			temp.Add(DeserializeByIndex(i));
		}
		return temp;
	}
}
