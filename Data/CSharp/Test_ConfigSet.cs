//该脚本为打表工具自动生成，切勿修改！
using UnityEngine;
using System;
using System.Collections.Generic;
public struct Test_ConfigSet:IDataConfig
{
	Dictionary<string, Dictionary<string,int>> configDic;
	List<string[]> data;
	TestDataConfig cache;
	Dictionary<string, int> dicCache;
	Dictionary<string, List<int>> dicListCache;
	List<int> indexListCache;
	List<TestDataConfig> configLineListCache;
	public TestDataConfig GetConfigByKey(string keyName, string value)
	{
		if (configDic.TryGetValue(keyName, out dicCache))
		{
			int index;
			dicCache.TryGetValue(value, out index);
			return DeserializeByIndex(index);
		}
		else
		{
			return new TestDataConfig();
		}
	}
	public List<IDataConfigLine> GetConfigsByKey(string keyName, string value)
	{
		List<IDataConfigLine> configLineList; 
		configLineList = new List<IDataConfigLine>(1);
		configLineList.Add(GetConfigByKey(keyName, value));
		return configLineList;
	}
	private TestDataConfig DeserializeByIndex(int i)
	{
		cache.id = data[i][0].ParseUInt32();
		cache.testString = data[i][1];
		for (int temp2 = 3; temp2 < 3 + 2; temp2++)
		{
			temp1[temp2 - 3] = data[i][temp2].ParseUInt32();
		}
		cache.testArray1 = temp1;
		cache.testArray2 = Array.ConvertAll(data[i][5].Split(';'), s => s.ParseUInt32());
		cache.testStruct1 = new ConfigDefine.TestStruct
		{
			aa=data[i][7].ParseUInt32(),
			bb=data[i][8],
		};
		for (int temp4 = 11; temp4 < 11 + 2 * 2; temp4 += 2)
		{
			temp3[(temp4 - 11) / 2] = new ConfigDefine.TestStruct{
				aa=data[i][temp4+0].ParseUInt32(),
				bb=data[i][temp4+1],
			};
		}
		cache.testArrayStruct1 = temp3;
		cache.testStructLoop = new ConfigDefine.TestStructLoop
		{
			aa=data[i][16].ParseUInt32(),
			testStruct1= new ConfigDefine.TestStruct{
				aa=data[i][18].ParseUInt32(),
				bb=data[i][19],
			},
			testStruct2= new ConfigDefine.TestStruct{
				aa=data[i][21].ParseUInt32(),
				bb=data[i][22],
			},
		};
		for (int temp6 = 25; temp6 < 25 + 2 * 7; temp6 += 7)
		{
			temp5[(temp6 - 25) / 7] = new ConfigDefine.TestStructLoop{
				aa=data[i][temp6+0].ParseUInt32(),
				testStruct1= new ConfigDefine.TestStruct{
					aa=data[i][temp6+2].ParseUInt32(),
					bb=data[i][temp6+3],
				};
				testStruct2= new ConfigDefine.TestStruct{
					aa=data[i][temp6+5].ParseUInt32(),
					bb=data[i][temp6+6],
				};
			};
		}
		cache.testStructLoopArray = temp5;
		return cache;
	}
	UInt32[]temp1;
	ConfigDefine.TestStruct[]temp3;
	ConfigDefine.TestStructLoop[]temp5;
	public void Init(List<string[]> data)
	{
		this.data = data;
		cache = new TestDataConfig();
		configDic = new Dictionary<string, Dictionary<string, int>>();
		Dictionary<string,int> idDic = new Dictionary<string,int>();
		temp1 = new UInt32[2];
		temp3 = new ConfigDefine.TestStruct[2];
		temp5 = new ConfigDefine.TestStructLoop[2];
		for (int i = 0; i < data.Count; i++)
		{
			idDic.Add(data[i][0], i);
		}
		configDic.Add("id",idDic);
	}
	public List<TestDataConfig> GetAllConfig()
	{
		List<TestDataConfig> temp = new List<TestDataConfig>(data.Count);
		for (int i = 0; i < data.Count; i++)
		{
			temp.Add(DeserializeByIndex(i));
		}
		return temp;
	}
}
