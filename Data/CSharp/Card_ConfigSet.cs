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
		cache.path = data[i][1];
		cache.timeIInterval = data[i][2].ParseInt32();
		for (int temp2 = 5; temp2 < 5 + 3 * 2; temp2 += 2)
		{
			temp1[(temp2 - 5) / 2] = new ConfigDefine.Currency
			{
				currencyType=(ConfigDefine.CurrencyType)data[i][temp2+0].ParseUInt32(),
				value=data[i][temp2+1].ParseInt32(),
			};
		}
		cache.currency = temp1;
		return cache;
	}
	ConfigDefine.Currency[]temp1;
	public void Init(List<string[]> data)
	{
		this.data = data;
		cache = new CardDataConfig();
		configDic = new Dictionary<string, Dictionary<string, int>>();
		Dictionary<string,int> idDic = new Dictionary<string,int>();
		temp1 = new ConfigDefine.Currency[3];
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
