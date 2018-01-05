# Tips of using CritterAI & MeshToTerrain Plugin 

��ǩ���ո�ָ����� Unity CritterAI MeshToTerrain

---

## Instructions

+ CritterAI Plugin
    + ����
    > CritterAI Plugin��[unity3d_nav_critterai](https://github.com/kbengine/unity3d_nav_critterai)
    + ����
    > ȫѡ ("CritterAI" && "Plugin" �ļ���), ������ Unity ��Ŀ��Ӧ�� "Assets" ��
    
    + ��������
    > ���������������Unity �������н����� "CritterAI" ѡ��
    
    + **���ɻ��������ļ�**��CritterAI -> Create NMGen Assets -> Navmesh Build : Standard 
         > - CAIBakedNavmesh.asset
         > - MeshCompiler.asset
         > - NavmeshBuild.asset
        
    
    + **���� TerrainCompiler**: CritterAI -> Create NMGen Assets -> Compiler : Terrain
            > TerrainCompiler.asset 
        
    + **�����ΰ��� TerrainCompiler ��**�� ѡ�� Terrain ��Ӧ�� TerrainData��������ק�� TerrainCompiler �� TerrainData ��
        
    + **�� TerrainComiler �� NavmeshBuild �����**���� TerrainCompiler ��ק�� NavmeshBuild -> Processors -> Add. (NOTE: ͬһ NavmeshBuild �п���Ӷ����ͬ�� TerrainCompiler)
        
    + **����**������ʵ�ʵ��Σ��� NavmeshBuild -> Primary, NavmeshBuild -> Advanced �µ���������ֵ
        
    + **����**��
        > - NavmeshBuild -> Derive
        > - NavmeshBuild -> Build & Bake
    
    + **����**��
        > CAIBakedNavmesh -> Save
        > ��������������ļ������С�srv_����ͷ���ļ����ڷ����Ѱ·����һ��������ڿͻ���ʹ�øò����Ѱ·

+ MeshToTerrain
    + ����
        + [Taobao](https://item.taobao.com/item.htm?spm=a1z09.2.0.0.5157b556zhUSFM&id=521353941565&_u=41vgfmvn9697)
        + [Asset Store](https://www.assetstore.unity3d.com/cn/#!/content/7271)
    + ��������

## Issues
+ ʹ�� MeshToTerrain ʱ�ı߽����⣺
    > - �����������������뽨�� Mesh ת��Ϊͬһ Terrain ʱ��������ģ���ڴ�ֱ����Ŵ��ת����� Terrain ����������ֵı߽�(���۶� Bounds ���� Auto Detect ���� From GameObject ���þ�����)
    > - ���������
        + �������뽨���ֱ�ת��Ϊ������ Terrain: Terrain_Ground, Terrain_Buildings
        + �½����� TerrainCompiler, �ֱ��� Terrain_Ground �� Terrain_Buildings ���а�
        + �������� Terrain Data ��� TerrainCompiler ����� NavmeshBuild ��
        + �� NavmeshBuild �н��к決�����õ� Terrain_Ground �� Terrain_Buildings ���ݵ��ι�ͬ���õ� Navmesh

+ ʹ�� CritterAI ʱ�ĺ決���⣺
    - �決������Ͼ�ȷ�Ƚϲ�������к決�� Terrain ��Ӧ�� TerrainCompiler �У��� "Resolution" ����ֵ������ 100
    - �ϸߴ���ƽ�沿�ֱ��決Ϊ Walkable ������ "NavmeshBuild" �У����� "Min Island Area" ��ֵ

    


