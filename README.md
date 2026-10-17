# DJConfigTool
配置工具子模块

## 索引与查询

表格第四行按列标记索引；没有标记的列不建索引。

| 标记 | Key 规则 | 生成的查询 |
| --- | --- | --- |
| `Index_Unique` | 唯一，重复时初始化失败 | `GetConfigByKey` |
| `Index_NoUnique` | 可重复，包括空字符串 | `GetConfigsByKey` |
| 无索引 | 不建索引 | 只保留 `GetAllConfig` |

只有表中存在对应索引种类，才生成对应查询方法。两种索引共存时，单条查询也可取多值索引的首个命中项，批量查询也可查询唯一索引。返回顺序与表格行顺序一致。不存在的列、值或 null 查询参数，单条返回默认结构体，批量返回空列表。

配置在 `Init` 后按只读数据使用。首次访问一行才解析它及其数组，后续查询复用结果；ConfigSet 的结构体副本共享同一份缓存。重新 `Init` 会替换当前容器的数据、索引和缓存，旧副本仍使用原数据。查询返回的列表可独立修改，行内数组必须当作只读数据；业务需要修改时先复制。容器不支持并发写入或并发首次解析。

## 数组

普通数组只支持跨列定长格式，例如类型行 `Array_2,UInt32,UInt32`，字段行 `,values,values`，数据行 `,10,20`。`Array_Struct` 结构体数组继续支持。已移除 `Array_UInt32` 等分号分隔格式，旧标记会提示改用 `Array_N` 并返回失败退出码。

## 生成与回归

配置行带 `Serializable` 标记，可作为版本化业务快照的值记录。四行表头且无数据的合法空表输出一个空行，生产 CSV 解析器读取为零条记录；不写入伪造的 ID=0 数据。唯一键重复时，生成的容器报告表名、数据行号、字段和键值。

`ConfigTool.exe <配置根目录>` 读取 `ConfigDefine.xml` 和 `Data/CsvOld`，输出到 `Data/CSharp`、`Data/CsvNew` 和 `Data/MD5`。不传目录或传空字符串时使用当前目录。失败返回非零退出码，批处理应停止同步。增量指纹包含生成器程序集版本、XML 定义和 CSV 内容，模板更新或输出丢失会重新生成。

实际批处理使用子模块根目录的 `ConfigTool.exe` / `ConfigTool.dll`，更新源码后须将经过验证的构建输出同步到这里。编译建议使用临时副本，避免更改仓库历史跟踪的 `bin` / `obj` 文件。

独立回归会调用真实生成器，编译四种索引组合并执行查询、唯一性和缓存检查：

```powershell
python Tests/run_regressions.py --tool <构建输出/ConfigTool.dll> --framework-config <DJGameFrameWork/FrameWork/Config> --output <临时目录>
```

加 `--unity-fixtures <项目/Assets/Scripts/Test/Editor/ConfigToolGeneratedFixtures.cs>` 可重新生成 Unity 回归夹具。`Tests/Fixtures/Mixed.csv` 是基础输入，其他组合由测试脚本变换索引标记得到。
