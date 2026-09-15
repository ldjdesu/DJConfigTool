using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine { }

/// <summary>对真实生成的四种索引表执行独立运行时回归。</summary>
public static class RuntimeChecks
{
    /// <summary>验证条件，不依赖额外测试包。</summary>
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        Console.WriteLine("PASS " + name);
    }

    /// <summary>创建行号不连续的重复索引数据。</summary>
    private static List<string[]> Rows()
    {
        return new List<string[]>
        {
            new[] { "10", "other", "", "1", "2" },
            new[] { "20", "target", "", "3", "4" },
            new[] { "30", "other", "", "5", "6" },
            new[] { "40", "target", "", "7", "8" }
        };
    }

    /// <summary>验证 API 条件生成、索引语义、缓存共享和重新初始化。</summary>
    public static int Main()
    {
        var rows = Rows();
        var mixed = new Mixed_ConfigSet(); mixed.Init(rows);
        Check(mixed.GetConfigsByKey("group", "target").Select(x => x.id).SequenceEqual(new uint[] { 20, 40 }), "noncontiguous rows in source order");
        Check(mixed.GetConfigByKey("group", "target").id == 20, "mixed single query returns first matching row");
        Check(mixed.GetConfigsByKey("id", "40").Single().id == 40, "mixed batch query accepts unique index");
        Check(mixed.GetConfigByKey("id", "missing").IsNull(), "unique missing is default instead of row zero");
        Check(mixed.GetConfigsByKey("group", "missing").Count == 0, "multi missing is empty");
        Check(mixed.GetConfigsByKey("unknown", "target").Count == 0 && mixed.GetConfigByKey("unknown", "target").IsNull(), "unknown field is empty");
        Check(mixed.GetConfigsByKey(null, "target").Count == 0 && mixed.GetConfigByKey("id", null).IsNull(), "null arguments are empty");
        Check(mixed.GetConfigsByKey("id", "missing").Count == 0 && mixed.GetConfigByKey("group", "missing").IsNull(), "mixed fallback misses are empty");
        var values = mixed.GetConfigByKey("id", "40").values;
        Check(values.SequenceEqual(new uint[] { 7, 8 }), "fixed array values");
        IDataConfig boxed = mixed;
        var copy = (Mixed_ConfigSet)boxed;
        Check(ReferenceEquals(values, copy.GetConfigByKey("id", "40").values), "cache survives struct copies and boxing");
        var untouchedCopy = copy;
        var freshValues = mixed.GetConfigByKey("id", "30").values;
        Check(ReferenceEquals(freshValues, untouchedCopy.GetConfigByKey("id", "30").values), "copy made before first query shares lazy result");
        var list = mixed.GetConfigsByKey("group", "target"); list.Clear();
        Check(mixed.GetConfigsByKey("group", "target").Count == 2, "returned lists are independent");
        var all = mixed.GetAllConfig(); all.Clear();
        Check(mixed.GetAllConfig().Select(x => x.id).SequenceEqual(new uint[] { 10, 20, 30, 40 }), "GetAll returns independent ordered list");
        var changed = Rows(); changed[3][3] = "99";
        mixed.Init(changed);
        Check(mixed.GetConfigByKey("id", "40").values[0] == 99 && copy.GetConfigByKey("id", "40").values[0] == 7, "Init resets only the reinitialized container");

        var unique = new Unique_ConfigSet(); unique.Init(rows);
        Check(typeof(Unique_ConfigSet).GetMethod("GetConfigsByKey") == null && typeof(Unique_ConfigSet).GetMethod("GetConfigByKey") != null, "unique-only API");
        Check(unique.GetConfigByKey("id", "missing").IsNull(), "unique-only missing");
        var duplicate = Rows(); duplicate[1][0] = "10";
        bool rejected = false;
        try { unique.Init(duplicate); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "unique duplicate rejected");

        var multi = new Multi_ConfigSet(); multi.Init(rows);
        Check(typeof(Multi_ConfigSet).GetMethod("GetConfigByKey") == null && typeof(Multi_ConfigSet).GetMethod("GetConfigsByKey") != null, "multi-only API");
        Check(multi.GetConfigsByKey("group", "target").Select(x => x.id).SequenceEqual(new uint[] { 20, 40 }), "multi-only duplicate rows");
        var emptyKeys = Rows(); emptyKeys[1][1] = ""; emptyKeys[3][1] = "";
        multi.Init(emptyKeys);
        Check(multi.GetConfigsByKey("group", "").Count == 2, "multi index accepts repeated empty string key");
        multi.Init(new List<string[]>());
        Check(multi.GetConfigsByKey("group", "target").Count == 0 && multi.GetAllConfig().Count == 0, "empty table");

        var unindexed = new Unindexed_ConfigSet(); unindexed.Init(rows);
        Check(typeof(Unindexed_ConfigSet).GetMethod("GetConfigByKey") == null && typeof(Unindexed_ConfigSet).GetMethod("GetConfigsByKey") == null, "unindexed API");
        Check(unindexed.GetAllConfig().Count == 4, "unindexed GetAll");
        return 0;
    }
}
