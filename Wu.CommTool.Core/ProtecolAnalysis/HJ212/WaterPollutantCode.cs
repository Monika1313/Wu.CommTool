namespace Wu.CommTool.Core.ProtecolAnalysis.HJ212;

/// <summary>
/// HJ525-2009 水污染物名称代码
/// </summary>
public class WaterPollutantCode
{
    /// <summary>
    /// 污染物信息类
    /// </summary>
    public class PollutantInfo
    {
        public string Code { get; set; }       // 污染物编码 (如w01001)
        public string Name { get; set; }       // 污染物名称 (如pH值)
        public string EnglishName { get; set; }   //英文名

        //public string Unit { get; set; }       // 主要单位 (如毫克/升)
        //public string AltUnit { get; set; }    // 备用单位 (如千克)
        //public string DataFormat { get; set; } // 数据格式 (如N3.1)
    }

    /// <summary>
    /// 污染物信息字典
    /// 未包含所有的
    /// </summary>
    private static readonly Lazy<Dictionary<string, PollutantInfo>> _lazyPollutantMap = new(() =>
    {
        return new Dictionary<string, PollutantInfo>
        {
            // 物理和综合指标
            { "w01000", new PollutantInfo { Code = "w01000", Name = "物理和综合指标", EnglishName = "Physical and Integrated Indicators" }},
            { "w01001", new PollutantInfo { Code = "w01001", Name = "pH值", EnglishName = "pH" }},
            { "w01002", new PollutantInfo { Code = "w01002", Name = "色度", EnglishName = "Color" }},
            { "w01003", new PollutantInfo { Code = "w01003", Name = "浑浊度", EnglishName = "Turbidity" }},
            { "w01004", new PollutantInfo { Code = "w01004", Name = "透明度", EnglishName = "Transparency Clarity" }},
            { "w01005", new PollutantInfo { Code = "w01005", Name = "嗅和味", EnglishName = "Odor and Sapor" }},
            { "w01006", new PollutantInfo { Code = "w01006", Name = "溶解性总固体", EnglishName = "Total Dissolved Solids" }},
            { "w01007", new PollutantInfo { Code = "w01007", Name = "总硬度", EnglishName = "Hardness (total)" }},
            { "w01008", new PollutantInfo { Code = "w01008", Name = "全盐量", EnglishName = "Total Salt" }},
            { "w01009", new PollutantInfo { Code = "w01009", Name = "溶解氧", EnglishName = "Dissolved Oxygen" }},
            { "w01010", new PollutantInfo { Code = "w01010", Name = "水温", EnglishName = "Temperature" }},
            { "w01011", new PollutantInfo { Code = "w01011", Name = "肉眼可见物", EnglishName = "Megascopic Matters" }},
            { "w01012", new PollutantInfo { Code = "w01012", Name = "悬浮物", EnglishName = "Suspended Solids" }},
            { "w01013", new PollutantInfo { Code = "w01013", Name = "漂浮物", EnglishName = "Floating Matters" }},
            { "w01014", new PollutantInfo { Code = "w01014", Name = "电导率", EnglishName = "Conductivity" }},
            { "w01015", new PollutantInfo { Code = "w01015", Name = "急性毒性", EnglishName = "Acute Toxicity" }},
            { "w01016", new PollutantInfo { Code = "w01016", Name = "叶绿素", EnglishName = "Chlorophyll" }},
            { "w01017", new PollutantInfo { Code = "w01017", Name = "五日生化需氧量", EnglishName = "Biochemical Oxygen Demand after 5 Days (BOD5)" }},
            { "w01018", new PollutantInfo { Code = "w01018", Name = "化学需氧量", EnglishName = "Chemical Oxygen Demand (COD)" }},
            { "w01019", new PollutantInfo { Code = "w01019", Name = "高锰酸盐指数", EnglishName = "Permanganate Index" }},
            { "w01020", new PollutantInfo { Code = "w01020", Name = "总有机碳", EnglishName = "Total Organic Carbon (TOC)" }},
            { "w01021", new PollutantInfo { Code = "w01021", Name = "可同化有机碳", EnglishName = "Assimilable Organic Carbon (AOC)" }},
            
            // 生物指标
            { "w02000", new PollutantInfo { Code = "w02000", Name = "生物指标", EnglishName = "Biological Indicators" }},
            { "w02001", new PollutantInfo { Code = "w02001", Name = "蛔虫卵", EnglishName = "Ascaris Eggs" }},
            { "w02002", new PollutantInfo { Code = "w02002", Name = "蠕虫卵", EnglishName = "Helminth Eggs" }},
            { "w02003", new PollutantInfo { Code = "w02003", Name = "粪大肠菌群", EnglishName = "Fecal Coliform" }},
            { "w02004", new PollutantInfo { Code = "w02004", Name = "总大肠菌群", EnglishName = "Total Coliform" }},
            { "w02005", new PollutantInfo { Code = "w02005", Name = "耐热大肠菌群", EnglishName = "Heat-resistant Coliform" }},
            { "w02006", new PollutantInfo { Code = "w02006", Name = "细菌总数", EnglishName = "Heterotrophic Plate Count" }},
            { "w02007", new PollutantInfo { Code = "w02007", Name = "大肠埃希氏菌", EnglishName = "E. Coliform" }},
            { "w02008", new PollutantInfo { Code = "w02008", Name = "肠球菌", EnglishName = "Enterococcus" }},
            { "w02009", new PollutantInfo { Code = "w02009", Name = "产气荚膜梭状芽胞杆菌", EnglishName = "Clostridium Perfringens" }},
            { "w02010", new PollutantInfo { Code = "w02010", Name = "沙门氏菌", EnglishName = "Salmonella" }},
            { "w02011", new PollutantInfo { Code = "w02011", Name = "志贺氏菌", EnglishName = "Shigella" }},
            { "w02012", new PollutantInfo { Code = "w02012", Name = "结核杆菌", EnglishName = "Mycobecterium Tuberculosis" }},
            { "w02013", new PollutantInfo { Code = "w02013", Name = "军团菌", EnglishName = "Legionella" }},
            { "w02014", new PollutantInfo { Code = "w02014", Name = "铜绿假单胞菌", EnglishName = "Pseudomonas Aeruginosa" }},
            { "w02015", new PollutantInfo { Code = "w02015", Name = "贾第鞭毛虫", EnglishName = "Giardia" }},
            { "w02016", new PollutantInfo { Code = "w02016", Name = "隐孢子虫", EnglishName = "Cryptosporidium Tyzzer" }},
            { "w02017", new PollutantInfo { Code = "w02017", Name = "兰伯氏贾第虫", EnglishName = "Giardia Lamblia" }},
            { "w02018", new PollutantInfo { Code = "w02018", Name = "病毒", EnglishName = "Virus" }},
            { "w02019", new PollutantInfo { Code = "w02019", Name = "藻类", EnglishName = "Algae" }},
            { "w03000", new PollutantInfo { Code = "w03000", Name = "放射性指标", EnglishName = "Radioactive Indicators" }},
            { "w03001", new PollutantInfo { Code = "w03001", Name = "总α放射性", EnglishName = "Total α-ray Intensity" }},
            { "w03002", new PollutantInfo { Code = "w03002", Name = "总β放射性", EnglishName = "Total β-ray Intensity" }},
            { "w03003", new PollutantInfo { Code = "w03003", Name = "镭-226 和镭-228", EnglishName = "Radium-226 and Radium-228 (combined)" }},
            { "w03004", new PollutantInfo { Code = "w03004", Name = "氚", EnglishName = "Tritium" }},
            
            // 其他指标
            { "w19000", new PollutantInfo { Code = "w19000", Name = "其他指标", EnglishName = "Other Indicators" }},
            { "w19001", new PollutantInfo { Code = "w19001", Name = "表面活性剂", EnglishName = "Surface Active Agent" }},
            { "w19002", new PollutantInfo { Code = "w19002", Name = "阴离子表面活性剂", EnglishName = "Anion Surface Active Agent" }},
            { "w19003", new PollutantInfo { Code = "w19003", Name = "阴离子合成洗涤剂", EnglishName = "Anion Detergent" }},
            { "w19004", new PollutantInfo { Code = "w19004", Name = "彩色显影剂", EnglishName = "Color Developing Agent" }},
            { "w19005", new PollutantInfo { Code = "w19005", Name = "显影剂及氧化物总量", EnglishName = "Developing Agent and Total Oxide" }},
            { "w19006", new PollutantInfo { Code = "w19006", Name = "腐蚀性", EnglishName = "Corrosivity" }},
            { "w19007", new PollutantInfo { Code = "w19007", Name = "发泡剂", EnglishName = "Foaming Agents" }},
            
            // 金属及金属化合物
            { "w20000", new PollutantInfo { Code = "w20000", Name = "金属及金属化合物", EnglishName = "Metal and Metallic Compounds" }},
            { "w20002", new PollutantInfo { Code = "w20002", Name = "铝", EnglishName = "Aluminum" }},
            { "w20004", new PollutantInfo { Code = "w20004", Name = "锑", EnglishName = "Antimony" }},
            { "w20012", new PollutantInfo { Code = "w20012", Name = "钡", EnglishName = "Barium" }},
            { "w20023", new PollutantInfo { Code = "w20023", Name = "硼", EnglishName = "Boron" }},
            { "w20038", new PollutantInfo { Code = "w20038", Name = "钴", EnglishName = "Cobalt" }},
            { "w20047", new PollutantInfo { Code = "w20047", Name = "四乙基铅", EnglishName = "Tetraethyl-lead" }},
            { "w20061", new PollutantInfo { Code = "w20061", Name = "钼", EnglishName = "Molybdenum" }},
            { "w20075", new PollutantInfo { Code = "w20075", Name = "钠", EnglishName = "Sodium" }},
            { "w20089", new PollutantInfo { Code = "w20089", Name = "铊", EnglishName = "Thallium" }},
            { "w20092", new PollutantInfo { Code = "w20092", Name = "锡", EnglishName = "Tin" }},
            { "w20095", new PollutantInfo { Code = "w20095", Name = "钛", EnglishName = "Titanium" }},
            { "w20098", new PollutantInfo { Code = "w20098", Name = "钨", EnglishName = "Tungsten" }},
            { "w20101", new PollutantInfo { Code = "w20101", Name = "钒", EnglishName = "Vanadium" }},
            { "w20110", new PollutantInfo { Code = "w20110", Name = "铀", EnglishName = "Uranium" }},
            { "w20111", new PollutantInfo { Code = "w20111", Name = "总汞", EnglishName = "Mercury (total)" }},
            { "w20112", new PollutantInfo { Code = "w20112", Name = "无机汞", EnglishName = "Mercury (inorganic)" }},
            { "w20113", new PollutantInfo { Code = "w20113", Name = "烷基汞", EnglishName = "Alkyl Mercury" }},
            { "w20114", new PollutantInfo { Code = "w20114", Name = "氯化乙基汞", EnglishName = "Ethylmercurry Chloride" }},
            { "w20115", new PollutantInfo { Code = "w20115", Name = "总镉", EnglishName = "Cadmium (total)" }},
            { "w20116", new PollutantInfo { Code = "w20116", Name = "总铬", EnglishName = "Chromium (total)" }},
            { "w20117", new PollutantInfo { Code = "w20117", Name = "六价铬", EnglishName = "Chromium (Ⅵ) Compounds" }},
            { "w20118", new PollutantInfo { Code = "w20118", Name = "三价铬", EnglishName = "Chromium (Ⅲ) Compounds" }},
            { "w20119", new PollutantInfo { Code = "w20119", Name = "总砷", EnglishName = "Arsenic (total)" }},
            { "w20120", new PollutantInfo { Code = "w20120", Name = "总铅", EnglishName = "Lead (total)" }},
            { "w20121", new PollutantInfo { Code = "w20121", Name = "总镍", EnglishName = "Nickel (total)" }},
            { "w20122", new PollutantInfo { Code = "w20122", Name = "总铜", EnglishName = "Copper (total)" }},
            { "w20123", new PollutantInfo { Code = "w20123", Name = "总锌", EnglishName = "Zinc (total)" }},
            { "w20124", new PollutantInfo { Code = "w20124", Name = "总锰", EnglishName = "Manganese (total)" }},
            { "w20125", new PollutantInfo { Code = "w20125", Name = "总铁", EnglishName = "Iron (total)" }},
            { "w20126", new PollutantInfo { Code = "w20126", Name = "总银", EnglishName = "Silver (total)" }},
            { "w20127", new PollutantInfo { Code = "w20127", Name = "总铍", EnglishName = "Beryllium (total)" }},
            { "w20128", new PollutantInfo { Code = "w20128", Name = "总硒", EnglishName = "Selenium (total)" }},
            { "w20129", new PollutantInfo { Code = "w20129", Name = "二烃基锡", EnglishName = "Dialkyltins" }},
            { "w20130", new PollutantInfo { Code = "w20130", Name = "三丁基氧化锡", EnglishName = "Tributin Oxide" }},
            { "w20131", new PollutantInfo { Code = "w20131", Name = "四乙基锡", EnglishName = "Tetraethyltin" }},
            { "w20132", new PollutantInfo { Code = "w20132", Name = "三丁基锡", EnglishName = "Tributyltin" }},
            { "w20133", new PollutantInfo { Code = "w20133", Name = "二氯二丁基锡", EnglishName = "Dichloride Dibutyltin" }},
            { "w20134", new PollutantInfo { Code = "w20134", Name = "二丁基硫化锡", EnglishName = "Dibutyltin Sulfide" }},
            { "w20135", new PollutantInfo { Code = "w20135", Name = "三苯基锡", EnglishName = "Triphenyltin" }},
            { "w20136", new PollutantInfo { Code = "w20136", Name = "甲基汞", EnglishName = "Methyl Mercury" }},
            { "w20137", new PollutantInfo { Code = "w20137", Name = "二乙基汞", EnglishName = "Diethyl Mercury" }},

            // 无机污染物
            { "w21000", new PollutantInfo { Code = "w21000", Name = "无机污染物", EnglishName = "Inorganic Pollutants" }},
            { "w21001", new PollutantInfo { Code = "w21001", Name = "总氮（以 N 计）", EnglishName = "Nitrogen (total)" }},
            { "w21002", new PollutantInfo { Code = "w21002", Name = "无机氮", EnglishName = "Inorganic Nitrogen" }},
            { "w21003", new PollutantInfo { Code = "w21003", Name = "氨氮（NH3-N）", EnglishName = "Ammonnia-Nitrogen" }},
            { "w21004", new PollutantInfo { Code = "w21004", Name = "凯氏氮", EnglishName = "Kjeldahl Nitrogen" }},
            { "w21005", new PollutantInfo { Code = "w21005", Name = "非离子氨", EnglishName = "Nonionized Ammonia (UIA)" }},
            { "w21006", new PollutantInfo { Code = "w21006", Name = "亚硝酸盐", EnglishName = "Nitrite" }},
            { "w21007", new PollutantInfo { Code = "w21007", Name = "硝酸盐（以 N 计）", EnglishName = "Nitrate (measured as Nitrogen)" }},
            { "w21008", new PollutantInfo { Code = "w21008", Name = "肼", EnglishName = "Hydrazine" }},
            { "w21009", new PollutantInfo { Code = "w21009", Name = "水合肼", EnglishName = "Hydrazine Hydrate" }},
            { "w21010", new PollutantInfo { Code = "w21010", Name = "叠氮化物（以 N3−计）", EnglishName = "Sodium Azide" }},
            { "w21011", new PollutantInfo { Code = "w21011", Name = "总磷（以 P 计）", EnglishName = "Phosphorus (total)" }},
            { "w21012", new PollutantInfo { Code = "w21012", Name = "元素磷", EnglishName = "Phosphorus" }},
            { "w21013", new PollutantInfo { Code = "w21013", Name = "黄磷", EnglishName = "Yellow Phosphorus" }},
            { "w21014", new PollutantInfo { Code = "w21014", Name = "磷酸", EnglishName = "Phosphoric Acid" }},
            { "w21015", new PollutantInfo { Code = "w21015", Name = "磷酸盐", EnglishName = "Phosphate" }},
            { "w21016", new PollutantInfo { Code = "w21016", Name = "氰化物", EnglishName = "Cyanide" }},
            { "w21017", new PollutantInfo { Code = "w21017", Name = "氟化物（以 F−计）", EnglishName = "Fluoride" }},
            { "w21018", new PollutantInfo { Code = "w21018", Name = "碘化物", EnglishName = "Iodide" }},
            { "w21019", new PollutantInfo { Code = "w21019", Name = "硫化物", EnglishName = "Sulphide" }},
            { "w21020", new PollutantInfo { Code = "w21020", Name = "溴化物", EnglishName = "Bromide" }},
            { "w21021", new PollutantInfo { Code = "w21021", Name = "硫氰化物", EnglishName = "Rhodanide" }},
            { "w21022", new PollutantInfo { Code = "w21022", Name = "氯化物（以 Cl−计）", EnglishName = "Chloride" }},
            { "w21023", new PollutantInfo { Code = "w21023", Name = "活性氯", EnglishName = "Active Chlorine" }},
            { "w21024", new PollutantInfo { Code = "w21024", Name = "余氯", EnglishName = "Residual Chlorine" }},
            { "w21025", new PollutantInfo { Code = "w21025", Name = "游离氯", EnglishName = "Free Chlorine" }},
            { "w21026", new PollutantInfo { Code = "w21026", Name = "二氧化硫", EnglishName = "Sulfur Dioxide" }},
            { "w21027", new PollutantInfo { Code = "w21027", Name = "石棉（纤维＞10 μm）", EnglishName = "Asbestos (fiber＞10micrometers)" }},
            { "w21028", new PollutantInfo { Code = "w21028", Name = "硫化氢", EnglishName = "Hydrogen Sulfide" }},
            { "w21029", new PollutantInfo { Code = "w21029", Name = "碘", EnglishName = "Iodine" }},
            { "w21030", new PollutantInfo { Code = "w21030", Name = "铁（Ⅱ、Ⅲ）氰络合物", EnglishName = "Ferrocyanide" }},
            { "w21031", new PollutantInfo { Code = "w21031", Name = "氯化氰", EnglishName = "Cyanogen Chloride" }},
            { "w21032", new PollutantInfo { Code = "w21032", Name = "酸度", EnglishName = "Acidity" }},
            { "w21033", new PollutantInfo { Code = "w21033", Name = "碱度", EnglishName = "Alkalinity" }},
            { "w21034", new PollutantInfo { Code = "w21034", Name = "游离碳酸", EnglishName = "Free Carbonic Acid" }},
            { "w21035", new PollutantInfo { Code = "w21035", Name = "盐酸", EnglishName = "Hydrochloric Acid" }},
            { "w21036", new PollutantInfo { Code = "w21036", Name = "硫酸", EnglishName = "Sulfuric Acid" }},
            { "w21037", new PollutantInfo { Code = "w21037", Name = "亚硫酸", EnglishName = "Sulfurous Acid" }},
            { "w21038", new PollutantInfo { Code = "w21038", Name = "硫酸盐（以 SO₄²⁻计）", EnglishName = "Sulfate" }},
            { "w21039", new PollutantInfo { Code = "w21039", Name = "亚硫酸盐", EnglishName = "Sulfite" }},
            { "w21040", new PollutantInfo { Code = "w21040", Name = "氯化钠", EnglishName = "Sodium Chloride" }},
            { "w21041", new PollutantInfo { Code = "w21041", Name = "碳酸钠", EnglishName = "Sodium Carbonate" }},

            // 油类
            { "w22000", new PollutantInfo { Code = "w22000", Name = "油类", EnglishName = "Oils" }},
            { "w22001", new PollutantInfo { Code = "w22001", Name = "石油类", EnglishName = "Petroleum Oils" }},
            { "w22002", new PollutantInfo { Code = "w22002", Name = "动植物油", EnglishName = "Animal and Vegetable Oil" }},
            { "w22003", new PollutantInfo { Code = "w22003", Name = "油", EnglishName = "Oil" }},
            { "w22004", new PollutantInfo { Code = "w22004", Name = "煤油", EnglishName = "Kerosine" }},
            { "w22005", new PollutantInfo { Code = "w22005", Name = "松油", EnglishName = "Pine Oil" }},
            { "w22006", new PollutantInfo { Code = "w22006", Name = "塔罗油", EnglishName = "Tall Oil" }},
            { "w22007", new PollutantInfo { Code = "w22007", Name = "松节油", EnglishName = "Turpentine Oil" }},

            // 酚类
            { "w23000", new PollutantInfo { Code = "w23000", Name = "酚", EnglishName = "Phenols" }},
            { "w23001", new PollutantInfo { Code = "w23001", Name = "酚类", EnglishName = "Phenols" }},
            { "w23002", new PollutantInfo { Code = "w23002", Name = "挥发酚", EnglishName = "Volatile Phenols" }},
            { "w23003", new PollutantInfo { Code = "w23003", Name = "苯酚", EnglishName = "Hydroxybenzene" }},
            { "w23004", new PollutantInfo { Code = "w23004", Name = "4-甲基苯酚（对甲苯酚）", EnglishName = "4-Methylphenol" }},
            { "w23005", new PollutantInfo { Code = "w23005", Name = "2-甲基苯酚（邻甲苯酚）", EnglishName = "2-Methylphenol" }},
            { "w23006", new PollutantInfo { Code = "w23006", Name = "2,5-二甲苯酚", EnglishName = "2,5-Xylenol" }},
            { "w23007", new PollutantInfo { Code = "w23007", Name = "2,4-二甲苯酚", EnglishName = "2,4-Xylenol" }},
            { "w23008", new PollutantInfo { Code = "w23008", Name = "3,4-二甲苯酚", EnglishName = "3,4-Xylenol" }},
            { "w23009", new PollutantInfo { Code = "w23009", Name = "3,5-二甲苯酚", EnglishName = "3,5-Xylenol" }},
            { "w23010", new PollutantInfo { Code = "w23010", Name = "3-甲基-4-氯苯酚", EnglishName = "3-Methyl-4-chlorophenol" }},
            { "w23011", new PollutantInfo { Code = "w23011", Name = "对氯-间甲苯酚", EnglishName = "p-Choro-m-cresol" }},
            { "w23012", new PollutantInfo { Code = "w23012", Name = "苯二酚", EnglishName = "Dihydroxybenzne" }},
            { "w23013", new PollutantInfo { Code = "w23013", Name = "1,3-苯二酚（间苯二酚）", EnglishName = "1,3-Benzenediol" }},
            { "w23014", new PollutantInfo { Code = "w23014", Name = "1,2-苯二酚（邻苯二酚）", EnglishName = "1,2-Benzenediol" }},
            { "w23015", new PollutantInfo { Code = "w23015", Name = "1,4-苯二酚（对苯二酚）", EnglishName = "1,4-Benzenediol" }},
            { "w23016", new PollutantInfo { Code = "w23016", Name = "5-甲基间苯二酚", EnglishName = "1,3-Dihydroxy-5-methylbenzene" }},
            { "w23017", new PollutantInfo { Code = "w23017", Name = "苯甲酚", EnglishName = "Cresol" }},
            { "w23018", new PollutantInfo { Code = "w23018", Name = "间-甲酚", EnglishName = "m-Methylphenol" }},
            { "w23019", new PollutantInfo { Code = "w23019", Name = "氯酚类", EnglishName = "Chlorophenols" }},
            { "w23020", new PollutantInfo { Code = "w23020", Name = "2,4-二氯苯酚", EnglishName = "2,4-Dichlorophenol" }},
            { "w23021", new PollutantInfo { Code = "w23021", Name = "4-氯苯酚（对氯苯酚）", EnglishName = "4-Chlorophenol" }},
            { "w23022", new PollutantInfo { Code = "w23022", Name = "2,4,6-三氯苯酚", EnglishName = "2,4,6-Trichlorophenol" }},
            { "w23023", new PollutantInfo { Code = "w23023", Name = "2-氯酚", EnglishName = "2-Chlorophenol" }},
            { "w23024", new PollutantInfo { Code = "w23024", Name = "2,4,5-三氯酚", EnglishName = "2,4,5-Trichlorophenol" }},
            { "w23025", new PollutantInfo { Code = "w23025", Name = "五氯酚", EnglishName = "Pentachlorophenol" }},
            { "w23026", new PollutantInfo { Code = "w23026", Name = "五氯酚钠", EnglishName = "Sodium Pentachlorophenate" }},
            { "w23027", new PollutantInfo { Code = "w23027", Name = "双酚 A", EnglishName = "2,2-Di(4-hydroxyphenyl)propane" }},
            { "w23028", new PollutantInfo { Code = "w23028", Name = "硝基酚类", EnglishName = "Nitrophenols" }},
            { "w23029", new PollutantInfo { Code = "w23029", Name = "间硝基苯酚", EnglishName = "m-Nitrophenol" }},
            { "w23030", new PollutantInfo { Code = "w23030", Name = "邻硝基苯酚", EnglishName = "o-Nitrophenol" }},
            { "w23031", new PollutantInfo { Code = "w23031", Name = "对硝基苯酚", EnglishName = "p-Nitrophenol" }},
            { "w23032", new PollutantInfo { Code = "w23032", Name = "二硝基苯酚", EnglishName = "Dinitrophenols" }},
            { "w23033", new PollutantInfo { Code = "w23033", Name = "2,4-二硝基苯酚", EnglishName = "2,4-Dinitrophenol" }},
            { "w23034", new PollutantInfo { Code = "w23034", Name = "2-甲-4,6-二硝苯酚", EnglishName = "2-Methyl-4,6-dinitrophenol" }},
            { "w23035", new PollutantInfo { Code = "w23035", Name = "4,6-二硝基-对甲苯酚", EnglishName = "4,6-Dinitro-p-cresol" }},
            { "w23036", new PollutantInfo { Code = "w23036", Name = "2,4,6-三硝基酚（苦味酸）", EnglishName = "2,4,6-Trinitrophenol" }},
            { "w23037", new PollutantInfo { Code = "w23037", Name = "α-萘酚", EnglishName = "α-Naphthol" }},
            { "w23038", new PollutantInfo { Code = "w23038", Name = "β-萘酚", EnglishName = "β-Naphthol" }},

            // 脂肪烃和卤代脂肪烃
            { "w24000", new PollutantInfo { Code = "w24000", Name = "脂肪烃和卤代脂肪烃", EnglishName = "Aliphatic Hydrocarbons and Halogenated Aliphatic Hydrocarbons" }},
            { "w24001", new PollutantInfo { Code = "w24001", Name = "挥发性卤代烃", EnglishName = "Volatile Halohydrocarbon" }},
            { "w24002", new PollutantInfo { Code = "w24002", Name = "一氯甲烷", EnglishName = "Chloromethane" }},
            { "w24003", new PollutantInfo { Code = "w24003", Name = "二氯甲烷", EnglishName = "Dichloromethane" }},
            { "w24004", new PollutantInfo { Code = "w24004", Name = "三氯甲烷", EnglishName = "Trichloromethane" }},
            { "w24005", new PollutantInfo { Code = "w24005", Name = "四氯甲烷（四氯化碳）", EnglishName = "Tetrachloromethane" }},
            { "w24006", new PollutantInfo { Code = "w24006", Name = "一氯二溴甲烷", EnglishName = "Dibromochloromethane" }},
            { "w24007", new PollutantInfo { Code = "w24007", Name = "二氯一溴甲烷", EnglishName = "Bromodichloromethane" }},
            { "w24008", new PollutantInfo { Code = "w24008", Name = "溴甲烷", EnglishName = "Bromomethane" }},
            { "w24009", new PollutantInfo { Code = "w24009", Name = "三溴甲烷", EnglishName = "Tribromomethane" }},
            { "w24010", new PollutantInfo { Code = "w24010", Name = "硝基甲烷", EnglishName = "Nitromethane" }},
            { "w24011", new PollutantInfo { Code = "w24011", Name = "三硝基甲烷", EnglishName = "Trinitromethane" }},
            { "w24012", new PollutantInfo { Code = "w24012", Name = "四硝基甲烷", EnglishName = "Tetranitromethane" }},
            { "w24013", new PollutantInfo { Code = "w24013", Name = "三氯硝基甲烷", EnglishName = "Nitrotrichloromethane" }},
            { "w24014", new PollutantInfo { Code = "w24014", Name = "二氰甲烷（丙二腈）", EnglishName = "Propanedinitrile" }},
            { "w24015", new PollutantInfo { Code = "w24015", Name = "氯乙烷", EnglishName = "Chloroethane" }},
            { "w24016", new PollutantInfo { Code = "w24016", Name = "1,1-二氯乙烷", EnglishName = "1,1-Dichloroethene" }},
            { "w24017", new PollutantInfo { Code = "w24017", Name = "1,2-二氯乙烷", EnglishName = "1,2-Dichloroethane" }},
            { "w24018", new PollutantInfo { Code = "w24018", Name = "1,1,1-三氯乙烷", EnglishName = "1,1,1-Trichloroethane" }},
            { "w24019", new PollutantInfo { Code = "w24019", Name = "1,1,2-三氯乙烷", EnglishName = "1,1,2-Trichloroethane" }},
            { "w24020", new PollutantInfo { Code = "w24020", Name = "1,1,2,2-四氯乙烷", EnglishName = "1,1,2,2-Tetrachloroethane" }},
            { "w24021", new PollutantInfo { Code = "w24021", Name = "六氯乙烷", EnglishName = "Hexachloroethane" }},
            { "w24022", new PollutantInfo { Code = "w24022", Name = "硝基乙烷", EnglishName = "Nitroethane" }},
            { "w24023", new PollutantInfo { Code = "w24023", Name = "1,2-二氰基乙烷（丁二腈）", EnglishName = "1,2-Dicyanoethane" }},
            { "w24024", new PollutantInfo { Code = "w24024", Name = "环氧氯丙烷", EnglishName = "Epichlorohydrin" }},
            { "w24025", new PollutantInfo { Code = "w24025", Name = "五氯丙烷", EnglishName = "Pentachloropropane" }},
            { "w24026", new PollutantInfo { Code = "w24026", Name = "1,2-二氯-2-甲基丙烷", EnglishName = "1,2-Dichloro-2-methylpropane" }},
            { "w24027", new PollutantInfo { Code = "w24027", Name = "1,2-二氯丙烷", EnglishName = "1,2-Dichloropropane" }},
            { "w24028", new PollutantInfo { Code = "w24028", Name = "1,3-二氯丙烷", EnglishName = "1,3-Dichloropropane" }},
            { "w24029", new PollutantInfo { Code = "w24029", Name = "1,2-二溴-3-氯丙烷", EnglishName = "1,2-Dibromo-3-chloropropane (DBCP)" }},
            { "w24030", new PollutantInfo { Code = "w24030", Name = "1,3-二苯基丙烷", EnglishName = "1,3-Diphenylpropane" }},
            { "w24032", new PollutantInfo { Code = "w24032", Name = "硝基丙烷", EnglishName = "Nitropropane" }},
            { "w24033", new PollutantInfo { Code = "w24033", Name = "四氯丙烷", EnglishName = "Tetrachloropropane" }},
            { "w24034", new PollutantInfo { Code = "w24034", Name = "1,1,1,3,3-五氯丁烷", EnglishName = "1,1,1,3,3-Pentachlorobutane" }},
            { "w24035", new PollutantInfo { Code = "w24035", Name = "1,1,1,5-四氯戊烷", EnglishName = "1,1,1,5-Tetrachloropentane" }},
            { "w24036", new PollutantInfo { Code = "w24036", Name = "环己烷", EnglishName = "Cyclohexane" }},
            { "w24037", new PollutantInfo { Code = "w24037", Name = "硝基环己烷", EnglishName = "Nitrocyclohexane" }},
            { "w24038", new PollutantInfo { Code = "w24038", Name = "1,4-二氯环己烷", EnglishName = "1,4-Dichlorocyclohexane" }},
            { "w24039", new PollutantInfo { Code = "w24039", Name = "氯代环己烷", EnglishName = "Chlorocyclohexane" }},
            { "w24040", new PollutantInfo { Code = "w24040", Name = "1,4-氧氮杂环己烷（吗啉）", EnglishName = "Morpholine" }},
            
            // 氟利昂类
            { "w24041", new PollutantInfo { Code = "w24041", Name = "氟三氯甲烷", EnglishName = "Trichorofluoromethane" }},
            { "w24042", new PollutantInfo { Code = "w24042", Name = "二氟二氯甲烷（氟利昂-12）", EnglishName = "Difluorodichloromethane" }},
            { "w24043", new PollutantInfo { Code = "w24043", Name = "二氟氯甲烷（氟利昂-22）", EnglishName = "Chlorodifluoromethane" }},
            { "w24044", new PollutantInfo { Code = "w24044", Name = "3,3,3-三氟-1氯丙烷（氟利昂-253）", EnglishName = "Freon-253" }},
            
            // 烯烃类
            { "w24045", new PollutantInfo { Code = "w24045", Name = "乙烯", EnglishName = "Ethylene" }},
            { "w24046", new PollutantInfo { Code = "w24046", Name = "氯乙烯", EnglishName = "Vinyl Chloride" }},
            { "w24047", new PollutantInfo { Code = "w24047", Name = "1,1-二氯乙烯", EnglishName = "1,1-Dichloroethylene" }},
            { "w24048", new PollutantInfo { Code = "w24048", Name = "1,2-二氯乙烯", EnglishName = "1,2-Dichloroethylene" }},
            { "w24049", new PollutantInfo { Code = "w24049", Name = "三氯乙烯", EnglishName = "Trichloroethylene" }},
            { "w24050", new PollutantInfo { Code = "w24050", Name = "四氯乙烯", EnglishName = "Tetrachloroethylene" }},
            { "w24051", new PollutantInfo { Code = "w24051", Name = "二溴乙烯", EnglishName = "Dibromoethylene" }},
            { "w24052", new PollutantInfo { Code = "w24052", Name = "α-甲基苯乙烯", EnglishName = "α-Methylstyrene" }},
            { "w24053", new PollutantInfo { Code = "w24053", Name = "丙烯", EnglishName = "Propylene" }},
            { "w24054", new PollutantInfo { Code = "w24054", Name = "1,3-二氯丙烯", EnglishName = "1,3-Dichloropropene" }},
            { "w24055", new PollutantInfo { Code = "w24055", Name = "3,3-二氯-2-甲基丙烯-[1]", EnglishName = "2,3-Dichloro-2-methyl-1-propene" }},
            { "w24056", new PollutantInfo { Code = "w24056", Name = "2,3-二氯丙烯-[1]", EnglishName = "2,3-Dichloro-1-propene" }},
            { "w24057", new PollutantInfo { Code = "w24057", Name = "三氯丙烯", EnglishName = "Trichloropropylene" }},
            { "w24058", new PollutantInfo { Code = "w24058", Name = "异丁烯", EnglishName = "Isobutene" }},
            { "w24059", new PollutantInfo { Code = "w24059", Name = "1,3-二氯丁烯-[2]", EnglishName = "1,3-Dichloro-2-butene" }},
            { "w24060", new PollutantInfo { Code = "w24060", Name = "丁二烯", EnglishName = "Butadiene" }},
            { "w24061", new PollutantInfo { Code = "w24061", Name = "异戊二烯", EnglishName = "Isoprene" }},
            { "w24062", new PollutantInfo { Code = "w24062", Name = "氯丁二烯", EnglishName = "Chloroprene" }},
            { "w24063", new PollutantInfo { Code = "w24063", Name = "氯丁二烯-[1,3]", EnglishName = "Chloro-1,3-butadiene" }},
            { "w24064", new PollutantInfo { Code = "w24064", Name = "六氯丁二烯", EnglishName = "Hexachlorobutadiene" }},
            { "w24065", new PollutantInfo { Code = "w24065", Name = "六氯环戊二烯", EnglishName = "Hexachlorocyclopentadiene" }},
            { "w24066", new PollutantInfo { Code = "w24066", Name = "二聚环戊二烯", EnglishName = "Dicyclopentadiene" }},
            { "w24067", new PollutantInfo { Code = "w24067", Name = "环己烯", EnglishName = "Cyclohexene" }},
            { "w24068", new PollutantInfo { Code = "w24068", Name = "乙烯基乙炔", EnglishName = "Vinyl Acetylene" }},
            { "w24069", new PollutantInfo { Code = "w24069", Name = "七氯", EnglishName = "Heptachlor" }},
            
            // 芳香族化合物
            { "w25000", new PollutantInfo { Code = "w25000", Name = "芳香族化合物", EnglishName = "Aromatic Compounds" }},
            { "w25001", new PollutantInfo { Code = "w25001", Name = "苯系物", EnglishName = "Benzene Series" }},
            { "w25002", new PollutantInfo { Code = "w25002", Name = "苯", EnglishName = "Benzene" }},
            { "w25003", new PollutantInfo { Code = "w25003", Name = "甲苯", EnglishName = "Toluene" }},
            { "w25004", new PollutantInfo { Code = "w25004", Name = "乙苯", EnglishName = "Ethylbenzene" }},
            { "w25005", new PollutantInfo { Code = "w25005", Name = "二甲苯", EnglishName = "Xylene" }},
            { "w25006", new PollutantInfo { Code = "w25006", Name = "邻二甲苯", EnglishName = "o-Xylenes" }},
            { "w25007", new PollutantInfo { Code = "w25007", Name = "对二甲苯", EnglishName = "p-Xylenes" }},
            { "w25008", new PollutantInfo { Code = "w25008", Name = "间二甲苯", EnglishName = "m-Xylenes" }},
            { "w25009", new PollutantInfo { Code = "w25009", Name = "氯代苯类", EnglishName = "Chlorobenzenes" }},
            { "w25010", new PollutantInfo { Code = "w25010", Name = "氯苯", EnglishName = "Chlorobenzene" }},
            { "w25011", new PollutantInfo { Code = "w25011", Name = "1,2-二氯苯", EnglishName = "1,2-Dichlorobenzene" }},
            { "w25012", new PollutantInfo { Code = "w25012", Name = "1,3-二氯苯", EnglishName = "1,3-Dichlorobenzene" }},
            { "w25013", new PollutantInfo { Code = "w25013", Name = "1,4-二氯苯", EnglishName = "1,4-Dichlorobenzene" }},
            { "w25014", new PollutantInfo { Code = "w25014", Name = "三氯苯（总）", EnglishName = "Trichlorobenzenes" }},
            { "w25015", new PollutantInfo { Code = "w25015", Name = "1,2,4-三氯苯", EnglishName = "1,2,4-Trichlorobenzene" }},
            { "w25016", new PollutantInfo { Code = "w25016", Name = "四氯苯", EnglishName = "Tetrachlorobenzene" }},
            { "w25017", new PollutantInfo { Code = "w25017", Name = "1,2,4,5-四氯苯", EnglishName = "1,2,4,5-Tetrachlorobenzene" }},
            { "w25018", new PollutantInfo { Code = "w25018", Name = "五氯苯", EnglishName = "Pentachlorobenzene" }},
            { "w25019", new PollutantInfo { Code = "w25019", Name = "六氯苯", EnglishName = "Hexachlorobenzene" }},
            
            // 硝基苯类
            { "w25020", new PollutantInfo { Code = "w25020", Name = "硝基氯苯", EnglishName = "Nitrochlorobenzene" }},
            { "w25021", new PollutantInfo { Code = "w25021", Name = "对硝基氯苯", EnglishName = "p-Chloronitrobenzene" }},
            { "w25022", new PollutantInfo { Code = "w25022", Name = "2,4-二硝基氯苯", EnglishName = "2,4-Dinitrochlorobenzene" }},
            { "w25023", new PollutantInfo { Code = "w25023", Name = "硝基苯类", EnglishName = "Nitrobenzenes" }},
            { "w25024", new PollutantInfo { Code = "w25024", Name = "二硝基甲苯", EnglishName = "Dinitrotoluene" }},
            { "w25025", new PollutantInfo { Code = "w25025", Name = "三硝基甲苯", EnglishName = "Trinitrotoluene" }},
            { "w25026", new PollutantInfo { Code = "w25026", Name = "环三亚甲基三硝胺（黑索今）", EnglishName = "Cyclotrimethylene Trinitramine (RDX)" }},
            { "w25027", new PollutantInfo { Code = "w25027", Name = "二硝基苯", EnglishName = "Dinitrobenzene" }},
            { "w25028", new PollutantInfo { Code = "w25028", Name = "邻硝基甲苯", EnglishName = "o-Nitrotoluene" }},
            { "w25029", new PollutantInfo { Code = "w25029", Name = "对硝基甲苯", EnglishName = "p-Nitrotoluene" }},
            { "w25030", new PollutantInfo { Code = "w25030", Name = "2,4-二硝基甲苯", EnglishName = "2,4-Dinitrotoluene" }},
            { "w25031", new PollutantInfo { Code = "w25031", Name = "2,6-二硝基甲苯", EnglishName = "2,6-Dinitrotoluene" }},
            { "w25032", new PollutantInfo { Code = "w25032", Name = "2,4,6-三硝基甲苯", EnglishName = "2,4,6-Trinitrotoluene" }},
            
            // 烷基苯类
            { "w25033", new PollutantInfo { Code = "w25033", Name = "丙苯", EnglishName = "n-Propylbenzene" }},
            { "w25034", new PollutantInfo { Code = "w25034", Name = "异丙苯", EnglishName = "Isopropylbenzene" }},
            { "w25035", new PollutantInfo { Code = "w25035", Name = "间二异丙基苯", EnglishName = "1,3-Diisopropylbenzene" }},
            { "w25036", new PollutantInfo { Code = "w25036", Name = "对二异丙基苯", EnglishName = "1,4-Diisopropylbenzene" }},
            { "w25037", new PollutantInfo { Code = "w25037", Name = "丁苯", EnglishName = "n-Butylbenzene" }},
            { "w25038", new PollutantInfo { Code = "w25038", Name = "苯乙烯", EnglishName = "Styrene" }},
            { "w25039", new PollutantInfo { Code = "w25039", Name = "二氨基甲苯", EnglishName = "Diaminotoluene" }},
            { "w25040", new PollutantInfo { Code = "w25040", Name = "二溴乙苯", EnglishName = "Ethylene Dibromide" }},
            
            // 多环芳烃
            { "w25041", new PollutantInfo { Code = "w25041", Name = "多环芳烃", EnglishName = "Polycyclic Aromatic Hydrocarbons" }},
            { "w25042", new PollutantInfo { Code = "w25042", Name = "芘", EnglishName = "Pyrene" }},
            { "w25043", new PollutantInfo { Code = "w25043", Name = "苯并[a]芘", EnglishName = "Benzo(a)Pyrene (PAHs)" }},
            { "w25044", new PollutantInfo { Code = "w25044", Name = "苯并[ghi]芘", EnglishName = "Benzo(g,h,i)Pyrene" }},
            { "w25045", new PollutantInfo { Code = "w25045", Name = "茚[1,2,3-cd]芘", EnglishName = "Indeno(1,2,3-cd)Pyrene" }},
            { "w25046", new PollutantInfo { Code = "w25046", Name = "蒽", EnglishName = "Anthracene" }},
            { "w25047", new PollutantInfo { Code = "w25047", Name = "苯并[a]蒽", EnglishName = "Benzo(a)Anthracene" }},
            { "w25048", new PollutantInfo { Code = "w25048", Name = "二苯并(a,h)蒽", EnglishName = "Dibenzo(a,h)Anthracene" }},
            { "w25049", new PollutantInfo { Code = "w25049", Name = "荧蒽", EnglishName = "Fluoranthene" }},
            { "w25050", new PollutantInfo { Code = "w25050", Name = "苯并[b]荧蒽", EnglishName = "Benzo(b)Fluoranthene" }},
            { "w25051", new PollutantInfo { Code = "w25051", Name = "苯并[k]荧蒽", EnglishName = "Benzo(k)Fluoranthene" }},
            
            // 吡啶类
            { "w25052", new PollutantInfo { Code = "w25052", Name = "吡啶", EnglishName = "Pyridine" }},
            { "w25053", new PollutantInfo { Code = "w25053", Name = "α-甲基吡啶", EnglishName = "α-Methylpyridine" }},
            { "w25054", new PollutantInfo { Code = "w25054", Name = "2,5-二甲吡啶", EnglishName = "2,5-Dimethylpyridine" }},
        };
    });

    public static Dictionary<string, PollutantInfo> PollutantMap => _lazyPollutantMap.Value;


    // 获取污染物信息
    public static PollutantInfo GetPollutantInfo(string code)
    {
        if (PollutantMap.TryGetValue(code, out var info))
        {
            return info;
        }
        return new PollutantInfo { Code = code, Name = code };
    }

    // 获取所有污染物编码映射
    public static Dictionary<string, PollutantInfo> GetAllPollutantCodes()
    {
        return new Dictionary<string, PollutantInfo>(PollutantMap);
    }
}