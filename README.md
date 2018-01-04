# Tips of using CritterAI & MeshToTerrain Plugin 

标签（空格分隔）： Unity CritterAI MeshToTerrain

---

##Instructions

+ CritterAI Plugin
    + 下载
    > CritterAI Plugin：[unity3d_nav_critterai](https://github.com/kbengine/unity3d_nav_critterai)
    + 导入
    > 全选 ("CritterAI" && "Plugin" 文件夹), 拷贝至 Unity 项目对应的 "Assets" 中
    
    + 操作流程
    > 若插件导入正常，Unity 工具栏中将新增 "CritterAI" 选项
    
    + **生成基本配置文件**：CritterAI -> Create NMGen Assets -> Navmesh Build : Standard 
         > - CAIBakedNavmesh.asset
         > - MeshCompiler.asset
         > - NavmeshBuild.asset
        
    
    + **生成 TerrainCompiler**: CritterAI -> Create NMGen Assets -> Compiler : Terrain
            > TerrainCompiler.asset 
        
    + **将地形绑定至 TerrainCompiler 上**： 选择 Terrain 对应的 TerrainData，将其拖拽至 TerrainCompiler 的 TerrainData 上
        
    + **将 TerrainComiler 与 NavmeshBuild 相关联**：将 TerrainCompiler 拖拽至 NavmeshBuild -> Processors -> Add. (NOTE: 同一 NavmeshBuild 中可添加多个不同的 TerrainCompiler)
        
    + **调参**：根据实际地形，在 NavmeshBuild -> Primary, NavmeshBuild -> Advanced 下调整参数数值
        
    + **烘培**：
        > - NavmeshBuild -> Derive
        > - NavmeshBuild -> Build & Bake
    
    + **导出**：
        > CAIBakedNavmesh -> Save
        > 保存后将生成两个文件，其中“srv_”开头的文件用于服务端寻路，另一个则可用于客户端使用该插件来寻路

+ MeshToTerrain
    + 购买：
        + [Taobao](https://item.taobao.com/item.htm?spm=a1z09.2.0.0.5157b556zhUSFM&id=521353941565&_u=41vgfmvn9697)
        + [Asset Store](https://www.assetstore.unity3d.com/cn/#!/content/7271)
    + 其它：略

##Issues##
+ 使用 MeshToTerrain 时的边界问题：
    > - 问题描述：将地面与建筑 Mesh 转化为同一 Terrain 时，将建筑模型在垂直方向放大后，转化后的 Terrain 出现两条奇怪的边界(无论对 Bounds 进行 Auto Detect 还是 From GameObject 设置均出现)
    > - 解决方法：
        + 将地面与建筑分别转化为单独的 Terrain: Terrain_Ground, Terrain_Buildings
        + 新建两个 TerrainCompiler, 分别与 Terrain_Ground 和 Terrain_Buildings 进行绑定
        + 将两个绑定 Terrain Data 后的 TerrainCompiler 添加至 NavmeshBuild 中
        + 在 NavmeshBuild 中进行烘焙，将得到 Terrain_Ground 和 Terrain_Buildings 两份地形共同作用的 Navmesh

+ 使用 CritterAI 时的烘焙问题：
    - 烘焙后的贴合精确度较差：在欲进行烘焙的 Terrain 对应的 TerrainCompiler 中，将 "Resolution" 属性值增大至 100
    - 较高处的平面部分被烘焙为 Walkable 区域：在 "NavmeshBuild" 中，增大 "Min Island Area" 的值

    


